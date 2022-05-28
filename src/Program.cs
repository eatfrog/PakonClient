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
        static WORKER_THREAD_PROGRESS_000 _wtProgress = WORKER_THREAD_PROGRESS_000.WTP_Initialize;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                //Console.ReadKey();
                Console.WriteLine("Creating new scanner");
                var scanner = new Scanner();

                scanner.m_evTLXScanProgress += (a, b) =>
                {
                    _wtProgress = (WORKER_THREAD_PROGRESS_000)b;
                    Console.WriteLine("Scan event: " + a + " - " + _wtProgress);
                };
                scanner.m_evTLXError += (a, c) =>
                {
                    var errorCode = (ERROR_CODES_000)c;
                    Console.WriteLine("Error event: " + a + " - " + errorCode);
                };

                scanner.m_evTLXHardware += (a, c) =>
                {
                    var errorCode = (ERROR_CODES_000)c;
                    Console.WriteLine("Hardware event: " + a + " - " + errorCode);
                };

                Thread.Sleep(1000);

                Console.WriteLine("Init TLX");
                scanner.InitializeTLX(INITIALIZE_CONTROL_000.INITIALIZE_CSharpClient);
                
                while (_wtProgress != WORKER_THREAD_PROGRESS_000.WTP_ProgressComplete)
                {
                    Console.Write(".");
                    Thread.Sleep(100);
                }
                Console.WriteLine("Init done!");


                Console.WriteLine("Get and clear last error");
                string errorstring = String.Empty;
                string errornr = string.Empty;
                int retnr = 0;
                scanner.GetAndClearLastErrorTLX(WORKER_THREAD_OPERATION_000.WTO_InitializeProgress, ref errorstring, ref errornr, out retnr);
                Console.WriteLine("{0} {1} {2}", errorstring, errornr, retnr);

                var type = SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN;
                int serial = 0;
                var scannerhw1 = ScannerHW135.Unknown;
                var scannerhw2 = ScannerHW235.Unknown;
                var scannerhw3 = ScannerHW335.Unknown;
                Console.WriteLine("Get scanner info");
                scanner.IScan.GetScannerInfo000(ref type, ref serial, ref scannerhw1, ref scannerhw2, ref scannerhw3);
                Console.WriteLine("{0} {1} {2} {3} {4}", type, serial, scannerhw1, scannerhw2, scannerhw3);

                Console.WriteLine("Press key to end");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                ERROR_CODES_000 errorCode = (ERROR_CODES_000)1;
                if (Enum.TryParse<ERROR_CODES_000>(ex.Message, out errorCode))
                    Console.WriteLine(errorCode);
                else
                    Console.WriteLine(ex.Message);
            }

        }
    }
}
