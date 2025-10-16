// Pakon.ISavePictures
using TLXLib;
using PakonLib.Models;

namespace PakonLib.Interfaces
{
    public interface ISavePictures
    {
        PictureCountScanGroupResult GetPictureCountScanGroup(int iRollIndex);

        void MoveOldestRollToSaveGroup();

        PictureCountSaveGroupResult GetPictureCountSaveGroup();

        void SaveToDisk(INDEX_000 eIndex, SAVE_CONTROL_000 eSaveControl, int iBoundingWidth, int iBoundingHeight, SCALING_METHOD_000 eScalingMethod, FILE_FORMAT_000 eFileFormat, int iCompression, int iDpi, int iColorBits);

        void ClientMemoryBufferAdd(int iByteStartPointer, int iByteCount);

        void ClientMemoryBufferDismissAll();

        void SaveToClientMemory(SCANNER_TYPE_000 stType, INDEX_000 eIndex, SAVE_CONTROL_000 eSaveControl, int iBoundingWidth, int iBoundingHeight, SCALING_METHOD_000 eScalingMethod, FILE_FORMAT_SAVE_TO_MEMORY_000 eFileFormat, bool bFourChannel);

        void SaveCancel();

        void PutPictureInfo(int iIndex, int iFrameNumber, string strFileName, string strDirectory, int iRotation, S_OR_H_000 eSelectedHidden);

        void PutPictureSelection(INDEX_000 eIndex, S_OR_H_000 eSelectOrHidden, bool bSkipHidden);

        PictureFramingInfo GetPictureFramingUserInfo(int iIndex);

        PictureFramingInfo GetPictureFramingUserInfoLowRes(int iIndex);
    }
}



