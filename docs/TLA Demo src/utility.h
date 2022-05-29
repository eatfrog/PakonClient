// Utility.h: interface for the CiElapsedTime class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_UTILITY_H__2ACD21EE_6DD1_4bcd_98E2_43A3F5537CA1__INCLUDED_)
#define AFX_UTILITY_H__2ACD21EE_6DD1_4bcd_98E2_43A3F5537CA1__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class CiElapsedTime  
{
public:
	CiElapsedTime();
	virtual ~CiElapsedTime();

	void Start();
	UINT uiGetElapsedMilliseconds(BOOL bStop);

private:
	UINT m_uiStartTime;
};

#endif // !defined(AFX_UTILITY_H__2ACD21EE_6DD1_4bcd_98E2_43A3F5537CA1__INCLUDED_)
