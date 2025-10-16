using System;
using TLXLib;
using PakonLib.Models;

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
            PictureCountSaveGroupResult saveGroupCounts = scanner.ISave.GetPictureCountSaveGroup();
            if (saveGroupCounts.PictureCount > 0)
            {
                scanner.ISave.PutPictureSelection(INDEX_000.INDEX_All, S_OR_H_000.S_OR_H_NONE, true);
            }
            PictureCountScanGroupResult scanGroupCounts = scanner.ISave.GetPictureCountScanGroup(0);
            scanner.ISave.MoveOldestRollToSaveGroup();
            for (int i = saveGroupCounts.PictureCount; i < saveGroupCounts.PictureCount + scanGroupCounts.PictureCount; i++)
            {
                PictureAddedToSaveGroup?.Invoke(i);
            }
        }

        public void SetPictureInfo(Scanner scanner, int indexValue, int newFrameNumber, string newFileName, string newDirectory, int newRotation, S_OR_H_000 newSelectedHidden, IntBits info)
        {
            PictureInfo pictureInfo = scanner.ISave3.GetPictureInfo3(indexValue);
            int frameNumber = pictureInfo.FrameNumber;
            string fileName = pictureInfo.FileName;
            string directory = pictureInfo.Directory;
            int rotation = pictureInfo.Rotation;
            S_OR_H_000 selectedHidden = pictureInfo.SelectedHidden;
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

        private BoundingRectangleMetrics FindBoundingRectangle(Scanner scanner, bool fourChannel)
        {
            PictureCountSaveGroupResult saveGroupCounts = scanner.ISave.GetPictureCountSaveGroup();
            int pictureCount = saveGroupCounts.PictureCount;
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
            int boundingWidth = 0;
            int boundingHeight = 0;
            int bufferByteCount = 0;
            for (int currentIndex = startIndex; currentIndex < endIndex; currentIndex++)
            {
                bool flag = true;
                if (index == INDEX_000.INDEX_AllSelected)
                {
                    PakonLib.Interfaces.ISavePictures3 saveInterface = scanner.ISave3;
                    PictureInfo pictureInfo = saveInterface.GetPictureInfo3(currentIndex);
                    flag = pictureInfo.SelectedHidden == S_OR_H_000.S_OR_H_SELECTED;
                }
                if (flag)
                {
                    PictureFramingInfo framingInfo = ((saveControl & SAVE_CONTROL_000.SAV_UseLoResBuffer) == SAVE_CONTROL_000.SAV_UseLoResBuffer)
                        ? scanner.ISave.GetPictureFramingUserInfoLowRes(currentIndex)
                        : scanner.ISave.GetPictureFramingUserInfo(currentIndex);
                    int width = framingInfo.Right + 1 - framingInfo.Left;
                    int height = framingInfo.Bottom + 1 - framingInfo.Top;
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
            return new BoundingRectangleMetrics(boundingWidth, boundingHeight, bufferByteCount);
        }

        public void SaveToClientMemory(Scanner scanner, ScannerSettings scannerSettings, bool fourChannel)
        {
            BoundingRectangleMetrics boundingMetrics = FindBoundingRectangle(scanner, fourChannel);
            if (boundingMetrics.BufferByteCount == 0)
            {
                throw new ArgumentException("No pictures to save");
            }
            scanner.Unsafe.MemoryFormat = MemoryFormat;
            scanner.Unsafe.Allocate(boundingMetrics.BufferByteCount);
            scanner.Unsafe.NextBuffer(scanner);
            scanner.ISave.SaveToClientMemory(scannerSettings.Type, index, saveControl, boundingMetrics.Width, boundingMetrics.Height, scalingMethod, memoryFormat, fourChannel);
        }
    }

}

