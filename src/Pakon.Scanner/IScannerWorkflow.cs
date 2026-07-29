namespace Pakon.Scanner;

public interface IScannerWorkflow : IDisposable
{
    string BackendName { get; }

    void EnsureAvailable();

    void Restart();

    Task<ScannerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task BeginCaptureAsync(CaptureRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CapturedFrame>> CompleteCaptureAsync(CancellationToken cancellationToken = default);

    Task<RenderedFrame> RenderFrameAsync(FrameRenderRequest request, CancellationToken cancellationToken = default);

    Task CancelCaptureAsync(CancellationToken cancellationToken = default);

    Task CloseAsync(CancellationToken cancellationToken = default);
}
