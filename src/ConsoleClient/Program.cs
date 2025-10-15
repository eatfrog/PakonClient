using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLXLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using PakonLib;
using PakonLib.Enums;
namespace ConsoleClient
{
    class Program
    {
        static WORKER_THREAD_PROGRESS_000 _wtProgress = WORKER_THREAD_PROGRESS_000.WTP_Initialize;

        [STAThread]
        static void Main(string[] args)
        {
            //Console.ReadKey();
            Console.WriteLine("Creating new scanner");
            var scanner = new Scanner();
            try
            {

                scanner.TlxScanProgress += (a, b) =>
                {
                    _wtProgress = (WORKER_THREAD_PROGRESS_000)b;
                    Console.WriteLine("Scan event: " + a + " - " + _wtProgress);
                };
                scanner.TlxError += (a, c) =>
                {
                    var errorCode = (ERROR_CODES_000)c;
                    Console.WriteLine("Error event: " + a + " - " + errorCode);
                    throw new Exception(a + "- " + errorCode);
                };

                scanner.TlxHardware += (a, c) =>
                {
                    var errorCode = (ERROR_CODES_000)c;
                    Console.WriteLine("Hardware event: " + a + " - " + errorCode);
                };

                scanner.TlxSaveProgress += (a, b) =>
                {
                    _wtProgress = (WORKER_THREAD_PROGRESS_000)b;
                    Console.WriteLine("Save event: " + a + " - " + _wtProgress);
                };

                Thread.Sleep(1000);

                Console.WriteLine("Init TLX");
                scanner.InitializeTLX(InitializationRequest.CSharpClient);
                
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
                scanner.GetAndClearLastErrorTLX(WorkerThreadOperation.InitializeProgress, ref errorstring, ref errornr, out retnr);
                Console.WriteLine("{0} {1} {2}", errorstring, errornr, retnr);

                var type = SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN;
                int serial = 0;
                var scannerhw1 = ScannerHW135.Unknown;
                var scannerhw2 = ScannerHW235.Unknown;
                var scannerhw3 = ScannerHW335.Unknown;
                Console.WriteLine("Get scanner info");
                scanner.IScan.GetScannerInfo000(ref type, ref serial, ref scannerhw1, ref scannerhw2, ref scannerhw3);
                Console.WriteLine("{0} {1} {2} {3} {4}", type, serial, scannerhw1, scannerhw2, scannerhw3);

                Console.WriteLine("Start scan");
                _wtProgress = WORKER_THREAD_PROGRESS_000.WTP_Initialize;
                scanner.IScan.ScanPictures(
                    RESOLUTION_000.RESOLUTION_BASE_8,
                    FILM_COLOR_000.FILM_COLOR_NEGATIVE,
                    FILM_FORMAT_000.FILM_FORMAT_35MM,
                    STRIP_MODE_000.STRIP_MODE_FULL_ROLL,
                    SCAN_CONTROL_000.SCAN_None);

                while (_wtProgress != WORKER_THREAD_PROGRESS_000.WTP_ProgressComplete)
                {
                    Thread.Sleep(100);
                }
                Console.WriteLine("Scan done");

                Console.WriteLine("Move roll to save group");
                scanner.ISave.MoveOldestRollToSaveGroup();

                Console.WriteLine("Saving to disk");
                _wtProgress = WORKER_THREAD_PROGRESS_000.WTP_Initialize;
                scanner.ISave.SaveToDisk(
                    INDEX_000.INDEX_All,
                    SAVE_CONTROL_000.SAV_SizeOriginal,
                    0,
                    0,
                    SCALING_METHOD_000.SCALING_METHOD_BICUBIC,
                    FILE_FORMAT_000.iFILE_FORMAT_JPG,
                    90,
                    300,
                    24);

                while (_wtProgress != WORKER_THREAD_PROGRESS_000.WTP_ProgressComplete)
                {
                    Thread.Sleep(100);
                }
                Console.WriteLine("Save done");

                Console.WriteLine("Press key to end");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                ERROR_CODES_000 errorCode = (ERROR_CODES_000)1;
                string error = String.Empty;
                string errornumbers = String.Empty;
                WorkerThreadOperation wtoperation = WorkerThreadOperation.TlxError;

                scanner.GetAndClearLastErrorTLX(wtoperation, ref error, ref errornumbers, out int returnint);
                if (Enum.TryParse<ERROR_CODES_000>(ex.Message, out errorCode))
                    Console.WriteLine(error);
                else
                    Console.WriteLine(ex.Message + " Error: " + error);

                Thread.Sleep(3000);
            }

        }
    }
}
