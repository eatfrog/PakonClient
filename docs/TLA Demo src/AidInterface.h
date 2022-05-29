// AidInterface.h: interface from TLA Client Demo to AID Toolkit
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_AIDINTERFACE_H__6D0665D5_A59A_404d_81C9_C3BBBFD237A5__INCLUDED_)
#define AFX_AIDINTERFACE_H__6D0665D5_A59A_404d_81C9_C3BBBFD237A5__INCLUDED_
/*
#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000
*/

#include "Constants.h"

#ifdef KODAK_AID

interface ISavePictures;

extern "C"
{
	BOOL bRunAid(ISavePictures *pISavePictures, HRESULT *phr, int iSavePictureCount);
};

#endif // KODAK_AID

#endif // !defined(AFX_AIDROLL_H__787ABC95_9E59_11D2_A4F0_006097DDDAE7__INCLUDED_)
