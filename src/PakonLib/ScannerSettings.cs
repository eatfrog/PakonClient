using System;
using PakonLib;
using TLXLib;
using PakonLib.Enums;

namespace PakonLib
{
    public class ScannerSettings
    {
        private ScannerSettingsSave m_csScannerSettingsSave = null;

        protected ScannerInitializeWarnings m_iwScanner = ScannerInitializeWarnings.Unknown;

        protected SCANNER_TYPE_000 m_stType = SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN;

        protected int m_nSerialNumber = 0;

        protected ScannerHW135 m_sh135 = ScannerHW135.Unknown;

        protected ScannerHW235 m_sh235 = ScannerHW235.Unknown;

        protected ScannerHW335 m_sh335 = ScannerHW335.Unknown;

        private IntBits m_capabilities = null;

        public ScannerInitializeWarnings InitializeWarnings
        {
            get
            {
                return m_iwScanner;
            }
        }

        public SCANNER_TYPE_000 Type
        {
            get
            {
                return m_stType;
            }
        }

        public int SerialNumber
        {
            get
            {
                return m_nSerialNumber;
            }
        }

        public ScannerHW135 Hardware135
        {
            get
            {
                return m_sh135;
            }
        }

        public ScannerHW235 Hardware235
        {
            get
            {
                return m_sh235;
            }
        }

        public ScannerHW335 Hardware335
        {
            get
            {
                return m_sh335;
            }
        }

        public bool this[ScannerCapabilities iCapability]
        {
            get
            {
                if (m_capabilities == null)
                {
                    return false;
                }
                return m_capabilities[(int)iCapability];
            }
        }

        public ScannerSettingsSave Save
        {
            get
            {
                return m_csScannerSettingsSave;
            }
        }

        public ScannerSettings()
        {
            m_csScannerSettingsSave = new ScannerSettingsSave();
        }

        public virtual void Reset()
        {
            m_iwScanner = ScannerInitializeWarnings.Unknown;
            m_stType = SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN;
            m_nSerialNumber = 0;
            m_sh135 = ScannerHW135.Unknown;
            m_sh235 = ScannerHW235.Unknown;
            m_sh335 = ScannerHW335.Unknown;
            m_capabilities = null;
        }

        protected void SetCapabilities()
        {
            m_capabilities = new IntBits();
            switch (Type)
            {
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
                    m_capabilities[26] = true;
                    m_capabilities[27] = true;
                    m_capabilities[28] = true;
                    m_capabilities[22] = true;
                    m_capabilities[23] = true;
                    m_capabilities[24] = true;
                    m_capabilities[29] = true;
                    m_capabilities[30] = true;
                    m_capabilities[31] = true;
                    m_capabilities[0] = true;
                    m_capabilities[2] = true;
                    m_capabilities[4] = true;
                    m_capabilities[3] = true;
                    m_capabilities[7] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                    m_capabilities[26] = true;
                    m_capabilities[27] = true;
                    m_capabilities[28] = true;
                    m_capabilities[22] = true;
                    m_capabilities[23] = true;
                    m_capabilities[24] = true;
                    m_capabilities[25] = true;
                    m_capabilities[29] = true;
                    m_capabilities[30] = true;
                    m_capabilities[31] = true;
                    m_capabilities[0] = true;
                    m_capabilities[2] = true;
                    m_capabilities[4] = true;
                    m_capabilities[3] = true;
                    m_capabilities[5] = true;
                    m_capabilities[7] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                    m_capabilities[29] = true;
                    m_capabilities[30] = true;
                    m_capabilities[31] = true;
                    m_capabilities[6] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                    m_capabilities[29] = true;
                    m_capabilities[30] = true;
                    m_capabilities[31] = true;
                    m_capabilities[6] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
                    m_capabilities[26] = true;
                    m_capabilities[27] = true;
                    m_capabilities[28] = true;
                    m_capabilities[22] = true;
                    m_capabilities[23] = true;
                    m_capabilities[24] = true;
                    m_capabilities[29] = true;
                    m_capabilities[30] = true;
                    m_capabilities[31] = true;
                    m_capabilities[2] = true;
                    m_capabilities[4] = true;
                    m_capabilities[3] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                    m_capabilities[26] = true;
                    m_capabilities[27] = true;
                    m_capabilities[28] = true;
                    m_capabilities[22] = true;
                    m_capabilities[23] = true;
                    m_capabilities[24] = true;
                    m_capabilities[25] = true;
                    m_capabilities[29] = true;
                    m_capabilities[30] = true;
                    m_capabilities[31] = true;
                    m_capabilities[2] = true;
                    m_capabilities[4] = true;
                    m_capabilities[3] = true;
                    break;
            }
        }

        public void SetScannerType(SCANNER_TYPE_000 stNewType)
        {
            switch (Type)
            {
                case SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                    if (m_stType != stNewType)
                    {
                        throw new ArgumentException("Scanner type change not allowed");
                    }
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                    if (m_stType != SCANNER_TYPE_000.SCANNER_TYPE_F_235 && m_stType != SCANNER_TYPE_000.SCANNER_TYPE_F_235C)
                    {
                        throw new ArgumentException("Scanner type change not allowed");
                    }
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                    if (m_stType != SCANNER_TYPE_000.SCANNER_TYPE_F_335 && m_stType != SCANNER_TYPE_000.SCANNER_TYPE_F_335C)
                    {
                        throw new ArgumentException("Scanner type change not allowed");
                    }
                    break;
            }
            m_stType = stNewType;
        }

        public void CompleteTLXInitialization(Scanner csScanner)
        {
            csScanner.GetInitializeWarnings(ref m_iwScanner);
            csScanner.IScan.GetScannerInfo000(ref m_stType, ref m_nSerialNumber, ref m_sh135, ref m_sh235, ref m_sh335);
            SetCapabilities();
        }

        public void OpenTLX(Scanner csScanner)
        {
            INITIALIZE_CONTROL_000 iInitializeControl = (INITIALIZE_CONTROL_000)1073741825;
            csScanner.InitializeTLX(iInitializeControl);
        }

        public void CloseTLX(Scanner csScanner)
        {
            csScanner.CBUnadviseTLX();
        }

        public int GetResolutionHeight(Scanner csScanner, RESOLUTION_000 srResolution, FILM_FORMAT_000 ffFormat)
        {
            return Global.GetResolutionHeight(Type, srResolution, ffFormat);
        }

        public void Scan(Scanner csScanner)
        {
            FILM_COLOR_000 fFilmColor = FILM_COLOR_000.FILM_COLOR_NEGATIVE;
            FILM_FORMAT_000 fFilmFormat = FILM_FORMAT_000.FILM_FORMAT_35MM;
            RESOLUTION_000 rResolution = RESOLUTION_000.RESOLUTION_BASE_4;
            STRIP_MODE_000 smStripMode = STRIP_MODE_000.STRIP_MODE_FULL_ROLL;
            SCAN_CONTROL_000 scControl = SCAN_CONTROL_000.SCAN_None;
            switch (Type)
            {
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                    rResolution = RESOLUTION_000.RESOLUTION_BASE_4;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                    rResolution = RESOLUTION_000.RESOLUTION_BASE_8;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                    rResolution = RESOLUTION_000.RESOLUTION_BASE_8;
                    break;
            }
            csScanner.IScan.ScanPictures(rResolution, fFilmColor, fFilmFormat, smStripMode, scControl);
        }

        public void EjectFilm(Scanner csScanner)
        {
            int iAdvanceMilliseconds = 0;
            int iAdvanceSpeed = 0;
            switch (Type)
            {
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                    iAdvanceMilliseconds = 500;
                    iAdvanceSpeed = 1000;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                    iAdvanceMilliseconds = 5000;
                    iAdvanceSpeed = 1000;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                    iAdvanceMilliseconds = 5000;
                    iAdvanceSpeed = 1000;
                    break;
            }
            csScanner.IScan.AdvanceFilm(iAdvanceMilliseconds, iAdvanceSpeed);
        }
    }

}
