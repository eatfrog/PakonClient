#if !defined(AFX_SCANDLG_H__C22CC275_1CA6_4e91_B29C_7460BA893E68__INCLUDED_)
#define AFX_SCANDLG_H__C22CC275_1CA6_4e91_B29C_7460BA893E68__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// ScanDlg.h : header file
//

enum
{
	iSCAN_OP_CORRECTION_GAIN,
	iSCAN_OP_CORRECTION_FP,
	iSCAN_OP_CORRECTION_FOCUS,
	iSCAN_OP_CORRECTION_FOCUS_ADVANCE_FILM,
	iSCAN_OP_DO_FILM_TRACK_TEST,
	iSCAN_OP_SET_FILM_TRACK,
	iSCAN_OP_IMPORT,
	iSCAN_OP_RESET_MOTOR_SPEED_ADJUST,
	iSCAN_OP_PRE_SCAN,
	iSCAN_OP_SCAN,
};

/////////////////////////////////////////////////////////////////////////////
// CScanDlg dialog

class CScanDlg : public CDialog
{
// Construction
public:
	BOOL m_bF235C;
	int m_iOperation;
	CScanDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CScanDlg)
	enum { IDD = IDD_SCAN_DIALOG };
	int		m_iResolution;
	int		m_iFilmFormat;
	int		m_iFilmColor;
	int		m_iFramesPerStrip;
	BOOL	m_bReelToReelMode;
	BOOL	m_bRead_DX;
	BOOL	m_bScratchRemoval;
	BOOL	m_bAggressiveFraming;
	BOOL	m_bLampWarmUp;
	BOOL	m_bExerciseSteppers;
	BOOL	m_bHasFilmDrag;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CScanDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CScanDlg)
	afx_msg void OnGainCorrections();
	afx_msg void OnFpCorrections();
	afx_msg void OnSetFilmTrack();
	afx_msg void OnCorrectionsFocus();
	afx_msg void OnPreScan();
	afx_msg void OnScan();
	afx_msg void OnCorrectionsFocusAdvanceFilm();
	afx_msg void OnDoFilmTrackTest();
	afx_msg void OnImport();
	virtual BOOL OnInitDialog();
	afx_msg void OnResetMotorSpeedAdjust();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SCANDLG_H__C22CC275_1CA6_4e91_B29C_7460BA893E68__INCLUDED_)
