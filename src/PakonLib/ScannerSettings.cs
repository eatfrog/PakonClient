using System;
using TLXLib;
using PakonLib.Enums;
using PakonLib.Models;

namespace PakonLib
{
    public class ScannerSettings
    {
        private readonly ScannerSettingsSave scannerSettingsSave;

        protected ScannerInitializeWarnings initializeWarnings = ScannerInitializeWarnings.Unknown;

        protected ScannerType scannerType = ScannerType.Unknown;

        protected int serialNumber = 0;

        protected ScannerHW135 hardware135 = ScannerHW135.Unknown;

        protected ScannerHW235 hardware235 = ScannerHW235.Unknown;

        protected ScannerHW335 hardware335 = ScannerHW335.Unknown;

        private IntBits capabilities = null;

        public ScannerInitializeWarnings InitializeWarnings => initializeWarnings;

        public ScannerType Type => scannerType;

        public int SerialNumber => serialNumber;

        public ScannerHW135 Hardware135 => hardware135;

        public ScannerHW235 Hardware235 => hardware235;

        public ScannerHW335 Hardware335 => hardware335;

        public bool this[ScannerCapabilities capability]
        {
            get
            {
                if (capabilities == null)
                {
                    return false;
                }

                return capabilities[(int)capability];
            }
        }

        public ScannerSettingsSave Save => scannerSettingsSave;

        public ScannerSettings()
        {
            scannerSettingsSave = new ScannerSettingsSave();
        }

        public virtual void Reset()
        {
            initializeWarnings = ScannerInitializeWarnings.Unknown;
            scannerType = ScannerType.Unknown;
            serialNumber = 0;
            hardware135 = ScannerHW135.Unknown;
            hardware235 = ScannerHW235.Unknown;
            hardware335 = ScannerHW335.Unknown;
            capabilities = null;
        }

        protected void SetCapabilities()
        {
            capabilities = new IntBits();
            if (Type == ScannerType.F235)
            {
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
            }
            else if (Type == ScannerType.F235C)
            {
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
            }
            else if (Type == ScannerType.F135 || Type == ScannerType.F135Plus)
            {
                capabilities[29] = true;
                capabilities[30] = true;
                capabilities[31] = true;
                capabilities[6] = true;
            }
            else if (Type == ScannerType.F335)
            {
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
            }
            else if (Type == ScannerType.F335C)
            {
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
            }
        }

        public void SetScannerType(ScannerType newType)
        {
            if (Type == ScannerType.Unknown || Type == ScannerType.F135 || Type == ScannerType.F135Plus)
            {
                if (scannerType != newType)
                {
                    throw new ArgumentException("Scanner type change not allowed");
                }
            }
            else if (Type == ScannerType.F235 || Type == ScannerType.F235C)
            {
                if (scannerType != ScannerType.F235 && scannerType != ScannerType.F235C)
                {
                    throw new ArgumentException("Scanner type change not allowed");
                }
            }
            else if (Type == ScannerType.F335 || Type == ScannerType.F335C)
            {
                if (scannerType != ScannerType.F335 && scannerType != ScannerType.F335C)
                {
                    throw new ArgumentException("Scanner type change not allowed");
                }
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
            if (Type == ScannerType.F135)
            {
                resolution = RESOLUTION_000.RESOLUTION_BASE_4;
            }
            else if (Type == ScannerType.F235 || Type == ScannerType.F235C || Type == ScannerType.F135Plus)
            {
                resolution = RESOLUTION_000.RESOLUTION_BASE_8;
            }
            else if (Type == ScannerType.F335 || Type == ScannerType.F335C)
            {
                resolution = RESOLUTION_000.RESOLUTION_BASE_8;
            }

            scanner.IScan.ScanPictures(resolution, filmColor, filmFormat, stripMode, scanControl);
        }

        public void EjectFilm(Scanner scanner)
        {
            int advanceMilliseconds = 0;
            int advanceSpeed = 0;
            if (Type == ScannerType.F135 || Type == ScannerType.F135Plus)
            {
                advanceMilliseconds = 500;
                advanceSpeed = 1000;
            }
            else if (Type == ScannerType.F235 || Type == ScannerType.F235C)
            {
                advanceMilliseconds = 5000;
                advanceSpeed = 1000;
            }
            else if (Type == ScannerType.F335 || Type == ScannerType.F335C)
            {
                advanceMilliseconds = 5000;
                advanceSpeed = 1000;
            }

            scanner.IScan.AdvanceFilm(advanceMilliseconds, advanceSpeed);
        }
    }
}
