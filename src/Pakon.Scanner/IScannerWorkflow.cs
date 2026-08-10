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

    Task<IReadOnlyList<CapturedFrame>> GetFramesAsync(CancellationToken cancellationToken = default);

    Task<RenderedFrame> RenderFrameAsync(FrameRenderRequest request, CancellationToken cancellationToken = default);

    Task UpdateFrameFramingAsync(int frameIndex, FrameBounds bounds, CancellationToken cancellationToken = default);

    Task InsertFrameAsync(int insertBeforeIndex, int stripIndex, FrameBounds bounds, CancellationToken cancellationToken = default);

    Task DeleteFrameAsync(int frameIndex, CancellationToken cancellationToken = default);

    Task CancelCaptureAsync(CancellationToken cancellationToken = default);

    Task CloseAsync(CancellationToken cancellationToken = default);
}
