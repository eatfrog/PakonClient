namespace PakonLib.Models
{
    public class PictureCountScanGroupResult
    {
        public PictureCountScanGroupResult(int stripCount, int pictureCount, int scanWarnings)
        {
            StripCount = stripCount;
            PictureCount = pictureCount;
            ScanWarnings = scanWarnings;
        }

        public int StripCount { get; }

        public int PictureCount { get; }

        public int ScanWarnings { get; }
    }
}
