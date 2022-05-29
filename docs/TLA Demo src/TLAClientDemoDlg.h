// TLAClientDemoDlg.h : header file
//

#if !defined(AFX_TLACLIENTDEMODLG_H__5C7470A5_3C05_449C_816F_173D0FB885DF__INCLUDED_)
#define AFX_TLACLIENTDEMODLG_H__5C7470A5_3C05_449C_816F_173D0FB885DF__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "stdafx.h"
#include "globals.h"

#include "ClientBuffers.h"
#include <afxtempl.h>

interface ILongOpsCB;
interface ITLAMain;
interface IScanPictures;
interface ISavePictures;

/////////////////////////////////////////////////////////////////////////////
// CTLAClientDemoDlg dialog

class CTLAClientDemoDlg : public CDialog
{
// Construction
public:
	void AdjustColor(int iRed, int iGreen, int iBlue, int iBright, int iContrast, int iSharpness);
	void UpdatePrevNext();
	CTLAClientDemoDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CTLAClientDemoDlg)
	enum { IDD = IDD_TLACLIENTDEMO_DIALOG };
	CProgressCtrl	m_ProgressScan;
	CProgressCtrl	m_ProgressSave;
	int		m_iSavePictureCount;
	int		m_iIndex;
	//}}AFX_DATA

	// store a pointer to the server COM ojbect
	ILongOpsCB *m_pILongOpsCB;

	// cookie for identifying ourselves to server
	long m_lCookie;

	// our interfaces to the scanner object
	ITLAMain *m_pITLAMain;
	IScanPictures *m_pIScanPictures;
	ISavePictures *m_pISavePictures;

	IStream *m_pIStream;	// MARSHALLING
	BOOL bComMarshallInterface(BOOL bScanPictures);// MARSHALLING

	int m_iResolution;
	int m_iFilmFormat;
	int m_iFilmColor;
	int m_iFramesPerStrip;
	BOOL m_bReelToReelMode;
	BOOL m_bRead_DX;
	BOOL m_bScratchRemoval;
	BOOL m_bHasFilmDrag;
	BOOL m_bAggressiveFraming;
	BOOL m_bLampWarmUp;
	BOOL m_bExerciseSteppers;
	BOOL m_bLoadFirmwareAtStartup;
	BOOL m_bMotorSelfTestAtStartup;
	UINT m_uiScanQuantity;
	
	// Used for SaveToMemory
	int m_iFileFormat;
	int m_iDPI;
	int m_iCompression;
	int m_iRotateType;
	int m_iRotation;
	BOOL m_bSavUseLoRes;
	BOOL m_bUseColorCorrection;
	UINT m_uiUseKcdfs;
	int m_iSaveDlgIndex;
	BOOL m_bFastUpdates;
	BOOL m_bTopDownDIB;
	int m_iSaveToMemoryFormat;
	int m_iSaveMethod;
	int m_iPictureBoundaryHeight;
	int m_iPictureBoundaryWidth;
	int m_iSizeType;
	BOOL m_bUseColorAdjustments;
	BOOL m_bUseScratchRemoval;
	BOOL m_bPlanarFileHeader;
	BOOL m_bMirror;
	BOOL m_bDibFileHeader;
	BOOL m_bUseColorSceneBalance;

	// Used for SaveToSharedMemory
	UCHAR *m_puchFileMapData;
	HANDLE m_hFileMapHandle;
	HANDLE m_hFileMapEventHandle;
	CList<SiSaveToMemoryDirAndFile, SiSaveToMemoryDirAndFile&> m_clSaveToSharedMemoryDirAndFile;

	// Used for SaveToClientMemory
	CClientBuffers m_ClientBuffers;
	CList<SiSaveToMemoryDirAndFile, SiSaveToMemoryDirAndFile&> m_clSaveToClientMemoryDirAndFile;

	// Scanner Attributes
	int m_iScannerType;    
	int m_iScannerSerialNumber;
	int m_iScannerVersionHardware;
	int m_iColorPortraitMode;
	int m_i_GetScanLineTimeOut;
	int m_iNoFilmTimeOut;
	int m_iLampSaverSec;
	int m_iSpliceDarkness;
	int m_iDarkPointCorrectInterval;

	// Advance film parameters
	int m_iAdvanceFilmMilliseconds;	// -1 if forever, positive for number of milliseconds
	int m_AdvanceFilmSpeed;				// -1016 to -111 Reverse, 111 to 1016 Forward
	
	// number of pictures in the scanner's scan group
	int m_iScanStripCount;
	int m_iScanPictureCount;
	int m_iScanWarnings;
	int m_iScanRollCount;

	// number of pictures in the scanner's save group
	int m_iSaveRollCount;
	int m_iSaveStripCount;
	int m_iSavePictureSelectedCount;
	int m_iSavePictureHiddenCount;

	// Picture Info
	int m_iCurrentRollIndex;

	CSize		m_csStripHR;
	CSize		m_csStripLR;

	CTypedPtrArray<CPtrArray, PiPicture*> m_Pictures;

	BOOL m_bScannerInitializationCompleted;

	UINT m_uiScanQuantityCounter;

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTLAClientDemoDlg)
	public:
	virtual BOOL DestroyWindow();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	// COM callback handlers
	void UpdateSaveProgress(LONG lStatus);
	void UpdateDisplayNewProgress(LONG lStatus);
	void UpdateDisplayEditProgress(LONG lStatus);
	void UpdateExportProgress(LONG lStatus);
	void UpdateScanProgress(LONG lStatus);
	void UpdateImportProgress(LONG lStatus);
	void UpdateVariousScanProgresses(UINT ulOperation, LONG lStatus);
	void UpdateCorrectionsProgress(UINT ulOperation, LONG lStatus);
	void UpdateInitializeProgress(UINT ulOperation, LONG lStatus);
	void HandleWTOError(int iShort_IID, int iOperationID, LONG lStatus);

	// COM callback handler helpers
	void SaveFileUsingSaveToSharedMemory();
	void Save8BitDibUsingSaveToClientMemory();
	void SavePlanarUsingSaveToClientMemory();
	void CompleteInitialization();
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTLAClientDemoDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnScan();
	afx_msg void OnScanCancel();
	afx_msg void OnSave();
	afx_msg void OnPrevious();
	afx_msg void OnNext();
	afx_msg void OnSaveCancel();
	afx_msg void OnApply();
	afx_msg void OnFraming();
	afx_msg void OnPictureModeOne();
	afx_msg void OnPictureModeFour();
	afx_msg void OnColor();
	afx_msg void OnAdvanceFilm();
	afx_msg void OnDeletePicture();
	afx_msg void OnInsertPicture();
	afx_msg void OnHelpAbout();
	afx_msg void OnReleaseSaveGroup();
	afx_msg void OnMoveToSaveGroup();
	afx_msg void OnRotate();
	afx_msg void OnConnect();
	afx_msg void OnRunAid();
	afx_msg void OnDeleteRoll();
	afx_msg void OnControlLampOff();
	afx_msg void OnControlLampDim();
	afx_msg void OnControlLampOn();
	afx_msg void OnControlRollersRelease();
	afx_msg void OnControlApsManualRetract();
	afx_msg void OnUpdateControlApsManualRetract(CCmdUI* pCmdUI);
	afx_msg void OnControlDeleteOldestMofRoll();
	afx_msg void OnResetLeds();
	afx_msg void OnLoadFirmwareAtStartup();
	afx_msg void OnSelfTestAtStartup();
	//}}AFX_MSG
	afx_msg void OnUpdatePreScan(CCmdUI* pCmdUI);
	afx_msg void OnPreScan(UINT nID);
	afx_msg LRESULT OnMsgComCallBack(UINT ulOperation, LONG lStatus);
	afx_msg LRESULT OnMsgResaveImages(UINT ulOperation, LONG lStatus);
	DECLARE_MESSAGE_MAP()

	int m_iInitState;
	BOOL bInitCom();
	BOOL bInitClientCallback(BOOL bConnect);
	BOOL bInitScanner();

	BOOL UpdateServerData(int iIndex, BOOL bSaveAndValidate = TRUE);
	int UpdatePictureCounts();
	int UpdateScanCounts();
	void UpdatePicture();

	void CheckScanWarnings(int iScanWarnings);

private:
	BOOL m_bClearExistingImages;
	int  m_iSaveMode;
	BOOL m_bScanning;
	int m_bDisplayFourPictures;	// if 0 display one picture at a time
											// if 1 display four pictures at a time
	void SetDlgSaving(int iSaveMode);
	void SetDlgScanning(BOOL bScanning = TRUE);
	void PaintPicture();
	void AddBitmap(UCHAR * pDib);
	void ReplaceBitmap(UCHAR * pDib);
	void DrawBitmap(CDC *pDC,CBitmap *pBitmap,RECT crDisplayLimit);
	BOOL m_bResave;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TLACLIENTDEMODLG_H__5C7470A5_3C05_449C_816F_173D0FB885DF__INCLUDED_)
