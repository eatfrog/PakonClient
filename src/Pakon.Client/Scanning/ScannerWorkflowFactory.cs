using Pakon.Scanner;
using Pakon.Scanner.Legacy;

namespace Pakon.Client.Scanning;

internal static class ScannerWorkflowFactory
{
    public static IScannerWorkflow CreateDefault() => new LegacyF135ScannerWorkflow();
}
