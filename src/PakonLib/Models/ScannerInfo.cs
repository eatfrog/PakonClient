using PakonLib.Enums;

namespace PakonLib.Models
{
    public class ScannerInfo
    {
        public ScannerInfo(
            ScannerType scannerType,
            int scannerSerialNumber,
            ScannerHW135 hardware135,
            ScannerHW235 hardware235,
            ScannerHW335 hardware335)
        {
            ScannerType = scannerType;
            ScannerSerialNumber = scannerSerialNumber;
            Hardware135 = hardware135;
            Hardware235 = hardware235;
            Hardware335 = hardware335;
        }

        public ScannerType ScannerType { get; }

        public int ScannerSerialNumber { get; }

        public ScannerHW135 Hardware135 { get; }

        public ScannerHW235 Hardware235 { get; }

        public ScannerHW335 Hardware335 { get; }
    }
}
