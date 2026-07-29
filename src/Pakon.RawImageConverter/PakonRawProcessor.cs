using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

internal sealed class PakonRawProcessor
{
    public string LastTiming { get; private set; } = "";

    public Image<Rgb48> ProcessImage(
        string filename, bool isBwImage, bool invertImage, double gamma, float contrast, float saturation,
        float brightness = 1f, float redBalance = 1f, float greenBalance = 1f,
        float blueBalance = 1f, int rotation = 0, float exposureStops = 0f,
        float contrastAdjustment = 0f)
    {
        var readStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var header = new byte[16];
        byte[] buffer;
        byte[] interleaved;
        int pixelCount;
        int width;
        int height;

        using (var fileStream = File.OpenRead(filename))
        {
            fileStream.ReadExactly(header, 0, 16);
            width = (int)BitConverter.ToUInt32(header, 4);
            height = (int)BitConverter.ToUInt32(header, 8);
            var bitCount = BitConverter.ToUInt32(header, 12);

            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException("Pakon raw file has invalid dimensions " + width + "x" + height + ".");
            }

            if (width > 5000 || height > 5000 || bitCount != 48)
            {
                throw new InvalidOperationException("You are probably not processing a Pakon raw file.");
            }

            buffer = new byte[width * height * 6];
            interleaved = new byte[width * height * 6];
            fileStream.ReadExactly(buffer, 0, width * height * 6);
        }
        pixelCount = width * height;
        readStopwatch.Stop();

        var extremaStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var (darkest, brightest) = FindDarkestAndBrightestValues(buffer, pixelCount, isBwImage);
        extremaStopwatch.Stop();

        var mapStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var redMap = BuildLevelsGammaMap(brightest.R, darkest.R, gamma * 0.98);
        var greenMap = BuildLevelsGammaMap(brightest.G, darkest.G, gamma * 1.02);
        var blueMap = BuildLevelsGammaMap(brightest.B, darkest.B, gamma * 1.03);
        mapStopwatch.Stop();

        var transformStopwatch = System.Diagnostics.Stopwatch.StartNew();
        InterleaveAndTransformBuffer(pixelCount, buffer, interleaved, redMap, greenMap, blueMap);
        transformStopwatch.Stop();

        var loadStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var image = Image.LoadPixelData<Rgb48>(interleaved, width, height);
        loadStopwatch.Stop();

        var adjustStopwatch = System.Diagnostics.Stopwatch.StartNew();
        if (invertImage)
        {
            image.Mutate(x => x.Invert());
        }

        if (isBwImage)
        {
            image.Mutate(x => x.Saturate(0f));
        }

        image.Mutate(x => x.Contrast(contrast));
        if (!isBwImage)
        {
            image.Mutate(x => x.Saturate(saturation));
        }
        image.Mutate(x => x.Brightness(Math.Clamp(brightness, 0.1f, 2f)));
        ApplyToneAdjustments(image, exposureStops, contrastAdjustment);
        if (!isBwImage)
        {
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                    {
                        ref var pixel = ref row[x];
                        pixel.R = (ushort)Math.Clamp(pixel.R * redBalance, 0, ushort.MaxValue);
                        pixel.G = (ushort)Math.Clamp(pixel.G * greenBalance, 0, ushort.MaxValue);
                        pixel.B = (ushort)Math.Clamp(pixel.B * blueBalance, 0, ushort.MaxValue);
                    }
                }
            });
        }
        var normalizedRotation = ((rotation % 360) + 360) % 360;
        if (normalizedRotation != 0)
        {
            image.Mutate(x => x.Rotate(normalizedRotation));
        }
        adjustStopwatch.Stop();

        LastTiming =
            "read-raw=" + FormatDuration(readStopwatch.Elapsed) +
            ", extrema=" + FormatDuration(extremaStopwatch.Elapsed) +
            ", maps=" + FormatDuration(mapStopwatch.Elapsed) +
            ", transform=" + FormatDuration(transformStopwatch.Elapsed) +
            ", load-pixels=" + FormatDuration(loadStopwatch.Elapsed) +
            ", adjust=" + FormatDuration(adjustStopwatch.Elapsed);

        return image;
    }

    private static void ApplyToneAdjustments(
        Image<Rgb48> image,
        double exposureStops,
        double contrastAdjustment)
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

    private static ushort AdjustChannel(ushort value, double exposureFactor, double contrastExponent)
    {
        var encoded = value / (double)ushort.MaxValue;
        var linear = encoded <= 0.04045
            ? encoded / 12.92
            : Math.Pow((encoded + 0.055) / 1.055, 2.4);
        linear = Math.Clamp(linear * exposureFactor, 0, 1);
        encoded = linear <= 0.0031308
            ? linear * 12.92
            : 1.055 * Math.Pow(linear, 1 / 2.4) - 0.055;

        var distance = Math.Abs(encoded - 0.5) * 2;
        var curved = 0.5 + Math.Sign(encoded - 0.5) * 0.5 * Math.Pow(distance, contrastExponent);
        return (ushort)Math.Clamp(Math.Round(curved * ushort.MaxValue), 0, ushort.MaxValue);
    }

    private static void InterleaveAndTransformBuffer(int pixelCount, byte[] buffer, byte[] interleaved, ushort[] redMap, ushort[] greenMap, ushort[] blueMap)
    {
        const int pixelSize = 6;
        var greenOffset = pixelCount * 2;
        var blueOffset = pixelCount * 4;

        for (var source = 0; source != pixelCount * 2; source += 2)
        {
            var target = source / 2 * pixelSize;
            WriteUInt16(interleaved, target + 0, redMap[ReadUInt16(buffer, source)]);
            WriteUInt16(interleaved, target + 2, greenMap[ReadUInt16(buffer, greenOffset + source)]);
            WriteUInt16(interleaved, target + 4, blueMap[ReadUInt16(buffer, blueOffset + source)]);
        }
    }

    private static ushort[] BuildLevelsGammaMap(ushort brightest, ushort darkest, double gamma)
    {
        var map = new ushort[ushort.MaxValue + 1];
        var range = darkest - brightest;
        if (range == 0)
        {
            return map;
        }

        for (var value = 0; value < map.Length; value++)
        {
            var normalized = Math.Clamp((value - brightest) / (double)range, 0, 1);
            var leveled = 65_534 * normalized;
            var gammaInput = leveled / 65_500;
            map[value] = (ushort)Math.Clamp(Math.Pow(gammaInput, gamma) * 65_500, 0, ushort.MaxValue);
        }

        return map;
    }

    private static (Rgb48, Rgb48) FindDarkestAndBrightestValues(byte[] buffer, int pixelCount, bool bwNegative)
    {
        ushort darkestR = 0;
        ushort darkestG = 0;
        ushort darkestB = 0;
        ushort smallestR = 65_534;
        ushort smallestG = 65_534;
        ushort smallestB = 65_534;
        var greenOffset = pixelCount * 2;
        var blueOffset = pixelCount * 4;

        for (var source = 0; source != pixelCount * 2; source += 2)
        {
            var r = ReadUInt16(buffer, source);
            var g = ReadUInt16(buffer, greenOffset + source);
            var b = ReadUInt16(buffer, blueOffset + source);
            if (r > darkestR)
                darkestR = r;
            if (g > darkestG)
                darkestG = g;
            if (b > darkestB)
                darkestB = b;

            if (r < smallestR)
                smallestR = r;
            if (g < smallestG)
                smallestG = g;
            if (b < smallestB)
                smallestB = b;
        }

        if (bwNegative)
        {
            darkestR -= darkestR > 99 ? (ushort)100 : darkestR;
            darkestG -= darkestG > 99 ? (ushort)100 : darkestG;
            darkestB -= darkestB > 99 ? (ushort)100 : darkestB;

            smallestR = Math.Clamp(smallestR, (ushort)0, (ushort)65_454);
            smallestG = Math.Clamp(smallestG, (ushort)0, (ushort)65_454);
            smallestB = Math.Clamp(smallestB, (ushort)0, (ushort)65_454);

            smallestR += 80;
            smallestG += 80;
            smallestB += 80;
        }

        return (
            new Rgb48(darkestR, darkestG, darkestB),
            new Rgb48(smallestR, smallestG, smallestB));
    }

    private static ushort ReadUInt16(byte[] buffer, int offset)
    {
        return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
    }

    private static void WriteUInt16(byte[] buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }

    private static string FormatDuration(TimeSpan elapsed)
    {
        return elapsed.TotalSeconds >= 1
            ? elapsed.TotalSeconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s"
            : elapsed.TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " ms";
    }
}
