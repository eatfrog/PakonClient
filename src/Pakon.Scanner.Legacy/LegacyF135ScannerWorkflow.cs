using Pakon.LegacyBridge.Client;
using Pakon.LegacyBridge.Protocol;
using System.Globalization;

namespace Pakon.Scanner.Legacy;

public sealed class LegacyF135ScannerWorkflow : IScannerWorkflow
{
    private readonly LegacyBridgeClient bridge = new();
    private readonly BridgeProcessHost bridgeHost = new();

    public string BackendName => "Legacy TLX";

    public void EnsureAvailable()
    {
        PakonNativeRuntimePreflight.Validate();
        bridgeHost.EnsureStarted();
    }

    public void Restart()
    {
        PakonNativeRuntimePreflight.Validate();
        bridgeHost.RestartOwnedOrStart();
    }

    public async Task<ScannerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var response = await RequireSuccess(bridge.GetTlxSessionStatusAsync(cancellationToken));
        var nativeState = response.Values.GetValueOrDefault("state", "Unknown");
        return new ScannerStatus(
            MapState(nativeState),
            nativeState,
            ParseNullableInt(response.Values, "lastStatus"),
            response.Values.GetValueOrDefault("lastStatusMeaning", string.Empty),
            response.Values.GetValueOrDefault("failure"));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        PakonNativeRuntimePreflight.Validate();
        await RequireSuccess(bridge.InitializeTlxSessionAsync(cancellationToken: cancellationToken));
    }

    public async Task BeginCaptureAsync(CaptureRequest request, CancellationToken cancellationToken = default)
    {
        var filmColor = request.FilmKind switch
        {
            FilmKind.ColorNegative => 1,
            FilmKind.ColorPositive => 2,
            FilmKind.BlackAndWhite when request.UseDigitalIce => 8,
            FilmKind.BlackAndWhite => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(request))
        };
        var scanControl = request.UseDigitalIce ? 0x08 : 0;

        // The legacy adapter is the only place where TLX enum values are allowed.
        await RequireSuccess(bridge.ScanRollAsync(
            resolution: 2,
            filmColor,
            filmFormat: 1,
            stripMode: 0,
            scanControl,
            cancellationToken));
    }

    public async Task<IReadOnlyList<CapturedFrame>> CompleteCaptureAsync(CancellationToken cancellationToken = default)
    {
        await RequireSuccess(bridge.MoveOldestRollToSaveGroupAsync(cancellationToken));
        var response = await RequireSuccess(bridge.GetFramesAsync(cancellationToken));
        var count = ParseInt(response.Values, "count", 0);
        var frames = new List<CapturedFrame>(count);
        for (var index = 0; index < count; index++)
        {
            var key = $"frame.{index}.";
            frames.Add(new CapturedFrame(
                index,
                ParseInt(response.Values, key + "frameNumber", index + 1),
                response.Values.GetValueOrDefault(key + "frameName", string.Empty),
                ParseInt(response.Values, key + "filmProduct", -1),
                ParseInt(response.Values, key + "filmSpecifier", -1),
                ParseInt(response.Values, key + "rotation", 0),
                ParseInt(response.Values, key + "selection", 1) != 0));
        }
        return frames;
    }

    public async Task<RenderedFrame> RenderFrameAsync(
        FrameRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        var saveControl = BuildSaveControl(request);
        if (request.Format == FrameRenderFormat.Planar16)
        {
            // Planar client-memory output must not request the stored rotation; the
            // managed output path applies rotation after its own image adjustments.
            saveControl &= ~0x04;
            var response = await RequireSuccess(bridge.RenderFrameToRawAsync(
                request.FrameIndex,
                request.OutputPath,
                saveControl,
                cancellationToken));
            return new RenderedFrame(
                response.Values.GetValueOrDefault("outputPath", request.OutputPath),
                ParseNullableInt(response.Values, "width"),
                ParseNullableInt(response.Values, "height"));
        }

        var diskResponse = await RequireSuccess(bridge.RenderFrameToDiskAsync(
            request.FrameIndex,
            request.OutputPath,
            saveControl,
            request.Width,
            request.Height,
            request.JpegQuality,
            cancellationToken));
        return new RenderedFrame(diskResponse.Values.GetValueOrDefault("outputPath", request.OutputPath));
    }

    public async Task CancelCaptureAsync(CancellationToken cancellationToken = default) =>
        await RequireSuccess(bridge.CancelScanAsync(cancellationToken));

    public async Task CloseAsync(CancellationToken cancellationToken = default) =>
        await RequireSuccess(bridge.CloseTlxSessionAsync(cancellationToken));

    public void Dispose() => bridgeHost.Dispose();

    private static int BuildSaveControl(FrameRenderRequest request)
    {
        var value = request.FilmKind == FilmKind.ColorNegative ? 0x74 : 0x04;
        if (!request.ApplyStoredRotation) value &= ~0x04;
        if (request.UseDigitalIce) value |= 0x80;
        if (request.LowResolution) value |= 0x08;
        return value;
    }

    private static ScannerState MapState(string nativeState) => nativeState switch
    {
        "Closed" => ScannerState.Closed,
        "Initializing" => ScannerState.Initializing,
        "Ready" => ScannerState.Ready,
        "Scanning" => ScannerState.Capturing,
        "CancellingScan" => ScannerState.CancellingCapture,
        "Saving" => ScannerState.Rendering,
        "CancellingSave" => ScannerState.CancellingRender,
        "Faulted" => ScannerState.Faulted,
        _ => ScannerState.Unknown
    };

    private static int ParseInt(
        IReadOnlyDictionary<string, string> values,
        string key,
        int fallback) =>
        values.TryGetValue(key, out var value) &&
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;

    private static int? ParseNullableInt(
        IReadOnlyDictionary<string, string> values,
        string key) =>
        values.TryGetValue(key, out var value) &&
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static async Task<BridgeResponse> RequireSuccess(Task<BridgeResponse> request)
    {
        var response = await request;
        if (!response.Succeeded)
            throw new InvalidOperationException(response.Error ?? "The legacy scanner backend rejected the request.");
        return response;
    }
}
