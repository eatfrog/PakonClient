using System;
using TLXLib;
using PakonLib.Enums;
using PakonLib.Models;

namespace PakonLib
{
    public class ScannerSettings
    {
        private ScannerSettingsSave scannerSettingsSave = null;

        protected ScannerInitializeWarnings initializeWarnings = ScannerInitializeWarnings.Unknown;

        protected SCANNER_TYPE_000 scannerType = SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN;

        protected int serialNumber = 0;

        protected ScannerHW135 hardware135 = ScannerHW135.Unknown;

        protected ScannerHW235 hardware235 = ScannerHW235.Unknown;

        protected ScannerHW335 hardware335 = ScannerHW335.Unknown;

        private IntBits capabilities = null;

        public ScannerInitializeWarnings InitializeWarnings
        {
            get
            {
                return initializeWarnings;
            }
        }

        public SCANNER_TYPE_000 Type
        {
            get
            {
                return scannerType;
            }
        }

        public int SerialNumber
        {
            get
            {
                return serialNumber;
            }
        }

        public ScannerHW135 Hardware135
        {
            get
            {
                return hardware135;
            }
        }

        public ScannerHW235 Hardware235
        {
            get
            {
                return hardware235;
            }
        }

        public ScannerHW335 Hardware335
        {
            get
            {
                return hardware335;
            }
        }

        public bool this[ScannerCapabilities iCapability]
        {
            get
            {
                if (capabilities == null)
                {
                    return false;
                }
                return capabilities[(int)iCapability];
            }
        }

        public ScannerSettingsSave Save
        {
            get
            {
                return scannerSettingsSave;
            }
        }

        public ScannerSettings()
        {
            scannerSettingsSave = new ScannerSettingsSave();
        }

        public virtual void Reset()
        {
            initializeWarnings = ScannerInitializeWarnings.Unknown;
            scannerType = SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN;
            serialNumber = 0;
            hardware135 = ScannerHW135.Unknown;
            hardware235 = ScannerHW235.Unknown;
            hardware335 = ScannerHW335.Unknown;
            capabilities = null;
        }

        protected void SetCapabilities()
        {
            capabilities = new IntBits();
            switch (Type)
            {
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
                    capabilities[26] = true;
                    capabilities[27] = true;
                    capabilities[28] = true;
                    capabilities[22] = true;
                    capabilities[23] = true;
                    capabilities[24] = true;
                    capabilities[29] = true;
                    capabilities[30] = true;
                    capabilities[31] = true;
                    capabilities[0] = true;
                    capabilities[2] = true;
                    capabilities[4] = true;
                    capabilities[3] = true;
                    capabilities[7] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                    capabilities[26] = true;
                    capabilities[27] = true;
                    capabilities[28] = true;
                    capabilities[22] = true;
                    capabilities[23] = true;
                    capabilities[24] = true;
                    capabilities[25] = true;
                    capabilities[29] = true;
                    capabilities[30] = true;
                    capabilities[31] = true;
                    capabilities[0] = true;
                    capabilities[2] = true;
                    capabilities[4] = true;
                    capabilities[3] = true;
                    capabilities[5] = true;
                    capabilities[7] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                    capabilities[29] = true;
                    capabilities[30] = true;
                    capabilities[31] = true;
                    capabilities[6] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                    capabilities[29] = true;
                    capabilities[30] = true;
                    capabilities[31] = true;
                    capabilities[6] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
                    capabilities[26] = true;
                    capabilities[27] = true;
                    capabilities[28] = true;
                    capabilities[22] = true;
                    capabilities[23] = true;
                    capabilities[24] = true;
                    capabilities[29] = true;
                    capabilities[30] = true;
                    capabilities[31] = true;
                    capabilities[2] = true;
                    capabilities[4] = true;
                    capabilities[3] = true;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                    capabilities[26] = true;
                    capabilities[27] = true;
                    capabilities[28] = true;
                    capabilities[22] = true;
                    capabilities[23] = true;
                    capabilities[24] = true;
                    capabilities[25] = true;
                    capabilities[29] = true;
                    capabilities[30] = true;
                    capabilities[31] = true;
                    capabilities[2] = true;
                    capabilities[4] = true;
                    capabilities[3] = true;
                    break;
            }
        }

        public void SetScannerType(SCANNER_TYPE_000 newType)
        {
            switch (Type)
            {
                case SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                    if (scannerType != newType)
                    {
                        throw new ArgumentException("Scanner type change not allowed");
                    }
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                    if (scannerType != SCANNER_TYPE_000.SCANNER_TYPE_F_235 && scannerType != SCANNER_TYPE_000.SCANNER_TYPE_F_235C)
                    {
                        throw new ArgumentException("Scanner type change not allowed");
                    }
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                    if (scannerType != SCANNER_TYPE_000.SCANNER_TYPE_F_335 && scannerType != SCANNER_TYPE_000.SCANNER_TYPE_F_335C)
                    {
                        throw new ArgumentException("Scanner type change not allowed");
                    }
                    break;
            }
            scannerType = newType;
        }

        public void CompleteTLXInitialization(Scanner scanner)
        {
            initializeWarnings = scanner.GetInitializeWarnings();
            ScannerInfo scannerInfo = scanner.IScan.GetScannerInfo();
            scannerType = scannerInfo.ScannerType;
            serialNumber = scannerInfo.ScannerSerialNumber;
            hardware135 = scannerInfo.Hardware135;
            hardware235 = scannerInfo.Hardware235;
            hardware335 = scannerInfo.Hardware335;
            SetCapabilities();
        }

        public void OpenTLX(Scanner scanner)
        {
            var request = InitializationRequest.FromRawValue(1073741825);
            scanner.InitializeTLX(request);
        }

        public void CloseTLX(Scanner scanner)
        {
            scanner.CBUnadviseTLX();
        }

        public int GetResolutionHeight(Scanner scanner, RESOLUTION_000 resolution, FILM_FORMAT_000 filmFormat)
        {
            return Global.GetResolutionHeight(Type, resolution, filmFormat);
        }

        public void Scan(Scanner scanner)
        {
            FILM_COLOR_000 filmColor = FILM_COLOR_000.FILM_COLOR_NEGATIVE;
            FILM_FORMAT_000 filmFormat = FILM_FORMAT_000.FILM_FORMAT_35MM;
            RESOLUTION_000 resolution = RESOLUTION_000.RESOLUTION_BASE_4;
            STRIP_MODE_000 stripMode = STRIP_MODE_000.STRIP_MODE_FULL_ROLL;
            SCAN_CONTROL_000 scanControl = SCAN_CONTROL_000.SCAN_None;
            switch (Type)
            {
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                    resolution = RESOLUTION_000.RESOLUTION_BASE_4;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                    resolution = RESOLUTION_000.RESOLUTION_BASE_8;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                    resolution = RESOLUTION_000.RESOLUTION_BASE_8;
                    break;
            }
            scanner.IScan.ScanPictures(resolution, filmColor, filmFormat, stripMode, scanControl);
        }

        public void EjectFilm(Scanner scanner)
        {
            int advanceMilliseconds = 0;
            int advanceSpeed = 0;
            switch (Type)
            {
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                    advanceMilliseconds = 500;
                    advanceSpeed = 1000;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                    advanceMilliseconds = 5000;
                    advanceSpeed = 1000;
                    break;
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
                case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                    advanceMilliseconds = 5000;
                    advanceSpeed = 1000;
                    break;
            }
            scanner.IScan.AdvanceFilm(advanceMilliseconds, advanceSpeed);
        }
    }

}
