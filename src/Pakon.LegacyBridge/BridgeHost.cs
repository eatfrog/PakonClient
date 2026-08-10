using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Json;
using System.Text;
using Pakon.LegacyBridge.Protocol;

namespace Pakon.LegacyBridge
{
    internal sealed class BridgeHost
    {
        private readonly string pipeName;
        private readonly bool once;
        private readonly TlcSession tlcSession = new TlcSession();
        private readonly TlxSession tlxSession = new TlxSession();
        private readonly StaComWorker tlxWorker = new StaComWorker();

        public BridgeHost(string pipeName, bool once)
        {
            this.pipeName = pipeName;
            this.once = once;
        }

        public int Run()
        {
            try
            {
                Console.WriteLine("Pakon legacy bridge (x86) listening on pipe '{0}'.", pipeName);
                do
                {
                    using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None))
                    {
                        pipe.WaitForConnection();
                        var request = PipeJson.Read<BridgeRequest>(pipe);
                        var response = Dispatch(request);
                        if (!response.Succeeded)
                        {
                            Console.Error.WriteLine(
                                "[{0:O}] bridge operation {1} failed: {2}",
                                DateTime.UtcNow,
                                request == null ? "<missing>" : request.Operation,
                                response.Error ?? "Unknown error.");
                        }
                        PipeJson.Write(pipe, response);
                    }
                }
                while (!once);
            }
            catch (IOException exception)
            {
                Console.Error.WriteLine("Could not open bridge pipe '{0}': {1}", pipeName, exception.Message);
                Console.Error.WriteLine("Another Pakon legacy bridge may already be running.");
                return 3;
            }

            return 0;
        }

        private BridgeResponse Dispatch(BridgeRequest request)
        {
            if (request == null)
            {
                return new BridgeResponse { Succeeded = false, Error = "Missing request." };
            }

            try
            {
                if (request.Operation == BridgeOperations.ProbeTlc)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(TlcActivationProbe.ActivateAndRelease()) };
                }

                if (request.Operation == BridgeOperations.InitializeTlc)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlcSession.Initialize(request.Arguments)) };
                }

                if (request.Operation == BridgeOperations.GetTlcSessionStatus)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlcSession.GetStatus()) };
                }

                if (request.Operation == BridgeOperations.CloseTlcSession)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlcSession.Close()) };
                }

                if (request.Operation == BridgeOperations.InitializeTlxSession)
                {
                    var values = tlxSession.QueueInitialize(request.Arguments);
                    tlxWorker.Enqueue(() => { try { tlxSession.Initialize(request.Arguments); } catch (Exception exception) { tlxSession.ReportAsynchronousFailure(exception); } });
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(values) };
                }

                if (request.Operation == BridgeOperations.GetTlxSessionStatus)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxSession.GetStatus()) };
                }

                if (request.Operation == BridgeOperations.ScanRoll)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.ScanRoll(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.MoveOldestRollToSaveGroup)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.MoveOldestRollToSaveGroup())) };
                }

                if (request.Operation == BridgeOperations.SaveFramesToDisk)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.SaveFramesToDisk(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.GetFrames)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.GetFrames())) };
                }

                if (request.Operation == BridgeOperations.UpdateFrame)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.UpdateFrame(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.ConfigureFrameLayout)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.ConfigureFrameLayout(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.UpdateFrameFraming)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.UpdateFrameFraming(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.InsertFrame)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.InsertFrame(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.DeleteFrame)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.DeleteFrame(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.RenderFrameToDisk)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.RenderFrameToDisk(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.RenderFrameToRaw)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.RenderFrameToRaw(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.CancelScan)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.CancelScan())) };
                }

                if (request.Operation == BridgeOperations.CancelSave)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.CancelSave())) };
                }

                if (request.Operation == BridgeOperations.CloseTlxSession)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.Close())) };
                }

                if (request.Operation == BridgeOperations.BeginTlxTrace)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxSession.BeginTrace(request.Arguments)) };
                }

                if (request.Operation == BridgeOperations.EndTlxTrace)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxSession.EndTrace()) };
                }

                if (request.Operation == BridgeOperations.SnapshotTlxTrace)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.SnapshotTrace(request.Arguments))) };
                }

                if (request.Operation == BridgeOperations.ScanTlxTraceProfile)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.ScanTraceProfile())) };
                }

                if (request.Operation == BridgeOperations.SaveTlxTraceJpegs)
                {
                    return new BridgeResponse { RequestId = request.RequestId, Succeeded = true, Values = new System.Collections.Generic.Dictionary<string, string>(tlxWorker.Invoke(() => tlxSession.SaveTraceJpegs())) };
                }

                return new BridgeResponse { RequestId = request.RequestId, Succeeded = false, Error = "Unsupported bridge operation: " + request.Operation };
            }
            catch (Exception exception)
            {
                return new BridgeResponse { RequestId = request.RequestId, Succeeded = false, Error = exception.GetType().FullName + ": " + exception.Message };
            }
        }

        private static class PipeJson
        {
            public static void Write<T>(Stream stream, T value)
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var memory = new MemoryStream())
                {
                    serializer.WriteObject(memory, value);
                    var bytes = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(memory.ToArray()) + "\n");
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            }

            public static T Read<T>(Stream stream) where T : class
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true))
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) return null;
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    using (var memory = new MemoryStream(Encoding.UTF8.GetBytes(line)))
                    {
                        return serializer.ReadObject(memory) as T;
                    }
                }
            }
        }
    }
}
