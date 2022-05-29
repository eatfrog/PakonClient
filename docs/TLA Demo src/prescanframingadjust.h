#if !defined(AFX_PRESCANFRAMINGADJUST_H__A082A79D_F350_4ff5_B3E3_B0DC3A50F8B8__INCLUDED_)
#define AFX_PRESCANFRAMINGADJUST_H__A082A79D_F350_4ff5_B3E3_B0DC3A50F8B8__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

// PreScanFramingAdjust.h : header file
//


/////////////////////////////////////////////////////////////////////////////
// CPreScanFramingAdjust dialog

class CPreScanFramingAdjust : public CDialog
{
// Construction
public:
	CPreScanFramingAdjust(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CPreScanFramingAdjust)
	enum { IDD = IDD_PRE_SCAN_DIALOG };
	int		m_iHeightHR;
	int		m_iHeightHR_mm;
	int		m_iWidthHR_Adj;
	int		m_iWidthUnitHR_Adj;
	//}}AFX_DATA
	CRect m_crCropHR_Adj;
	CRect m_crCropHR;				// Default cropping rectangle
	int m_iWidthHR;				// Width of default High Res frame
	int m_iWidthUnitHR;			// Unit Width of default High Res frame

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CPreScanFramingAdjust)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CPreScanFramingAdjust)
	virtual BOOL OnInitDialog();
	afx_msg void OnReset();
	virtual void OnOK();
	//}}AFX_MSG
	BOOL bCheckRect(CRect &rcrRect, CRect &rcrBoundary, CSize &rcsMinSize);
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_PRESCANFRAMINGADJUST_H__A082A79D_F350_4ff5_B3E3_B0DC3A50F8B8__INCLUDED_)
