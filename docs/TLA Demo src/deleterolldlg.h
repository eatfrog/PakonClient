#if !defined(AFX_DELETEROLLDLG_H__979B5460_17A1_4127_B9ED_81CCF90262B8__INCLUDED_)
#define AFX_DELETEROLLDLG_H__979B5460_17A1_4127_B9ED_81CCF90262B8__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// deleterolldlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CDeleteRollDlg dialog

class CDeleteRollDlg : public CDialog
{
// Construction
public:
	int m_iRollCount;
	CDeleteRollDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CDeleteRollDlg)
	enum { IDD = IDD_DELETE_ROLL };
	int		m_iRollIndex;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDeleteRollDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CDeleteRollDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_DELETEROLLDLG_H__979B5460_17A1_4127_B9ED_81CCF90262B8__INCLUDED_)
