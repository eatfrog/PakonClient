// Pakon.ScannerSave
using TLXLib;

public class ScannerSave : Pakon.ISavePictures, Pakon.ISavePictures3
{
    private TLXMainClass m_csTLX = null;

    private ScannerUnsafe m_csUnsafe = null;

    public ScannerSave(TLXMainClass csTLX, ScannerUnsafe csUnsafe)
    {
        m_csTLX = csTLX;
        m_csUnsafe = csUnsafe;
    }

    public void GetPictureCountScanGroup(int iRollIndex, ref int iStripCount, ref int iPictureCount, ref int iScanWarnings)
    {
        m_csTLX.GetPictureCountScanGroup(iRollIndex, ref iStripCount, ref iPictureCount, ref iScanWarnings);
    }

    public void MoveOldestRollToSaveGroup()
    {
        m_csTLX.MoveOldestRollToSaveGroup();
    }

    public void GetPictureCountSaveGroup(ref int iRollCount, ref int iStripCount, ref int iPictureCount, ref int iPictureSelectedCount, ref int iPictureHiddenCount)
    {
        m_csTLX.GetPictureCountSaveGroup(ref iRollCount, ref iStripCount, ref iPictureCount, ref iPictureSelectedCount, ref iPictureHiddenCount);
    }

    public void SaveToDisk(INDEX_000 eIndex, SAVE_CONTROL_000 eSaveControl, int iBoundingWidth, int iBoundingHeight, SCALING_METHOD_000 eScalingMethod, FILE_FORMAT_000 eFileFormat, int iCompression, int iDpi, int iColorBits)
    {
        eSaveControl |= (SAVE_CONTROL_000)132;
        m_csTLX.SaveToDisk((int)eIndex, null, (int)eSaveControl, iBoundingWidth, iBoundingHeight, 0, (int)eScalingMethod, (int)eFileFormat, iCompression, iDpi, iColorBits);
    }

    public void ClientMemoryBufferAdd(int iByteStartPointer, int iByteCount)
    {
        m_csTLX.ClientMemoryBufferAdd(iByteStartPointer, iByteCount);
    }

    public void ClientMemoryBufferDismissAll()
    {
        m_csTLX.ClientMemoryBufferDismissAll();
    }

    public void SaveToClientMemory(SCANNER_TYPE_000 stType, INDEX_000 eIndex, SAVE_CONTROL_000 eSaveControl, int iBoundingWidth, int iBoundingHeight, SCALING_METHOD_000 eScalingMethod, FILE_FORMAT_SAVE_TO_MEMORY_000 eFileFormat, bool bFourChannel)
    {
        eSaveControl = ((!bFourChannel) ? (eSaveControl | (SAVE_CONTROL_000)1156) : (eSaveControl | (SAVE_CONTROL_000)1152));
        ROTATE_000 iRotation = (bFourChannel ? ((stType != SCANNER_TYPE_000.SCANNER_TYPE_F_135 && stType != SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS) ? ROTATE_000.ROTATE_90L : ROTATE_000.ROTATE_90R) : ROTATE_000.ROTATE_0);
        m_csTLX.SaveToClientMemory((int)eIndex, (int)eSaveControl, iBoundingWidth, iBoundingHeight, (int)iRotation, (int)eScalingMethod, (int)eFileFormat, 1);
    }

    public void SaveCancel()
    {
        m_csTLX.SaveCancel();
    }

    public void GetPictureInfo3(int iIndex, out int iRollIndexFromStrip, out int iStripIndexFromStrip, out int iFilmProductFromStrip, out int iFilmSpecifierFromStrip, out string strFrameName, out int iFrameNumber, out int iPrintAspectRatio, out string strFileName, out string strDirectory, out int iRotation, out S_OR_H_000 eSelectedHidden)
    {
        int piSelectedHidden = 0;
        m_csTLX.GetPictureInfo3(iIndex, out iRollIndexFromStrip, out iStripIndexFromStrip, out iFilmProductFromStrip, out iFilmSpecifierFromStrip, out strFrameName, out iFrameNumber, out iPrintAspectRatio, out strFileName, out strDirectory, out iRotation, out piSelectedHidden);
        eSelectedHidden = (S_OR_H_000)piSelectedHidden;
    }

    public void PutPictureInfo(int iIndex, int iFrameNumber, string strFileName, string strDirectory, int iRotation, S_OR_H_000 eSelectedHidden)
    {
        m_csTLX.PutPictureInfo(iIndex, iFrameNumber, strFileName, strDirectory, iRotation, (int)eSelectedHidden);
    }

    public void PutPictureSelection(INDEX_000 eIndex, S_OR_H_000 eSelectOrHidden, bool bSkipHidden)
    {
        m_csTLX.PutPictureSelection((int)eIndex, (int)eSelectOrHidden, bSkipHidden ? 1 : 0);
    }

    public void GetPictureFramingUserInfo(int iIndex, ref int iLeftHR, ref int iTopHR, ref int iRightHR, ref int iBottomHR)
    {
        iLeftHR = 0;
        iTopHR = 0;
        iRightHR = 0;
        iBottomHR = 0;
        m_csTLX.GetPictureFramingUserInfo(iIndex, ref iLeftHR, ref iTopHR, ref iRightHR, ref iBottomHR);
    }

    public void GetPictureFramingUserInfoLowRes(int iIndex, ref int iLeftLR, ref int iTopLR, ref int iRightLR, ref int iBottomLR)
    {
        iLeftLR = 0;
        iTopLR = 0;
        iRightLR = 0;
        iBottomLR = 0;
        m_csTLX.GetPictureFramingUserInfoLowRes(iIndex, ref iLeftLR, ref iTopLR, ref iRightLR, ref iBottomLR);
    }
}
