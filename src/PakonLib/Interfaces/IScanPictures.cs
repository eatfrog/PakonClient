using PakonLib.Enums;
using PakonLib.Models;

namespace PakonLib.Interfaces
{
    public interface IScanPictures
    {
        void FocusCorrection(Resolution resolution, FilmColor filmColor, FilmFormat filmFormat, bool advanceFilm);

        void LightCorrection(Resolution resolution, FilmColor filmColor, FilmFormat filmFormat, bool includeIr);

        void FilmTrackTest(bool adjustPots);

        int FilmTrackTestResults();

        void ScanPictures(
            Resolution resolution,
            FilmColor filmColor,
            FilmFormat filmFormat,
            StripMode stripMode,
            ScanControl scanControl);

        void ScanCancel();

        void AdvanceFilm(int advanceMilliseconds, int advanceSpeed);

        ScannerInfo GetScannerInfo();
    }
}
