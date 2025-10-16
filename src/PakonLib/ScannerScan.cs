// Pakon.ScannerScan
using TLXLib;
using PakonLib.Enums;
using PakonLib.Models;

namespace PakonLib 
{
    public class ScannerScan : PakonLib.Interfaces.IScanPictures
    {
        private TLXMainClass tlx = null;

        public ScannerScan(TLXMainClass tlxMain)
        {
            tlx = tlxMain;
        }

        public void FocusCorrection(Resolution resolution, FilmColor filmColor, FilmFormat filmFormat, bool advanceFilm)
        {
            CALIBRATE_CONTROL_000 calibrateControl = (advanceFilm ? CALIBRATE_CONTROL_000.CALIBRATE_FocusAdvanceFilm : CALIBRATE_CONTROL_000.CALIBRATE_Focus);
            tlx.ForceCorrections((int)resolution.NativeValue, (int)filmColor.NativeValue, (int)filmFormat.NativeValue, (int)calibrateControl);
        }

        public void LightCorrection(Resolution resolution, FilmColor filmColor, FilmFormat filmFormat, bool includeIr)
        {
            CALIBRATE_CONTROL_000 calibrateControl = CALIBRATE_CONTROL_000.CALIBRATE_Light;
            if (includeIr)
            {
                calibrateControl |= CALIBRATE_CONTROL_000.CALIBRATE_UseScratchRemoval;
            }
            tlx.ForceCorrections((int)resolution.NativeValue, (int)filmColor.NativeValue, (int)filmFormat.NativeValue, (int)calibrateControl);
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

        public void ScanPictures(Resolution resolution, FilmColor filmColor, FilmFormat filmFormat, StripMode stripMode, ScanControl scanControl)
        {
            string rollId = "1000";
            tlx.ScanPictures(
                (int)resolution.NativeValue,
                (int)filmColor.NativeValue,
                (int)filmFormat.NativeValue,
                (int)stripMode.NativeValue,
                (int)scanControl.NativeValue,
                rollId);
        }

        public void ScanCancel()
        {
            tlx.ScanCancel();
        }

        public void AdvanceFilm(int advanceMilliseconds, int advanceSpeed)
        {
            tlx.AdvanceFilm(advanceMilliseconds, advanceSpeed);
        }

        public ScannerInfo GetScannerInfo()
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
            int scannerSerialNumber = 0;
            tlx.GetScannerInfo000(ref nativeScannerType, ref romVersion, ref scannerModel, ref scannerSerialNumber, ref nativeScannerVersionHardware, ref tlaVersion, ref darkPointCorrectIntervalMinutes, ref colorPortraitMode, ref scanPacketReadyTimeout, ref noFilmTimeout, ref lampSaverSeconds, ref tlxVersion);
            ScannerHW135 hardware135;
            ScannerHW235 hardware235;
            ScannerHW335 hardware335;
            Global.Convert(nativeScannerType, nativeScannerVersionHardware, out ScannerType scannerType, out hardware135, out hardware235, out hardware335);
            return new ScannerInfo(scannerType, scannerSerialNumber, hardware135, hardware235, hardware335);
        }
    }

    }
