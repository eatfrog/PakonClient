namespace Pakon.Scanner;

public enum FilmKind
{
    ColorNegative,
    ColorPositive,
    BlackAndWhite
}

public enum FrameLayout
{
    Standard,
    HalfFrame,
    Panorama
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

public sealed record FrameBounds(int Left, int Top, int Right, int Bottom)
{
    public int Width => Right - Left;
    public int Height => Bottom - Top;
}

public sealed record FrameFraming(
    FrameBounds Current,
    FrameBounds Detected,
    int StripWidth,
    int StripHeight);

public sealed record CaptureRequest(FilmKind FilmKind, bool UseDigitalIce, FrameLayout FrameLayout = FrameLayout.Standard);

public sealed record CapturedFrame(
    int Index,
    int StripIndex,
    int FrameNumber,
    string FrameName,
    int FilmProduct,
    int FilmSpecifier,
    int Rotation,
    bool IsSelected,
    FrameFraming Framing);

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
