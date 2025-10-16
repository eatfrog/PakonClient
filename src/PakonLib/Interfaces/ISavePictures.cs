// Pakon.ISavePictures
using PakonLib.Enums;
using PakonLib.Models;

namespace PakonLib.Interfaces
{
    public interface ISavePictures
    {
        PictureCountScanGroupResult GetPictureCountScanGroup(int rollIndex);

        void MoveOldestRollToSaveGroup();

        PictureCountSaveGroupResult GetPictureCountSaveGroup();

        void SaveToDisk(PictureIndex index, SaveControl saveControl, int boundingWidth, int boundingHeight, ScalingMethod scalingMethod, FileFormat fileFormat, int compression, int dpi, int colorBits);

        void ClientMemoryBufferAdd(int byteStartPointer, int byteCount);

        void ClientMemoryBufferDismissAll();

        void SaveToClientMemory(ScannerType scannerType, PictureIndex index, SaveControl saveControl, int boundingWidth, int boundingHeight, ScalingMethod scalingMethod, MemoryFileFormat fileFormat, bool fourChannel);

        void SaveCancel();

        void PutPictureInfo(int index, int frameNumber, string fileName, string directory, int rotation, PictureSelection selectedHidden);

        void PutPictureSelection(PictureIndex index, PictureSelection selectOrHidden, bool skipHidden);

        PictureFramingInfo GetPictureFramingUserInfo(int index);

        PictureFramingInfo GetPictureFramingUserInfoLowRes(int index);
    }
}
