using TLXLib;

namespace PakonLib
{
    public enum WorkerThreadOperation
    {
        InitializeError = (int)WORKER_THREAD_OPERATION_000.WTO_InitializeError,
        FirmwareUpdateApsError = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateApsError,
        FirmwareUpdateCcdError = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateCcdError,
        FirmwareUpdateDxError = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateDxError,
        FirmwareUpdateLampError = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateLampError,
        FirmwareUpdateMotorError = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateMotorError,
        FilmTrackTestError = (int)WORKER_THREAD_OPERATION_000.WTO_FilmTrackTestError,
        DiagnosticsError = (int)WORKER_THREAD_OPERATION_000.WTO_DiagnosticsError,
        CorrectionsError = (int)WORKER_THREAD_OPERATION_000.WTO_CorrectionsError,
        ExerciseSteppersError = (int)WORKER_THREAD_OPERATION_000.WTO_ExerciseSteppersError,
        LampWarmUpError = (int)WORKER_THREAD_OPERATION_000.WTO_LampWarmUpError,
        AdvanceFilmError = (int)WORKER_THREAD_OPERATION_000.WTO_AdvanceFilmError,
        PutFilmGuidePositionError = (int)WORKER_THREAD_OPERATION_000.WTO_PutFilmGuidePositionError,
        PutFilmPressureRollersPositionError = (int)WORKER_THREAD_OPERATION_000.WTO_PutFilmPressureRollersPositionError,
        Fx35CManualRetractError = (int)WORKER_THREAD_OPERATION_000.WTO_FX35C_ManualRetractError,
        ScanError = (int)WORKER_THREAD_OPERATION_000.WTO_ScanError,
        ImportFromFileError = (int)WORKER_THREAD_OPERATION_000.WTO_ImportFromFileError,
        SaveError = (int)WORKER_THREAD_OPERATION_000.WTO_SaveError,
        TlxError = (int)WORKER_THREAD_OPERATION_000.WTO_TLXError,
        HardwareProgress = (int)WORKER_THREAD_OPERATION_000.WTO_HardwareProgress,
        HardwareError = (int)WORKER_THREAD_OPERATION_000.WTO_HardwareError,
        HardwareApsProgress = (int)WORKER_THREAD_OPERATION_000.WTO_HardwareAPSProgress,
        HardwareApsError = (int)WORKER_THREAD_OPERATION_000.WTO_HardwareAPSError,
        InitializeProgress = (int)WORKER_THREAD_OPERATION_000.WTO_InitializeProgress,
        FirmwareUpdateApsProgress = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateApsProgress,
        FirmwareUpdateCcdProgress = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateCcdProgress,
        FirmwareUpdateDxProgress = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateDxProgress,
        FirmwareUpdateLampProgress = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateLampProgress,
        FirmwareUpdateMotorProgress = (int)WORKER_THREAD_OPERATION_000.WTO_FirmwareUpdateMotorProgress,
        FilmTrackTestProgress = (int)WORKER_THREAD_OPERATION_000.WTO_FilmTrackTestProgress,
        DiagnosticsProgress = (int)WORKER_THREAD_OPERATION_000.WTO_DiagnosticsProgress,
        CorrectionsProgress = (int)WORKER_THREAD_OPERATION_000.WTO_CorrectionsProgress,
        ExerciseSteppersProgress = (int)WORKER_THREAD_OPERATION_000.WTO_ExerciseSteppersProgress,
        LampWarmUpProgress = (int)WORKER_THREAD_OPERATION_000.WTO_LampWarmUpProgress,
        AdvanceFilmProgress = (int)WORKER_THREAD_OPERATION_000.WTO_AdvanceFilmProgress,
        PutFilmGuidePositionProgress = (int)WORKER_THREAD_OPERATION_000.WTO_PutFilmGuidePositionProgress,
        PutFilmPressureRollersPositionProgress = (int)WORKER_THREAD_OPERATION_000.WTO_PutFilmPressureRollersPositionProgress,
        Fx35CManualRetractProgress = (int)WORKER_THREAD_OPERATION_000.WTO_FX35C_ManualRetractProgress,
        ScanProgress = (int)WORKER_THREAD_OPERATION_000.WTO_ScanProgress,
        ImportFromFileProgress = (int)WORKER_THREAD_OPERATION_000.WTO_ImportFromFileProgress,
        TlxProgress = (int)WORKER_THREAD_OPERATION_000.WTO_TLXProgress,
        SaveProgress = (int)WORKER_THREAD_OPERATION_000.WTO_SaveProgress
    }

    internal static class WorkerThreadOperationExtensions
    {
        public static WorkerThreadOperation ToWorkerThreadOperation(this WORKER_THREAD_OPERATION_000 value)
        {
            return (WorkerThreadOperation)(int)value;
        }

        public static WORKER_THREAD_OPERATION_000 ToInterop(this WorkerThreadOperation value)
        {
            return (WORKER_THREAD_OPERATION_000)(int)value;
        }
    }
}
