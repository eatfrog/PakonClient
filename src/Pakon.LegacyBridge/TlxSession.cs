using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using TLXLib;

namespace Pakon.LegacyBridge
{
    /// <summary>
    /// Owns the established TLX scan/save facade inside the x86 bridge. The public pipe contract
    /// uses only behaviour and scalar values; COM interfaces and their lifetime never leave here.
    /// Direct TLC remains available separately for protocol research.
    /// </summary>
    internal sealed class TlxSession
    {
        private const int DefaultInitializationFlags = 0x40000001;
        private const int DefaultMemoryTimeoutMilliseconds = 200000;
        private const int DefaultInitializationReturnTimeoutSeconds = 60;
        private static readonly Guid TlxMainClassId = new Guid("EA82986B-E47C-4C0F-97EA-FB50ED216D2E");
        private readonly object sync = new object();
        private TLXMainClass instance;
        private TlxProgressCallback callback;
        private int callbackCookie;
        private string state = "Closed";
        private string failure;
        private TlxTraceRecorder trace;
        private Dictionary<string, PfsBufferState> pfsBuffersAtOpen;
        private IntPtr rawBufferPointer1;
        private IntPtr rawBufferPointer2;
        private int rawBufferSize;
        private bool rawBufferOneIsNext = true;
        private string pendingRawOutputPath;

        public IDictionary<string, string> BeginTrace(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                if (trace != null) throw new InvalidOperationException("A TLX trace is already active. End it before starting another.");
                var directory = GetString(arguments, "directory");
                if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Bridge argument 'directory' is required.");
                trace = new TlxTraceRecorder(directory);
                trace.CaptureRuntimeEvidence();
                Log("trace started: " + trace.DirectoryPath);
                Trace("session-status", GetStatusCore());
                var result = GetStatusCore(); result["traceDirectory"] = trace.DirectoryPath;
                return result;
            }
        }

        public IDictionary<string, string> EndTrace()
        {
            lock (sync)
            {
                if (trace == null) throw new InvalidOperationException("No TLX trace is active.");
                var directory = trace.DirectoryPath;
                trace.CaptureRuntimeEvidence();
                Trace("trace-ended", GetStatusCore());
                trace.Dispose(); trace = null;
                Log("trace ended: " + directory);
                var result = GetStatusCore(); result["traceDirectory"] = directory;
                return result;
            }
        }

        public IDictionary<string, string> Initialize(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                RequireClosed();
                var flags = GetInt32(arguments, "initializationFlags", DefaultInitializationFlags);
                var timeout = GetInt32(arguments, "memoryTimeoutMilliseconds", DefaultMemoryTimeoutMilliseconds);
                var returnTimeoutSeconds = GetInt32(arguments, "initializationReturnTimeoutSeconds", DefaultInitializationReturnTimeoutSeconds);
                if ((flags & 0x2) != 0) throw new InvalidOperationException("Firmware update is permanently prohibited by this bridge.");
                if (timeout < 1000) throw new ArgumentOutOfRangeException("TLC initialization timeout must be at least 1,000 ms.");
                if (returnTimeoutSeconds < 1) throw new ArgumentOutOfRangeException("TLX initialization return timeout must be at least one second.");

                PrepareNativeRuntime(GetString(arguments, "comServerDirectory"));
                pfsBuffersAtOpen = CapturePfsBuffers();
                Log("initializing TLX facade");
                Trace("initialize-requested", new Dictionary<string, string>(arguments));
                try
                {
                    instance = new TLXMainClass();
                    callback = new TlxProgressCallback(this);
                    callbackCookie = instance.CBAdvise(callback);
                    if (callbackCookie == 0) throw new InvalidOperationException("TLX rejected the progress callback; initialization was not started.");
                    Log("TLX callback registered; cookie=" + callbackCookie.ToString(CultureInfo.InvariantCulture));
                    state = "Initializing";
                    // InitializeScanner can take a while. Do not hold the session lock across the
                    // native call: COM delivers progress/error callbacks concurrently, and the
                    // pipe controller must be able to observe them.
                    Monitor.Exit(sync);
                    // A healthy TLX facade accepts this asynchronous request promptly. The legacy
                    // client has observed pathological installations where this call never returns;
                    // terminate the isolated bridge rather than retain a wedged COM server.
                    try
                    {
                        using (var initializationReturned = new ManualResetEvent(false))
                        {
                            var watchdog = new Thread(
                                () =>
                                {
                                    if (!initializationReturned.WaitOne(TimeSpan.FromSeconds(returnTimeoutSeconds)))
                                    {
                                        Console.Error.WriteLine("TLX InitializeScanner did not return within " + returnTimeoutSeconds + " seconds; terminating the isolated bridge.");
                                        Environment.FailFast("TLX InitializeScanner blocked in the isolated bridge.");
                                    }
                                });
                            watchdog.IsBackground = true;
                            watchdog.Start();
                            Log("initialization watchdog armed for " + returnTimeoutSeconds.ToString(CultureInfo.InvariantCulture) + " seconds");
                            try { instance.InitializeScanner(flags, timeout); }
                            finally { initializationReturned.Set(); }
                        }
                    }
                    finally { Monitor.Enter(sync); }
                }
                catch (Exception exception)
                {
                    // EC_PreviousError (25) is a stack marker, not the underlying cause.
                    // Drain the native queue while the COM object is still alive so the
                    // controller receives the actionable initialization error.
                    var nativeErrors = DrainInitializationErrors(GetComErrorValue(exception));
                    CloseCore();
                    if (!string.IsNullOrWhiteSpace(nativeErrors))
                    {
                        throw new InvalidOperationException(
                            "TLX initialization failed. Native error stack: " + nativeErrors,
                            exception);
                    }
                    throw;
                }

                var result = GetStatusCore(); Trace("initialize-accepted", result); return result;
            }
        }

        public IDictionary<string, string> QueueInitialize(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                if (state != "Closed" || instance != null) throw new InvalidOperationException("A TLX session is already opening or open.");
                state = "InitializationQueued";
                failure = null;
                Trace("initialize-queued", new Dictionary<string, string>(arguments));
                return GetStatusCore();
            }
        }

        public void ReportAsynchronousFailure(Exception exception)
        {
            lock (sync)
            {
                state = "Faulted";
                failure = exception.GetType().Name + ": " + exception.Message;
                Log("asynchronous TLX failure: " + exception.GetType().Name + ": " + exception.Message);
                Trace("asynchronous-failure", new Dictionary<string, string> { { "exception", exception.GetType().FullName + ": " + exception.Message } });
            }
        }

        public IDictionary<string, string> ScanRoll(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                RequireReady();
                state = "Scanning";
                Log(
                    "starting scan: resolution=" + GetRequiredInt32(arguments, "resolution").ToString(CultureInfo.InvariantCulture) +
                    ", filmColor=" + GetRequiredInt32(arguments, "filmColor").ToString(CultureInfo.InvariantCulture) +
                    ", filmFormat=" + GetRequiredInt32(arguments, "filmFormat").ToString(CultureInfo.InvariantCulture) +
                    ", stripMode=" + GetRequiredInt32(arguments, "stripMode").ToString(CultureInfo.InvariantCulture) +
                    ", scanControl=0x" + unchecked((uint)GetRequiredInt32(arguments, "scanControl")).ToString("X8", CultureInfo.InvariantCulture));
                Trace("scan-requested", new Dictionary<string, string>(arguments));
                try
                {
                    instance.ScanPictures(
                        GetRequiredInt32(arguments, "resolution"), GetRequiredInt32(arguments, "filmColor"),
                        GetRequiredInt32(arguments, "filmFormat"), GetRequiredInt32(arguments, "stripMode"),
                        GetRequiredInt32(arguments, "scanControl"), "1000");
                }
                catch (Exception exception)
                {
                    var nativeErrors = DrainLastErrors(35, GetComErrorValue(exception));
                    state = "Ready";
                    throw new InvalidOperationException(
                        "TLX rejected the scan request (COM " +
                        GetComErrorValue(exception).ToString(CultureInfo.InvariantCulture) +
                        DescribeErrorCode(GetComErrorValue(exception)) + ")." +
                        (string.IsNullOrWhiteSpace(nativeErrors) ? string.Empty : " Native error stack: " + nativeErrors),
                        exception);
                }
                var result = GetStatusCore(); Trace("scan-accepted", result); return result;
            }
        }

        /// <summary>Known conservative F135 trace profile. It intentionally does not enable scratch removal or blind framing.</summary>
        public IDictionary<string, string> ScanTraceProfile()
        {
            lock (sync)
            {
                if (trace == null) throw new InvalidOperationException("A trace must be active before starting the trace scan profile.");
                var profile = new Dictionary<string, string>
                {
                    { "resolution", "2" }, { "filmColor", "1" }, { "filmFormat", "1" },
                    { "stripMode", "0" }, { "scanControl", "0" }, { "profile", "base16-negative-35mm-full-roll-normal-framing" }
                };
                Trace("trace-scan-profile", profile);
                return ScanRoll(profile);
            }
        }

        /// <summary>Writes an observable scanner and group-state snapshot without changing scanner state.</summary>
        public IDictionary<string, string> SnapshotTrace(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                if (trace == null) throw new InvalidOperationException("No TLX trace is active.");
                var label = GetString(arguments, "label");
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Bridge argument 'label' is required.");
                var values = SnapshotMetadata(label);
                Trace("metadata-snapshot", values);
                trace.WriteMetadataSnapshot(label, values);
                return values;
            }
        }

        /// <summary>Saves every trace-run picture as a deterministic JPEG in the trace folder.</summary>
        public IDictionary<string, string> SaveTraceJpegs()
        {
            lock (sync)
            {
                RequireReady();
                if (trace == null) throw new InvalidOperationException("A trace must be active before saving trace JPEGs.");
                var outputDirectory = Path.Combine(trace.DirectoryPath, "output-jpeg");
                Directory.CreateDirectory(outputDirectory);
                int rolls = 0, strips = 0, pictureCount = 0, selected = 0, hidden = 0;
                instance.GetPictureCountSaveGroup(ref rolls, ref strips, ref pictureCount, ref selected, ref hidden);
                if (pictureCount == 0) throw new InvalidOperationException("The TLX save group has no pictures. Move the scanned roll to the save group before saving.");
                for (var index = 0; index < pictureCount; index++)
                {
                    int roll = 0, strip = 0, product = 0, specifier = 0, frameNumber = 0, aspect = 0, rotation = 0, selection = 0;
                    string frameName = string.Empty, oldFileName = string.Empty, oldDirectory = string.Empty;
                    instance.GetPictureInfo3(index, out roll, out strip, out product, out specifier, out frameName, out frameNumber, out aspect, out oldFileName, out oldDirectory, out rotation, out selection);
                    instance.PutPictureInfo(index, frameNumber, string.Format(CultureInfo.InvariantCulture, "frame-{0:D3}.jpg", index + 1), outputDirectory, rotation, selection);
                }
                // Original dimensions, rotation, color correction + scene balance + adjustments;
                // Bicubic scaling, JPEG, quality 90, 300 DPI, 24-bit output.
                const int saveControl = 0x74;
                state = "Saving";
                var values = new Dictionary<string, string>
                {
                    { "outputDirectory", outputDirectory }, { "pictureCount", pictureCount.ToString(CultureInfo.InvariantCulture) },
                    { "saveControl", "0x74" }, { "fileFormat", "JPEG" }, { "compression", "90" }
                };
                Trace("trace-jpeg-save-requested", values);
                Log("saving " + pictureCount.ToString(CultureInfo.InvariantCulture) + " trace JPEG(s) to " + outputDirectory);
                instance.SaveToDisk(-3, null, saveControl, 0, 0, 0, 2, 0, 90, 300, 24);
                return values;
            }
        }

        public IDictionary<string, string> MoveOldestRollToSaveGroup()
        {
            lock (sync)
            {
                RequireReady();
                Log("moving oldest roll to save group");
                Trace("move-oldest-roll-to-save-group-requested", null);
                instance.MoveOldestRollToSaveGroup();
                var result = GetStatusCore(); Trace("move-oldest-roll-to-save-group-completed", result); return result;
            }
        }

        public IDictionary<string, string> SaveFramesToDisk(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                RequireReady();
                state = "Saving";
                Log("starting save-to-disk");
                Trace("save-to-disk-requested", new Dictionary<string, string>(arguments));
                instance.SaveToDisk(
                    GetRequiredInt32(arguments, "index"), null, GetRequiredInt32(arguments, "saveControl"),
                    GetRequiredInt32(arguments, "width"), GetRequiredInt32(arguments, "height"), 0,
                    GetRequiredInt32(arguments, "scalingMethod"), GetRequiredInt32(arguments, "fileFormat"),
                    GetRequiredInt32(arguments, "compression"), GetRequiredInt32(arguments, "dpi"),
                    GetRequiredInt32(arguments, "colorBits"));
                var result = GetStatusCore(); Trace("save-to-disk-accepted", result); return result;
            }
        }

        public IDictionary<string, string> GetFrames()
        {
            lock (sync)
            {
                RequireReady();
                int rolls = 0, strips = 0, count = 0, selected = 0, hidden = 0;
                instance.GetPictureCountSaveGroup(ref rolls, ref strips, ref count, ref selected, ref hidden);
                var result = new Dictionary<string, string>
                {
                    { "count", count.ToString(CultureInfo.InvariantCulture) }
                };
                for (var index = 0; index < count; index++)
                {
                    int roll, strip, product, specifier, frameNumber, aspect, rotation, selection;
                    string frameName, fileName, directory;
                    instance.GetPictureInfo3(index, out roll, out strip, out product, out specifier, out frameName, out frameNumber, out aspect, out fileName, out directory, out rotation, out selection);
                    var key = "frame." + index.ToString(CultureInfo.InvariantCulture) + ".";
                    result[key + "frameName"] = frameName ?? string.Empty;
                    result[key + "frameNumber"] = frameNumber.ToString(CultureInfo.InvariantCulture);
                    result[key + "filmProduct"] = product.ToString(CultureInfo.InvariantCulture);
                    result[key + "filmSpecifier"] = specifier.ToString(CultureInfo.InvariantCulture);
                    result[key + "rotation"] = rotation.ToString(CultureInfo.InvariantCulture);
                    result[key + "selection"] = selection.ToString(CultureInfo.InvariantCulture);
                }
                return result;
            }
        }

        public IDictionary<string, string> UpdateFrame(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                RequireReady();
                var index = GetRequiredInt32(arguments, "index");
                int roll, strip, product, specifier, frameNumber, aspect, oldRotation, oldSelection;
                string frameName, oldFileName, oldDirectory;
                instance.GetPictureInfo3(index, out roll, out strip, out product, out specifier, out frameName, out frameNumber, out aspect, out oldFileName, out oldDirectory, out oldRotation, out oldSelection);
                var fileName = GetString(arguments, "fileName");
                var directory = GetString(arguments, "directory");
                var rotation = GetInt32(arguments, "rotation", oldRotation);
                bool selected;
                if (!bool.TryParse(GetString(arguments, "selected"), out selected)) selected = oldSelection != 0;
                instance.PutPictureInfo(index, frameNumber, string.IsNullOrWhiteSpace(fileName) ? oldFileName : fileName, string.IsNullOrWhiteSpace(directory) ? oldDirectory : directory, rotation, selected ? 1 : 0);
                return new Dictionary<string, string> { { "index", index.ToString(CultureInfo.InvariantCulture) } };
            }
        }

        public IDictionary<string, string> RenderFrameToDisk(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                RequireReady();
                var index = GetRequiredInt32(arguments, "index");
                var requestedPath = Path.GetFullPath(GetString(arguments, "outputPath"));
                var directory = Path.GetDirectoryName(requestedPath);
                if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Bridge argument 'outputPath' must include a directory.");
                Directory.CreateDirectory(directory);
                int roll, strip, product, specifier, frameNumber, aspect, rotation, selection;
                string frameName, oldFileName, oldDirectory;
                instance.GetPictureInfo3(index, out roll, out strip, out product, out specifier, out frameName, out frameNumber, out aspect, out oldFileName, out oldDirectory, out rotation, out selection);
                var stem = Path.GetFileNameWithoutExtension(requestedPath);
                instance.PutPictureInfo(index, frameNumber, stem, directory, rotation, selection);
                state = "Saving";
                instance.SaveToDisk(index, null, GetRequiredInt32(arguments, "saveControl"),
                    GetInt32(arguments, "width", 0), GetInt32(arguments, "height", 0), 0, 0, 0,
                    GetInt32(arguments, "quality", 95), 300, 24);
                return new Dictionary<string, string>
                {
                    { "outputPath", Path.Combine(directory, stem + ".jpg") },
                    { "index", index.ToString(CultureInfo.InvariantCulture) }
                };
            }
        }

        public IDictionary<string, string> RenderFrameToRaw(IDictionary<string, string> arguments)
        {
            lock (sync)
            {
                RequireReady();
                var index = GetRequiredInt32(arguments, "index");
                var outputPath = Path.GetFullPath(GetString(arguments, "outputPath"));
                var directory = Path.GetDirectoryName(outputPath);
                if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Bridge argument 'outputPath' must include a directory.");
                Directory.CreateDirectory(directory);

                int left = 0, top = 0, right = 0, bottom = 0;
                instance.GetPictureFramingUserInfo(index, ref left, ref top, ref right, ref bottom);
                var width = right + 1 - left;
                var height = bottom + 1 - top;
                if (width <= 0 || height <= 0) throw new InvalidOperationException("Frame has invalid dimensions " + width + "x" + height + ".");
                AllocateRawBuffers(checked(16 + width * height * 6));
                pendingRawOutputPath = outputPath;
                RegisterNextRawBuffer();
                RegisterNextRawBuffer();
                state = "Saving";
                var saveControl = GetRequiredInt32(arguments, "saveControl") | 0x00000400;
                instance.SaveToClientMemory(index, saveControl, width, height, 0, 0, (int)FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_16, 1);
                return new Dictionary<string, string>
                {
                    { "outputPath", outputPath }, { "width", width.ToString(CultureInfo.InvariantCulture) },
                    { "height", height.ToString(CultureInfo.InvariantCulture) }
                };
            }
        }

        public IDictionary<string, string> CancelScan()
        {
            lock (sync) { RequireOpen(); Log("cancelling scan"); instance.ScanCancel(); state = "CancellingScan"; var result = GetStatusCore(); Trace("scan-cancel-requested", result); return result; }
        }

        public IDictionary<string, string> CancelSave()
        {
            lock (sync) { RequireOpen(); Log("cancelling save"); instance.SaveCancel(); state = "CancellingSave"; var result = GetStatusCore(); Trace("save-cancel-requested", result); return result; }
        }

        public IDictionary<string, string> GetStatus()
        {
            // Initialization can block in native code. Status must remain available to the pipe
            // controller while that call owns the session lock.
            if (!Monitor.TryEnter(sync))
            {
                return new Dictionary<string, string>
                {
                    { "sessionOpen", (instance != null).ToString() }, { "state", state },
                    { "busy", "true" }
                };
            }
            try { var result = GetStatusCore(); Trace("session-status", result); return result; }
            finally { Monitor.Exit(sync); }
        }

        public IDictionary<string, string> Close()
        {
            lock (sync) { Trace("session-close-requested", GetStatusCore()); CloseCore(); var result = GetStatusCore(); Trace("session-closed", result); return result; }
        }

        private IDictionary<string, string> GetStatusCore()
        {
            var result = new Dictionary<string, string>
            {
                { "sessionOpen", (instance != null).ToString() }, { "state", state },
                { "callbackCookie", callbackCookie.ToString(CultureInfo.InvariantCulture) }
            };
            if (callback != null)
            {
                result["callbackCount"] = callback.Count.ToString(CultureInfo.InvariantCulture);
                result["lastOperation"] = callback.LastOperation.ToString(CultureInfo.InvariantCulture);
                result["lastStatus"] = callback.LastStatus.ToString(CultureInfo.InvariantCulture);
                result["lastOperationName"] = TlxCallbackDecoder.OperationName(callback.LastOperation);
                result["lastStatusMeaning"] = TlxCallbackDecoder.StatusMeaning(callback.LastOperation, callback.LastStatus);
            }
            if (trace != null) result["traceDirectory"] = trace.DirectoryPath;
            if (!string.IsNullOrWhiteSpace(failure)) result["failure"] = failure;
            return result;
        }

        private void OnCallback(int operation, int status)
        {
            lock (sync)
            {
                // The installed TLX 1.1 type library uses 3000 for WTP_ProgressComplete.
                if (IsErrorOperation(operation)) state = "Faulted";
                else if (status == 3000 && state != "Closed") state = "Ready";
                var operationName = TlxCallbackDecoder.OperationName(operation);
                var statusMeaning = TlxCallbackDecoder.StatusMeaning(operation, status);
                Log("callback operation=" + operation.ToString(CultureInfo.InvariantCulture) + " (" + operationName + "); status=" + status.ToString(CultureInfo.InvariantCulture) + " (" + statusMeaning + "); state=" + state);
                Trace("callback", new Dictionary<string, string>
                {
                    { "operation", operation.ToString(CultureInfo.InvariantCulture) },
                    { "operationName", operationName }, { "status", status.ToString(CultureInfo.InvariantCulture) },
                    { "statusHex", "0x" + unchecked((uint)status).ToString("X8", CultureInfo.InvariantCulture) }, { "statusMeaning", statusMeaning }
                });
                if (operation == 38 && pendingRawOutputPath != null)
                {
                    if (status == 3000)
                    {
                        ReleaseRawBuffers();
                    }
                    else if (status != 0 && status != 1000)
                    {
                        WriteCompletedRawBuffer();
                        RegisterNextRawBuffer();
                    }
                }
                if (IsErrorOperation(operation))
                {
                    ReleaseRawBuffers();
                    var nativeErrors = DrainLastErrors(operation, status);
                    if (!string.IsNullOrWhiteSpace(nativeErrors)) failure = nativeErrors;
                }
                if (operation == 38 && status == 3000 && trace != null) trace.CaptureOutputEvidence();
            }
        }

        private static bool IsErrorOperation(int operation)
        {
            return operation == 1 || operation == 13 || operation == 15 || operation == 35 || operation == 39 || operation == 41;
        }

        private string DrainLastErrors(int callbackOperation, int callbackStatus)
        {
            if (instance == null) return string.Empty;
            return DrainErrorQueue(GetErrorInterface(callbackOperation), callbackOperation, callbackStatus, false);
        }

        private string DrainInitializationErrors(int callbackStatus)
        {
            if (instance == null) return string.Empty;
            var errors = new List<string>();
            var interfaces = new[]
            {
                (int)INT_IID_000.INT_IID_ITLAMain,
                (int)INT_IID_000.INT_IID_ITLXMain,
                (int)INT_IID_000.INT_IID_IScanPictures,
                (int)INT_IID_000.INT_IID_ISavePictures
            };
            foreach (var interfaceId in interfaces)
            {
                var current = DrainErrorQueue(interfaceId, 1, callbackStatus, false);
                if (!string.IsNullOrWhiteSpace(current)) errors.Add(current);
            }
            return string.Join(" | ", errors.ToArray());
        }

        private string DrainErrorQueue(int interfaceId, int callbackOperation, int callbackStatus, bool startupCleanup)
        {
            var errors = new List<string>();
            for (var attempt = 0; attempt < 16; attempt++)
            {
                try
                {
                    var message = string.Empty;
                    var number = string.Empty;
                    var detail = string.Empty;
                    var result = instance.GetAndClearLastError(interfaceId, ref message, ref number);
                    var values = new Dictionary<string, string>
                    {
                        { "interfaceId", interfaceId.ToString(CultureInfo.InvariantCulture) },
                        { "callbackOperation", callbackOperation.ToString(CultureInfo.InvariantCulture) },
                        { "callbackStatus", callbackStatus.ToString(CultureInfo.InvariantCulture) },
                        { "attempt", attempt.ToString(CultureInfo.InvariantCulture) },
                        { "returnCode", result.ToString(CultureInfo.InvariantCulture) },
                        { "message", message ?? string.Empty }, { "number", number ?? string.Empty }, { "detail", detail ?? string.Empty }
                    };
                    Log((startupCleanup ? "cleared startup error" : "native error") +
                        ": interface=" + interfaceId.ToString(CultureInfo.InvariantCulture) +
                        "; return=" + result.ToString(CultureInfo.InvariantCulture) +
                        "; message=" + values["message"] + "; number=" + values["number"]);
                    Trace("native-error", values);
                    if (!startupCleanup && (result != 0 || !string.IsNullOrWhiteSpace(message) || !string.IsNullOrWhiteSpace(number)))
                    {
                        errors.Add(
                            "interface=" + interfaceId.ToString(CultureInfo.InvariantCulture) +
                            ", return=" + result.ToString(CultureInfo.InvariantCulture) +
                            DescribeErrorCode(result) +
                            (string.IsNullOrWhiteSpace(message) ? string.Empty : ", message=" + message) +
                            (string.IsNullOrWhiteSpace(number) ? string.Empty : ", number=" + number));
                    }
                    if (result != 25) break; // EC_PreviousError signals a queued error behind this one.
                }
                catch (Exception exception)
                {
                    Trace("native-error-drain-failed", new Dictionary<string, string> { { "exception", exception.GetType().FullName + ": " + exception.Message } });
                    var errorValue = GetComErrorValue(exception);
                    Log((startupCleanup ? "startup error cleanup" : "native error queue") +
                        ": interface=" + interfaceId.ToString(CultureInfo.InvariantCulture) +
                        "; COM return=" + errorValue.ToString(CultureInfo.InvariantCulture) +
                        DescribeErrorCode(errorValue));
                    if (!startupCleanup)
                    {
                        errors.Add(
                            "interface=" + interfaceId.ToString(CultureInfo.InvariantCulture) +
                            ", COM return=" + errorValue.ToString(CultureInfo.InvariantCulture) +
                            DescribeErrorCode(errorValue));
                    }
                    break;
                }
            }
            return string.Join(" | ", errors.ToArray());
        }

        private static int GetErrorInterface(int operation)
        {
            if (operation == 35) return (int)INT_IID_000.INT_IID_IScanPictures;
            if (operation == 39) return (int)INT_IID_000.INT_IID_ISavePictures;
            if (operation == 41) return (int)INT_IID_000.INT_IID_ITLXMain;
            return (int)INT_IID_000.INT_IID_ITLAMain;
        }

        private static string DescribeErrorCode(int value)
        {
            if (value == 0) return " (no error)";
            if (value == 10) return " (scanner not initialized)";
            if (value == 15) return " (invalid parameter)";
            if (value == 25) return " (previous error queued)";
            return string.Empty;
        }

        private static int GetComErrorValue(Exception exception)
        {
            var comException = exception as COMException;
            int value;
            if (int.TryParse(exception.Message == null ? string.Empty : exception.Message.Trim().Trim('\'', '"'), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return value;
            return comException == null ? 0 : comException.ErrorCode;
        }

        private IDictionary<string, string> SnapshotMetadata(string label)
        {
            var values = new Dictionary<string, string> { { "label", label }, { "state", state } };
            if (instance == null) return values;
            try
            {
                int scannerType = 0, scannerHardwareVersion = 0, darkPointMinutes = 0, colorPortraitMode = 0, packetTimeout = 0, noFilmTimeout = 0, lampSaverSeconds = 0, scannerSerial = 0;
                string rom = string.Empty, model = string.Empty, tlaVersion = string.Empty, tlxVersion = string.Empty;
                instance.GetScannerInfo000(ref scannerType, ref rom, ref model, ref scannerSerial, ref scannerHardwareVersion, ref tlaVersion, ref darkPointMinutes, ref colorPortraitMode, ref packetTimeout, ref noFilmTimeout, ref lampSaverSeconds, ref tlxVersion);
                values["scannerType"] = scannerType.ToString(CultureInfo.InvariantCulture); values["scannerModel"] = model ?? string.Empty; values["scannerSerial"] = scannerSerial.ToString(CultureInfo.InvariantCulture); values["romVersion"] = rom ?? string.Empty; values["tlxVersion"] = tlxVersion ?? string.Empty;
            }
            catch (Exception exception) { values["scannerInfo.error"] = exception.GetType().Name + ": " + exception.Message; }
            try
            {
                int scanStrips = 0, scanPictures = 0, scanWarnings = 0;
                instance.GetPictureCountScanGroup(0, ref scanStrips, ref scanPictures, ref scanWarnings);
                values["scanGroup.strips"] = scanStrips.ToString(CultureInfo.InvariantCulture); values["scanGroup.pictures"] = scanPictures.ToString(CultureInfo.InvariantCulture); values["scanGroup.warnings"] = scanWarnings.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception exception) { values["scanGroup.error"] = exception.GetType().Name + ": " + exception.Message; }
            try
            {
                int saveRolls = 0, saveStrips = 0, savePictures = 0, saveSelected = 0, saveHidden = 0;
                instance.GetPictureCountSaveGroup(ref saveRolls, ref saveStrips, ref savePictures, ref saveSelected, ref saveHidden);
                values["saveGroup.rolls"] = saveRolls.ToString(CultureInfo.InvariantCulture); values["saveGroup.strips"] = saveStrips.ToString(CultureInfo.InvariantCulture); values["saveGroup.pictures"] = savePictures.ToString(CultureInfo.InvariantCulture); values["saveGroup.selected"] = saveSelected.ToString(CultureInfo.InvariantCulture); values["saveGroup.hidden"] = saveHidden.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception exception) { values["saveGroup.error"] = exception.GetType().Name + ": " + exception.Message; }
            return values;
        }

        private void Trace(string eventName, IDictionary<string, string> values)
        {
            if (trace != null) trace.Write(eventName, state, values);
        }

        private static void Log(string message)
        {
            Console.WriteLine("[{0:O}] TLX {1}", DateTime.UtcNow, message);
        }

        private void CloseCore()
        {
            if (instance != null)
            {
                try { instance.ScanCancel(); } catch { }
                try { instance.SaveCancel(); } catch { }
            }
            if (instance != null && callbackCookie != 0) try { instance.CBUnadvise(callbackCookie); } catch (COMException) { }
            callbackCookie = 0; callback = null;
            if (instance != null && Marshal.IsComObject(instance)) Marshal.FinalReleaseComObject(instance);
            instance = null; state = "Closed";
            ReleaseRawBuffers();
            CleanupOwnedPfsBuffers();
        }

        private void AllocateRawBuffers(int byteCount)
        {
            ReleaseRawBuffers();
            rawBufferSize = byteCount;
            rawBufferPointer1 = Marshal.AllocHGlobal(byteCount);
            rawBufferPointer2 = Marshal.AllocHGlobal(byteCount);
            rawBufferOneIsNext = true;
        }

        private void RegisterNextRawBuffer()
        {
            var pointer = rawBufferOneIsNext ? rawBufferPointer1 : rawBufferPointer2;
            if (pointer == IntPtr.Zero) return;
            // Only the header must be cleared before TLX owns the buffer. Clearing an entire
            // Base16 frame here would briefly duplicate tens of megabytes in the x86 process.
            Marshal.Copy(new byte[16], 0, pointer, 16);
            instance.ClientMemoryBufferAdd(checked(pointer.ToInt32()), rawBufferSize);
            rawBufferOneIsNext = !rawBufferOneIsNext;
        }

        private void WriteCompletedRawBuffer()
        {
            var pointer = rawBufferOneIsNext ? rawBufferPointer1 : rawBufferPointer2;
            if (pointer == IntPtr.Zero || string.IsNullOrWhiteSpace(pendingRawOutputPath)) return;
            var header = new byte[16];
            Marshal.Copy(pointer, header, 0, header.Length);
            var headerSize = BitConverter.ToUInt32(header, 0);
            var width = BitConverter.ToUInt32(header, 4);
            var height = BitConverter.ToUInt32(header, 8);
            var bitCount = BitConverter.ToUInt32(header, 12);
            var byteCount = checked((int)(headerSize + (ulong)width * height * (bitCount / 8u)));
            if (headerSize < 16 || width == 0 || height == 0 || bitCount != 48 || byteCount > rawBufferSize)
                throw new InvalidOperationException("TLX returned an invalid planar 16-bit buffer.");
            var bytes = new byte[byteCount];
            Marshal.Copy(pointer, bytes, 0, byteCount);
            File.WriteAllBytes(pendingRawOutputPath, bytes);
        }

        private void ReleaseRawBuffers()
        {
            if (instance != null)
            {
                try { instance.ClientMemoryBufferDismissAll(); } catch { }
            }
            if (rawBufferPointer1 != IntPtr.Zero) Marshal.FreeHGlobal(rawBufferPointer1);
            if (rawBufferPointer2 != IntPtr.Zero) Marshal.FreeHGlobal(rawBufferPointer2);
            rawBufferPointer1 = IntPtr.Zero;
            rawBufferPointer2 = IntPtr.Zero;
            rawBufferSize = 0;
            pendingRawOutputPath = null;
        }

        private void CleanupOwnedPfsBuffers()
        {
            var before = pfsBuffersAtOpen; pfsBuffersAtOpen = null;
            if (before == null) return;
            foreach (var current in CapturePfsBuffers())
            {
                PfsBufferState original;
                if (before.TryGetValue(current.Key, out original) && original.Length == current.Value.Length && original.LastWriteUtcTicks == current.Value.LastWriteUtcTicks) continue;
                try
                {
                    File.Delete(current.Key);
                    Log("deleted bridge-owned PFS staging buffer: " + current.Key);
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine("Could not delete bridge-owned PFS staging buffer '" + current.Key + "': " + exception.Message);
                }
            }
        }

        private static Dictionary<string, PfsBufferState> CapturePfsBuffers()
        {
            const string buffersDirectory = @"C:\Buffers";
            var result = new Dictionary<string, PfsBufferState>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(buffersDirectory)) return result;
            foreach (var path in Directory.GetFiles(buffersDirectory, "PFS*.bin", SearchOption.TopDirectoryOnly))
            {
                var info = new FileInfo(path); result[path] = new PfsBufferState { Length = info.Length, LastWriteUtcTicks = info.LastWriteTimeUtc.Ticks };
            }
            return result;
        }

        private sealed class PfsBufferState { public long Length; public long LastWriteUtcTicks; }

        private void RequireClosed() { if (instance != null) throw new InvalidOperationException("A TLX session is already open. Close it before initializing again."); }
        private void RequireOpen() { if (instance == null) throw new InvalidOperationException("No TLX session is open."); }
        private void RequireReady() { RequireOpen(); if (state != "Ready") throw new InvalidOperationException("TLX is not ready; query get-tlx-session-status until state is Ready."); }
        private static int GetRequiredInt32(IDictionary<string, string> arguments, string key) { if (arguments == null || !arguments.ContainsKey(key)) throw new ArgumentException("Bridge argument '" + key + "' is required."); return GetInt32(arguments, key, 0); }
        private static int GetInt32(IDictionary<string, string> arguments, string key, int fallback) { string raw; int value; if (arguments == null || !arguments.TryGetValue(key, out raw) || string.IsNullOrWhiteSpace(raw)) return fallback; if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) throw new ArgumentException("Bridge argument '" + key + "' must be a signed 32-bit integer."); return value; }
        private static string GetString(IDictionary<string, string> arguments, string key) { string value; return arguments != null && arguments.TryGetValue(key, out value) ? value : string.Empty; }

        private static void PrepareNativeRuntime(string overrideDirectory)
        {
            var directory = string.IsNullOrWhiteSpace(overrideDirectory) ? ReadRegisteredTlxDirectory() : Path.GetFullPath(overrideDirectory);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) throw new DirectoryNotFoundException("Could not find the Pakon F-X35 COM SERVER directory. Supply comServerDirectory when initializing the TLX session.");
            AddNativeRuntimeDirectoriesToPath(directory);
            if (!SetDllDirectory(directory)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "SetDllDirectory failed for '" + directory + "'.");
            Environment.CurrentDirectory = directory;
        }

        private static void AddNativeRuntimeDirectoriesToPath(string comServerDirectory)
        {
            var pakonDirectory = Directory.GetParent(comServerDirectory);
            var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var directories = new[] { AppContext.BaseDirectory, pakonDirectory == null ? null : pakonDirectory.FullName };

            for (var index = directories.Length - 1; index >= 0; index--)
            {
                var candidate = directories[index];
                if (string.IsNullOrWhiteSpace(candidate) || !Directory.Exists(candidate)) continue;

                var entries = currentPath.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (Array.Exists(entries, entry => string.Equals(entry.Trim(), candidate, StringComparison.OrdinalIgnoreCase))) continue;
                currentPath = candidate + Path.PathSeparator + currentPath;
            }

            Environment.SetEnvironmentVariable("PATH", currentPath);
        }

        private static string ReadRegisteredTlxDirectory()
        {
            const string path = @"CLSID\{EA82986B-E47C-4C0F-97EA-FB50ED216D2E}\InprocServer32";
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32))
            using (var key = baseKey.OpenSubKey(path))
            {
                var value = key == null ? null : key.GetValue(null) as string;
                return string.IsNullOrEmpty(value) ? null : Path.GetDirectoryName(value);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)] private static extern bool SetDllDirectory(string path);
        [ComVisible(true), ClassInterface(ClassInterfaceType.None)] private sealed class TlxProgressCallback : StandardOleMarshalObject, ICallBackClient
        {
            private readonly TlxSession owner; private readonly object callbackSync = new object(); private int count; private int lastOperation; private int lastStatus;
            public TlxProgressCallback(TlxSession owner) { this.owner = owner; }
            public int Count { get { lock (callbackSync) return count; } } public int LastOperation { get { lock (callbackSync) return lastOperation; } } public int LastStatus { get { lock (callbackSync) return lastStatus; } }
            public void Awake(int operation, int status) { lock (callbackSync) { count++; lastOperation = operation; lastStatus = status; } owner.OnCallback(operation, status); }
        }
    }
}
