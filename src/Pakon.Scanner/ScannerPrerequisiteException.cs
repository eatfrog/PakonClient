namespace Pakon.Scanner;

public sealed class ScannerPrerequisiteException : InvalidOperationException
{
    public ScannerPrerequisiteException(string message)
        : base(message)
    {
    }
}
