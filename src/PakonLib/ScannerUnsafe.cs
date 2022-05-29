// Pakon.ScannerUnsafe
using System;
using System.Runtime.InteropServices;
using TLXLib;

namespace PakonLib 
{
    public class ScannerUnsafe
    {
        private FILE_FORMAT_SAVE_TO_MEMORY_000 m_eMemoryFormat;

        private bool m_bUsingBuffer1 = true;

        private int m_nBufferSize;

        private IntPtr m_ipClientBuffer1;

        private unsafe byte* m_pubClientBuffer1 = null;

        private int m_nClientBuffer1 = 0;

        private IntPtr m_ipClientBuffer2;

        private unsafe byte* m_pubClientBuffer2 = null;

        private int m_nClientBuffer2 = 0;

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

        public event ImageFromClientBuffer m_evImageFromClientBuffer;

        public unsafe void Allocate(int nBufferByteCount)
        {
            m_bUsingBuffer1 = true;
            m_nBufferSize = nBufferByteCount;
            m_ipClientBuffer1 = Marshal.AllocHGlobal(nBufferByteCount);
            m_pubClientBuffer1 = (byte*)(void*)m_ipClientBuffer1;
            m_nClientBuffer1 = m_ipClientBuffer1.ToInt32();
            m_ipClientBuffer2 = Marshal.AllocHGlobal(nBufferByteCount);
            m_pubClientBuffer2 = (byte*)(void*)m_ipClientBuffer2;
            m_nClientBuffer2 = m_ipClientBuffer2.ToInt32();
        }

        public unsafe void Deallocate()
        {
            if (m_pubClientBuffer1 != null)
            {
                Marshal.FreeHGlobal(m_ipClientBuffer1);
            }
            m_pubClientBuffer1 = null;
            m_nClientBuffer1 = 0;
            if (m_pubClientBuffer2 != null)
            {
                Marshal.FreeHGlobal(m_ipClientBuffer2);
            }
            m_pubClientBuffer2 = null;
            m_nClientBuffer2 = 0;
        }

        public unsafe void NextBuffer(Scanner csScanner)
        {
            if (m_pubClientBuffer1 == null)
            {
                return;
            }
            if (m_bUsingBuffer1)
            {
                byte* ptr = m_pubClientBuffer1;
                int num = 0;
                while (num < m_nBufferSize)
                {
                    *ptr = 0;
                    num++;
                    ptr++;
                }
                csScanner.ISave.ClientMemoryBufferAdd(m_nClientBuffer1, m_nBufferSize);
            }
            else
            {
                byte* ptr2 = m_pubClientBuffer2;
                int num2 = 0;
                while (num2 < m_nBufferSize)
                {
                    *ptr2 = 0;
                    num2++;
                    ptr2++;
                }
                csScanner.ISave.ClientMemoryBufferAdd(m_nClientBuffer2, m_nBufferSize);
            }
            m_bUsingBuffer1 = !m_bUsingBuffer1;
        }

        public unsafe void ProcessBuffer(Scanner csScanner)
        {
            if (m_pubClientBuffer1 != null)
            {
                byte* ptr = (m_bUsingBuffer1 ? m_pubClientBuffer2 : m_pubClientBuffer1);
                uint num = 0u;
                switch (m_eMemoryFormat)
                {
                    case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_16:
                        {
                            SiPlanarFileHeader* ptr2 = (SiPlanarFileHeader*)ptr;
                            num = ptr2->m_uiWidth * ptr2->m_uiHeight * (ptr2->m_uiBitCount / 8u) + (uint)sizeof(SiPlanarFileHeader);
                            break;
                        }
                    default:
                        throw new ArgumentException("Format not supported");
                    case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_8:
                    case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8:
                        break;
                }
                if (num == 0)
                {
                    throw new ArgumentException("Format not implemented");
                }
                byte[] array = new byte[num];
                byte* ptr3 = ptr;
                uint num2 = 0u;
                while (num2 < num)
                {
                    array[num2] = *ptr3;
                    num2++;
                    ptr3++;
                }
                if (this.m_evImageFromClientBuffer != null)
                {
                    this.m_evImageFromClientBuffer(array, num);
                }
            }
        }
    }

}

