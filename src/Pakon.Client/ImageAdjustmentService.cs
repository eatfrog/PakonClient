using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Windows.Media.Imaging;

namespace Pakon.Client;

internal static class ImageAdjustmentService
{
    public static async Task<BitmapImage> CreatePreviewAsync(
        FrameItem frame,
        bool invertImage,
        bool blackAndWhite,
        string outputPath)
    {
        await Task.Run(() =>
        {
            using var image = Image.Load<Rgb24>(frame.SourcePath);
            Apply(image, frame, invertImage, blackAndWhite);
            image.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(900, 620) }));
            image.Save(outputPath, new JpegEncoder { Quality = 88 });
        });
        return LoadUnlocked(outputPath);
    }

    public static async Task SaveJpegAsync(
        string sourcePath,
        string outputPath,
        FrameItem frame,
        bool invertImage,
        bool blackAndWhite)
    {
        await Task.Run(() =>
        {
            using var image = Image.Load<Rgb24>(sourcePath);
            Apply(image, frame, invertImage, blackAndWhite);
            image.Save(outputPath, new JpegEncoder { Quality = 95 });
        });
    }

    public static BitmapImage LoadUnlocked(string path)
    {
        using var stream = File.OpenRead(path);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static void Apply(Image<Rgb24> image, FrameItem frame, bool invertImage, bool blackAndWhite)
    {
        if (invertImage)
        {
            image.Mutate(x => x.Invert());
        }

        ApplyAutoLevels(image);

        if (blackAndWhite)
        {
            image.Mutate(x => x.Grayscale());
        }

        ApplyToneAdjustments(image, frame.Exposure, frame.Contrast);

        if (!blackAndWhite)
        {
            var red = 1 + frame.RedBalance / 100d;
            var green = 1 + frame.GreenBalance / 100d;
            var blue = 1 + frame.BlueBalance / 100d;
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                    {
                        ref var pixel = ref row[x];
                        pixel.R = (byte)Math.Clamp(pixel.R * red, 0, 255);
                        pixel.G = (byte)Math.Clamp(pixel.G * green, 0, 255);
                        pixel.B = (byte)Math.Clamp(pixel.B * blue, 0, 255);
                    }
                }
            });
        }
        if (frame.Rotation != 0) image.Mutate(x => x.Rotate(frame.Rotation));
    }

    private static void ApplyToneAdjustments(Image<Rgb24> image, double exposureStops, double contrastAdjustment)
    {
        var exposureFactor = Math.Pow(2, Math.Clamp(exposureStops, -4, 4));
        var contrastExponent = Math.Pow(2, -Math.Clamp(contrastAdjustment, -100, 100) / 100d);

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    ref var pixel = ref row[x];
                    pixel.R = AdjustChannel(pixel.R, exposureFactor, contrastExponent);
                    pixel.G = AdjustChannel(pixel.G, exposureFactor, contrastExponent);
                    pixel.B = AdjustChannel(pixel.B, exposureFactor, contrastExponent);
                }
            }
        });
    }

    private static byte AdjustChannel(byte value, double exposureFactor, double contrastExponent)
    {
        var encoded = value / 255d;
        var linear = encoded <= 0.04045
            ? encoded / 12.92
            : Math.Pow((encoded + 0.055) / 1.055, 2.4);
        linear = Math.Clamp(linear * exposureFactor, 0, 1);
        encoded = linear <= 0.0031308
            ? linear * 12.92
            : 1.055 * Math.Pow(linear, 1 / 2.4) - 0.055;

        var distance = Math.Abs(encoded - 0.5) * 2;
        var curved = 0.5 + Math.Sign(encoded - 0.5) * 0.5 * Math.Pow(distance, contrastExponent);
        return (byte)Math.Clamp(Math.Round(curved * 255), 0, 255);
    }

    private static void ApplyAutoLevels(Image<Rgb24> image)
    {
        var redHistogram = new int[256];
        var greenHistogram = new int[256];
        var blueHistogram = new int[256];

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                foreach (var pixel in row)
                {
                    redHistogram[pixel.R]++;
                    greenHistogram[pixel.G]++;
                    blueHistogram[pixel.B]++;
                }
            }
        });

        var pixelCount = image.Width * image.Height;
        var redRange = FindLevelRange(redHistogram, pixelCount);
        var greenRange = FindLevelRange(greenHistogram, pixelCount);
        var blueRange = FindLevelRange(blueHistogram, pixelCount);

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    ref var pixel = ref row[x];
                    pixel.R = MapLevel(pixel.R, redRange.Low, redRange.High);
                    pixel.G = MapLevel(pixel.G, greenRange.Low, greenRange.High);
                    pixel.B = MapLevel(pixel.B, blueRange.Low, blueRange.High);
                }
            }
        });
    }

    private static (int Low, int High) FindLevelRange(int[] histogram, int pixelCount)
    {
        var clippedPixels = Math.Max(1, (int)(pixelCount * 0.005));
        var cumulative = 0;
        var low = 0;
        for (; low < 255; low++)
        {
            cumulative += histogram[low];
            if (cumulative >= clippedPixels) break;
        }

        cumulative = 0;
        var high = 255;
        for (; high > 0; high--)
        {
            cumulative += histogram[high];
            if (cumulative >= clippedPixels) break;
        }

        return high > low ? (low, high) : (0, 255);
    }

    private static byte MapLevel(byte value, int low, int high)
    {
        if (value <= low) return 0;
        if (value >= high) return 255;
        return (byte)Math.Round((value - low) * 255d / (high - low));
    }
}
