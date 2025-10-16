// Pakon.ScannerSave
using TLXLib;
using PakonLib.Interfaces;
using PakonLib.Models;
using PakonLib;
using PakonLib.Enums;

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

    public void SaveToDisk(PictureIndex index, SaveControl saveControl, int boundingWidth, int boundingHeight, ScalingMethod scalingMethod, FileFormat fileFormat, int compression, int dpi, int colorBits)
    {
        var control = saveControl | SaveControl.FromRawValue(132);
        tlx.SaveToDisk((int)index.NativeValue, null, (int)control.NativeValue, boundingWidth, boundingHeight, 0, (int)scalingMethod.NativeValue, (int)fileFormat.NativeValue, compression, dpi, colorBits);
    }

    public void ClientMemoryBufferAdd(int iByteStartPointer, int iByteCount)
    {
        tlx.ClientMemoryBufferAdd(iByteStartPointer, iByteCount);
    }

    public void ClientMemoryBufferDismissAll()
    {
        tlx.ClientMemoryBufferDismissAll();
    }

    public void SaveToClientMemory(ScannerType scannerType, PictureIndex index, SaveControl saveControl, int boundingWidth, int boundingHeight, ScalingMethod scalingMethod, MemoryFileFormat fileFormat, bool fourChannel)
    {
        var control = fourChannel
            ? saveControl | SaveControl.FromRawValue(1152)
            : saveControl | SaveControl.FromRawValue(1156);
        ROTATE_000 rotation = fourChannel
            ? ((scannerType == ScannerType.F135 || scannerType == ScannerType.F135Plus) ? ROTATE_000.ROTATE_90R : ROTATE_000.ROTATE_90L)
            : ROTATE_000.ROTATE_0;
        tlx.SaveToClientMemory((int)index.NativeValue, (int)control.NativeValue, boundingWidth, boundingHeight, (int)rotation, (int)scalingMethod.NativeValue, (int)fileFormat.NativeValue, 1);
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
        PictureSelection selectedHidden = PictureSelection.FromRawValue(piSelectedHidden);
        return new PictureInfo(rollIndexFromStrip, stripIndexFromStrip, filmProductFromStrip, filmSpecifierFromStrip, frameName, frameNumber, printAspectRatio, fileName, directory, rotation, selectedHidden);
    }

    public void PutPictureInfo(int iIndex, int iFrameNumber, string strFileName, string strDirectory, int iRotation, PictureSelection eSelectedHidden)
    {
        tlx.PutPictureInfo(iIndex, iFrameNumber, strFileName, strDirectory, iRotation, (int)eSelectedHidden.NativeValue);
    }

    public void PutPictureSelection(PictureIndex eIndex, PictureSelection eSelectOrHidden, bool bSkipHidden)
    {
        tlx.PutPictureSelection((int)eIndex.NativeValue, (int)eSelectOrHidden.NativeValue, bSkipHidden ? 1 : 0);
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
