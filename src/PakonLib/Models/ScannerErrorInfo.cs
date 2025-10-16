namespace PakonLib.Models
{
    public class ScannerErrorInfo
    {
        public ScannerErrorInfo(string errorMessage, string errorNumbers, int returnValue)
        {
            ErrorMessage = errorMessage;
            ErrorNumbers = errorNumbers;
            ReturnValue = returnValue;
        }

        public string ErrorMessage { get; }

        public string ErrorNumbers { get; }

        public int ReturnValue { get; }
    }
}
