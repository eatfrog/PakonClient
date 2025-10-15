using System;
using PakonLib;
using TLXLib;

namespace PakonLib
{
    public class ScannerSettingsSave
    {
        private INDEX_000 index;

        private SAVE_CONTROL_000 saveControl;

        private SCALING_METHOD_000 scalingMethod;

        private FILE_FORMAT_SAVE_TO_MEMORY_000 memoryFormat;

        public INDEX_000 Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

        public SAVE_CONTROL_000 Control
        {
            get
            {
                return saveControl;
            }
            set
            {
                saveControl = value;
            }
        }

        public SCALING_METHOD_000 ScalingMethod
        {
            get
            {
                return scalingMethod;
            }
            set
            {
                scalingMethod = value;
            }
        }

        public FILE_FORMAT_SAVE_TO_MEMORY_000 MemoryFormat
        {
            get
            {
                return memoryFormat;
            }
            set
            {
                memoryFormat = value;
            }
        }

        public event AddPictureToSaveGroup PictureAddedToSaveGroup;

        public void MoveRollToSaveGroup(Scanner scanner)
        {
            int rollCount = 0;
            int pictureCount = 0;
            int picturesToAdd = 0;
            scanner.ISave.GetPictureCountSaveGroup(ref rollCount, ref rollCount, ref pictureCount, ref rollCount, ref rollCount);
            if (pictureCount > 0)
            {
                scanner.ISave.PutPictureSelection(INDEX_000.INDEX_All, S_OR_H_000.S_OR_H_NONE, true);
            }
            scanner.ISave.GetPictureCountScanGroup(0, ref rollCount, ref picturesToAdd, ref rollCount);
            scanner.ISave.MoveOldestRollToSaveGroup();
            for (int i = pictureCount; i < pictureCount + picturesToAdd; i++)
            {
                PictureAddedToSaveGroup?.Invoke(i);
            }
        }

        public void SetPictureInfo(Scanner scanner, int indexValue, int newFrameNumber, string newFileName, string newDirectory, int newRotation, S_OR_H_000 newSelectedHidden, IntBits info)
        {
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
            S_OR_H_000 selectedHidden;
            scanner.ISave3.GetPictureInfo3(indexValue, out rollIndexFromStrip, out stripIndexFromStrip, out filmProductFromStrip, out filmSpecifierFromStrip, out frameName, out frameNumber, out printAspectRatio, out fileName, out directory, out rotation, out selectedHidden);
            if (info[0])
            {
                frameNumber = newFrameNumber;
            }
            if (info[1])
            {
                fileName = newFileName;
            }
            if (info[2])
            {
                directory = newDirectory;
            }
            if (info[3])
            {
                rotation = newRotation;
            }
            if (info[4])
            {
                selectedHidden = newSelectedHidden;
            }
            scanner.ISave.PutPictureInfo(indexValue, frameNumber, fileName, directory, rotation, selectedHidden);
        }

        private void FindBoundingRectangle(Scanner scanner, out int boundingWidth, out int boundingHeight, out int bufferByteCount, bool fourChannel)
        {
            int rollCount = 0;
            int pictureCount = 0;
            scanner.ISave.GetPictureCountSaveGroup(ref rollCount, ref rollCount, ref pictureCount, ref rollCount, ref rollCount);
            int startIndex;
            int endIndex;
            switch (index)
            {
                case INDEX_000.INDEX_All:
                case INDEX_000.INDEX_AllSelected:
                    startIndex = 0;
                    endIndex = pictureCount;
                    break;
                case INDEX_000.INDEX_Current:
                case INDEX_000.INDEX_First:
                    startIndex = (int)index;
                    endIndex = startIndex + 1;
                    break;
                case INDEX_000.INDEX_InsertPictureAtEnd:
                    throw new ArgumentException("Index not supported");
                default:
                    if (pictureCount >= (int)index)
                    {
                        throw new ArgumentException("Index out of range");
                    }
                    startIndex = (int)index;
                    endIndex = startIndex + 1;
                    break;
            }
            boundingWidth = 0;
            boundingHeight = 0;
            bufferByteCount = 0;
            for (int currentIndex = startIndex; currentIndex < endIndex; currentIndex++)
            {
                bool flag = true;
                if (index == INDEX_000.INDEX_AllSelected)
                {
                    S_OR_H_000 selectedHidden = S_OR_H_000.S_OR_H_NONE;
                    PakonLib.Interfaces.ISavePictures3 saveInterface = scanner.ISave3;
                    int pictureIndex = currentIndex;
                    string frameName = null;
                    string fileName = null;
                    string directory = null;
                    saveInterface.GetPictureInfo3(pictureIndex, out rollCount, out rollCount, out rollCount, out rollCount, out frameName, out rollCount, out rollCount, out fileName, out directory, out rollCount, out selectedHidden);
                    flag = selectedHidden == S_OR_H_000.S_OR_H_SELECTED;
                }
                if (flag)
                {
                    int left = 0;
                    int top = 0;
                    int right = 0;
                    int bottom = 0;
                    if ((saveControl & SAVE_CONTROL_000.SAV_UseLoResBuffer) == SAVE_CONTROL_000.SAV_UseLoResBuffer)
                    {
                        scanner.ISave.GetPictureFramingUserInfoLowRes(currentIndex, ref left, ref top, ref right, ref bottom);
                    }
                    else
                    {
                        scanner.ISave.GetPictureFramingUserInfo(currentIndex, ref left, ref top, ref right, ref bottom);
                    }
                    int width = right + 1 - left;
                    int height = bottom + 1 - top;
                    int bufferSize = Global.BufferSize(width, height, memoryFormat, fourChannel);
                    if (boundingWidth < width)
                    {
                        boundingWidth = width;
                    }
                    if (boundingHeight < height)
                    {
                        boundingHeight = height;
                    }
                    if (bufferByteCount < bufferSize)
                    {
                        bufferByteCount = bufferSize;
                    }
                }
            }
        }

        public void SaveToClientMemory(Scanner scanner, ScannerSettings scannerSettings, bool fourChannel)
        {
            int boundingWidth;
            int boundingHeight;
            int bufferByteCount;
            FindBoundingRectangle(scanner, out boundingWidth, out boundingHeight, out bufferByteCount, fourChannel);
            if (bufferByteCount == 0)
            {
                throw new ArgumentException("No pictures to save");
            }
            scanner.Unsafe.MemoryFormat = MemoryFormat;
            scanner.Unsafe.Allocate(bufferByteCount);
            scanner.Unsafe.NextBuffer(scanner);
            scanner.ISave.SaveToClientMemory(scannerSettings.Type, index, saveControl, boundingWidth, boundingHeight, scalingMethod, memoryFormat, fourChannel);
        }
    }

}

