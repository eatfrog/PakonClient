#if !defined(AFX_SAVEDLG_H__75BE3938_645B_4802_9216_3D1C9A49E128__INCLUDED_)
#define AFX_SAVEDLG_H__75BE3938_645B_4802_9216_3D1C9A49E128__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// SaveDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CSaveDlg dialog

class CSaveDlg : public CDialog
{
// Construction
public:
	CSaveDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CSaveDlg)
	enum { IDD = IDD_SAVE_DIALOG };
	CSpinButtonCtrl	m_SpinDpi;
	CSpinButtonCtrl	m_SpinCompression;
	int		m_iFileFormat;
	int		m_iDPI;
	int		m_iCompression;
	int		m_iRotateType;
	int		m_iRotation;
	BOOL	m_bSavUseLoRes;
	BOOL	m_bUseColorCorrection;
	int		m_iIndex;
	BOOL	m_bFastUpdates;
	BOOL	m_bTopDownDIB;
	int		m_iSaveToMemoryFormat;
	int		m_iSaveMethod;
	int		m_iPictureBoundaryHeight;
	int		m_iPictureBoundaryWidth;
	int		m_iSizeType;
	BOOL	m_bUseColorAdjustments;
	BOOL	m_bUseScratchRemoval;
	BOOL	m_bPlanarFileHeader;
	BOOL	m_bMirror;
	BOOL	m_bDibFileHeader;
	BOOL	m_bUseColorSceneBalance;
	BOOL	m_bUseWorkerThread;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSaveDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CSaveDlg)
	afx_msg void OnSelchangeFileFormatType();
	afx_msg void OnRotateCurrent();
	afx_msg void OnRotateNew();
	virtual BOOL OnInitDialog();
	afx_msg void On8bitDib();
	afx_msg void OnPlanar();
	afx_msg void OnSavMethodDisk();
	afx_msg void OnSavMethodMemoryClient();
	afx_msg void OnSavMethodMemoryShared();
	afx_msg void OnSavSizeDisplay();
	afx_msg void OnSavSizeSave();
	afx_msg void OnSavSizeOriginal();
	afx_msg void OnSavUseColorCorrection();
	afx_msg void OnSavFastUpdates();
	afx_msg void OnSavUseColorSceneBalance();
	afx_msg void OnIndexCurrent();
	afx_msg void OnIndexAll();
	afx_msg void OnIndexSelected();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	void EnableColorSettings(BOOL bEnable = TRUE);
	void SetColorSettings(BOOL bCheck = TRUE);
	void EnableRotateAmount(BOOL bEnable = TRUE);
	void EnableHeightAndWidth(BOOL bEnable = TRUE);
	void EnableCompressionAmount(BOOL bEnable = TRUE);
	void EnableDiskControls(BOOL bEnable = TRUE);
	void EnableMemoryControls(BOOL bEnable = TRUE);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SAVEDLG_H__75BE3938_645B_4802_9216_3D1C9A49E128__INCLUDED_)
