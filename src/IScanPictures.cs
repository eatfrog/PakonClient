using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pakon;
using TLXLib;
namespace Pakon
{

    public interface IScanPictures
    {
        void FocusCorrection(RESOLUTION_000 iResolution, FILM_COLOR_000 iFilmColor, FILM_FORMAT_000 iFilmFormat, bool bAdvanceFilm);

        void LightCorrection(RESOLUTION_000 iResolution, FILM_COLOR_000 iFilmColor, FILM_FORMAT_000 iFilmFormat, bool bIR);

        void FilmTrackTest(bool bAdjustPots);

        int FilmTrackTestResults();

        void ScanPictures(RESOLUTION_000 rResolution, FILM_COLOR_000 fFilmColor, FILM_FORMAT_000 fFilmFormat, STRIP_MODE_000 smStripMode, SCAN_CONTROL_000 scControl);

        void ScanCancel();

        void AdvanceFilm(int iAdvanceMilliseconds, int iAdvanceSpeed);

        void GetScannerInfo000(ref SCANNER_TYPE_000 iScannerType, ref int iScannerSerialNumber, ref ScannerHW135 iHw135, ref ScannerHW235 iHw235, ref ScannerHW335 iHw335);
    }

}
