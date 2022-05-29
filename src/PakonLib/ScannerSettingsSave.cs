using System;
using PakonLib;
using TLXLib;

namespace PakonLib
{
    public class ScannerSettingsSave
    {
        private INDEX_000 m_eIndex;

        private SAVE_CONTROL_000 m_eSaveControl;

        private SCALING_METHOD_000 m_eScalingMethod;

        private FILE_FORMAT_SAVE_TO_MEMORY_000 m_eMemoryFormat;

        public INDEX_000 Index
        {
            get
            {
                return m_eIndex;
            }
            set
            {
                m_eIndex = value;
            }
        }

        public SAVE_CONTROL_000 Control
        {
            get
            {
                return m_eSaveControl;
            }
            set
            {
                m_eSaveControl = value;
            }
        }

        public SCALING_METHOD_000 ScalingMethod
        {
            get
            {
                return m_eScalingMethod;
            }
            set
            {
                m_eScalingMethod = value;
            }
        }

        public FILE_FORMAT_SAVE_TO_MEMORY_000 MemoryFormat
        {
            get
            {
                return m_eMemoryFormat;
            }
            set
            {
                m_eMemoryFormat = value;
            }
        }

        public event AddPictureToSaveGroup m_evAddPictureToSaveGroup;

        public void MoveRollToSaveGroup(Scanner csScanner)
        {
            int iRollCount = 0;
            int iPictureCount = 0;
            int iPictureCount2 = 0;
            csScanner.ISave.GetPictureCountSaveGroup(ref iRollCount, ref iRollCount, ref iPictureCount, ref iRollCount, ref iRollCount);
            if (iPictureCount > 0)
            {
                csScanner.ISave.PutPictureSelection(INDEX_000.INDEX_All, S_OR_H_000.S_OR_H_NONE, true);
            }
            csScanner.ISave.GetPictureCountScanGroup(0, ref iRollCount, ref iPictureCount2, ref iRollCount);
            csScanner.ISave.MoveOldestRollToSaveGroup();
            for (int i = iPictureCount; i < iPictureCount + iPictureCount2; i++)
            {
                this.m_evAddPictureToSaveGroup(i);
            }
        }

        public void SetPictureInfo(Scanner csScanner, int iIndex, int iNewFrameNumber, string strNewFileName, string strNewDirectory, int iNewRotation, S_OR_H_000 eNewSelectedHidden, IntBits ibInfo)
        {
            int iRollIndexFromStrip;
            int iStripIndexFromStrip;
            int iFilmProductFromStrip;
            int iFilmSpecifierFromStrip;
            string strFrameName;
            int iFrameNumber;
            int iPrintAspectRatio;
            string strFileName;
            string strDirectory;
            int iRotation;
            S_OR_H_000 eiSelectedHidden;
            csScanner.ISave3.GetPictureInfo3(iIndex, out iRollIndexFromStrip, out iStripIndexFromStrip, out iFilmProductFromStrip, out iFilmSpecifierFromStrip, out strFrameName, out iFrameNumber, out iPrintAspectRatio, out strFileName, out strDirectory, out iRotation, out eiSelectedHidden);
            if (ibInfo[0])
            {
                iFrameNumber = iNewFrameNumber;
            }
            if (ibInfo[1])
            {
                strFileName = strNewFileName;
            }
            if (ibInfo[2])
            {
                strDirectory = strNewDirectory;
            }
            if (ibInfo[3])
            {
                iRotation = iNewRotation;
            }
            if (ibInfo[4])
            {
                eiSelectedHidden = eNewSelectedHidden;
            }
            csScanner.ISave.PutPictureInfo(iIndex, iFrameNumber, strFileName, strDirectory, iRotation, eiSelectedHidden);
        }

        private void FindBoundingRectangle(Scanner csScanner, out int iBoundingWidth, out int iBoundingHeight, out int iBufferByteCount, bool bFourChannel)
        {
            int iRollCount = 0;
            int iPictureCount = 0;
            csScanner.ISave.GetPictureCountSaveGroup(ref iRollCount, ref iRollCount, ref iPictureCount, ref iRollCount, ref iRollCount);
            int i;
            int num;
            switch (m_eIndex)
            {
                case INDEX_000.INDEX_All:
                case INDEX_000.INDEX_AllSelected:
                    i = 0;
                    num = iPictureCount;
                    break;
                case INDEX_000.INDEX_Current:
                case INDEX_000.INDEX_First:
                    i = (int)m_eIndex;
                    num = i + 1;
                    break;
                case INDEX_000.INDEX_InsertPictureAtEnd:
                    throw new ArgumentException("Index not supported");
                default:
                    if (iPictureCount >= (int)m_eIndex)
                    {
                        throw new ArgumentException("Index out of range");
                    }
                    i = (int)m_eIndex;
                    num = i + 1;
                    break;
            }
            iBoundingWidth = 0;
            iBoundingHeight = 0;
            iBufferByteCount = 0;
            for (; i < num; i++)
            {
                bool flag = true;
                if (m_eIndex == INDEX_000.INDEX_AllSelected)
                {
                    S_OR_H_000 eiSelectedHidden = S_OR_H_000.S_OR_H_NONE;
                    PakonLib.Interfaces.ISavePictures3 iSave = csScanner.ISave3;
                    int iIndex = i;
                    string strFrameName = null;
                    string strFileName = null;
                    string strDirectory = null;
                    iSave.GetPictureInfo3(iIndex, out iRollCount, out iRollCount, out iRollCount, out iRollCount, out strFrameName, out iRollCount, out iRollCount, out strFileName, out strDirectory, out iRollCount, out eiSelectedHidden);
                    flag = eiSelectedHidden == S_OR_H_000.S_OR_H_SELECTED;
                }
                if (flag)
                {
                    int iLeftLR = 0;
                    int iTopLR = 0;
                    int iRightLR = 0;
                    int iBottomLR = 0;
                    if ((m_eSaveControl & SAVE_CONTROL_000.SAV_UseLoResBuffer) == SAVE_CONTROL_000.SAV_UseLoResBuffer)
                    {
                        csScanner.ISave.GetPictureFramingUserInfoLowRes(i, ref iLeftLR, ref iTopLR, ref iRightLR, ref iBottomLR);
                    }
                    else
                    {
                        csScanner.ISave.GetPictureFramingUserInfo(i, ref iLeftLR, ref iTopLR, ref iRightLR, ref iBottomLR);
                    }
                    int num2 = iRightLR + 1 - iLeftLR;
                    int num3 = iBottomLR + 1 - iTopLR;
                    int num4 = Global.BufferSize(num2, num3, m_eMemoryFormat, bFourChannel);
                    if (iBoundingWidth < num2)
                    {
                        iBoundingWidth = num2;
                    }
                    if (iBoundingHeight < num3)
                    {
                        iBoundingHeight = num3;
                    }
                    if (iBufferByteCount < num4)
                    {
                        iBufferByteCount = num4;
                    }
                }
            }
        }

        public void SaveToClientMemory(Scanner csScanner, ScannerSettings csScannerSettings, bool bFourChannel)
        {
            int iBoundingWidth;
            int iBoundingHeight;
            int iBufferByteCount;
            FindBoundingRectangle(csScanner, out iBoundingWidth, out iBoundingHeight, out iBufferByteCount, bFourChannel);
            if (iBufferByteCount == 0)
            {
                throw new ArgumentException("No pictures to save");
            }
            csScanner.Unsafe.MemoryFormat = MemoryFormat;
            csScanner.Unsafe.Allocate(iBufferByteCount);
            csScanner.Unsafe.NextBuffer(csScanner);
            csScanner.ISave.SaveToClientMemory(csScannerSettings.Type, m_eIndex, m_eSaveControl, iBoundingWidth, iBoundingHeight, m_eScalingMethod, m_eMemoryFormat, bFourChannel);
        }
    }

}

