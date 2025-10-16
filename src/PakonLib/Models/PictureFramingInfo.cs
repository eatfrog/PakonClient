namespace PakonLib.Models
{
    public class PictureFramingInfo
    {
        public PictureFramingInfo(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left { get; }

        public int Top { get; }

        public int Right { get; }

        public int Bottom { get; }
    }
}
