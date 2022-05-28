using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLXLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
namespace pakontest3
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                var scanner = new Scanner();
                //scanner.InitializeTLX(INITIALIZE_CONTROL_000.INITIALIZE_CSharpClient);
                var settings = new ScannerSettings();
                settings.OpenTLX(scanner);
                settings.CompleteTLXInitialization(scanner);
                //scanner.IScan.GetScannerInfo000();
            }
            catch (Exception ex)
            {
                var errorCode = Enum.Parse(typeof(ERROR_CODES_000), ex.Message);
                Console.WriteLine(errorCode);
            }

        }
    }
}
