#if !defined(AFX_COLORDLG_H__3C2D92EE_4F81_4b20_B971_7397D49B3FBA__INCLUDED_)
#define AFX_COLORDLG_H__3C2D92EE_4F81_4b20_B971_7397D49B3FBA__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// ColorDlg.h : header file
//

#include "stdafx.h"
#include "TLAClientDemoDlg.h"

/////////////////////////////////////////////////////////////////////////////
// CColorDlg dialog

class CColorDlg : public CDialog
{
// Construction
public:
	CColorDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CColorDlg)
	enum { IDD = IDD_COLOR_DIALOG };
	int		m_iContrast;
	int		m_iBrightness;
	int		m_iGreen;
	int		m_iBlue;
	int		m_iRed;
	int		m_iSharpness;
	//}}AFX_DATA

	int m_bDisplayFourPictures;
	int m_iIndex;

private:
	CTLAClientDemoDlg* m_pParent;

	int		m_iRedUndo;
	int		m_iGreenUndo;
	int		m_iBlueUndo;
	int		m_iBrightnessUndo;
	int		m_iContrastUndo;
	int		m_iSharpnessUndo;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CColorDlg)
	public:
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CColorDlg)
	virtual BOOL OnInitDialog();
	virtual void OnCancel();
	afx_msg void OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_COLORDLG_H__3C2D92EE_4F81_4b20_B971_7397D49B3FBA__INCLUDED_)
