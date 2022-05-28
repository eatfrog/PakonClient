// Pakon.ISavePictures
using TLXLib;

namespace Pakon
{
    public interface ISavePictures
    {
        void GetPictureCountScanGroup(int iRollIndex, ref int iStripCount, ref int iPictureCount, ref int iScanWarnings);

        void MoveOldestRollToSaveGroup();

        void GetPictureCountSaveGroup(ref int iRollCount, ref int iStripCount, ref int iPictureCount, ref int iPictureSelectedCount, ref int iPictureHiddenCount);

        void SaveToDisk(INDEX_000 eIndex, SAVE_CONTROL_000 eSaveControl, int iBoundingWidth, int iBoundingHeight, SCALING_METHOD_000 eScalingMethod, FILE_FORMAT_000 eFileFormat, int iCompression, int iDpi, int iColorBits);

        void ClientMemoryBufferAdd(int iByteStartPointer, int iByteCount);

        void ClientMemoryBufferDismissAll();

        void SaveToClientMemory(SCANNER_TYPE_000 stType, INDEX_000 eIndex, SAVE_CONTROL_000 eSaveControl, int iBoundingWidth, int iBoundingHeight, SCALING_METHOD_000 eScalingMethod, FILE_FORMAT_SAVE_TO_MEMORY_000 eFileFormat, bool bFourChannel);

        void SaveCancel();

        void PutPictureInfo(int iIndex, int iFrameNumber, string strFileName, string strDirectory, int iRotation, S_OR_H_000 eSelectedHidden);

        void PutPictureSelection(INDEX_000 eIndex, S_OR_H_000 eSelectOrHidden, bool bSkipHidden);

        void GetPictureFramingUserInfo(int iIndex, ref int iLeftHR, ref int iTopHR, ref int iRightHR, ref int iBottomHR);

        void GetPictureFramingUserInfoLowRes(int iIndex, ref int iLeftLR, ref int iTopLR, ref int iRightLR, ref int iBottomLR);
    }
}



