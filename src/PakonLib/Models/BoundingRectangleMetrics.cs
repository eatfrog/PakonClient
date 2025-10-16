namespace PakonLib.Models
{
    public class BoundingRectangleMetrics
    {
        public BoundingRectangleMetrics(int width, int height, int bufferByteCount)
        {
            Width = width;
            Height = height;
            BufferByteCount = bufferByteCount;
        }

        public int Width { get; }

        public int Height { get; }

        public int BufferByteCount { get; }
    }
}
