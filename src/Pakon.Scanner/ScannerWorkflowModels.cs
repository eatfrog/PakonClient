namespace Pakon.Scanner;

public enum FilmKind
{
    ColorNegative,
    ColorPositive,
    BlackAndWhite
}

public enum ScannerState
{
    Unknown,
    Closed,
    Initializing,
    Ready,
    Capturing,
    CancellingCapture,
    Rendering,
    CancellingRender,
    Faulted
}

public enum FrameRenderFormat
{
    Jpeg,
    Planar16
}

public sealed record CaptureRequest(FilmKind FilmKind, bool UseDigitalIce);

public sealed record CapturedFrame(
    int Index,
    int FrameNumber,
    string FrameName,
    int FilmProduct,
    int FilmSpecifier,
    int Rotation,
    bool IsSelected);

public sealed record FrameRenderRequest(
    int FrameIndex,
    string OutputPath,
    FrameRenderFormat Format,
    FilmKind FilmKind,
    bool UseDigitalIce,
    bool LowResolution = false,
    bool ApplyStoredRotation = true,
    int Width = 0,
    int Height = 0,
    int JpegQuality = 95);

public sealed record RenderedFrame(string OutputPath, int? Width = null, int? Height = null);

public sealed record ScannerStatus(
    ScannerState State,
    string BackendState,
    int? Progress,
    string ProgressMeaning,
    string? Failure);
