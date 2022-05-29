// Utility.cpp: implementation of the CiElapsedTime class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "Utility.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CiElapsedTime::CiElapsedTime()
{
	m_uiStartTime = 0L;
}

CiElapsedTime::~CiElapsedTime()
{

}

void CiElapsedTime::Start()
{
	m_uiStartTime = ::GetTickCount();
}

UINT CiElapsedTime::uiGetElapsedMilliseconds(BOOL bStop)
{
	UINT uiEndTime = GetTickCount();
	if(m_uiStartTime == 0L)
		return 0L;

	UINT uiRetVal;
	if(uiEndTime > m_uiStartTime)
		uiRetVal = uiEndTime - m_uiStartTime;
	else
		uiRetVal = (UINT_MAX - uiEndTime) + m_uiStartTime;

	if(bStop)
		m_uiStartTime = 0L;

	return uiRetVal;
}
