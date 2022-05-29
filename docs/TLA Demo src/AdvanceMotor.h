#if !defined(AFX_ADVANCEMOTOR_H__D8ED293B_BA15_486A_863E_E65801A35E0B__INCLUDED_)
#define AFX_ADVANCEMOTOR_H__D8ED293B_BA15_486A_863E_E65801A35E0B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// AdvanceMotor.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CAdvanceMotor dialog

class CAdvanceMotor : public CDialog
{
// Construction
public:
	CAdvanceMotor(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CAdvanceMotor)
	enum { IDD = IDD_ADVANCE_MOTOR };
	CSpinButtonCtrl	m_spinSpeed;
	int		m_iMilliseconds;
	int		m_iSpeed;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAdvanceMotor)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CAdvanceMotor)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_ADVANCEMOTOR_H__D8ED293B_BA15_486A_863E_E65801A35E0B__INCLUDED_)
