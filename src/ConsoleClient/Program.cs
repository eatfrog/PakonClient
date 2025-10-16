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
using PakonLib.Models;
using System.Security.Principal;
namespace ConsoleClient
{

    // TODO: this sample creates some huge buffer files in c:\buffers - tlxclientdemo does not do this
    // TODO: the executable must be run in the C:\Program Files (x86)\Pakon\F-X35 COM SERVER folder, otherwise it will fail
    // I think it needs more of the dlls in that folder to work properly
    class Program
    {
        static WorkerThreadProgress _wtProgress = WorkerThreadProgress.Initialize;

        static Scanner scanner;

        [STAThread]
        static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("This application requires administrator privileges.");
                Console.WriteLine("Please restart the application as an administrator.");
                Console.ReadKey();
                return;
            }
            //Console.ReadKey();
            Console.WriteLine("Creating new scanner");
            scanner = new Scanner();
            try
            {

                scanner.TlxScanProgress += (a, b) =>
                {
                    _wtProgress = WorkerThreadProgress.FromValue(b);
                    Console.WriteLine("Scan event: " + a + " - " + _wtProgress);
                };
                scanner.TlxError += (a, c) =>
                {
                    var errorCode = ErrorCode.FromValue(c);
                    Console.WriteLine("Error event: " + a + " - " + errorCode);
                    throw new Exception(errorCode.Name);
                };

                scanner.TlxHardware += (a, c) =>
                {
                    var errorCode = ErrorCode.FromValue(c);
                    Console.WriteLine("Hardware event: " + a + " - " + errorCode);
                };

                scanner.TlxSaveProgress += (a, b) =>
                {
                    _wtProgress = WorkerThreadProgress.FromValue(b);
                    Console.WriteLine("Save event: " + a + " - " + _wtProgress);
                };

                Thread.Sleep(1000);

                Console.WriteLine("Init TLX");
                ClearErrors();
                scanner.InitializeTLX(InitializationRequest.CSharpClient);
                
                while (_wtProgress != WorkerThreadProgress.ProgressComplete)
                {
                    Console.Write(".");
                    Thread.Sleep(100);
                }
                Console.WriteLine("Init done!");


                Console.WriteLine("Get scanner info");
                var scannerInfo = scanner.IScan.GetScannerInfo();
                Console.WriteLine("{0} {1} {2} {3} {4}", scannerInfo.ScannerType, scannerInfo.ScannerSerialNumber, scannerInfo.Hardware135, scannerInfo.Hardware235, scannerInfo.Hardware335);

                Console.WriteLine("Start scan");
                _wtProgress = WorkerThreadProgress.Initialize;
                scanner.IScan.ScanPictures(
                    RESOLUTION_000.RESOLUTION_BASE_8,
                    FILM_COLOR_000.FILM_COLOR_NEGATIVE,
                    FILM_FORMAT_000.FILM_FORMAT_35MM,
                    STRIP_MODE_000.STRIP_MODE_FULL_ROLL,
                    SCAN_CONTROL_000.SCAN_None);

                while (_wtProgress != WorkerThreadProgress.ProgressComplete)
                {
                    Thread.Sleep(100);
                }
                Console.WriteLine("Scan done");

                //Console.WriteLine("Move roll to save group");
                //scanner.ISave.MoveOldestRollToSaveGroup();

                Console.WriteLine("Saving to disk");
                _wtProgress = WorkerThreadProgress.Initialize;
                scanner.ISave.SaveToDisk(
                    PictureIndex.All,
                    SaveControl.SizeOriginal,
                    0,
                    0,
                    ScalingMethod.Bicubic,
                    FileFormat.Jpeg,
                    90,
                    300,
                    24);

                while (_wtProgress != WorkerThreadProgress.ProgressComplete)
                {
                    Thread.Sleep(100);
                }
                Console.WriteLine("Save done");

                Console.WriteLine("Press key to end");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                ClearErrors(ex);

                Thread.Sleep(3000);
            }

        }

        private static void ClearErrors(Exception ex)
        {

            int returnint = 0;
            do
            {
                var errors = scanner.GetAndClearLastErrorTLX(WorkerThreadOperation.TlxError);
                var errorCode = ErrorCode.FromValue(errors.ReturnValue);
                Console.WriteLine($"TLX error - exception: {ex.Message} message: {errors.ErrorMessage} num: {errors.ErrorNumbers} int: {errorCode.RawValue} ({errorCode})");
            } while (returnint == 25);
        }

        private static void ClearErrors()
        {
            int ret = 0;
            do
            {
                var errors = scanner.GetAndClearLastErrorTLX(WorkerThreadOperation.TlxError);
                var errorCode = ErrorCode.FromValue(errors.ReturnValue);
                Console.WriteLine($"TLX error - message: {errors.ErrorMessage} num: {errors.ErrorNumbers} int: {errorCode.RawValue} ({errorCode})");
                ret = errors.ReturnValue;
            } while (ret == 25);
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
