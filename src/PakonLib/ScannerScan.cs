// Pakon.ScannerScan
using TLXLib;
using PakonLib.Enums;

namespace PakonLib 
{
    public class ScannerScan : PakonLib.Interfaces.IScanPictures
    {
        private TLXMainClass tlx = null;

        public ScannerScan(TLXMainClass tlxMain)
        {
            tlx = tlxMain;
        }

        public void FocusCorrection(RESOLUTION_000 resolution, FILM_COLOR_000 filmColor, FILM_FORMAT_000 filmFormat, bool advanceFilm)
        {
            CALIBRATE_CONTROL_000 calibrateControl = (advanceFilm ? CALIBRATE_CONTROL_000.CALIBRATE_FocusAdvanceFilm : CALIBRATE_CONTROL_000.CALIBRATE_Focus);
            tlx.ForceCorrections((int)resolution, (int)filmColor, (int)filmFormat, (int)calibrateControl);
        }

        public void LightCorrection(RESOLUTION_000 resolution, FILM_COLOR_000 filmColor, FILM_FORMAT_000 filmFormat, bool includeIr)
        {
            CALIBRATE_CONTROL_000 calibrateControl = CALIBRATE_CONTROL_000.CALIBRATE_Light;
            if (includeIr)
            {
                calibrateControl |= CALIBRATE_CONTROL_000.CALIBRATE_UseScratchRemoval;
            }
            tlx.ForceCorrections((int)resolution, (int)filmColor, (int)filmFormat, (int)calibrateControl);
        }

        public void FilmTrackTest(bool adjustPots)
        {
            int adjustPotsValue = (adjustPots ? 1 : 0);
            tlx.FilmTrackTest(adjustPotsValue, 1);
        }

        public int FilmTrackTestResults()
        {
            return tlx.FilmTrackTestResults();
        }

        public void ScanPictures(RESOLUTION_000 resolution, FILM_COLOR_000 filmColor, FILM_FORMAT_000 filmFormat, STRIP_MODE_000 stripMode, SCAN_CONTROL_000 scanControl)
        {
            string rollId = "1000";
            tlx.ScanPictures((int)resolution, (int)filmColor, (int)filmFormat, (int)stripMode, (int)scanControl, rollId);
        }

        public void ScanCancel()
        {
            tlx.ScanCancel();
        }

        public void AdvanceFilm(int advanceMilliseconds, int advanceSpeed)
        {
            tlx.AdvanceFilm(advanceMilliseconds, advanceSpeed);
        }

        public void GetScannerInfo000(ref SCANNER_TYPE_000 scannerType,
            ref int scannerSerialNumber,
            ref ScannerHW135 hardware135,
            ref ScannerHW235 hardware235,
            ref ScannerHW335 hardware335)
        {
            int nativeScannerType = 0;
            int nativeScannerVersionHardware = 0;
            int darkPointCorrectIntervalMinutes = 0;
            int colorPortraitMode = 0;
            int scanPacketReadyTimeout = 0;
            int noFilmTimeout = 0;
            int lampSaverSeconds = 0;
            string romVersion = "";
            string scannerModel = "";
            string tlaVersion = "";
            string tlxVersion = "";
            tlx.GetScannerInfo000(ref nativeScannerType, ref romVersion, ref scannerModel, ref scannerSerialNumber, ref nativeScannerVersionHardware, ref tlaVersion, ref darkPointCorrectIntervalMinutes, ref colorPortraitMode, ref scanPacketReadyTimeout, ref noFilmTimeout, ref lampSaverSeconds, ref tlxVersion);
            Global.Convert(nativeScannerType, nativeScannerVersionHardware, out scannerType, out hardware135, out hardware235, out hardware335);
        }
    }

    }
