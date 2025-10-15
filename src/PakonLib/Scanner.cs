// Pakon.Scanner
using System;
using TLXLib;
using PakonLib;
using PakonLib.Interfaces;

namespace PakonLib 
{
    public class Scanner
    {
        private ScannerScan m_csScannerScan = null;

        private ScannerSave m_csScannerSave = null;

        private ScannerUnsafe m_csUnsafe = new ScannerUnsafe();

        protected TLXMainClass m_csTLX = new TLXMainClass();

        private int m_iTLXCookie = 0;

        public ScannerUnsafe Unsafe
        {
            get
            {
                return m_csUnsafe;
            }
        }

        public PakonLib.Interfaces.IScanPictures IScan
        {
            get
            {
                return m_csScannerScan;
            }
        }

        public PakonLib.Interfaces.ISavePictures ISave
        {
            get
            {
                return m_csScannerSave;
            }
        }

        public PakonLib.Interfaces.ISavePictures3 ISave3
        {
            get
            {
                return m_csScannerSave;
            }
        }

        public event TLXError m_evTLXError;

        public event TLXHardware m_evTLXHardware;

        public event TLXScanProgress m_evTLXScanProgress;

        public event TLXSaveProgress m_evTLXSaveProgress;

        public Scanner()
        {
            //m_csUnsafe.Allocate(10000);
            m_csScannerScan = new ScannerScan(m_csTLX);
            m_csScannerSave = new ScannerSave(m_csTLX, m_csUnsafe);
        }

        public void TLXAwake(WorkerThreadOperation operation, int lStatus)
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
                    if (this.m_evTLXError != null)
                    {
                        this.m_evTLXError(operation, lStatus);
                    }
                    break;
                case WorkerThreadOperation.HardwareProgress:
                case WorkerThreadOperation.HardwareError:
                case WorkerThreadOperation.HardwareApsProgress:
                case WorkerThreadOperation.HardwareApsError:
                    if (this.m_evTLXHardware != null)
                    {
                        this.m_evTLXHardware(operation, lStatus);
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
                    if (this.m_evTLXScanProgress != null)
                    {
                        this.m_evTLXScanProgress(operation, lStatus);
                    }
                    break;
                case WorkerThreadOperation.SaveProgress:
                    switch (lStatus)
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
                    if (this.m_evTLXSaveProgress != null)
                    {
                        this.m_evTLXSaveProgress(operation, lStatus);
                    }
                    break;
            }

            return;

        }

        public virtual bool IsClosed()
        {
            return m_iTLXCookie == 0;
        }

        public virtual void Closing()
        {
        }

        public void InitializeTLX(INITIALIZE_CONTROL_000 iInitializeControl)
        {
            int iSaveToMemoryTimeout = 200000;
            m_iTLXCookie = m_csTLX.CBAdvise(new CallBackClient(this));
            if (m_iTLXCookie == 0)
            {
                throw new ArgumentNullException("No Scanner Detected");
            }
            m_csTLX.InitializeScanner((int)iInitializeControl, iSaveToMemoryTimeout);
        }

        public void GetAndClearLastErrorTLX(WorkerThreadOperation operation, ref string strError, ref string strErrorNumbers, out int iReturn)
        {
            INT_IID_000 iShort_IID = INT_IID_000.INT_IID_ITLAMain;
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
                    iShort_IID = INT_IID_000.INT_IID_ITLAMain;
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
                    iShort_IID = INT_IID_000.INT_IID_IScanPictures;
                    break;
                case WorkerThreadOperation.SaveError:
                    iShort_IID = INT_IID_000.INT_IID_ISavePictures;
                    break;
                case WorkerThreadOperation.TlxError:
                    iShort_IID = INT_IID_000.INT_IID_ITLXMain;
                    break;
            }
            iReturn = m_csTLX.GetAndClearLastError((int)iShort_IID, ref strError, ref strErrorNumbers);
        }

        public void GetInitializeWarnings(ref ScannerInitializeWarnings iwScanner)
        {
            int piInitializeWarnings = 0;
            m_csTLX.GetInitializeWarnings(ref piInitializeWarnings);
            iwScanner = (ScannerInitializeWarnings)piInitializeWarnings;
        }

        public void CBUnadviseTLX()
        {
            if (m_iTLXCookie != 0)
            {
                m_csTLX.CBUnadvise(m_iTLXCookie);
                m_iTLXCookie = 0;
            }
        }
    }

}
