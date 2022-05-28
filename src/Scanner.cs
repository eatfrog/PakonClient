// Pakon.Scanner
using System;
using TLXLib;
using pakontest3;

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

    public Pakon.IScanPictures IScan
    {
        get
        {
            return m_csScannerScan;
        }
    }

    public Pakon.ISavePictures ISave
    {
        get
        {
            return m_csScannerSave;
        }
    }

    public Pakon.ISavePictures3 ISave3
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

    public void TLXAwake(WORKER_THREAD_OPERATION_000 lOperation, int lStatus)
    {
        WORKER_THREAD_OPERATION_000 wORKER_THREAD_OPERATION_ = WORKER_THREAD_OPERATION_000.WTO_InitializeProgress;

        switch (lOperation)
        {
            case WORKER_THREAD_OPERATION_000.WTO_InitializeError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateApsError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateCcdError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateDxError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateLampError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateMotorError:
            case WORKER_THREAD_OPERATION_000.WTO_FilmTrackTestError:
            case WORKER_THREAD_OPERATION_000.WTO_DiagnosticsError:
            case WORKER_THREAD_OPERATION_000.WTO_CorrectionsError:
            case WORKER_THREAD_OPERATION_000.WTO_ExerciseSteppersError:
            case WORKER_THREAD_OPERATION_000.WTO_LampWarmUpError:
            case WORKER_THREAD_OPERATION_000.WTO_AdvanceFilmError:
            case WORKER_THREAD_OPERATION_000.WTO_PutFilmGuidePositionError:
            case WORKER_THREAD_OPERATION_000.WTO_PutFilmPressureRollersPositionError:
            case WORKER_THREAD_OPERATION_000.WTO_FX35C_ManualRetractError:
            case WORKER_THREAD_OPERATION_000.WTO_ScanError:
            case WORKER_THREAD_OPERATION_000.WTO_ImportFromFileError:
            case WORKER_THREAD_OPERATION_000.WTO_SaveError:
            case WORKER_THREAD_OPERATION_000.WTO_TLXError:
                if (this.m_evTLXError != null)
                {
                    this.m_evTLXError(wORKER_THREAD_OPERATION_, lStatus);
                }
                break;
            case WORKER_THREAD_OPERATION_000.WTO_HardwareProgress:
            case WORKER_THREAD_OPERATION_000.WTO_HardwareError:
            case WORKER_THREAD_OPERATION_000.WTO_HardwareAPSProgress:
            case WORKER_THREAD_OPERATION_000.WTO_HardwareAPSError:
                if (this.m_evTLXHardware != null)
                {
                    this.m_evTLXHardware(wORKER_THREAD_OPERATION_, lStatus);
                }
                break;
            case WORKER_THREAD_OPERATION_000.WTO_InitializeProgress:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateApsProgress:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateCcdProgress:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateDxProgress:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateLampProgress:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateMotorProgress:
            case WORKER_THREAD_OPERATION_000.WTO_FilmTrackTestProgress:
            case WORKER_THREAD_OPERATION_000.WTO_DiagnosticsProgress:
            case WORKER_THREAD_OPERATION_000.WTO_CorrectionsProgress:
            case WORKER_THREAD_OPERATION_000.WTO_ExerciseSteppersProgress:
            case WORKER_THREAD_OPERATION_000.WTO_LampWarmUpProgress:
            case WORKER_THREAD_OPERATION_000.WTO_AdvanceFilmProgress:
            case WORKER_THREAD_OPERATION_000.WTO_PutFilmGuidePositionProgress:
            case WORKER_THREAD_OPERATION_000.WTO_PutFilmPressureRollersPositionProgress:
            case WORKER_THREAD_OPERATION_000.WTO_FX35C_ManualRetractProgress:
            case WORKER_THREAD_OPERATION_000.WTO_ScanProgress:
            case WORKER_THREAD_OPERATION_000.WTO_ImportFromFileProgress:
            case WORKER_THREAD_OPERATION_000.WTO_TLXProgress:
                if (this.m_evTLXScanProgress != null)
                {
                    this.m_evTLXScanProgress(wORKER_THREAD_OPERATION_, lStatus);
                }
                break;
            case WORKER_THREAD_OPERATION_000.WTO_SaveProgress:
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
                    this.m_evTLXSaveProgress(wORKER_THREAD_OPERATION_, lStatus);
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

    public void GetAndClearLastErrorTLX(WORKER_THREAD_OPERATION_000 wtoOperation, ref string strError, ref string strErrorNumbers, out int iReturn)
    {
        INT_IID_000 iShort_IID = INT_IID_000.INT_IID_ITLAMain;
        switch (wtoOperation)
        {
            case WORKER_THREAD_OPERATION_000.WTO_InitializeError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateApsError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateCcdError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateDxError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateLampError:
            case WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateMotorError:
            case WORKER_THREAD_OPERATION_000.WTO_HardwareError:
            case WORKER_THREAD_OPERATION_000.WTO_HardwareAPSError:
                iShort_IID = INT_IID_000.INT_IID_ITLAMain;
                break;
            case WORKER_THREAD_OPERATION_000.WTO_FilmTrackTestError:
            case WORKER_THREAD_OPERATION_000.WTO_CorrectionsError:
            case WORKER_THREAD_OPERATION_000.WTO_ExerciseSteppersError:
            case WORKER_THREAD_OPERATION_000.WTO_LampWarmUpError:
            case WORKER_THREAD_OPERATION_000.WTO_AdvanceFilmError:
            case WORKER_THREAD_OPERATION_000.WTO_PutFilmGuidePositionError:
            case WORKER_THREAD_OPERATION_000.WTO_PutFilmPressureRollersPositionError:
            case WORKER_THREAD_OPERATION_000.WTO_FX35C_ManualRetractError:
            case WORKER_THREAD_OPERATION_000.WTO_ScanError:
            case WORKER_THREAD_OPERATION_000.WTO_ImportFromFileError:
                iShort_IID = INT_IID_000.INT_IID_IScanPictures;
                break;
            case WORKER_THREAD_OPERATION_000.WTO_SaveError:
                iShort_IID = INT_IID_000.INT_IID_ISavePictures;
                break;
            case WORKER_THREAD_OPERATION_000.WTO_TLXError:
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
