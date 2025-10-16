namespace PakonLib.Models
{
    public class PictureCountSaveGroupResult
    {
        public PictureCountSaveGroupResult(int rollCount, int stripCount, int pictureCount, int pictureSelectedCount, int pictureHiddenCount)
        {
            RollCount = rollCount;
            StripCount = stripCount;
            PictureCount = pictureCount;
            PictureSelectedCount = pictureSelectedCount;
            PictureHiddenCount = pictureHiddenCount;
        }

        public int RollCount { get; }

        public int StripCount { get; }

        public int PictureCount { get; }

        public int PictureSelectedCount { get; }

        public int PictureHiddenCount { get; }
    }
}
