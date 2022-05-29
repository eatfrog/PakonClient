// Pakon.ScannerScan
using TLXLib;
using PakonLib.Enums;

namespace PakonLib 
{
    public class ScannerScan : PakonLib.Interfaces.IScanPictures
    {
        private TLXMainClass m_csTLX = null;

        public ScannerScan(TLXMainClass csTLX)
        {
            m_csTLX = csTLX;
        }

        public void FocusCorrection(RESOLUTION_000 iResolution, FILM_COLOR_000 iFilmColor, FILM_FORMAT_000 iFilmFormat, bool bAdvanceFilm)
        {
            CALIBRATE_CONTROL_000 iCalibrateControl = (bAdvanceFilm ? CALIBRATE_CONTROL_000.CALIBRATE_FocusAdvanceFilm : CALIBRATE_CONTROL_000.CALIBRATE_Focus);
            m_csTLX.ForceCorrections((int)iResolution, (int)iFilmColor, (int)iFilmFormat, (int)iCalibrateControl);
        }

        public void LightCorrection(RESOLUTION_000 iResolution, FILM_COLOR_000 iFilmColor, FILM_FORMAT_000 iFilmFormat, bool bIR)
        {
            CALIBRATE_CONTROL_000 cALIBRATE_CONTROL_ = CALIBRATE_CONTROL_000.CALIBRATE_Light;
            if (bIR)
            {
                cALIBRATE_CONTROL_ |= CALIBRATE_CONTROL_000.CALIBRATE_UseScratchRemoval;
            }
            m_csTLX.ForceCorrections((int)iResolution, (int)iFilmColor, (int)iFilmFormat, (int)cALIBRATE_CONTROL_);
        }

        public void FilmTrackTest(bool bAdjustPots)
        {
            int bDxPotsAdjust = (bAdjustPots ? 1 : 0);
            m_csTLX.FilmTrackTest(bDxPotsAdjust, 1);
        }

        public int FilmTrackTestResults()
        {
            return m_csTLX.FilmTrackTestResults();
        }

        public void ScanPictures(RESOLUTION_000 rResolution, FILM_COLOR_000 fFilmColor, FILM_FORMAT_000 fFilmFormat, STRIP_MODE_000 smStripMode, SCAN_CONTROL_000 scControl)
        {
            string bstrRollId = "1000";
            m_csTLX.ScanPictures((int)rResolution, (int)fFilmColor, (int)fFilmFormat, (int)smStripMode, (int)scControl, bstrRollId);
        }

        public void ScanCancel()
        {
            m_csTLX.ScanCancel();
        }

        public void AdvanceFilm(int iAdvanceMilliseconds, int iAdvanceSpeed)
        {
            m_csTLX.AdvanceFilm(iAdvanceMilliseconds, iAdvanceSpeed);
        }

        public void GetScannerInfo000(ref SCANNER_TYPE_000 iScannerType, 
            ref int iScannerSerialNumber, 
            ref ScannerHW135 iHw135, 
            ref ScannerHW235 iHw235, 
            ref ScannerHW335 iHw335)
        {
            int piScannerType = 0;
            int piScannerVersionHw = 0;
            int piDarkPointCorrectIntervalMinutes = 0;
            int piColorPortraitMode = 0;
            int pi_uiScanPacketReadyTimeOut = 0;
            int pi_uiNoFilmTimeOut = 0;
            int piLampSaverSec = 0;
            string pbstrRomVersion = "";
            string pbstrScannerModel = "";
            string pbstrTLAVersion = "";
            string pbstrTLXVersion = "";        
            m_csTLX.GetScannerInfo000(ref piScannerType, ref pbstrRomVersion, ref pbstrScannerModel, ref iScannerSerialNumber, ref piScannerVersionHw, ref pbstrTLAVersion, ref piDarkPointCorrectIntervalMinutes, ref piColorPortraitMode, ref pi_uiScanPacketReadyTimeOut, ref pi_uiNoFilmTimeOut, ref piLampSaverSec, ref pbstrTLXVersion);
            Global.Convert(piScannerType, piScannerVersionHw, out iScannerType, out iHw135, out iHw235, out iHw335);
        }
    }

    }
