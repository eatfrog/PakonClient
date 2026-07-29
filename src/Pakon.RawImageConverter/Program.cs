using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;

var tempRawPath = Path.Combine(Path.GetTempPath(), "pakon-" + Guid.NewGuid().ToString("N") + ".raw");
var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

try
{
    var options = CliOptions.Parse(args);
    var readStopwatch = System.Diagnostics.Stopwatch.StartNew();
    if (!string.IsNullOrWhiteSpace(options.InputPath))
    {
        File.Copy(options.InputPath, tempRawPath, overwrite: true);
    }
    else
    {
        await using var temp = File.Create(tempRawPath);
        await Console.OpenStandardInput().CopyToAsync(temp);
    }
    readStopwatch.Stop();

    var processStopwatch = System.Diagnostics.Stopwatch.StartNew();
    var processor = new PakonRawProcessor();
    using var image = processor.ProcessImage(
        tempRawPath, options.IsBwImage, options.InvertImage, NormalizeGamma(options.Gamma), options.Contrast,
        options.Saturation, options.Brightness, options.RedBalance, options.GreenBalance,
        options.BlueBalance, options.Rotation, options.Exposure, options.ContrastAdjustment);
    processStopwatch.Stop();

    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))!);
    var saveStopwatch = System.Diagnostics.Stopwatch.StartNew();
    SaveImage(image, options);
    saveStopwatch.Stop();
    totalStopwatch.Stop();
    Console.WriteLine(options.OutputPath);
    Console.WriteLine(
        "timing read-temp={0}, process={1}, process-detail=[{2}], save={3}, total={4}",
        FormatDuration(readStopwatch.Elapsed),
        FormatDuration(processStopwatch.Elapsed),
        processor.LastTiming,
        FormatDuration(saveStopwatch.Elapsed),
        FormatDuration(totalStopwatch.Elapsed));
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
}
finally
{
    if (File.Exists(tempRawPath))
    {
        File.Delete(tempRawPath);
    }
}

static string FormatDuration(TimeSpan elapsed)
{
    return elapsed.TotalSeconds >= 1
        ? elapsed.TotalSeconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s"
        : elapsed.TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " ms";
}

static double NormalizeGamma(double value)
{
    const double defaultGamma = 0.4545454545454545;
    if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
    {
        return defaultGamma;
    }

    if (value > 1)
    {
        value = 1 / value;
    }

    return Math.Clamp(value, 0.4, 1.0);
}

static void SaveImage(SixLabors.ImageSharp.Image<Rgb48> image, CliOptions options)
{
    if (options.IsBwImage && options.Format == OutputFormat.Png)
    {
        using var output = File.Create(options.OutputPath);
        image.Save(output, new PngEncoder { ColorType = PngColorType.Grayscale, BitDepth = PngBitDepth.Bit16, CompressionLevel = PngCompressionLevel.BestSpeed });
        return;
    }

    using var stream = File.Create(options.OutputPath);
    switch (options.Format)
    {
        case OutputFormat.Jpeg:
            image.Save(stream, new JpegEncoder { Quality = Math.Clamp(options.Quality, 1, 100) });
            break;
        case OutputFormat.Tiff:
            image.Save(stream, new TiffEncoder { BitsPerPixel = TiffBitsPerPixel.Bit16 });
            break;
        case OutputFormat.Bmp:
            image.Save(stream, new BmpEncoder());
            break;
        default:
            image.Save(stream, new PngEncoder { BitDepth = PngBitDepth.Bit16, CompressionLevel = PngCompressionLevel.BestSpeed });
            break;
    }
}

internal enum OutputFormat
{
    Png,
    Jpeg,
    Tiff,
    Bmp
}

internal sealed class CliOptions
{
    public string OutputPath { get; private init; } = "";

    public string? InputPath { get; private init; }

    public OutputFormat Format { get; private init; } = OutputFormat.Png;

    public bool IsBwImage { get; private init; }

    public bool InvertImage { get; private init; }

    public double Gamma { get; private init; } = 0.4545454545454545;

    public float Contrast { get; private init; } = 1.08f;

    public float Saturation { get; private init; } = 1.08f;

    public int Quality { get; private init; } = 90;

    public float Brightness { get; private init; } = 1f;

    public float RedBalance { get; private init; } = 1f;

    public float GreenBalance { get; private init; } = 1f;

    public float BlueBalance { get; private init; } = 1f;

    public int Rotation { get; private init; }

    public float Exposure { get; private init; }

    public float ContrastAdjustment { get; private init; }

    public static CliOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException("Unexpected argument '" + arg + "'.");
            }

            var key = arg.Substring(2);
            if (string.Equals(key, "bw", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "invert", StringComparison.OrdinalIgnoreCase))
            {
                flags.Add(key);
                continue;
            }

            if (index + 1 >= args.Length)
            {
                throw new ArgumentException("Missing value for --" + key + ".");
            }

            values[key] = args[++index];
        }

        if (!values.TryGetValue("output", out var output) || string.IsNullOrWhiteSpace(output))
        {
            throw new ArgumentException("--output is required.");
        }

        return new CliOptions
        {
            OutputPath = output,
            InputPath = values.TryGetValue("input", out var input) ? Path.GetFullPath(input) : null,
            Format = ParseFormat(Get(values, "format", "png")),
            IsBwImage = flags.Contains("bw"),
            InvertImage = flags.Contains("invert") || flags.Contains("bw"),
            Gamma = ParseDouble(Get(values, "gamma", "0.4545454545454545"), "--gamma"),
            Contrast = ParseFloat(Get(values, "contrast", "1.08"), "--contrast"),
            Saturation = ParseFloat(Get(values, "saturation", "1.08"), "--saturation"),
            Quality = ParseInt(Get(values, "quality", "90"), "--quality"),
            Brightness = ParseFloat(Get(values, "brightness", "1"), "--brightness"),
            RedBalance = ParseFloat(Get(values, "red-balance", "1"), "--red-balance"),
            GreenBalance = ParseFloat(Get(values, "green-balance", "1"), "--green-balance"),
            BlueBalance = ParseFloat(Get(values, "blue-balance", "1"), "--blue-balance"),
            Rotation = ParseInt(Get(values, "rotation", "0"), "--rotation"),
            Exposure = ParseFloat(Get(values, "exposure", "0"), "--exposure"),
            ContrastAdjustment = ParseFloat(Get(values, "contrast-adjustment", "0"), "--contrast-adjustment")
        };
    }

    private static string Get(Dictionary<string, string> values, string key, string fallback)
    {
        return values.TryGetValue(key, out var value) ? value : fallback;
    }

    private static OutputFormat ParseFormat(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => OutputFormat.Jpeg,
            "tif" or "tiff" => OutputFormat.Tiff,
            "bmp" or "bitmap" => OutputFormat.Bmp,
            "png" or "png16" => OutputFormat.Png,
            _ => throw new ArgumentException("Unknown format '" + value + "'.")
        };
    }

    private static double ParseDouble(string value, string option)
    {
        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new ArgumentException(option + " expects a number.");
    }

    private static float ParseFloat(string value, string option)
    {
        return float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new ArgumentException(option + " expects a number.");
    }

    private static int ParseInt(string value, string option)
    {
        return int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new ArgumentException(option + " expects an integer.");
    }
}
