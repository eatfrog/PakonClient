// ClientBuffers.cpp: implementation of the CClientBuffers class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "ClientBuffers.h"
#include "globals.h"

#include SERVER_CONSTANTS
#include SERVER_GUIDS

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CClientBuffers::CClientBuffers()
{
	m_pClientBuffer1 = NULL;
	m_pClientBuffer2 = NULL;
	m_bUsingBuffer1 = TRUE;
	m_pISavePictures = NULL;
}

CClientBuffers::~CClientBuffers()
{

}

int CClientBuffers::iRequiredBufferSize(CSize& csSaveSize, int iSaveToMemoryFormat)
{
	int iBufferSize;
	if (iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_16 == iSaveToMemoryFormat)
	{
		HKEY hKey = HKEY_LOCAL_MACHINE;
		DWORD dwBuffer;
		DWORD dwDataSize = sizeof(DWORD);
		DWORD dwKeyType;
		BOOL bIrChannelSavedInPlanarFile = FALSE;
/*
		if(ERROR_SUCCESS == ::RegOpenKeyEx(hKey, _T("Software\\Pakon\\TLA\\Scan\\Simulator"), 0L, KEY_READ, &hKey))
		{																			// Open Key
			if(ERROR_SUCCESS == ::RegQueryValueEx(hKey, _T("IrChannelSavedInPlanarFile"), NULL, &dwKeyType, (BYTE*)(&dwBuffer), &dwDataSize))
			{																		// Get the data.
				if(REG_DWORD == dwKeyType)									// verify data type
					bIrChannelSavedInPlanarFile = dwBuffer;
			}

			::RegCloseKey(hKey);
		}
*/
		if(ERROR_SUCCESS == ::RegOpenKeyEx(hKey, _T("Software\\Pakon\\TLA\\Scan\\Test"), 0L, KEY_READ, &hKey))
		{																			// Open Key
			if(ERROR_SUCCESS == ::RegQueryValueEx(hKey, _T("IrChannelSavedInPlanarFile"), NULL, &dwKeyType, (BYTE*)(&dwBuffer), &dwDataSize))
			{																		// Get the data.
				if(REG_DWORD == dwKeyType)									// verify data type
					bIrChannelSavedInPlanarFile = dwBuffer;
			}

			::RegCloseKey(hKey);
		}

		if(bIrChannelSavedInPlanarFile)
			iBufferSize = (csSaveSize.cx) * (csSaveSize.cy) * sizeof(WORD) * 4/*channels*/;
		else
			iBufferSize = (csSaveSize.cx) * (csSaveSize.cy) * sizeof(WORD) * 3/*channels*/;

		iBufferSize += sizeof(SiPlanarFileHeader) + 64;
	}
	else // if (iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8 == iSaveToMemoryFormat)
	{
		// Dib line may contain up to 3 byte pad
		// assume narrowest dimension is the line (largest amount of pad)
		if (csSaveSize.cx > csSaveSize.cy)
		{
			iBufferSize = csSaveSize.cy + 3;
			iBufferSize *= csSaveSize.cx * 3; // 3 channels
		}
		else
		{
			iBufferSize = csSaveSize.cx + 3;
			iBufferSize *= csSaveSize.cy * 3; // 3 channels
		}

		iBufferSize += sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + 16;
	}
	return iBufferSize;
}

BOOL CClientBuffers::bAllocateBuffers(CSize csSaveSize, int iSaveToMemoryFormat, BOOL bMultiplePictures)
{
	HRESULT hr;

	// make sure somethings not screwed up
	ASSERT(m_pClientBuffer1 == NULL);
	ASSERT(m_pClientBuffer2 == NULL);
	ASSERT(m_pISavePictures != NULL);

	m_bMultiplePictures = bMultiplePictures;	

	// use our member variable here as a return also
	m_bUsingBuffer1 = FALSE;

	m_iBufferSize = iRequiredBufferSize(csSaveSize, iSaveToMemoryFormat);

	// allocate buffer one and add to server
	if(m_pClientBuffer1 = new UCHAR[m_iBufferSize])
	{
		hr = m_pISavePictures->ClientMemoryBufferAdd((int)m_pClientBuffer1, m_iBufferSize);
		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			Delete();
		}
		else
		{
//			TRACE2("CClientBuffers:  Buffer1 Added to Server %x, %d\n", m_pClientBuffer1, m_iBufferSize);
			// set the success/active buffer flag
			m_bUsingBuffer1 = TRUE;
		}
	}

	// if two buffers are required
	if (m_bMultiplePictures)
	{
		// reset our allocation success flag
		m_bUsingBuffer1 = FALSE;
		// allocate buffer two and add to server
		if(m_pClientBuffer2 = new UCHAR[m_iBufferSize])
		{
			hr = m_pISavePictures->ClientMemoryBufferAdd((int)m_pClientBuffer2, m_iBufferSize);
			if(FAILED(hr))
			{
				::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
				Delete();
			}
			else
			{
//				TRACE2("CClientBuffers:  Buffer2 Added to Server %x, %d\n", m_pClientBuffer2, m_iBufferSize);
				// set the success/active buffer flag
				m_bUsingBuffer1 = TRUE;
			}
		}
	}
	// return success/fail status
	return m_bUsingBuffer1;
}

void CClientBuffers::Delete()
{
	::Sleep(100);

	if (m_pISavePictures)
	{
		HRESULT hr = m_pISavePictures->ClientMemoryBufferDismissAll();
		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
	/*		int iErrorNumber;
			if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
			{
				::Sleep(100);
				hr = m_pISavePictures->ClientMemoryBufferDismissAll();
				if(FAILED(hr))
					::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			}
			else
				::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
	*/
		}
	}

//	TRACE0("CClientBuffers:  All Buffers Released from Server\n");

	if(NULL != m_pClientBuffer1)
	{
		delete [] m_pClientBuffer1;
		m_pClientBuffer1 = NULL;
	}

	if(NULL != m_pClientBuffer2)
	{
		delete [] m_pClientBuffer2;
		m_pClientBuffer2 = NULL;
	}
}

UCHAR* CClientBuffers::GetActiveBuffer()
{
	if(m_bUsingBuffer1)
	{
//		TRACE1("CClientBuffers:  returning Buffer1 %x\n", m_pClientBuffer1);
		return m_pClientBuffer1;
	}
	else
	{
//		TRACE1("CClientBuffers:  returning Buffer2 %x\n", m_pClientBuffer2);
		return m_pClientBuffer2;
	}
}

#ifdef _DEBUG
void CClientBuffers::Init(ISavePictures *pISavePictures)
{
	m_pISavePictures = pISavePictures;
}

int CClientBuffers::GetBufferSize()
{
	return (m_iBufferSize);
}
#endif

BOOL CClientBuffers::bNextBuffer()
{
	ASSERT(m_pISavePictures);

	if (m_bMultiplePictures)
	{
		HRESULT hr;
		if (m_bUsingBuffer1)
		{
			hr = m_pISavePictures->ClientMemoryBufferAdd((int)m_pClientBuffer1, m_iBufferSize);
//			TRACE2("CClientBuffers:  Buffer1 Re-Added to Server %x, %d\n", m_pClientBuffer1, m_iBufferSize);
		}
		else
		{
			hr = m_pISavePictures->ClientMemoryBufferAdd((int)m_pClientBuffer2, m_iBufferSize);
//			TRACE2("CClientBuffers:  Buffer2 Re-Added to Server %x, %d\n", m_pClientBuffer2, m_iBufferSize);
		}
		if(FAILED(hr))
		{
			// FIXME - What do we do now?
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			ASSERT(0);
			return FALSE;
		}
		m_bUsingBuffer1 = !m_bUsingBuffer1;
	}
	return TRUE;
}
