// ClientBuffers.h: interface for the CClientBuffers class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_CLIENTBUFFERS_H__CB3FD0F0_F57E_42CB_8984_93E3E31E652A__INCLUDED_)
#define AFX_CLIENTBUFFERS_H__CB3FD0F0_F57E_42CB_8984_93E3E31E652A__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "constants.h"

interface ISavePictures;

class CClientBuffers : public CObject  
{
public:
	BOOL bNextBuffer();
	int GetBufferSize();
	UCHAR* GetActiveBuffer();
	void Delete();
	BOOL bAllocateBuffers(CSize csSaveSize, int iSaveToMemoryFormat, BOOL bMultiplePictures = TRUE);
	CClientBuffers();
	virtual ~CClientBuffers();
	void Init(ISavePictures *pISavePictures);

private:
	BOOL m_bUsingBuffer1;
	UCHAR *m_pClientBuffer1;
	UCHAR *m_pClientBuffer2;
	ISavePictures *m_pISavePictures;
	int m_iBufferSize;
	BOOL m_bMultiplePictures;
	int iRequiredBufferSize(CSize& csSaveSize, int iSaveToMemoryFormat);
};

#ifndef _DEBUG

inline void CClientBuffers::Init(ISavePictures *pISavePictures)
{
	m_pISavePictures = pISavePictures;
}

inline int CClientBuffers::GetBufferSize()
{
	return (m_iBufferSize);
}

#endif

#endif // !defined(AFX_CLIENTBUFFERS_H__CB3FD0F0_F57E_42CB_8984_93E3E31E652A__INCLUDED_)
