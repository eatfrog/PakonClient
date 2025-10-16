// Pakon.ScannerSave
using TLXLib;
using PakonLib.Interfaces;
using PakonLib.Models;

public class ScannerSave : PakonLib.Interfaces.ISavePictures, PakonLib.Interfaces.ISavePictures3
{
    private TLXMainClass tlx = null;

    private ScannerUnsafe scannerUnsafe = null;

    public ScannerSave(TLXMainClass tlxMain, ScannerUnsafe scannerUnsafeInstance)
    {
        tlx = tlxMain;
        scannerUnsafe = scannerUnsafeInstance;
    }

    public PictureCountScanGroupResult GetPictureCountScanGroup(int iRollIndex)
    {
        int stripCount = 0;
        int pictureCount = 0;
        int scanWarnings = 0;
        tlx.GetPictureCountScanGroup(iRollIndex, ref stripCount, ref pictureCount, ref scanWarnings);
        return new PictureCountScanGroupResult(stripCount, pictureCount, scanWarnings);
    }

    public void MoveOldestRollToSaveGroup()
    {
        tlx.MoveOldestRollToSaveGroup();
    }

    public PictureCountSaveGroupResult GetPictureCountSaveGroup()
    {
        int rollCount = 0;
        int stripCount = 0;
        int pictureCount = 0;
        int pictureSelectedCount = 0;
        int pictureHiddenCount = 0;
        tlx.GetPictureCountSaveGroup(ref rollCount, ref stripCount, ref pictureCount, ref pictureSelectedCount, ref pictureHiddenCount);
        return new PictureCountSaveGroupResult(rollCount, stripCount, pictureCount, pictureSelectedCount, pictureHiddenCount);
    }

    public void SaveToDisk(INDEX_000 eIndex, SAVE_CONTROL_000 eSaveControl, int iBoundingWidth, int iBoundingHeight, SCALING_METHOD_000 eScalingMethod, FILE_FORMAT_000 eFileFormat, int iCompression, int iDpi, int iColorBits)
    {
        eSaveControl |= (SAVE_CONTROL_000)132;
        tlx.SaveToDisk((int)eIndex, null, (int)eSaveControl, iBoundingWidth, iBoundingHeight, 0, (int)eScalingMethod, (int)eFileFormat, iCompression, iDpi, iColorBits);
    }

    public void ClientMemoryBufferAdd(int iByteStartPointer, int iByteCount)
    {
        tlx.ClientMemoryBufferAdd(iByteStartPointer, iByteCount);
    }

    public void ClientMemoryBufferDismissAll()
    {
        tlx.ClientMemoryBufferDismissAll();
    }

    public void SaveToClientMemory(SCANNER_TYPE_000 stType, INDEX_000 eIndex, SAVE_CONTROL_000 eSaveControl, int iBoundingWidth, int iBoundingHeight, SCALING_METHOD_000 eScalingMethod, FILE_FORMAT_SAVE_TO_MEMORY_000 eFileFormat, bool bFourChannel)
    {
        eSaveControl = ((!bFourChannel) ? (eSaveControl | (SAVE_CONTROL_000)1156) : (eSaveControl | (SAVE_CONTROL_000)1152));
        ROTATE_000 iRotation = (bFourChannel ? ((stType != SCANNER_TYPE_000.SCANNER_TYPE_F_135 && stType != SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS) ? ROTATE_000.ROTATE_90L : ROTATE_000.ROTATE_90R) : ROTATE_000.ROTATE_0);
        tlx.SaveToClientMemory((int)eIndex, (int)eSaveControl, iBoundingWidth, iBoundingHeight, (int)iRotation, (int)eScalingMethod, (int)eFileFormat, 1);
    }

    public void SaveCancel()
    {
        tlx.SaveCancel();
    }

    public PictureInfo GetPictureInfo3(int iIndex)
    {
        int piSelectedHidden = 0;
        int rollIndexFromStrip;
        int stripIndexFromStrip;
        int filmProductFromStrip;
        int filmSpecifierFromStrip;
        string frameName;
        int frameNumber;
        int printAspectRatio;
        string fileName;
        string directory;
        int rotation;
        tlx.GetPictureInfo3(iIndex, out rollIndexFromStrip, out stripIndexFromStrip, out filmProductFromStrip, out filmSpecifierFromStrip, out frameName, out frameNumber, out printAspectRatio, out fileName, out directory, out rotation, out piSelectedHidden);
        S_OR_H_000 selectedHidden = (S_OR_H_000)piSelectedHidden;
        return new PictureInfo(rollIndexFromStrip, stripIndexFromStrip, filmProductFromStrip, filmSpecifierFromStrip, frameName, frameNumber, printAspectRatio, fileName, directory, rotation, selectedHidden);
    }

    public void PutPictureInfo(int iIndex, int iFrameNumber, string strFileName, string strDirectory, int iRotation, S_OR_H_000 eSelectedHidden)
    {
        tlx.PutPictureInfo(iIndex, iFrameNumber, strFileName, strDirectory, iRotation, (int)eSelectedHidden);
    }

    public void PutPictureSelection(INDEX_000 eIndex, S_OR_H_000 eSelectOrHidden, bool bSkipHidden)
    {
        tlx.PutPictureSelection((int)eIndex, (int)eSelectOrHidden, bSkipHidden ? 1 : 0);
    }

    public PictureFramingInfo GetPictureFramingUserInfo(int iIndex)
    {
        int left = 0;
        int top = 0;
        int right = 0;
        int bottom = 0;
        tlx.GetPictureFramingUserInfo(iIndex, ref left, ref top, ref right, ref bottom);
        return new PictureFramingInfo(left, top, right, bottom);
    }

    public PictureFramingInfo GetPictureFramingUserInfoLowRes(int iIndex)
    {
        int left = 0;
        int top = 0;
        int right = 0;
        int bottom = 0;
        tlx.GetPictureFramingUserInfoLowRes(iIndex, ref left, ref top, ref right, ref bottom);
        return new PictureFramingInfo(left, top, right, bottom);
}
}
