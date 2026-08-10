using System.IO.Pipes;
using System.Globalization;
using System.Runtime.Serialization.Json;
using System.Text;
using Pakon.LegacyBridge.Protocol;

namespace Pakon.LegacyBridge.Client;

/// <summary>COM-free .NET client for the isolated x86 legacy bridge process.</summary>
public sealed class LegacyBridgeClient
{
    private readonly string pipeName;

    public LegacyBridgeClient(string pipeName = "PakonLegacyBridge")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeName);
        this.pipeName = pipeName;
    }

    public Task<BridgeResponse> ProbeTlcAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.ProbeTlc, cancellationToken);

    /// <summary>
    /// Opens a persistent direct-TLC scanner session and begins TLC initialization. This can access
    /// hardware; it is intentionally separate from the read-only TLC activation probe. The default
    /// requests percent-based progress callbacks. TLC's legacy C#-client flag is a proven no-op.
    /// </summary>
    public Task<BridgeResponse> InitializeTlcAsync(int initializationFlags = 0x00000001, int memoryTimeoutMilliseconds = 200000, int sharedMemoryBytes = 0, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.InitializeTlc, new Dictionary<string, string>
        {
            ["initializationFlags"] = initializationFlags.ToString(CultureInfo.InvariantCulture),
            ["memoryTimeoutMilliseconds"] = memoryTimeoutMilliseconds.ToString(CultureInfo.InvariantCulture),
            ["sharedMemoryBytes"] = sharedMemoryBytes.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> GetTlcSessionStatusAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.GetTlcSessionStatus, cancellationToken);

    public Task<BridgeResponse> CloseTlcSessionAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.CloseTlcSession, cancellationToken);

    /// <summary>Opens the proven TLX facade inside the isolated x86 process. This accesses hardware.</summary>
    public Task<BridgeResponse> InitializeTlxSessionAsync(string? comServerDirectory = null, int initializationFlags = 0x40000001, int memoryTimeoutMilliseconds = 200000, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.InitializeTlxSession, new Dictionary<string, string>
        {
            ["initializationFlags"] = initializationFlags.ToString(CultureInfo.InvariantCulture),
            ["memoryTimeoutMilliseconds"] = memoryTimeoutMilliseconds.ToString(CultureInfo.InvariantCulture),
            ["comServerDirectory"] = comServerDirectory ?? string.Empty
        }, cancellationToken);

    public Task<BridgeResponse> GetTlxSessionStatusAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.GetTlxSessionStatus, cancellationToken);

    /// <summary>
    /// Starts a TLX scan. Values are the installed TLX type-library integer values; keeping them
    /// here avoids leaking COM enum types through the .NET 10 boundary.
    /// </summary>
    public Task<BridgeResponse> ScanRollAsync(int resolution, int filmColor, int filmFormat, int stripMode, int scanControl, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.ScanRoll, new Dictionary<string, string>
        {
            ["resolution"] = resolution.ToString(CultureInfo.InvariantCulture),
            ["filmColor"] = filmColor.ToString(CultureInfo.InvariantCulture),
            ["filmFormat"] = filmFormat.ToString(CultureInfo.InvariantCulture),
            ["stripMode"] = stripMode.ToString(CultureInfo.InvariantCulture),
            ["scanControl"] = scanControl.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> MoveOldestRollToSaveGroupAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.MoveOldestRollToSaveGroup, cancellationToken);

    public Task<BridgeResponse> SaveFramesToDiskAsync(int index, int saveControl, int width, int height, int scalingMethod, int fileFormat, int compression, int dpi, int colorBits, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.SaveFramesToDisk, new Dictionary<string, string>
        {
            ["index"] = index.ToString(CultureInfo.InvariantCulture),
            ["saveControl"] = saveControl.ToString(CultureInfo.InvariantCulture),
            ["width"] = width.ToString(CultureInfo.InvariantCulture),
            ["height"] = height.ToString(CultureInfo.InvariantCulture),
            ["scalingMethod"] = scalingMethod.ToString(CultureInfo.InvariantCulture),
            ["fileFormat"] = fileFormat.ToString(CultureInfo.InvariantCulture),
            ["compression"] = compression.ToString(CultureInfo.InvariantCulture),
            ["dpi"] = dpi.ToString(CultureInfo.InvariantCulture),
            ["colorBits"] = colorBits.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> GetFramesAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.GetFrames, cancellationToken);

    public Task<BridgeResponse> UpdateFrameAsync(int index, string fileName, string directory, int rotation, bool selected, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.UpdateFrame, new Dictionary<string, string>
        {
            ["index"] = index.ToString(CultureInfo.InvariantCulture),
            ["fileName"] = fileName,
            ["directory"] = directory,
            ["rotation"] = rotation.ToString(CultureInfo.InvariantCulture),
            ["selected"] = selected.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> ConfigureFrameLayoutAsync(int layout, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.ConfigureFrameLayout, new Dictionary<string, string>
        {
            ["layout"] = layout.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> UpdateFrameFramingAsync(int index, int left, int top, int right, int bottom, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.UpdateFrameFraming, new Dictionary<string, string>
        {
            ["index"] = index.ToString(CultureInfo.InvariantCulture),
            ["left"] = left.ToString(CultureInfo.InvariantCulture),
            ["top"] = top.ToString(CultureInfo.InvariantCulture),
            ["right"] = right.ToString(CultureInfo.InvariantCulture),
            ["bottom"] = bottom.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> InsertFrameAsync(int insertBeforeIndex, int stripIndex, int left, int top, int right, int bottom, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.InsertFrame, new Dictionary<string, string>
        {
            ["insertBeforeIndex"] = insertBeforeIndex.ToString(CultureInfo.InvariantCulture),
            ["stripIndex"] = stripIndex.ToString(CultureInfo.InvariantCulture),
            ["left"] = left.ToString(CultureInfo.InvariantCulture),
            ["top"] = top.ToString(CultureInfo.InvariantCulture),
            ["right"] = right.ToString(CultureInfo.InvariantCulture),
            ["bottom"] = bottom.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> DeleteFrameAsync(int index, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.DeleteFrame, new Dictionary<string, string>
        {
            ["index"] = index.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> RenderFrameToDiskAsync(int index, string outputPath, int saveControl, int width, int height, int quality = 95, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.RenderFrameToDisk, new Dictionary<string, string>
        {
            ["index"] = index.ToString(CultureInfo.InvariantCulture),
            ["outputPath"] = outputPath,
            ["saveControl"] = saveControl.ToString(CultureInfo.InvariantCulture),
            ["width"] = width.ToString(CultureInfo.InvariantCulture),
            ["height"] = height.ToString(CultureInfo.InvariantCulture),
            ["quality"] = quality.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> RenderFrameToRawAsync(int index, string outputPath, int saveControl, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.RenderFrameToRaw, new Dictionary<string, string>
        {
            ["index"] = index.ToString(CultureInfo.InvariantCulture),
            ["outputPath"] = outputPath,
            ["saveControl"] = saveControl.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    public Task<BridgeResponse> CancelScanAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.CancelScan, cancellationToken);

    public Task<BridgeResponse> CancelSaveAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.CancelSave, cancellationToken);

    public Task<BridgeResponse> CloseTlxSessionAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.CloseTlxSession, cancellationToken);

    /// <summary>Starts an opt-in bridge-side JSONL trace before a TLX workflow begins.</summary>
    public Task<BridgeResponse> BeginTlxTraceAsync(string directory, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.BeginTlxTrace, new Dictionary<string, string> { ["directory"] = directory }, cancellationToken);

    public Task<BridgeResponse> EndTlxTraceAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.EndTlxTrace, cancellationToken);

    public Task<BridgeResponse> SnapshotTlxTraceAsync(string label, CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.SnapshotTlxTrace, new Dictionary<string, string> { ["label"] = label }, cancellationToken);

    /// <summary>Starts the documented baseline trace capture: Base16, negative, 35 mm, full roll, normal framing.</summary>
    public Task<BridgeResponse> ScanTlxTraceProfileAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.ScanTlxTraceProfile, cancellationToken);

    public Task<BridgeResponse> SaveTlxTraceJpegsAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeOperations.SaveTlxTraceJpegs, cancellationToken);

    private async Task<BridgeResponse> SendAsync(string operation, CancellationToken cancellationToken)
    {
        return await SendAsync(operation, new Dictionary<string, string>(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<BridgeResponse> SendAsync(string operation, Dictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        var request = new BridgeRequest { RequestId = Guid.NewGuid().ToString("N"), Operation = operation, Arguments = arguments };
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(cancellationToken).ConfigureAwait(false);
        await PipeJson.WriteAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        var response = await PipeJson.ReadAsync<BridgeResponse>(pipe, cancellationToken).ConfigureAwait(false);
        if (response is null || response.RequestId != request.RequestId)
        {
            throw new InvalidDataException("The legacy bridge returned an invalid response.");
        }

        return response;
    }

    private static class PipeJson
    {
        public static async Task WriteAsync<T>(Stream stream, T value, CancellationToken cancellationToken)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using var memory = new MemoryStream();
            serializer.WriteObject(memory, value);
            var payload = Encoding.UTF8.GetString(memory.ToArray()) + "\n";
            var bytes = Encoding.UTF8.GetBytes(payload);
            await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task<T?> ReadAsync<T>(Stream stream, CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line)) return default;
            var serializer = new DataContractJsonSerializer(typeof(T));
            using var memory = new MemoryStream(Encoding.UTF8.GetBytes(line));
            return (T?)serializer.ReadObject(memory);
        }
    }
}
