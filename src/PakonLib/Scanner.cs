// Pakon.Scanner
using System;
using TLXLib;
using PakonLib;
using PakonLib.Enums;
using PakonLib.Interfaces;

namespace PakonLib 
{
    public class Scanner
    {
        private ScannerScan scannerScan = null;

        private ScannerSave scannerSave = null;

        private ScannerUnsafe scannerUnsafe = new ScannerUnsafe();

        protected TLXMainClass tlx = new TLXMainClass();

        private int tlxCookie = 0;

        public ScannerUnsafe Unsafe
        {
            get
            {
                return scannerUnsafe;
            }
        }

        public PakonLib.Interfaces.IScanPictures IScan
        {
            get
            {
                return scannerScan;
            }
        }

        public PakonLib.Interfaces.ISavePictures ISave
        {
            get
            {
                return scannerSave;
            }
        }

        public PakonLib.Interfaces.ISavePictures3 ISave3
        {
            get
            {
                return scannerSave;
            }
        }

        public event TLXError TlxError;

        public event TLXHardware TlxHardware;

        public event TLXScanProgress TlxScanProgress;

        public event TLXSaveProgress TlxSaveProgress;

        public Scanner()
        {
            //scannerUnsafe.Allocate(10000);
            scannerScan = new ScannerScan(tlx);
            scannerSave = new ScannerSave(tlx, scannerUnsafe);
        }

        public void TLXAwake(WorkerThreadOperation operation, int status)
        {
            switch (operation)
            {
                case WorkerThreadOperation.InitializeError:
                case WorkerThreadOperation.FirmwareUpdateApsError:
                case WorkerThreadOperation.FirmwareUpdateCcdError:
                case WorkerThreadOperation.FirmwareUpdateDxError:
                case WorkerThreadOperation.FirmwareUpdateLampError:
                case WorkerThreadOperation.FirmwareUpdateMotorError:
                case WorkerThreadOperation.FilmTrackTestError:
                case WorkerThreadOperation.DiagnosticsError:
                case WorkerThreadOperation.CorrectionsError:
                case WorkerThreadOperation.ExerciseSteppersError:
                case WorkerThreadOperation.LampWarmUpError:
                case WorkerThreadOperation.AdvanceFilmError:
                case WorkerThreadOperation.PutFilmGuidePositionError:
                case WorkerThreadOperation.PutFilmPressureRollersPositionError:
                case WorkerThreadOperation.Fx35CManualRetractError:
                case WorkerThreadOperation.ScanError:
                case WorkerThreadOperation.ImportFromFileError:
                case WorkerThreadOperation.SaveError:
                case WorkerThreadOperation.TlxError:
                    if (TlxError != null)
                    {
                        TlxError(operation, status);
                    }
                    break;
                case WorkerThreadOperation.HardwareProgress:
                case WorkerThreadOperation.HardwareError:
                case WorkerThreadOperation.HardwareApsProgress:
                case WorkerThreadOperation.HardwareApsError:
                    if (TlxHardware != null)
                    {
                        TlxHardware(operation, status);
                    }
                    break;
                case WorkerThreadOperation.InitializeProgress:
                case WorkerThreadOperation.FirmwareUpdateApsProgress:
                case WorkerThreadOperation.FirmwareUpdateCcdProgress:
                case WorkerThreadOperation.FirmwareUpdateDxProgress:
                case WorkerThreadOperation.FirmwareUpdateLampProgress:
                case WorkerThreadOperation.FirmwareUpdateMotorProgress:
                case WorkerThreadOperation.FilmTrackTestProgress:
                case WorkerThreadOperation.DiagnosticsProgress:
                case WorkerThreadOperation.CorrectionsProgress:
                case WorkerThreadOperation.ExerciseSteppersProgress:
                case WorkerThreadOperation.LampWarmUpProgress:
                case WorkerThreadOperation.AdvanceFilmProgress:
                case WorkerThreadOperation.PutFilmGuidePositionProgress:
                case WorkerThreadOperation.PutFilmPressureRollersPositionProgress:
                case WorkerThreadOperation.Fx35CManualRetractProgress:
                case WorkerThreadOperation.ScanProgress:
                case WorkerThreadOperation.ImportFromFileProgress:
                case WorkerThreadOperation.TlxProgress:
                    if (TlxScanProgress != null)
                    {
                        TlxScanProgress(operation, status);
                    }
                    break;
                case WorkerThreadOperation.SaveProgress:
                    switch (status)
                    {
                        case 3000:
                            ISave.ClientMemoryBufferDismissAll();
                            Unsafe.Deallocate();
                            break;
                        default:
                            Unsafe.ProcessBuffer(this);
                            Unsafe.NextBuffer(this);
                            break;
                        case 0:
                        case 1000:
                            break;
                    }
                    if (TlxSaveProgress != null)
                    {
                        TlxSaveProgress(operation, status);
                    }
                    break;
            }

            return;

        }

        public virtual bool IsClosed()
        {
            return tlxCookie == 0;
        }

        public virtual void Closing()
        {
        }

        public void InitializeTLX(InitializationRequest request)
        {
            int saveToMemoryTimeout = 200000;
            tlxCookie = tlx.CBAdvise(new CallBackClient(this));
            if (tlxCookie == 0)
            {
                throw new ArgumentNullException("No Scanner Detected");
            }
            tlx.InitializeScanner((int)request.NativeValue, saveToMemoryTimeout);
        }

        [Obsolete("Use the InitializationRequest overload to avoid referencing TLX enums directly.")]
        public void InitializeTLX(INITIALIZE_CONTROL_000 iInitializeControl)
        {
            InitializeTLX(InitializationRequest.FromNative(iInitializeControl));
        }

        public void GetAndClearLastErrorTLX(WorkerThreadOperation operation, ref string strError, ref string strErrorNumbers, out int returnValue)
        {
            INT_IID_000 shortInterfaceId = INT_IID_000.INT_IID_ITLAMain;
            switch (operation)
            {
                case WorkerThreadOperation.InitializeError:
                case WorkerThreadOperation.FirmwareUpdateApsError:
                case WorkerThreadOperation.FirmwareUpdateCcdError:
                case WorkerThreadOperation.FirmwareUpdateDxError:
                case WorkerThreadOperation.FirmwareUpdateLampError:
                case WorkerThreadOperation.FirmwareUpdateMotorError:
                case WorkerThreadOperation.HardwareError:
                case WorkerThreadOperation.HardwareApsError:
                    shortInterfaceId = INT_IID_000.INT_IID_ITLAMain;
                    break;
                case WorkerThreadOperation.FilmTrackTestError:
                case WorkerThreadOperation.CorrectionsError:
                case WorkerThreadOperation.ExerciseSteppersError:
                case WorkerThreadOperation.LampWarmUpError:
                case WorkerThreadOperation.AdvanceFilmError:
                case WorkerThreadOperation.PutFilmGuidePositionError:
                case WorkerThreadOperation.PutFilmPressureRollersPositionError:
                case WorkerThreadOperation.Fx35CManualRetractError:
                case WorkerThreadOperation.ScanError:
                case WorkerThreadOperation.ImportFromFileError:
                    shortInterfaceId = INT_IID_000.INT_IID_IScanPictures;
                    break;
                case WorkerThreadOperation.SaveError:
                    shortInterfaceId = INT_IID_000.INT_IID_ISavePictures;
                    break;
                case WorkerThreadOperation.TlxError:
                    shortInterfaceId = INT_IID_000.INT_IID_ITLXMain;
                    break;
            }
            returnValue = tlx.GetAndClearLastError((int)shortInterfaceId, ref strError, ref strErrorNumbers);
        }

        public void GetInitializeWarnings(ref ScannerInitializeWarnings iwScanner)
        {
            int initializeWarnings = 0;
            tlx.GetInitializeWarnings(ref initializeWarnings);
            iwScanner = (ScannerInitializeWarnings)initializeWarnings;
        }

        public void CBUnadviseTLX()
        {
            if (tlxCookie != 0)
            {
                tlx.CBUnadvise(tlxCookie);
                tlxCookie = 0;
            }
        }
    }

}
