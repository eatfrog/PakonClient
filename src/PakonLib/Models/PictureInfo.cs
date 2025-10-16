using PakonLib.Enums;

namespace PakonLib.Models
{
    public class PictureInfo
    {
        public PictureInfo(
            int rollIndexFromStrip,
            int stripIndexFromStrip,
            int filmProductFromStrip,
            int filmSpecifierFromStrip,
            string frameName,
            int frameNumber,
            int printAspectRatio,
            string fileName,
            string directory,
            int rotation,
            PictureSelection selectedHidden)
        {
            RollIndexFromStrip = rollIndexFromStrip;
            StripIndexFromStrip = stripIndexFromStrip;
            FilmProductFromStrip = filmProductFromStrip;
            FilmSpecifierFromStrip = filmSpecifierFromStrip;
            FrameName = frameName;
            FrameNumber = frameNumber;
            PrintAspectRatio = printAspectRatio;
            FileName = fileName;
            Directory = directory;
            Rotation = rotation;
            SelectedHidden = selectedHidden;
        }

        public int RollIndexFromStrip { get; }

        public int StripIndexFromStrip { get; }

        public int FilmProductFromStrip { get; }

        public int FilmSpecifierFromStrip { get; }

        public string FrameName { get; }

        public int FrameNumber { get; }

        public int PrintAspectRatio { get; }

        public string FileName { get; }

        public string Directory { get; }

        public int Rotation { get; }

        public PictureSelection SelectedHidden { get; }
    }
}
