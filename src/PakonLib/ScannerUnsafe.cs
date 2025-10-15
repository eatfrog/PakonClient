// Pakon.ScannerUnsafe
using System;
using System.Runtime.InteropServices;
using TLXLib;

namespace PakonLib 
{
    public class ScannerUnsafe
    {
        private FILE_FORMAT_SAVE_TO_MEMORY_000 memoryFormat;

        private bool usingFirstBuffer = true;

        private int bufferSize;

        private IntPtr clientBufferPointer1;

        private unsafe byte* clientBuffer1 = null;

        private int clientBufferAddress1 = 0;

        private IntPtr clientBufferPointer2;

        private unsafe byte* clientBuffer2 = null;

        private int clientBufferAddress2 = 0;

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

        public event ImageFromClientBuffer ImageFromClientBufferReceived;

        public unsafe void Allocate(int bufferByteCount)
        {
            usingFirstBuffer = true;
            bufferSize = bufferByteCount;
            clientBufferPointer1 = Marshal.AllocHGlobal(bufferByteCount);
            clientBuffer1 = (byte*)(void*)clientBufferPointer1;
            clientBufferAddress1 = clientBufferPointer1.ToInt32();
            clientBufferPointer2 = Marshal.AllocHGlobal(bufferByteCount);
            clientBuffer2 = (byte*)(void*)clientBufferPointer2;
            clientBufferAddress2 = clientBufferPointer2.ToInt32();
        }

        public unsafe void Deallocate()
        {
            if (clientBuffer1 != null)
            {
                Marshal.FreeHGlobal(clientBufferPointer1);
            }
            clientBuffer1 = null;
            clientBufferAddress1 = 0;
            if (clientBuffer2 != null)
            {
                Marshal.FreeHGlobal(clientBufferPointer2);
            }
            clientBuffer2 = null;
            clientBufferAddress2 = 0;
        }

        public unsafe void NextBuffer(Scanner scanner)
        {
            if (clientBuffer1 == null)
            {
                return;
            }
            if (usingFirstBuffer)
            {
                byte* buffer = clientBuffer1;
                int index = 0;
                while (index < bufferSize)
                {
                    *buffer = 0;
                    index++;
                    buffer++;
                }
                scanner.ISave.ClientMemoryBufferAdd(clientBufferAddress1, bufferSize);
            }
            else
            {
                byte* buffer = clientBuffer2;
                int index = 0;
                while (index < bufferSize)
                {
                    *buffer = 0;
                    index++;
                    buffer++;
                }
                scanner.ISave.ClientMemoryBufferAdd(clientBufferAddress2, bufferSize);
            }
            usingFirstBuffer = !usingFirstBuffer;
        }

        public unsafe void ProcessBuffer(Scanner scanner)
        {
            if (clientBuffer1 != null)
            {
                byte* buffer = (usingFirstBuffer ? clientBuffer2 : clientBuffer1);
                uint bytesToCopy = 0u;
                switch (memoryFormat)
                {
                    case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_16:
                        {
                            SiPlanarFileHeader* header = (SiPlanarFileHeader*)buffer;
                            bytesToCopy = header->Width * header->Height * (header->BitCount / 8u) + (uint)sizeof(SiPlanarFileHeader);
                            break;
                        }
                    default:
                        throw new ArgumentException("Format not supported");
                    case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_8:
                    case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8:
                        break;
                }
                if (bytesToCopy == 0)
                {
                    throw new ArgumentException("Format not implemented");
                }
                byte[] data = new byte[bytesToCopy];
                byte* source = buffer;
                uint index = 0u;
                while (index < bytesToCopy)
                {
                    data[index] = *source;
                    index++;
                    source++;
                }
                if (ImageFromClientBufferReceived != null)
                {
                    ImageFromClientBufferReceived(data, bytesToCopy);
                }
            }
        }
    }

}

