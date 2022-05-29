#if !defined(AFX_FRAMEDLG_H__C265C17C_04CC_4624_AA1A_7255740F35C7__INCLUDED_)
#define AFX_FRAMEDLG_H__C265C17C_04CC_4624_AA1A_7255740F35C7__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// FrameDlg.h : header file
//
#include "Globals.h"
#include "Constants.h"

struct ISavePictures;
class CTLAClientDemoDlg;

/////////////////////////////////////////////////////////////////////////////
// CFrameDlg dialog

class CFrameDlg : public CDialog
{
// Construction
public:
	CFrameDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CFrameDlg)
	enum { IDD = IDD_FRAME_DIALOG };
	int		m_iFrameOrCrop;
	long	m_iBottom;
	long	m_iLeft;
	long	m_iRight;
	long	m_iTop;
	int		m_iFramingRisk;
	//}}AFX_DATA

	int		m_iIndex;
	CRect		m_crDefault;		// Rectangle the framing algorithm found
	CSize		m_csStrip;			// Dimensions of the buffer
	int		m_iFrameWidth;		// Dimensions for framing adjust
	PiFramingUserInfo* m_pFramingUserInfo;
	PiImage* m_pImage;
private:
	BOOL bValidData();
	BOOL bCheckRect(CRect &rcrRect, CRect &rcrBoundary, CSize &rcsMinSize);
	void ErrorMsg(UINT iEditControl, CString &strMsg);
	CTLAClientDemoDlg	*m_pParent;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CFrameDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CFrameDlg)
	afx_msg void OnFrameOpAdjust();
	afx_msg void OnFrameOpCrop();
	virtual BOOL OnInitDialog();
	afx_msg void OnApply();
	afx_msg void OnReset();
	afx_msg void OnChangeHrLeft();
	afx_msg void OnChangeHrBottom();
	afx_msg void OnChangeHrRight();
	afx_msg void OnChangeHrTop();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_FRAMEDLG_H__C265C17C_04CC_4624_AA1A_7255740F35C7__INCLUDED_)
