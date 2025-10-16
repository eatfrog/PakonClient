using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PakonLib;
using TLXLib;
using PakonLib.Enums;
using PakonLib.Models;
namespace PakonLib.Interfaces
{
    public interface IScanPictures
    {
        void FocusCorrection(RESOLUTION_000 iResolution, FILM_COLOR_000 iFilmColor, FILM_FORMAT_000 iFilmFormat, bool bAdvanceFilm);

        void LightCorrection(RESOLUTION_000 iResolution, FILM_COLOR_000 iFilmColor, FILM_FORMAT_000 iFilmFormat, bool bIR);

        void FilmTrackTest(bool bAdjustPots);

        int FilmTrackTestResults();

        void ScanPictures(RESOLUTION_000 rResolution, 
            FILM_COLOR_000 fFilmColor, 
            FILM_FORMAT_000 fFilmFormat, 
            STRIP_MODE_000 smStripMode, 
            SCAN_CONTROL_000 scControl);

        void ScanCancel();

        void AdvanceFilm(int iAdvanceMilliseconds, int iAdvanceSpeed);

        ScannerInfo GetScannerInfo();
    }

}
