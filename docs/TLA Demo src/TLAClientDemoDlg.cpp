// TLAClientDemoDlg.cpp : implementation file
//
#include "stdafx.h"
#include <io.h>
#include <winver.h>
#include "AfxPriv.h"	// for string conversion macros (W2T, etc.)
#include "TLAClientDemo.h"
#include "TLAClientDemoDlg.h"
#include "globals.h"
#include "constants.h"
#include "scandlg.h"
#include "colordlg.h"
#include "savedlg.h"
#include "framedlg.h"
#include "deleterolldlg.h"
#include "PreScanFramingAdjust.h"
#include "AdvanceMotor.h"

// Get definitions from the server -- the server must be built first
#include SERVER_CONSTANTS

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

#ifdef KODAK_AID
	#include "AidInterface.h"
#endif

#define COMPLETE_UNLOADING

int AddRotation(int iStartRotation, int iAdditionalRotation);

// Callback interface to be implemented on client
class CCallBackClient : public ICallBackClient	// Use this interface (server.idl)
{
public:
	CCallBackClient();			// Default constructor
	virtual ~CCallBackClient() {};

public:							// IUnknown methods
	STDMETHODIMP QueryInterface(REFIID riid, void **ppv);
	STDMETHODIMP_(ULONG) AddRef(void);
	STDMETHODIMP_(ULONG) Release(void);

								// The callback method
	STDMETHODIMP Awake(long lOperation, long lStatus);

private:
	int m_iRefCount;
};

CCallBackClient::CCallBackClient()
{
	m_iRefCount = 1;
    return;
}

STDMETHODIMP CCallBackClient::QueryInterface(REFIID riid, void **ppv)
{
	if(IsEqualIID(riid, IID_IUnknown) || 
		IsEqualIID(riid, IID_ICallBackClient))
	{
		*ppv = (void*)this;  
		AddRef();
		return S_OK;
	}
	else
	{
		*ppv = NULL;       
		return E_NOINTERFACE;
	}
}

STDMETHODIMP_(ULONG) CCallBackClient::AddRef(void)
{
    return ++m_iRefCount;
}

STDMETHODIMP_(ULONG) CCallBackClient::Release(void)
{
	ULONG ulReleaseCount;

	m_iRefCount--;
	ulReleaseCount = m_iRefCount;

	if(m_iRefCount == 0L)
		delete this;

	return ulReleaseCount;
}

// This function is passed to the server for the server to provide status
// callbacks during long operations
STDMETHODIMP CCallBackClient::Awake(long lOperation, long lStatus)
{
	AfxGetApp()->m_pMainWnd->PostMessage(WM_COM_CALLBACK, (ULONG)lOperation, lStatus);
	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	//{{AFX_DATA(CAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
	CString	m_strVersion;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	//{{AFX_MSG(CAboutDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	m_strVersion = _T("");
	//}}AFX_DATA_INIT
	DWORD dwVerHnd = 0;				// ignored parameter
	TCHAR szPath[256];
	::GetModuleFileName(AfxGetInstanceHandle(), szPath, sizeof(szPath));
	DWORD dwVerInfoSize = ::GetFileVersionInfoSize(szPath, &dwVerHnd);
	if(0 != dwVerInfoSize) 
	{
		LPTSTR strVffInfo = new TCHAR[dwVerInfoSize / sizeof(TCHAR)];
		if(::GetFileVersionInfo(szPath, dwVerHnd, dwVerInfoSize, strVffInfo))
		{
			DWORD dwVersionMS, dwVersionLS;
			VS_FIXEDFILEINFO* pFileInfo;
			UINT uiFileInfoLen = 0U;
			if(::VerQueryValue((LPVOID)strVffInfo, 
										_T("\\"), 
										(void **)(&pFileInfo), 
										&uiFileInfoLen))
			{
				if (uiFileInfoLen)
				{
					CString strType;
					dwVersionMS = pFileInfo->dwProductVersionMS;
					dwVersionLS = pFileInfo->dwProductVersionLS;
					if (dwVersionLS == 0)
					{
						strType = _T("Release");
						m_strVersion.Format(_T("%d.%d %s"), HIWORD(dwVersionMS),
							LOWORD(dwVersionMS), strType);
					}
					else
					{
						strType = _T("Beta");
						m_strVersion.Format(_T("%d.%d %s %d.%d"), HIWORD(dwVersionMS),
							LOWORD(dwVersionMS), strType, HIWORD(dwVersionLS),LOWORD(dwVersionLS));
					}
				}
			}
		}
		delete[] strVffInfo;
	}
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	DDX_Text(pDX, IDC_VERSION, m_strVersion);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

enum
{
	iInitStateBegin,
	iInitStateConnected,
	iInitStateCallBack,
	iInitStateInitialized,
};

/////////////////////////////////////////////////////////////////////////////
// CTLAClientDemoDlg dialog

CTLAClientDemoDlg::CTLAClientDemoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTLAClientDemoDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CTLAClientDemoDlg)
	m_iSavePictureCount = 0;
	m_iIndex = 0;
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32


	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	m_lCookie = 0;
	m_pILongOpsCB = NULL;
	m_pITLAMain = NULL;
	m_pIScanPictures = NULL;
	m_pISavePictures = NULL;

	m_pIStream = NULL;	// MARSHALLING

	m_iInitState = iInitStateBegin;

	m_puchFileMapData = NULL;
	m_hFileMapHandle = NULL;
	m_hFileMapEventHandle = NULL;
	m_bDisplayFourPictures = FALSE;
	m_iScannerType = SCANNER_TYPE_F_235;
	m_iScannerSerialNumber = -1;
	m_iColorPortraitMode = 0;
	m_i_GetScanLineTimeOut = 0;
	m_iNoFilmTimeOut = 0;

	m_iAdvanceFilmMilliseconds = 3000;
	m_AdvanceFilmSpeed = 1016;

	m_iDarkPointCorrectInterval = 0;
	m_iSavePictureSelectedCount = 0;
	m_iSavePictureHiddenCount = 0;
	m_iSaveStripCount = 0;
	m_iSaveRollCount =0;
	m_iCurrentRollIndex = 0;
	m_iScanPictureCount = 0;
	m_iScanStripCount = 0;
	m_iScanRollCount = 0;
	m_iScanWarnings = 0;
//	m_iRotation = ROTATE_90L;

													// Scan dlg persistance
	m_iFilmFormat = 0;						// 35 mm
	m_iFilmColor = 0;							// Color Negative
	m_iResolution = RESOLUTION_BASE_8;	// 8 Base scan
	m_iFramesPerStrip = 0;					// Full roll or single strip
	m_bRead_DX = TRUE;
	m_bScratchRemoval = FALSE;
	m_bHasFilmDrag = FALSE;
	m_bReelToReelMode = FALSE;
	m_bAggressiveFraming = FALSE;
	m_bLampWarmUp = FALSE;
	m_bExerciseSteppers = FALSE;
	m_bLoadFirmwareAtStartup = FALSE;
	m_bMotorSelfTestAtStartup = FALSE;
	m_uiScanQuantity = 1;
	m_uiScanQuantityCounter = 0;
	
													// Save dlg persistance
	m_iSaveDlgIndex = 0;
	m_bPlanarFileHeader = TRUE;
	m_bDibFileHeader = TRUE;
	m_iFileFormat = iFILE_FORMAT_JPG;	// jpeg standard compression
	m_iDPI = 300;
	m_iCompression = 90;						// 100 is min compression, 0 is max
	m_iSizeType = 0;
	m_iRotateType = 0;
	m_iPictureBoundaryWidth = 2100;
	m_iPictureBoundaryHeight = 1400;
	m_bSavUseLoRes = FALSE;
	m_bUseScratchRemoval = TRUE;
	m_bUseColorCorrection = TRUE;
	m_uiUseKcdfs = 0;
	m_bUseColorSceneBalance = TRUE;
	m_bUseColorAdjustments = TRUE;
	m_iIndex = 0;								// INDEX_Current
	m_bPlanarFileHeader = m_bPlanarFileHeader;
	m_bDibFileHeader = m_bDibFileHeader;
	m_bFastUpdates = FALSE;
	m_bTopDownDIB = FALSE;
	m_iSaveMethod = 2;						// save to client mem

	m_iSaveToMemoryFormat = iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8;
	m_bResave = FALSE;
	m_bScanning = FALSE;
	m_iSaveMode = SAVE_MODE_IDLE;
	m_bScannerInitializationCompleted = FALSE;
}

void CTLAClientDemoDlg::DoDataExchange(CDataExchange* pDX)
{
	PiPicture* pPicture = NULL;
	PiPictureInfo* pPictureInfo = NULL;

	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTLAClientDemoDlg)
	DDX_Control(pDX, IDC_SCAN_PROGRESS, m_ProgressScan);
	DDX_Control(pDX, IDC_SAVE_PROGRESS, m_ProgressSave);
	DDX_Text(pDX, IDC_PICTURE_COUNT, m_iSavePictureCount);
	DDX_Text(pDX, IDC_INDEX, m_iIndex);
	//}}AFX_DATA_MAP

	// if we actually have pictures and a valid index
	if (m_Pictures.GetSize() && m_iIndex < m_Pictures.GetSize())
	{
		// use the real mcoy
		pPicture = m_Pictures.GetAt(m_iIndex);
		pPictureInfo = &(pPicture->m_PiPictureInfo);
		DDX_Text(pDX, IDC_DIRECTORY, pPictureInfo->m_strDirectory);
		DDX_Text(pDX, IDC_FILE_NAME, pPictureInfo->m_strFileName);
		DDX_Text(pDX, IDC_FILM_PRODUCT, pPictureInfo->m_iFilmProduct);
		DDX_Text(pDX, IDC_FILM_SPECIFIER, pPictureInfo->m_iFilmSpecifier);
		DDX_Text(pDX, IDC_FRAME_NAME, pPictureInfo->m_strFrameName);
		DDX_Text(pDX, IDC_STRIP_NUMBER, pPictureInfo->m_iStripIndex);
		DDX_Text(pDX, IDC_FRAME_NUMBER, pPictureInfo->m_iFrameNumber);
		DDX_Radio(pDX, IDC_NOT_SELECTED, pPictureInfo->m_iSelectedHidden);
	}
	else // we don't have any pictures
	{
		// so use these temporary variables 
		CString strDirectory = _T("");
		CString strFileName = _T("");
		int iFilmProduct = 0;
		int iFilmSpecifier = 0;
		CString strFrameName = _T("");
		int iIndex = 0;
		int iCurrentStripIndex = 0;
		int iFrameNumber = 0;
		int iPictureSelectedHidden = -1;

		// exchange between temps and controls
		DDX_Text(pDX, IDC_DIRECTORY, strDirectory);
		DDX_Text(pDX, IDC_FILE_NAME, strFileName);
		DDX_Text(pDX, IDC_FILM_PRODUCT, iFilmProduct);
		DDX_Text(pDX, IDC_FILM_SPECIFIER, iFilmSpecifier);
		DDX_Text(pDX, IDC_FRAME_NAME, strFrameName);
		DDX_Text(pDX, IDC_STRIP_NUMBER, iCurrentStripIndex);
		DDX_Text(pDX, IDC_FRAME_NUMBER, iFrameNumber);
		DDX_Radio(pDX, IDC_NOT_SELECTED, iPictureSelectedHidden);
	}
}

BEGIN_MESSAGE_MAP(CTLAClientDemoDlg, CDialog)
	//{{AFX_MSG_MAP(CTLAClientDemoDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_SCAN, OnScan)
	ON_BN_CLICKED(IDC_SCAN_CANCEL, OnScanCancel)
	ON_BN_CLICKED(IDC_SAVE, OnSave)
	ON_BN_CLICKED(IDC_PREVIOUS, OnPrevious)
	ON_BN_CLICKED(IDC_NEXT, OnNext)
	ON_BN_CLICKED(IDC_SAVE_CANCEL, OnSaveCancel)
	ON_BN_CLICKED(IDC_APPLY, OnApply)
	ON_BN_CLICKED(IDC_FRAMING, OnFraming)
	ON_BN_CLICKED(IDC_PICTURE_MODE_ONE, OnPictureModeOne)
	ON_BN_CLICKED(IDC_PICTURE_MODE_FOUR, OnPictureModeFour)
	ON_BN_CLICKED(IDC_COLOR, OnColor)
	ON_BN_CLICKED(IDC_ADVANCE_FILM, OnAdvanceFilm)
	ON_BN_CLICKED(IDC_DELETE_PICTURE, OnDeletePicture)
	ON_BN_CLICKED(IDC_INSERT_PICTURE, OnInsertPicture)
	ON_COMMAND(ID_HELP_ABOUT, OnHelpAbout)
	ON_BN_CLICKED(IDC_RELEASE_SAVE_GROUP, OnReleaseSaveGroup)
	ON_BN_CLICKED(IDC_MOVE_TO_SAVE_GROUP, OnMoveToSaveGroup)
	ON_BN_CLICKED(IDC_ROTATE, OnRotate)
	ON_BN_CLICKED(IDC_CONNECT, OnConnect)
	ON_BN_CLICKED(IDC_RUN_AID, OnRunAid)
	ON_BN_CLICKED(IDC_DELETE_ROLL, OnDeleteRoll)
	ON_COMMAND(ID_CONTROL_LAMPOFF, OnControlLampOff)
	ON_COMMAND(ID_CONTROL_LAMPDIM, OnControlLampDim)
	ON_COMMAND(ID_CONTROL_LAMPON, OnControlLampOn)
	ON_COMMAND(ID_CONTROL_ROLLERSRELEASE, OnControlRollersRelease)
	ON_COMMAND(ID_CONTROL_APSMANUALRETRACT, OnControlApsManualRetract)
	ON_UPDATE_COMMAND_UI(ID_CONTROL_APSMANUALRETRACT, OnUpdateControlApsManualRetract)
	ON_COMMAND(ID_CONTROL_DELETE_OLDEST_MOF_ROLL, OnControlDeleteOldestMofRoll)
	ON_BN_CLICKED(IDC_RESET_LEDS, OnResetLeds)
	ON_BN_CLICKED(IDC_LOAD_FIRMWARE_AT_STARTUP, OnLoadFirmwareAtStartup)
	ON_BN_CLICKED(IDC_SELF_TEST_AT_STARTUP, OnSelfTestAtStartup)
	//}}AFX_MSG_MAP
	ON_COMMAND_RANGE(ID_PRESCAN_35MM4BASE, ID_PRESCAN_24MM16BASE, OnPreScan)
	ON_UPDATE_COMMAND_UI_RANGE(ID_PRESCAN_35MM4BASE, ID_PRESCAN_24MM16BASE, OnUpdatePreScan)
	ON_MESSAGE(WM_COM_CALLBACK, OnMsgComCallBack)
	ON_MESSAGE(WM_RESAVE_IMAGES, OnMsgResaveImages)
END_MESSAGE_MAP()


BOOL bGetField_dw(HKEY hTopKey, LPCTSTR szSubKey, LPCTSTR szField, DWORD &rdwReturn)
{
	HKEY hKey;
	DWORD dwBuffer;
	DWORD dwDataSize = sizeof(DWORD);
	DWORD dwKeyType;
	BOOL bRetVal = TRUE;

	if(ERROR_SUCCESS != ::RegOpenKeyEx(hTopKey, szSubKey, 0L, KEY_READ, &hKey))
		bRetVal = FALSE;									// Open the Key
	else 
	{
		if(ERROR_SUCCESS != ::RegQueryValueEx(hKey, szField, NULL, &dwKeyType, (BYTE*)(&dwBuffer), &dwDataSize))
			bRetVal = FALSE;								// Get the data.
		else if(REG_DWORD != dwKeyType)
			bRetVal = FALSE;								// verify data type

		::RegCloseKey(hKey);
	}

	if(bRetVal)
		rdwReturn = dwBuffer;
	else
		rdwReturn = 0;

	return bRetVal;
}

BOOL bPutField_dw(HKEY hTopKey, LPCTSTR szSubKey, LPCTSTR szField, DWORD dwValue)
{
	HKEY hKey;
	DWORD dwDisposition;
	BOOL bRetVal = TRUE;

	if(ERROR_SUCCESS != ::RegCreateKeyEx(hTopKey,
											szSubKey,
											0L,
											_T(" "),
											REG_OPTION_NON_VOLATILE, KEY_WRITE,
											NULL,
											&hKey,
											&dwDisposition))
	{														// Open the key
		bRetVal = FALSE;
	}
	else if(ERROR_SUCCESS != ::RegSetValueEx(hKey,
											szField,
											0L,
											REG_DWORD,
											(BYTE*)(&dwValue),
											sizeof(DWORD)))
	{														// Set the data
		bRetVal = FALSE;
	}

	::RegCloseKey(hKey);

	return bRetVal;
}

BOOL bGetField(HKEY hTopKey, LPCTSTR szSubKey, LPCTSTR szField, BOOL &rbReturn)
{
	DWORD dwReturn;
	if(::bGetField_dw(hTopKey, szSubKey, szField, dwReturn))
	{
		rbReturn = (BOOL)dwReturn;
		return TRUE;
	}

	if(bPutField_dw(hTopKey, szSubKey, szField, rbReturn))
		return TRUE;

	rbReturn = FALSE;
	return FALSE;
}

BOOL bGetField(HKEY hTopKey, LPCTSTR szSubKey, LPCTSTR szField, UINT &ruiReturn)
{
	DWORD dwReturn;
	if(::bGetField_dw(hTopKey, szSubKey, szField, dwReturn))
	{
		ruiReturn = dwReturn;
		return TRUE;
	}

	if(bPutField_dw(hTopKey, szSubKey, szField, ruiReturn))
		return TRUE;

	ruiReturn = 0;
	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// CTLAClientDemoDlg message handlers

/*
// BOOL OnInitDialog()
// 
// Description:
//
//   Initializes the application, the server interfaces, the scanner, and the
//   application interface (dialog controls).
//
// Returns:
//
//   TRUE  since we did not set the focus to a control
*/
BOOL CTLAClientDemoDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	((CButton*)GetDlgItem(IDC_PICTURE_MODE_ONE))->SetCheck(1);
	SetDlgScanning(FALSE);
	SetDlgSaving(SAVE_MODE_IDLE);
	GetDlgItem(IDC_ADVANCE_FILM)->EnableWindow(FALSE);
	GetDlgItem(IDC_SCAN)->EnableWindow(FALSE);
	GetDlgItem(IDC_CONNECT)->EnableWindow(FALSE);

	bGetField(HKEY_LOCAL_MACHINE, _T("Software\\Pakon\\TLAClientDemo\\General"), _T("ScanQuantity"), m_uiScanQuantity);
	if(m_uiScanQuantity <= 1)
	{
		m_uiScanQuantity = 1;
		GetDlgItem(IDC_TEST_COUNT)->SetWindowText(_T(""));
	}

	BOOL bUseKcdfs = TRUE;
	bGetField(HKEY_LOCAL_MACHINE, _T("Software\\Pakon\\TLBClientDemo\\General"), _T("UseKcdfs"), bUseKcdfs);
	if(bUseKcdfs)
		m_uiUseKcdfs = SAV_UseColorKcdfs;

	m_ProgressScan.SetRange32(WTP_ProgressStart, WTP_ProgressEnd);
	m_ProgressScan.SetPos(WTP_ProgressStart);

	m_ProgressSave.SetRange32(WTP_ProgressStart, WTP_ProgressEnd);
	m_ProgressSave.SetPos(WTP_ProgressStart);

	SetDlgItemText(IDC_SCAN_STATUS, _T("Initializing"));
	SetDlgItemText(IDC_SAVE_STATUS, _T("Idle"));

	bGetField(HKEY_LOCAL_MACHINE, _T("Software\\Pakon\\TLAClientDemo\\General"), _T("LoadFirmwareAtStartup"), m_bLoadFirmwareAtStartup);
	CButton *pChk = (CButton*)GetDlgItem(IDC_LOAD_FIRMWARE_AT_STARTUP);
	if(m_bLoadFirmwareAtStartup)
		pChk->SetCheck(BST_CHECKED);
	else
		pChk->SetCheck(BST_UNCHECKED);

	bGetField(HKEY_LOCAL_MACHINE, _T("Software\\Pakon\\TLAClientDemo\\General"), _T("MotorSelfTestAtStartup"), m_bMotorSelfTestAtStartup);
	pChk = (CButton*)GetDlgItem(IDC_SELF_TEST_AT_STARTUP);
	if(m_bMotorSelfTestAtStartup)
		pChk->SetCheck(BST_CHECKED);
	else
		pChk->SetCheck(BST_UNCHECKED);

	if(!bInitCom())
	{
		PostMessage(WM_CLOSE, 0, 0);	// Close down the application on failure
		return TRUE;
	}

//	TRACE(_T("TLA Client Demo: bInitCom succeeded\n"));

	if(!bInitClientCallback(TRUE))
	{
		PostMessage(WM_CLOSE, 0, 0);	// Close down the application on failure
		return TRUE;
	}

//	TRACE(_T("TLA Client Demo: bInitClientCallback succeeded\n"));

	if(!bInitScanner())
	{
		PostMessage(WM_CLOSE, 0, 0);	// Close down the application on failure
		return TRUE;
	}

//	TRACE(_T("TLA Client Demo: bInitScanner succeeded\n"));

	SetDlgItemText(IDC_CONNECT, _T("Disconnect"));
	//	GetDlgItem(IDC_CONNECT)->EnableWindow(TRUE);

	m_ClientBuffers.Init(m_pISavePictures);

	UpdateScanCounts();
	((CComboBox*)GetDlgItem(IDC_ROTATE_AMOUNT))->SetCurSel(0);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

/*
// BOOL bInitCom()
// 
// Description:
//
//		Create server object
//		Called once during lifetime of program
//
// Returns:
//
//   TRUE if initialization was successful.
*/
BOOL CTLAClientDemoDlg::bInitCom()
{
	HRESULT hr;
	if(iInitStateBegin != m_iInitState)
	{
		AfxMessageBox(_T("bInitCom with invalid state"), MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}
						// Initialize COM
#ifdef STA_THREADED_EXAMPLE
	hr = ::CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
#else
	hr = ::CoInitializeEx(NULL, COINIT_MULTITHREADED);
#endif

	if(FAILED(hr))
	{
		CString strResult;
		::FormatHResult(hr, strResult);
		CString str;
		str.Format(_T("CoInitialize Failed\n%s"), strResult);
		AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

	// FIXME ::CoCreateInstanceEx for DCOM
	hr = ::CoCreateInstance(
		CLSID_TLAMain, 
		0, 
		CLSCTX_INPROC_SERVER,
		IID_ILongOpsCB,
		(void**)&m_pILongOpsCB);

	if(FAILED(hr))
	{
		CString strResult;
		::FormatHResult(hr, strResult);
		CString str;
		str.Format(_T("CoCreateInstance for ILongOpsCB Failed\n%s"), strResult);
		AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

	m_iInitState = iInitStateConnected;

	return TRUE;
}

/*
// BOOL bInitClientCallback()
// 
// Description:
//
//   Initializes or unintializes callback interface
//
// Returns:
//
//   TRUE if successful.
*/
BOOL CTLAClientDemoDlg::bInitClientCallback(BOOL bConnect)
{
	if(bConnect)
	{						// Create a callback object
		if(iInitStateConnected != m_iInitState)
		{
			AfxMessageBox(_T("bInitClientCallback with invalid state"), MB_OK | MB_ICONEXCLAMATION);
			return FALSE;
		}

		CCallBackClient *pICallBackClient = new CCallBackClient();
							// Set up the callback connection 
		HRESULT hr = m_pILongOpsCB->CBAdvise(pICallBackClient, &m_lCookie);
		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ILongOpsCB, m_pILongOpsCB, NULL, FALSE);
			delete pICallBackClient;
//			pICallBackClient->Release();
			return FALSE;
		}
							// Done with our ref count. Server did an AddRef
		pICallBackClient->Release();

		m_iInitState = iInitStateCallBack;
		return TRUE;
	}

	if((m_iInitState < iInitStateCallBack) || (NULL == m_pILongOpsCB))
	{
//		AfxMessageBox(_T("bInitClientCallback with invalid state"), MB_OK | MB_ICONEXCLAMATION);
		return TRUE;
	}

	// If we have a callback object, release it and unintialize scanner
	BOOL bRetVal = TRUE;

	// Remove servers callback connection
	HRESULT hr = m_pILongOpsCB->CBUnadvise(m_lCookie);
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ILongOpsCB, m_pILongOpsCB, NULL, FALSE);
		bRetVal = FALSE;
	}

	// Release the server object
	m_pILongOpsCB->Release();

	m_iInitState = iInitStateConnected;
	return bRetVal;
}

/*
// BOOL bInitScanner()
// 
// Description:
//
//		Creates interface pointers for client use
//		Calls m_pITLAMain->InitializeScanner
//
// Returns:
//
//		TRUE if initialization was successful.
*/
BOOL CTLAClientDemoDlg::bInitScanner()
{
	HRESULT hr;

	if(iInitStateCallBack != m_iInitState)
	{
		AfxMessageBox(_T("bInitScanner with invalid state"), MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

	hr = m_pILongOpsCB->QueryInterface(IID_ITLAMain, (void**)&m_pITLAMain);
	if(FAILED(hr))
	{
		m_pITLAMain = NULL;
		CString strResult;
		::FormatHResult(hr, strResult);
		CString str;
		str.Format(_T("CoInitializeEx Failed\n%s"), strResult);
		AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

	hr = m_pILongOpsCB->QueryInterface(IID_IScanPictures, (void**)&m_pIScanPictures);
	if(FAILED(hr))
	{
		m_pIScanPictures = NULL;
		CString strResult;
		::FormatHResult(hr, strResult);
		CString str;
		str.Format(_T("CoInitializeEx Failed\n%s"), strResult);
		AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

	hr = m_pILongOpsCB->QueryInterface(IID_ISavePictures, (void**)&m_pISavePictures);
	if(FAILED(hr))
	{
		m_pISavePictures = NULL;
		CString strResult;
		::FormatHResult(hr, strResult);
		CString str;
		str.Format(_T("CoInitializeEx Failed\n%s"), strResult);
		AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

#if 0
	int iInitializeControl = INITIALIZE_CONTROL;
#else
	int iInitializeControl = INITIALIZE_ProgressUpdatesAsPercent;
#endif

	if(m_bLoadFirmwareAtStartup)
		iInitializeControl |= INITIALIZE_FirmwareUpdate;
	
	if(m_bMotorSelfTestAtStartup)
		iInitializeControl |= INITIALIZE_MotorSelfTest;

	hr = m_pITLAMain->InitializeScanner(
		iInitializeControl,
		SAVE_TO_MEMORY_TIMEOUT,
		SAVE_TO_SHARED_MEMORY_SIZE);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ITLAMain, m_pITLAMain, NULL);
		return FALSE;
	}

	m_iInitState = iInitStateInitialized;
	return TRUE;
}

void CTLAClientDemoDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

/*
// void OnPaint()
// 
// Description:
//		If you add a minimize button to your dialog, you will need the code below
//		to draw the icon.  For MFC applications using the document/view model,
//		this is automatically done for you by the framework.
//
// Return:
//
*/
void CTLAClientDemoDlg::OnPaint() 
{
//	TRACE0("OnPaint Called\n");
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
		PaintPicture();
	}
}


// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CTLAClientDemoDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

/*
// void OnScan()
// 
// Description:
//
//   Queries the user (scan dialog) for scanning parameters and
//   starts the scan.
//
//	Remarks:
//   
//   This method returns after the scan is started and does
//   not wait for the scan to complete.
*/
void CTLAClientDemoDlg::OnScan() 
{
	SetDlgScanning(TRUE);

	// be sure the COM pointer is good
	ASSERT(NULL != m_pIScanPictures);

	BOOL bScanMassQuantities = (0 < m_uiScanQuantityCounter);

	CScanDlg ScanDlg;
	// initialize the dialog with some standard settings
	ScanDlg.m_iFilmFormat = m_iFilmFormat;
	ScanDlg.m_iFilmColor = m_iFilmColor;
	ScanDlg.m_iResolution = m_iResolution;
	ScanDlg.m_iFramesPerStrip = m_iFramesPerStrip;
	ScanDlg.m_bRead_DX = m_bRead_DX;
	ScanDlg.m_bScratchRemoval = m_bScratchRemoval;
	ScanDlg.m_bHasFilmDrag = m_bHasFilmDrag;
	ScanDlg.m_bReelToReelMode = m_bReelToReelMode;
	ScanDlg.m_bAggressiveFraming = m_bAggressiveFraming;
	ScanDlg.m_bLampWarmUp = m_bLampWarmUp;
	ScanDlg.m_bExerciseSteppers = m_bExerciseSteppers;
	ScanDlg.m_bF235C = (SCANNER_TYPE_F_235C == m_iScannerType);

	if(!bScanMassQuantities)
	{
		if(IDOK != ScanDlg.DoModal())
		{
			SetDlgScanning(FALSE);
			return;
		}
	}

	// if the user pressed the OK button on the scan dialog
	m_iFilmFormat = ScanDlg.m_iFilmFormat;
	m_iFilmColor = ScanDlg.m_iFilmColor;
	m_iResolution = ScanDlg.m_iResolution;
	m_iFramesPerStrip = ScanDlg.m_iFramesPerStrip;
	m_bRead_DX = ScanDlg.m_bRead_DX;
	m_bScratchRemoval = ScanDlg.m_bScratchRemoval;
	m_bHasFilmDrag = ScanDlg.m_bHasFilmDrag;
	m_bReelToReelMode = ScanDlg.m_bReelToReelMode;
	m_bAggressiveFraming = ScanDlg.m_bAggressiveFraming;
	m_bLampWarmUp = ScanDlg.m_bLampWarmUp;
	m_bExerciseSteppers = ScanDlg.m_bExerciseSteppers;

	// convert the Frames per stip radio's 
	switch(ScanDlg.m_iFramesPerStrip)
	{
		case 1:
		{
			ScanDlg.m_iFramesPerStrip = 4;
			break;
		}

		case 2:
		{
			ScanDlg.m_iFramesPerStrip = 5;
			break;
		}

		case 3:
		{
			ScanDlg.m_iFramesPerStrip = 6;
			break;
		}

		default:
		{
			ScanDlg.m_iFramesPerStrip = 0;
			break;
		}
	}

	// set the scan control options
	int iScanControl = 0;
	if(ScanDlg.m_bAggressiveFraming)
		iScanControl |= SCAN_AggressiveFraming;

	if(ScanDlg.m_bScratchRemoval)
		iScanControl |= SCAN_UseScratchRemoval;

	if(ScanDlg.m_bHasFilmDrag)
		iScanControl |= SCAN_HasFilmDrag;

	if(ScanDlg.m_bRead_DX)
		iScanControl |= SCAN_Read_DX;

	if(ScanDlg.m_bReelToReelMode)
		iScanControl |= SCAN_RFT_SenseSplice;

	int iFilmFormat;
	switch(ScanDlg.m_iFilmFormat)
	{
		case 1:		// Extracted Optical
		{
			iFilmFormat = FILM_FORMAT_24MM;
			break;
		}

		case 2:		// Extracted MOF File
		{
			iFilmFormat = FILM_FORMAT_24MM;
			iScanControl |= SCAN_Use24mmExternalFileMOF;
			break;
		}

		case 3:		// Cartridge Optical
		{
			iFilmFormat = FILM_FORMAT_24MM;
			iScanControl |= SCAN_Use24mmAutoLoader;
			break;
		}

		case 4:		// Cartridge MOF Reader
		{
			iFilmFormat = FILM_FORMAT_24MM;
			iScanControl |= SCAN_Use24mmAutoLoader;
			iScanControl |= SCAN_Use24mmAutoLoaderMOF;
			break;
		}

		case 5:		// Cartridge MOF File
		{
			iFilmFormat = FILM_FORMAT_24MM;
			iScanControl |= SCAN_Use24mmAutoLoader;
			iScanControl |= SCAN_Use24mmExternalFileMOF;
			break;
		}

		case 6:		// Cartridge MOF File or Cartridge MOF Reader (File takes precedence)
		{
			iFilmFormat = FILM_FORMAT_24MM;
			iScanControl |= SCAN_Use24mmAutoLoader;
			iScanControl |= SCAN_Use24mmExternalFileMOF;
			iScanControl |= SCAN_Use24mmAutoLoaderMOF;
			break;
		}

//		case 0:
		default:
		{
			iFilmFormat = FILM_FORMAT_35MM;
			break;
		}
	}

	// convert the film color radios
	int iFilmColor;
	switch(ScanDlg.m_iFilmColor)
	{
		case 1:
		{
			iFilmColor = FILM_COLOR_POSITIVE;
			break;
		}

		case 2:
		{
			iFilmColor = FILM_COLOR_BnW_NORMAL;
			break;
		}

		case 3:
		{
			iFilmColor = FILM_COLOR_BnW_C41;
			break;
		}

		default:
		{
			iFilmColor = FILM_COLOR_NEGATIVE;
			break;
		}
	}		
	
	HRESULT hr;
	if(bScanMassQuantities)
	{
		hr = m_pIScanPictures->ScanPictures(
								ScanDlg.m_iResolution, 
								iFilmColor, 
								iFilmFormat,
								ScanDlg.m_iFramesPerStrip,
								iScanControl);

		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
			SetDlgScanning(FALSE);
		}

		return;
	}

	switch(ScanDlg.m_iOperation)
	{
		case iSCAN_OP_CORRECTION_GAIN:
		{
			iScanControl = CALIBRATE_GainAndOffset;
			if(ScanDlg.m_bLampWarmUp)
				iScanControl |= CALIBRATE_LampWarmUp;

			if(ScanDlg.m_bExerciseSteppers)
				iScanControl |= CALIBRATE_ExerciseSteppers;

			hr = m_pIScanPictures->ForceCorrections(
									ScanDlg.m_iResolution,
									iFilmColor,
									ScanDlg.m_iFilmFormat == 0 ? FILM_FORMAT_35MM : FILM_FORMAT_24MM,
									iScanControl);
			break;
		}

		case iSCAN_OP_CORRECTION_FP:
		{
			iScanControl = CALIBRATE_FixedPattern;
			if(ScanDlg.m_bLampWarmUp)
				iScanControl |= CALIBRATE_LampWarmUp;

			if(ScanDlg.m_bExerciseSteppers)
				iScanControl |= CALIBRATE_ExerciseSteppers;

			hr = m_pIScanPictures->ForceCorrections(
									ScanDlg.m_iResolution,
									iFilmColor,
									ScanDlg.m_iFilmFormat == 0 ? FILM_FORMAT_35MM : FILM_FORMAT_24MM,
									iScanControl);
			break;
		}

		case iSCAN_OP_CORRECTION_FOCUS_ADVANCE_FILM:
		{
			iScanControl = CALIBRATE_FocusAdvanceFilm;
			if(ScanDlg.m_bLampWarmUp)
				iScanControl |= CALIBRATE_LampWarmUp;

			if(ScanDlg.m_bExerciseSteppers)
				iScanControl |= CALIBRATE_ExerciseSteppers;

			hr = m_pIScanPictures->ForceCorrections(
									ScanDlg.m_iResolution,
									iFilmColor,
									ScanDlg.m_iFilmFormat == 0 ? FILM_FORMAT_35MM : FILM_FORMAT_24MM,
									iScanControl);

			break;
		}

		case iSCAN_OP_DO_FILM_TRACK_TEST:
		{
			hr = m_pIScanPictures->FilmTrackTest(FALSE, ScanDlg.m_iFilmFormat == 0 ? FILM_FORMAT_35MM : FILM_FORMAT_24MM);
			break;
		}

		case iSCAN_OP_CORRECTION_FOCUS:
		{
			iScanControl = CALIBRATE_Focus;
			if(ScanDlg.m_bLampWarmUp)
				iScanControl |= CALIBRATE_LampWarmUp;

			if(ScanDlg.m_bExerciseSteppers)
				iScanControl |= CALIBRATE_ExerciseSteppers;

			hr = m_pIScanPictures->ForceCorrections(
									ScanDlg.m_iResolution,
									iFilmColor,
									ScanDlg.m_iFilmFormat == 0 ? FILM_FORMAT_35MM : FILM_FORMAT_24MM,
									iScanControl);

			break;
		}

		case iSCAN_OP_SET_FILM_TRACK:
		{
			hr = m_pIScanPictures->PutFilmGuidePosition(
									ScanDlg.m_iFilmFormat == 0 ? FILM_FORMAT_35MM : FILM_FORMAT_24MM);

			break;
		}

		case iSCAN_OP_IMPORT:
		{
			hr = m_pIScanPictures->ImportFromFile(::SysAllocString(L"C:\\Temp\\Test0.bmp"), FALSE);
			break;
		}

		case iSCAN_OP_RESET_MOTOR_SPEED_ADJUST:
		{
			hr = m_pIScanPictures->ResetFactoryDefaults(
									FACTORY_RESET_MotorSpeed,
									ScanDlg.m_iResolution,
									ScanDlg.m_iFilmFormat == 0 ? FILM_FORMAT_35MM : FILM_FORMAT_24MM);

			SetDlgScanning(FALSE);
			break;
		}

		case iSCAN_OP_PRE_SCAN:
		{
			if(ScanDlg.m_bLampWarmUp)
				iScanControl |= SCAN_LampWarmUp;

			hr = m_pIScanPictures->ScanPictures(
									ScanDlg.m_iResolution, 
									iFilmColor, 
									iFilmFormat,
									ScanDlg.m_iFramesPerStrip,
									iScanControl | SCAN_PreScan); 

			break;
		}

//		case iSCAN_OP_SCAN:
		default:
		{
			if(ScanDlg.m_bLampWarmUp)
				iScanControl |= SCAN_LampWarmUp;

			hr = m_pIScanPictures->ScanPictures(
									ScanDlg.m_iResolution, 
									iFilmColor, 
									iFilmFormat,
									ScanDlg.m_iFramesPerStrip,
									iScanControl);

			if(1 < m_uiScanQuantity)
			{
				if(SUCCEEDED(hr))
				{
					m_uiScanQuantityCounter = m_uiScanQuantity;
					CString str;
					str.Format(_T("%03d Done\n%03d Left"), m_uiScanQuantity - m_uiScanQuantityCounter, m_uiScanQuantityCounter);
					GetDlgItem(IDC_TEST_COUNT)->SetWindowText(str);
				}
			}

			break;
		}
	}

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		SetDlgScanning(FALSE);
	}
}

/*
// void OnScanCancel()
// 
// Description:
//
//   Cancels a scan that is in progress.
*/
void CTLAClientDemoDlg::OnScanCancel() 
{
	// be sure the COM pointer is good
	ASSERT(NULL != m_pIScanPictures);

	m_uiScanQuantityCounter = 0;

	if(1 < m_uiScanQuantity)
	{
		CString str;
		str.Format(_T("%03d Done\n%03d Left"), m_uiScanQuantity - m_uiScanQuantityCounter, m_uiScanQuantityCounter);
		GetDlgItem(IDC_TEST_COUNT)->SetWindowText(str);
	}

	HRESULT hr = m_pIScanPictures->ScanCancel();
	if(FAILED(hr)) 
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
	else
		SetDlgScanning(FALSE);
}

/*
// void OnFraming()
// 
// Description:
//
//   Prompts the user for framing data and applies the framing
//   data to the current picture.
//
//	Remarks
//
//		This funtion is only enabled if DisplayFourPictures is false
*/
void CTLAClientDemoDlg::OnFraming() 
{
	SetDlgSaving(SAVE_MODE_EDIT);
	HRESULT hr;
	CFrameDlg FrameDlg(this);

	FrameDlg.m_iFrameOrCrop = 0;								// adjust framing of picture
	FrameDlg.m_csStrip = m_csStripHR;						// boundary for picture frame
	FrameDlg.m_pFramingUserInfo = &(m_Pictures[m_iIndex]->m_PiFramingUserInfo);	// current picture

	// get the scanner framing points for handling of Reset button
	hr = m_pISavePictures->GetPictureFramingInfo(
									m_iIndex,
									&FrameDlg.m_iFramingRisk,
									&FrameDlg.m_crDefault.left,
									&FrameDlg.m_crDefault.top,
									&FrameDlg.m_crDefault.right,
									&FrameDlg.m_crDefault.bottom);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		SetDlgSaving(SAVE_MODE_IDLE);
		return;
	}

	SetDlgSaving(SAVE_MODE_IDLE);
	FrameDlg.DoModal();
}


/*
// void OnColor()
// 
// Description:
//
//   Displays a dialog for adjusting color of the currently displayed 
//   images.
//
//	Remarks:
//
*/
void CTLAClientDemoDlg::OnColor() 
{
	CColorDlg ColorDlg(this);
	ColorDlg.m_bDisplayFourPictures = m_bDisplayFourPictures;
	ColorDlg.m_iIndex = m_iIndex;

	if(m_bDisplayFourPictures)
	{
		// set sliders in the middle and do the
		// "differential" color adjust
		ColorDlg.m_iRed         = 0;
		ColorDlg.m_iGreen       = 0;
		ColorDlg.m_iBlue        = 0;
		ColorDlg.m_iBrightness  = 0;
		ColorDlg.m_iContrast    = 0;
		ColorDlg.m_iSharpness         = 0;
	}
	else
	{
		// set sliders to current picture's sliders
		PiColorSetting *pColorSetting = &(m_Pictures[m_iIndex]->m_PiColorSetting);
		ColorDlg.m_iRed = pColorSetting->m_iRed;
		ColorDlg.m_iGreen = pColorSetting->m_iGreen;
		ColorDlg.m_iBlue = pColorSetting->m_iBlue;
		ColorDlg.m_iBrightness = pColorSetting->m_iBrightness;
		ColorDlg.m_iContrast = pColorSetting->m_iContrast;
		ColorDlg.m_iSharpness = pColorSetting->m_iSharpness;
	}

	ColorDlg.DoModal();	
}

/*
// void OnSave()
// 
// Description:
//
//   Queries the user for image attributes (save dialog), then
//   saves the current picture as indicated by m_iIndex.
//
//	Remarks:
//
//   The picture is saved to a directory and file name as defined
//   by the pictures "FileName" and "Directory" attributes.  If the
//   picture needed to be saved elsewhere with a different name, we
//   could have passed that into the SaveToDisk method with a non-Null
//   second parameter.   
*/
void CTLAClientDemoDlg::OnSave() 
{
	int iNewRotation = 0;
	SetDlgSaving(SAVE_MODE_EXPORT);

	ASSERT(m_clSaveToSharedMemoryDirAndFile.GetCount() == 0);
	ASSERT(m_clSaveToClientMemoryDirAndFile.GetCount() == 0);

	// be sure the COM pointer is good
	ASSERT(NULL != m_pISavePictures);

	PiPictureInfo* pPictureInfo;
	for(int iIndex = 0; iIndex < m_Pictures.GetSize(); iIndex++)
	{
		pPictureInfo = &(m_Pictures[iIndex]->m_PiPictureInfo);
		BSTR bstrFileName   = pPictureInfo->m_strFileName.AllocSysString();
		BSTR bstrFrameName  = pPictureInfo->m_strFrameName.AllocSysString();
		BSTR bstrDirectory  = pPictureInfo->m_strDirectory.AllocSysString();
		HRESULT hr = m_pISavePictures->PutPictureInfo(
			iIndex,
			pPictureInfo->m_iFrameNumber,
			bstrFileName,
			bstrDirectory,
			pPictureInfo->m_iRotation,
			pPictureInfo->m_iSelectedHidden);

		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		}
	}

	CSaveDlg SaveDlg(this);

	SaveDlg.m_iFileFormat = m_iFileFormat;
	SaveDlg.m_iDPI = m_iDPI;
	SaveDlg.m_iCompression = m_iCompression;
	SaveDlg.m_iSizeType = m_iSizeType;
	SaveDlg.m_iRotateType = m_iRotateType;
	SaveDlg.m_iRotation = m_iRotation;
	SaveDlg.m_bMirror = m_bMirror;
	SaveDlg.m_iPictureBoundaryWidth = m_iPictureBoundaryWidth;
	SaveDlg.m_iPictureBoundaryHeight = m_iPictureBoundaryHeight;
	SaveDlg.m_bSavUseLoRes = m_bSavUseLoRes;
	SaveDlg.m_bUseScratchRemoval = m_bUseScratchRemoval;
	SaveDlg.m_bUseColorCorrection = m_bUseColorCorrection;
	SaveDlg.m_bUseColorSceneBalance = m_bUseColorSceneBalance;
	SaveDlg.m_bUseColorAdjustments = m_bUseColorAdjustments;
	SaveDlg.m_iIndex = m_iSaveDlgIndex;
	SaveDlg.m_bPlanarFileHeader = m_bPlanarFileHeader;
	SaveDlg.m_bDibFileHeader = m_bDibFileHeader;
	SaveDlg.m_bFastUpdates = m_bFastUpdates;
	SaveDlg.m_bTopDownDIB = m_bTopDownDIB;
	SaveDlg.m_iSaveMethod = m_iSaveMethod;
	if(iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8 == m_iSaveToMemoryFormat)
		SaveDlg.m_iSaveToMemoryFormat = 1;
	else
		SaveDlg.m_iSaveToMemoryFormat = 0;

	if(IDOK == SaveDlg.DoModal())
	{
		m_iFileFormat = SaveDlg.m_iFileFormat;
		m_iDPI = SaveDlg.m_iDPI;
		m_iCompression = SaveDlg.m_iCompression;
		m_iSizeType = SaveDlg.m_iSizeType;
		m_iRotateType = SaveDlg.m_iRotateType;
		m_iRotation = SaveDlg.m_iRotation;
		m_bMirror = SaveDlg.m_bMirror;
		m_iPictureBoundaryWidth = SaveDlg.m_iPictureBoundaryWidth;
		m_iPictureBoundaryHeight = SaveDlg.m_iPictureBoundaryHeight;
		m_bSavUseLoRes = SaveDlg.m_bSavUseLoRes;
		m_bUseScratchRemoval = SaveDlg.m_bUseScratchRemoval;
		m_bUseColorCorrection = SaveDlg.m_bUseColorCorrection;
		m_bUseColorSceneBalance = SaveDlg.m_bUseColorSceneBalance;
		m_bUseColorAdjustments = SaveDlg.m_bUseColorAdjustments;
		m_iSaveDlgIndex = SaveDlg.m_iIndex;
		m_bPlanarFileHeader = SaveDlg.m_bPlanarFileHeader;
		m_bDibFileHeader = SaveDlg.m_bDibFileHeader;
		m_bFastUpdates = SaveDlg.m_bFastUpdates;
		m_bTopDownDIB = SaveDlg.m_bTopDownDIB;
		m_iSaveMethod = SaveDlg.m_iSaveMethod;

		if(0 == SaveDlg.m_iSaveToMemoryFormat)
			m_iSaveToMemoryFormat = iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_16;
		else
			m_iSaveToMemoryFormat = iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8;

		// what index is specified
		int iIndex;
		switch(SaveDlg.m_iIndex)
		{
			case 1:
			{
				iIndex = INDEX_AllSelected;
				break;
			}

			case 2:
			{
				iIndex = INDEX_All;
				break;
			}

			default:
			{
				iIndex = m_iIndex;
				break;
			}
		}

		// how is the image size specified
		int iSaveOptions = SAV_SizeOriginal;
		switch(SaveDlg.m_iSizeType)
		{
			case 1:
			{
				iSaveOptions = SAV_SizeLimitForDisplay;
				break;
			}

			case 2:
			{
				iSaveOptions = SAV_SizeLimitForSave;
				break;
			}

			default:
				break;
		}

		// how is the rotation specified
		if(0 == SaveDlg.m_iRotateType)
			iSaveOptions |= SAV_UseCurrentRotation;
		else
		{
			switch(SaveDlg.m_iRotation)
			{
				case 0:
				{
					iNewRotation = ROTATE_0;
					break;
				}

				case 1:
				{
					iNewRotation = ROTATE_90L;
					break;
				}

				case 2:
				{
					iNewRotation = ROTATE_180;
					break;
				}

//				case 3:
				default:
				{
					iNewRotation = ROTATE_90R;
					break;
				}
			}
		}

		if (SaveDlg.m_bMirror)
			iNewRotation |= MIRROR_L_R;

		// what other options are specified
		if(SaveDlg.m_bSavUseLoRes)
			iSaveOptions |= SAV_UseLoResBuffer;

		if(SaveDlg.m_bUseScratchRemoval)
			iSaveOptions |= SAV_UseScratchRemovalIfAvailable;

		if(SaveDlg.m_bUseColorCorrection)
			iSaveOptions |= SAV_UseColorCorrection;

		iSaveOptions |= m_uiUseKcdfs;

		if(SaveDlg.m_bUseColorSceneBalance)
			iSaveOptions |= SAV_UseColorSceneBalance;

		if(SaveDlg.m_bUseColorAdjustments)
			iSaveOptions |= SAV_UseColorAdjustments;

		// tell the server to save the picture(s)
		HRESULT hr;
		if(m_iSaveMethod != 0)  // save to memory (client or shared)
		{
			if(0 == SaveDlg.m_iSaveToMemoryFormat)
			{
				if(m_bPlanarFileHeader)
					iSaveOptions |= SAV_FileHeader;
			}
			else
			{
				if(m_bDibFileHeader)
					iSaveOptions |= SAV_FileHeader;
			}

			// Header always saved to memory by server - client can save without it
			if(SaveDlg.m_bFastUpdates)
				iSaveOptions |= SAV_FastUpdate8BitDib;

			if(SaveDlg.m_bTopDownDIB)
				iSaveOptions |= SAV_TopDownDib;

			SiSaveToMemoryDirAndFile SaveToMemoryDirAndFile;
			BSTR bstrFileName  = NULL;
			BSTR bstrFrameName = NULL;
			BSTR bstrDirectory = NULL;
			int iCurrentRollIndex;
			int iCurrentStripIndex;
			int iFilmProduct;
			int iFilmSpecifier;
			int iFrameNumber;
			int iFrameType;
			int iRotation;
			int iPictureSelectedHidden;
			CRect rectFrame;
			CSize sizeMax(0,0);

			switch(iIndex)
			{
				case INDEX_All:
				case INDEX_AllSelected:
				{
					for(int iTempIndex = 0; iTempIndex < m_iSavePictureCount; iTempIndex++)
					{
						hr = m_pISavePictures->GetPictureInfo(
							iTempIndex,
							&iCurrentRollIndex,
							&iCurrentStripIndex,
							&iFilmProduct,
							&iFilmSpecifier,
							&bstrFrameName,
							&iFrameNumber,
							&iFrameType,
							&bstrFileName,
							&bstrDirectory,
							&iRotation,
							&iPictureSelectedHidden);

						if(FAILED(hr))
						{
							::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
							return;
						}

						::SysFreeString(bstrFrameName);
						bstrFrameName = NULL;

						if((S_OR_H_HIDDEN == iPictureSelectedHidden) ||
							((S_OR_H_SELECTED != iPictureSelectedHidden) && (INDEX_AllSelected == iIndex)))
						{									// picture not used
							::SysFreeString(bstrDirectory);
 							bstrDirectory = NULL;

							::SysFreeString(bstrFileName);
							bstrFileName = NULL;
							continue;
						}
														// add picture to list
						SaveToMemoryDirAndFile.m_strDirectory = bstrDirectory;
						::SysFreeString(bstrDirectory);
						bstrDirectory = NULL;

						SaveToMemoryDirAndFile.m_strFileName = bstrFileName;
						::SysFreeString(bstrFileName);
						bstrFileName = NULL;

						if (SaveDlg.m_iSaveMethod == 1) 
							m_clSaveToSharedMemoryDirAndFile.AddTail(SiSaveToMemoryDirAndFile(SaveToMemoryDirAndFile));
						else if (SaveDlg.m_iSaveMethod == 2)
						{
							m_clSaveToClientMemoryDirAndFile.AddTail(SiSaveToMemoryDirAndFile(SaveToMemoryDirAndFile));

							if(SAV_SizeOriginal == (SAV_SizeBitMask & iSaveOptions))
							{								// get the frame info
								hr = m_pISavePictures->GetPictureFramingUserInfo(
									iTempIndex,
									&rectFrame.left,
									&rectFrame.top,
									&rectFrame.right,
									&rectFrame.bottom);

								if(FAILED(hr))
								{
									::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
									return;
								}

								// what's the max pixel count?
								if (sizeMax.cx * sizeMax.cy < 
									 ((rectFrame.Width() + 1) * (rectFrame.Height() + 1)) )
								{
									sizeMax.cx = rectFrame.Width() + 1;
									sizeMax.cy = rectFrame.Height() + 1;
								}
							}
							else if(0 == sizeMax.cx)
							{
								sizeMax.cx = SaveDlg.m_iPictureBoundaryWidth + 1;
								sizeMax.cy = SaveDlg.m_iPictureBoundaryHeight + 1;
							}
						}
					} // end for each index (selected or All)

					break;
				}
				// discrete index
				default:
				{
					hr = m_pISavePictures->GetPictureInfo(
						iIndex,
						&iCurrentRollIndex,
						&iCurrentStripIndex,
						&iFilmProduct,
						&iFilmSpecifier,
						&bstrFrameName,
						&iFrameNumber,
						&iFrameType,
						&bstrFileName,
						&bstrDirectory,
						&iRotation,
						&iPictureSelectedHidden);

					if(FAILED(hr))
					{
						::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
						return;
					}

					::SysFreeString(bstrFrameName);
					bstrFrameName = NULL;
														// add picture to list
					SaveToMemoryDirAndFile.m_strDirectory = bstrDirectory;
					::SysFreeString(bstrDirectory);
					bstrDirectory = NULL;

					SaveToMemoryDirAndFile.m_strFileName = bstrFileName;
					::SysFreeString(bstrFileName);
					bstrFileName = NULL;

					if (SaveDlg.m_iSaveMethod == 1)
						m_clSaveToSharedMemoryDirAndFile.AddTail(SiSaveToMemoryDirAndFile(SaveToMemoryDirAndFile));
					else
					{
						m_clSaveToClientMemoryDirAndFile.AddTail(SiSaveToMemoryDirAndFile(SaveToMemoryDirAndFile));

						if(SAV_SizeOriginal == (SAV_SizeBitMask & iSaveOptions))
						{								// get the frame info
							hr = m_pISavePictures->GetPictureFramingUserInfo(
								iIndex,
								&rectFrame.left,
								&rectFrame.top,
								&rectFrame.right,
								&rectFrame.bottom);

							if(FAILED(hr))
							{
								::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
								return;
							}

							// what's the max pixel count?
							if (sizeMax.cx * sizeMax.cy <
								 ((rectFrame.Width() + 1) * (rectFrame.Height() + 1)) )
							{
								sizeMax.cx = rectFrame.Width() + 1;
								sizeMax.cy = rectFrame.Height() + 1;
							}
						}
						else if(0 == sizeMax.cx)
						{
							sizeMax.cx = SaveDlg.m_iPictureBoundaryWidth + 1;
							sizeMax.cy = SaveDlg.m_iPictureBoundaryHeight + 1;
						}
					}

					break;
				}
			}

			if(SaveDlg.m_iSaveMethod == 1)
			{
				hr = m_pISavePictures->SaveToSharedMemory(
					iIndex,
					iSaveOptions,
					SaveDlg.m_iPictureBoundaryWidth,
					SaveDlg.m_iPictureBoundaryHeight,
					iNewRotation,
					SCALING_METHOD_BICUBIC,
					m_iSaveToMemoryFormat,
					SaveDlg.m_bUseWorkerThread);
			}
			else
			{
				// how many buffers do we need
				BOOL bMultiplePictures = FALSE;
				// if saving "all selected or all" and that is greater than 1
				if (iIndex && m_clSaveToClientMemoryDirAndFile.GetCount() > 1)
					bMultiplePictures = TRUE;

				if (m_ClientBuffers.bAllocateBuffers(sizeMax, m_iSaveToMemoryFormat, bMultiplePictures))
				{
					if (!SaveDlg.m_bUseWorkerThread)
					{
						UpdateExportProgress(WTP_ProgressStart);
					}
					hr = m_pISavePictures->SaveToClientMemory(
						iIndex,
						iSaveOptions,
						SaveDlg.m_iPictureBoundaryWidth,
						SaveDlg.m_iPictureBoundaryHeight,
						iNewRotation,
						SCALING_METHOD_BICUBIC,
						m_iSaveToMemoryFormat,
						SaveDlg.m_bUseWorkerThread);
					if (!SaveDlg.m_bUseWorkerThread)
					{
						UpdateExportProgress(WTP_ProgressStart + 1000);
						UpdateExportProgress(WTP_ProgressComplete);
					}

				}
				else
				{
					ASSERT(FALSE);
					// FIXME - let user know what happened
					SetDlgSaving(SAVE_MODE_IDLE);
					return;
				}
			}
		}
		else // to disk 
		{
			int iFileFormat;
			switch(SaveDlg.m_iFileFormat)
			{
				case 0:
				default:
				{
					iFileFormat = iFILE_FORMAT_JPG;
					break;
				}
				case 1:
				{
					iFileFormat = iFILE_FORMAT_BMP;
					break;
				}

				case 2:
				{
					iFileFormat = iFILE_FORMAT_TIF;
					break;
				}
				case 3:
				{
					iFileFormat = iFILE_FORMAT_EXIF;
					break;
				}
			}

			hr = m_pISavePictures->SaveToDisk(
				iIndex,
				NULL,  // so the picture FileName and Directory are used
				iSaveOptions,
				SaveDlg.m_iPictureBoundaryWidth,
				SaveDlg.m_iPictureBoundaryHeight,
				iNewRotation,
				SCALING_METHOD_BICUBIC,
				iFileFormat,
				SaveDlg.m_iCompression,
				SaveDlg.m_iDPI,
				0);
		}

		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			while (m_clSaveToSharedMemoryDirAndFile.GetCount())
				m_clSaveToSharedMemoryDirAndFile.RemoveTail();
			while (m_clSaveToClientMemoryDirAndFile.GetCount())
				m_clSaveToClientMemoryDirAndFile.RemoveTail();
			m_ClientBuffers.Delete();
			SetDlgSaving(SAVE_MODE_IDLE);
		}
	}
	else // cancel save dialog
	{
		SetDlgSaving(SAVE_MODE_IDLE);
	}
}

/*
// void OnSaveCancel()
// 
// Description:
//
//   Cancels the scan currently in progress
*/
void CTLAClientDemoDlg::OnSaveCancel() 
{
	// be sure the COM pointer is good
	ASSERT(NULL != m_pISavePictures);

	HRESULT hr = m_pISavePictures->SaveCancel();
	if(FAILED(hr)) 
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);

	while (m_clSaveToClientMemoryDirAndFile.GetCount() != 0)
		m_clSaveToClientMemoryDirAndFile.RemoveTail();

	while (m_clSaveToSharedMemoryDirAndFile.GetCount() != 0)
		m_clSaveToSharedMemoryDirAndFile.RemoveTail();
}


/*
// void OnPrevious()
// 
// Description:
//
//   if we're not already at the first picture
//      update the scanner with the current pictures data
//      move to prev picture
//      update dialog data with the picture info from scanner
*/
void CTLAClientDemoDlg::OnPrevious() 
{
	if(m_iIndex > 0)
	{
		UpdateData();
		m_Pictures[m_iIndex]->m_PiPictureInfo.m_iUpdate |= UPDATE_DATA;
		UpdateServerData(m_iIndex);
		m_iIndex--;
		UpdateServerData(m_iIndex, FALSE);
		UpdateData(FALSE);
		UpdatePrevNext();
		UpdatePicture();
	}
}

/*
// void CTLAClientDemoDlg::OnNext()
// 
// Description:
//
//   if we're not already at the last picture
//      update the scanner with the current pictures data
//      move to next picture
//      update dialog data with the picture info from scanner
*/
void CTLAClientDemoDlg::OnNext() 
{
	if((m_iIndex + 1) < m_iSavePictureCount)
	{
		UpdateData();
		m_Pictures[m_iIndex]->m_PiPictureInfo.m_iUpdate |= UPDATE_DATA;
		UpdateServerData(m_iIndex);
		m_iIndex++;
		UpdateServerData(m_iIndex, FALSE);
		UpdateData(FALSE);
		UpdatePrevNext();
		UpdatePicture();
	}
}

/*
// void CTLAClientDemoDlg::OnApply()
// 
// Description:
//
//   Applys the data in the dialog box to the data in the scanner.
//
//	Remarks:
//
//   Because some of the data we are updating in the scanner (eg FrameNumber)
//   changes some of the read-only data in the scanner (eg FrameName),
//   after we update the scanner with the data from the dialog, we then 
//   update the dialog with the data in the scanner.
*/
void CTLAClientDemoDlg::OnApply() 
{
	UpdateData();
	m_Pictures[m_iIndex]->m_PiPictureInfo.m_iUpdate |= UPDATE_DATA;
	UpdateServerData(m_iIndex);
	UpdateServerData(m_iIndex, FALSE);
	UpdateData(FALSE);
}

/*
// void OnPictureModeOne()
// 
// Description:
//
//
// Return:
//
*/
void CTLAClientDemoDlg::OnPictureModeOne() 
{
	// show the picture attributes
	m_bDisplayFourPictures = FALSE;
	UpdatePicture();
}

/*
// void OnPictureModeFour()
// 
// Description:
//
//
// Return:
//
*/
void CTLAClientDemoDlg::OnPictureModeFour() 
{
	m_bDisplayFourPictures = TRUE;
	UpdatePicture();
}

/*
// int UpdateScanCounts()
// 
// Description:
//
//   Updates the scan count member variables with the scan group 
//   counts from the server and updates the respective dialog controls
//
// Return:
//
//   Returns the member variable m_iScanPictureCount after
//   it's updated with the picture count from the server.
*/
int CTLAClientDemoDlg::UpdateScanCounts()
{
	HRESULT hr = m_pISavePictures->GetRollCountScanGroup(&m_iScanRollCount);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		return -1;
	}

	SetDlgItemInt(IDC_SCAN_ROLL_COUNT, m_iScanRollCount);
	if (m_iScanRollCount)
	{
		// make move oldest roll to save group the default
		SetDefID(IDC_MOVE_TO_SAVE_GROUP);
	}
	else
	{
		// make scan the default
		SetDefID(IDC_SCAN);
	}

	return m_iScanRollCount;
}
/*
// int UpdatePictureCounts()
// 
// Description:
//
//   Updates the picture count member variables with the picture 
//   counts from the server's save group.
//
// Return:
//
//   Returns the member variable m_iSavePictureCount after
//   it's updated with the picture count from the server.
*/
int CTLAClientDemoDlg::UpdatePictureCounts()
{
	HRESULT hr = m_pISavePictures->GetPictureCountSaveGroup(
		&m_iSaveRollCount,
		&m_iSaveStripCount,
		&m_iSavePictureCount, 
		&m_iSavePictureSelectedCount, 
		&m_iSavePictureHiddenCount);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		return -1;
	}
	SetDlgItemInt(IDC_PICTURE_COUNT, m_iSavePictureCount);

	return m_iSavePictureCount;
}

/*
// BOOL UpdateServerData(BOOL bSaveAndValidate, BOOL bUpdateControls)
// 
// Description:
//
//   Moves data for the current picture, and it's associated strip,
//   into and out of server.
//
//   If bSaveAndValidate is TRUE, updates picture  
//   info in the server with data from the picture.
//
//   If bSaveAndValidate is FALSE, updates the picture
//   with data from the server.
//
// Return Value:
//
//   True if update was successful
//
// Parameters:
//
//   BOOL bSaveAndValidate
//     Flag that determines which way the data flows
//
//	Remarks:
//   
//   This table show the data flows depending on bSaveAndValidate
//   
//   bSaveAndValidate          DATA FLOW
//   -----------------------------------------------------------------
//   TRUE                 Picture -> Scanner   
// 
//   FALSE                Scanner  -> Picture
//
//	  If bUpdateControls is FALSE then the Controls steop is skipped.		
*/
BOOL CTLAClientDemoDlg::UpdateServerData(int iIndex, BOOL bSaveAndValidate)
{
	BOOL bResult = TRUE;
	HRESULT hr;
	PiPicture *pPicture = NULL;
	PiPictureInfo *pPictureInfo = NULL;
	PiFramingUserInfo *pFramingUserInfo = NULL;
	PiColorSetting *pColorSetting = NULL;
	PiImage *pImage = NULL;

	// if we actually have pictures and a valid index
	if (m_Pictures.GetSize()  == 0 || iIndex > m_Pictures.GetSize())
		return FALSE;

	pPicture = m_Pictures.GetAt(iIndex);
	pFramingUserInfo = &(pPicture->m_PiFramingUserInfo);
	pColorSetting = &(pPicture->m_PiColorSetting);
	pPictureInfo = &(pPicture->m_PiPictureInfo);
	pImage = &(pPicture->m_PiImage);

	if(bSaveAndValidate)
	{
		// Update Picture Info
		if (pPictureInfo->m_iUpdate)
		{
			BSTR bstrFileName   = pPictureInfo->m_strFileName.AllocSysString();
			BSTR bstrFrameName  = pPictureInfo->m_strFrameName.AllocSysString();
			BSTR bstrDirectory  = pPictureInfo->m_strDirectory.AllocSysString();
			hr = m_pISavePictures->PutPictureInfo(
				iIndex,
				pPictureInfo->m_iFrameNumber,
				bstrFileName,
				bstrDirectory,
				pPictureInfo->m_iRotation,
				pPictureInfo->m_iSelectedHidden);

			if(FAILED(hr))
			{
				int iErrorNumber;
				if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
				{
					bResult = FALSE;
				}
				else
				{
					::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
				}
			}
			else
			{
				BSTR bstrFileName  = NULL;
				BSTR bstrFrameName = NULL;
				BSTR bstrDirectory = NULL;
				int iDummy;

				hr = m_pISavePictures->GetPictureInfo(
					iIndex,
					&iDummy,
					&iDummy,
					&iDummy,
					&iDummy,
					&bstrFrameName,
					&iDummy,
					&iDummy,
					&bstrFileName,
					&bstrDirectory,
					&iDummy,
					&iDummy);

				if(FAILED(hr))
				{
					int iErrorNumber;
					if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
					{
						bResult = FALSE;
					}
					else
					{
						::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
					}
				}
				else
				{
					// the previous put may have changed the frame number
					// so we must get the frame name
					pPictureInfo->m_strFrameName = bstrFrameName;
					if (pPictureInfo->m_iUpdate == UPDATE_DATA_AND_IMAGE)
						pImage->m_bResave = TRUE;
					pPictureInfo->m_iUpdate = FALSE;
				}
			}

			::SysFreeString(bstrFrameName);
			::SysFreeString(bstrFileName);
			::SysFreeString(bstrDirectory);
		}

		// Update Strip Info
/*		if (pPicture->m_PiStripInfo.m_bUpdate)
		{
			hr = m_pISavePictures->PutStripInfo(
				pPictureInfo->m_iCurrentStripIndex,
				pPictureInfo->m_iFilmProduct,
				pPictureInfo->m_iFilmSpecifier);
			if(FAILED(hr))
				::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			else
				pPictureInfo->m_bUpdate = FALSE;
		}
*/
		// Update Color Settings
		if (pColorSetting->m_iUpdate)
		{
			hr = m_pISavePictures->PutPictureColorSettings(
				iIndex,
				pColorSetting->m_iRed,
				pColorSetting->m_iGreen,
				pColorSetting->m_iBlue,
				pColorSetting->m_iBrightness,
				pColorSetting->m_iContrast,
				pColorSetting->m_iSharpness);

			if(FAILED(hr))
			{
				int iErrorNumber;
				if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
				{
					bResult = FALSE;
				}
				else
				{
					::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
				}
			}
			else
			{
				pImage->m_bResave = TRUE;
				pColorSetting->m_iUpdate = FALSE;
			}
		}

		// Update Framing Info
		if (pFramingUserInfo->m_iUpdate)
		{
			hr = m_pISavePictures->PutPictureFramingUserInfo(
				iIndex,
				pFramingUserInfo->m_lLeftHR,
				pFramingUserInfo->m_lTopHR,
				pFramingUserInfo->m_lRightHR,
				pFramingUserInfo->m_lBottomHR);

			if(FAILED(hr))
			{
				int iErrorNumber;
				if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
				{
					bResult = FALSE;
				}
				else
				{
					::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
				}
			}
			else
			{
				hr = m_pISavePictures->GetPictureFramingUserInfoLowRes(
					iIndex,
					&(pFramingUserInfo->m_lLeftLR),
					&(pFramingUserInfo->m_lTopLR),
					&(pFramingUserInfo->m_lRightLR),
					&(pFramingUserInfo->m_lBottomLR));

				if(FAILED(hr))
				{
					int iErrorNumber;
					if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
					{
						bResult = FALSE;
					}
					else
					{
						::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
					}
				}
				else
				{
					pImage->m_bResave = TRUE;
					pFramingUserInfo->m_iUpdate = FALSE;
				}
			}
		}
	}
	else // server -> picture
	{
		int iDummy;
		// Get Picture Info
		BSTR bstrFileName  = NULL;
		BSTR bstrFrameName = NULL;
		BSTR bstrDirectory = NULL;
		int iFrameType;
		hr = m_pISavePictures->GetPictureInfo(
			iIndex,
			&(pPictureInfo->m_iRollIndex),
			&(pPictureInfo->m_iStripIndex),
			&(pPictureInfo->m_iFilmProduct),
			&(pPictureInfo->m_iFilmSpecifier),
			&bstrFrameName,
			&(pPictureInfo->m_iFrameNumber),
			&iFrameType,
			&bstrFileName,
			&bstrDirectory,
			&(pPictureInfo->m_iRotation),
			&(iDummy));

		if(FAILED(hr))
		{
			int iErrorNumber;
			if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
			{
				bResult = FALSE;
			}
			else
			{
				::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
			}
		}

		pPictureInfo->m_iUpdate = FALSE;

		pPictureInfo->m_strFrameName = bstrFrameName;
		::SysFreeString(bstrFrameName);
		pPictureInfo->m_strFileName = bstrFileName;
		::SysFreeString(bstrFileName);
		pPictureInfo->m_strDirectory = bstrDirectory;
		::SysFreeString(bstrDirectory);

		// Get Strip Info
		BOOL bIsStrip;                      //dummy vars to pass in
		int iScanWarnings, iFilmColor, iFilmFormat;
		int i24mmFilmId;
		int iDmin_R, iDmin_G, iDmin_B;

		hr = m_pISavePictures->GetStripInfo(
			pPictureInfo->m_iStripIndex,
			&(pPictureInfo->m_iRollIndex),
			&bIsStrip,
			&iFilmColor,
			&iFilmFormat,
			&m_csStripHR.cy,
			&m_csStripHR.cx,
			&m_csStripLR.cy,
			&m_csStripLR.cx,
			&iScanWarnings,
			&(pPictureInfo->m_iFilmProduct),
			&(pPictureInfo->m_iFilmSpecifier),
			&i24mmFilmId,
			&iDmin_R,
			&iDmin_G,
			&iDmin_B);

		if(FAILED(hr))
		{
			int iErrorNumber;
			if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
			{
				bResult = FALSE;
			}
			else
			{
				::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
			}
		}

		// Get Color Setting
		hr = m_pISavePictures->GetPictureColorSettings(
			iIndex,
			&(pColorSetting->m_iRed),
			&(pColorSetting->m_iGreen),
			&(pColorSetting->m_iBlue),
			&(pColorSetting->m_iBrightness),
			&(pColorSetting->m_iContrast),
			&(pColorSetting->m_iSharpness));

		if(FAILED(hr))
		{
			int iErrorNumber;
			if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
			{
				bResult = FALSE;
			}
			else
			{
				::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
			}
		}

		pColorSetting->m_iUpdate = FALSE;

		// Get Framing Info
		hr = m_pISavePictures->GetPictureFramingUserInfo(
			iIndex,
			&(pFramingUserInfo->m_lLeftHR),
			&(pFramingUserInfo->m_lTopHR),
			&(pFramingUserInfo->m_lRightHR),
			&(pFramingUserInfo->m_lBottomHR));

		if(FAILED(hr))
		{
			int iErrorNumber;
			if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
			{
				bResult = FALSE;
			}
			else
			{
				::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
			}
		}

		hr = m_pISavePictures->GetPictureFramingUserInfoLowRes(
			iIndex,
			&(pFramingUserInfo->m_lLeftLR),
			&(pFramingUserInfo->m_lTopLR),
			&(pFramingUserInfo->m_lRightLR),
			&(pFramingUserInfo->m_lBottomLR));

		if(FAILED(hr))
		{
			int iErrorNumber;
			if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
			{
				bResult = FALSE;
			}
			else
			{
				::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
			}
		}

		pFramingUserInfo->m_iUpdate = FALSE;

		// Set Image so it re-paints
//		pImage->m_bPaint = TRUE;
//		pImage->m_pBitmap = NULL;
//		pImage->m_bReplace = FALSE;
	}

	return bResult;
}

/*
// afx_msg void OnMsgComCallBack(UINT ulOperation, LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
LRESULT CTLAClientDemoDlg::OnMsgComCallBack(UINT ulOperation, LONG lStatus)
{
	switch(ulOperation)
	{
		case WTO_InitializeProgress:
		case WTO_FirmwareUpdateApsProgress:
		case WTO_FirmwareUpdateCcdProgress:
		case WTO_FirmwareUpdateDxProgress:
		case WTO_FirmwareUpdateLampProgress:
		case WTO_FirmwareUpdateMotorProgress:
		{
			UpdateInitializeProgress(ulOperation, lStatus);
			break;
		}

		case WTO_InitializeError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_InitializeError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ITLAMain, IDS_COM_ERROR_INITIALIZE, lStatus);
			break;
		}

		case WTO_FirmwareUpdateApsError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_FirmwareUpdateApsError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ITLAMain, IDS_COM_ERROR_FIRMWARE_APS, lStatus);
			break;
		}

		case WTO_FirmwareUpdateCcdError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_FirmwareUpdateCcdError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ITLAMain, IDS_COM_ERROR_FIRMWARE_CCD, lStatus);
			break;
		}

		case WTO_FirmwareUpdateDxError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_FirmwareUpdateDxError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ITLAMain, IDS_COM_ERROR_FIRMWARE_DX, lStatus);
			break;
		}

		case WTO_FirmwareUpdateLampError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_FirmwareUpdateLampError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ITLAMain, IDS_COM_ERROR_FIRMWARE_LAMP, lStatus);
			break;
		}

		case WTO_FirmwareUpdateMotorError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_FirmwareUpdateMotorError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ITLAMain, IDS_COM_ERROR_FIRMWARE_MOTOR, lStatus);
			break;
		}

		case WTO_HardwareProgress:
		{	// Note that these status bits can also change with a WTO_HardwareError ulOperation
			// as demonstrated in HandleWTOError()
			if(HARDWARE_CB_FILM_SENSE_ENTRY & lStatus)
				TRACE(_T("TLA Client Demo - Film Sense Entry"));
			else
				TRACE(_T("TLA Client Demo - Film Absent Entry"));

			if(HARDWARE_CB_FILM_SENSE_EXIT & lStatus)
				TRACE(_T(" - Film Sense Exit\n"));
			else
				TRACE(_T(" - Film Absent Exit\n"));

			break;
		}

		case WTO_HardwareError:
		{
//			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_HardwareError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ITLAMain, IDS_COM_ERROR_HARDWARE, lStatus);
			break;
		}

		case WTO_HardwareAPSProgress:
		{	// Note that these status bits can also change with a WTO_HardwareAPSError ulOperation
			// as demonstrated in HandleWTOError()
			if(HARDWARE_CB_APS_CARTRIDGE_LOADED & lStatus)
				TRACE(_T("TLA Client Demo - APS Cartridge Loaded\n"));
			else
				TRACE(_T("TLA Client Demo - APS Cartridge Unloaded\n"));

			break;
		}

		case WTO_HardwareAPSError:
		{
//			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_HardwareAPSError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ITLAMain, IDS_COM_ERROR_HARDWARE_APS, lStatus);
			break;
		}
/*
		case WTO_DiagnosticsProgress:
		{
			UpdateVariousScanProgresses(lStatus);
			break;
		}

		case WTO_DiagnosticsError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_DiagnosticsError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_DIAGNOSTICS, lStatus);
			break;
		}
*/
		case WTO_AdvanceFilmProgress:
		case WTO_PutFilmGuidePositionProgress:
		case WTO_PutFilmPressureRollersPositionProgress:
		case WTO_F235C_ManualRetractProgress:
		case WTO_FilmTrackTestProgress:
		{
			UpdateVariousScanProgresses(ulOperation, lStatus);
			break;
		}

		case WTO_AdvanceFilmError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_AdvanceFilmError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_ADVANCE_FILM, lStatus);
			break;
		}

		case WTO_PutFilmGuidePositionError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_PutFilmGuidePositionError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_FILM_GUIDES, lStatus);
			break;
		}

		case WTO_PutFilmPressureRollersPositionError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_PutFilmPressureRollersPositionError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_PRESSURE_ROLLERS, lStatus);
			break;
		}

		case WTO_F235C_ManualRetractError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_F235C_ManualRetractError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_F235C_MANUALRETRACT, lStatus);
			break;
		}

		case WTO_FilmTrackTestError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_FilmTrackTestError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_TRACK_TEST, lStatus);
			break;
		}

		case WTO_CorrectionsProgress:
		case WTO_ExerciseSteppersProgress:
		case WTO_LampWarmUpProgress:
		{
			UpdateCorrectionsProgress(ulOperation, lStatus);
			break;
		}

		case WTO_CorrectionsError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_CorrectionsError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_CORRECTIONS, lStatus);
			break;
		}

		case WTO_ExerciseSteppersError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_ExerciseSteppersError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_EXERCISE_STEPPERS, lStatus);
			break;
		}

		case WTO_LampWarmUpError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_LampWarmUpError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_LAMP_WARMUP, lStatus);
			break;
		}

		case WTO_ScanProgress:
		{
			UpdateScanProgress(lStatus);
			break;
		}

		case WTO_ScanError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_ScanError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_SCAN, lStatus);
			break;
		}

		case WTO_SaveProgress:
		{
			UpdateSaveProgress(lStatus);
			break;
		}

		case WTO_SaveError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_SaveError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_ISavePictures, IDS_COM_ERROR_SAVE, lStatus);
			break;
		}

		case WTO_ImportFromFileProgress:
		{
			UpdateImportProgress(lStatus);
			break;
		}

		case WTO_ImportFromFileError:
		{
			TRACE(_T("TLA Client Demo - HandleWTOError - WTO_ImportFromFileError WTP: %d\n"), lStatus);
			HandleWTOError(INT_IID_IScanPictures, IDS_COM_ERROR_IMPORT, lStatus);
			break;
		}

		default:
		{
			ASSERT(0);
			break;
		}
	}

	return 0;
}

/*
// void UpdateInitializeProgress(UINT ulOperation, LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::UpdateInitializeProgress(UINT ulOperation, LONG lStatus)
{
	TRACE(_T("TLA Client Demo - UpdateInitializeProgress - WTO: %u WTP: %d\n"), ulOperation, lStatus);
	switch(lStatus)
	{
		case WTP_Initialize:
		case WTP_ProgressStart:
		{
			m_ProgressScan.SetPos(WTP_ProgressStart);
			switch(ulOperation)
			{
				case WTO_FirmwareUpdateApsProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Firmware Update Aps"));
					TRACE(_T("TLA Client Demo - Firmware Update Aps Started\n"));
					break;
				}

				case WTO_FirmwareUpdateCcdProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Firmware Update CCD"));
					TRACE(_T("TLA Client Demo - Firmware Update CCD Started\n"));
					break;
				}

				case WTO_FirmwareUpdateDxProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Firmware Update DX"));
					TRACE(_T("TLA Client Demo - Firmware Update DX Started\n"));
					break;
				}

				case WTO_FirmwareUpdateLampProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Firmware Update Lamp"));
					TRACE(_T("TLA Client Demo - Firmware Update Lamp Started\n"));
					break;
				}

				case WTO_FirmwareUpdateMotorProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Firmware Update Motor"));
					TRACE(_T("TLA Client Demo - Firmware Update Motor Started\n"));
					break;
				}

//				case WTO_InitializeProgress:
				default:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Initializing Scanner"));
					TRACE(_T("TLA Client Demo - Initialization Started\n"));
					break;
				}
			}

			break;
		}

		case WTP_ProgressComplete:
		{
			m_bScannerInitializationCompleted = TRUE;
			CompleteInitialization();
			SetDlgItemText(IDC_SCAN_STATUS, _T("Idle"));
			m_ProgressScan.SetPos(WTP_ProgressStart);
			SetDlgScanning(FALSE);
			TRACE(_T("TLA Client Demo - Initialization Complete\n"));
			break;
		}

		default:
		{
			m_ProgressScan.SetPos(lStatus);
			break;
		}
	}
}


/*
// void UpdateVariousScanProgresses(UINT ulOperation, LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::UpdateVariousScanProgresses(UINT ulOperation, LONG lStatus)
{
	TRACE(_T("TLA Client Demo - UpdateVariousScanProgresses - WTO: %u WTP: %d\n"), ulOperation, lStatus);
	switch(lStatus)
	{
		case WTP_Initialize:
		case WTP_ProgressStart:
		{
			m_ProgressScan.SetPos(WTP_ProgressStart);
			switch(ulOperation)
			{
				case WTO_PutFilmGuidePositionProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Moving Film Guides"));
					TRACE(_T("TLA Client Demo - Moving Film Guides Started\n"));
					break;
				}

				case WTO_PutFilmPressureRollersPositionProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Moving Pressure Rollers"));
					TRACE(_T("TLA Client Demo - Moving Pressure Rollers Started\n"));
					break;
				}

				case WTO_F235C_ManualRetractProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("F235C Manual Retract"));
					TRACE(_T("TLA Client Demo - F235C Manual Retract Started\n"));
					break;
				}

				case WTO_FilmTrackTestProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Film Track Test"));
					TRACE(_T("TLA Client Demo - Film Track Test Started\n"));
					break;
				}

//				case WTO_AdvanceFilmProgress:
				default:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Advancing Film"));
					TRACE(_T("TLA Client Demo - Advance Film Started\n"));
					break;
				}
			}

			break;
		}

		case WTP_ProgressComplete:
		{
			switch(ulOperation)
			{
				case WTO_PutFilmGuidePositionProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Film Guides Set"));
					TRACE(_T("TLA Client Demo - Moving Film Guides Complete\n"));
					m_ProgressScan.SetPos(WTP_ProgressEnd);
					SetDlgScanning(FALSE);
					break;
				}

				case WTO_PutFilmPressureRollersPositionProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Pressure Rollers Set"));
					TRACE(_T("TLA Client Demo - Moving Pressure Rollers Complete\n"));
					m_ProgressScan.SetPos(WTP_ProgressEnd);
					SetDlgScanning(FALSE);
					break;
				}

				case WTO_F235C_ManualRetractProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("F235C Manual Retract Complete"));
					TRACE(_T("TLA Client Demo - F235C Manual Retract Complete\n"));
					m_ProgressScan.SetPos(WTP_ProgressEnd);
					SetDlgScanning(FALSE);
					break;
				}

				case WTO_FilmTrackTestProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Film Track Test Complete"));
					TRACE(_T("TLA Client Demo - Film Track Test Complete\n"));
					m_ProgressScan.SetPos(WTP_ProgressEnd);
					SetDlgScanning(FALSE);
					break;
				}

//				case WTO_AdvanceFilmProgress:
				default:
				{
					if(m_iAdvanceFilmMilliseconds != -1)
					{
						SetDlgItemText(IDC_SCAN_STATUS, _T("Advance Film Complete"));
						TRACE(_T("TLA Client Demo - Advance Film Complete\n"));
						m_ProgressScan.SetPos(WTP_ProgressEnd);
						SetDlgScanning(FALSE);
					}

					break;
				}
			}

			break;
		}

		default:
		{
			m_ProgressScan.SetPos(lStatus);
			break;
		}
	}
}

/*
// void UpdateCorrectionsProgress(LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::UpdateCorrectionsProgress(UINT ulOperation, LONG lStatus)
{
	TRACE(_T("TLA Client Demo - UpdateCorrectionsProgress - WTO: %u WTP: %d\n"), ulOperation, lStatus);
	switch(lStatus)
	{
		case WTP_Initialize:
		case WTP_ProgressStart:
		{
			m_ProgressScan.SetPos(WTP_ProgressStart);
			switch(ulOperation)
			{
				case WTO_ExerciseSteppersProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Exercise Steppers"));
					TRACE(_T("TLA Client Demo - Exercise Steppers Started\n"));
					break;
				}

				case WTO_LampWarmUpProgress:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Lamp Warm Up"));
					TRACE(_T("TLA Client Demo - Lamp Warm Up Started\n"));
					break;
				}

//				case WTO_CorrectionsProgress:
				default:
				{
					SetDlgItemText(IDC_SCAN_STATUS, _T("Corrections"));
					TRACE(_T("TLA Client Demo - Corrections Started\n"));
					break;
				}
			}

			break;
		}

		case WTP_ProgressComplete:
		{
			SetDlgItemText(IDC_SCAN_STATUS, _T("Corrections Complete"));
			m_ProgressScan.SetPos(WTP_ProgressEnd);
			SetDlgScanning(FALSE);
			TRACE(_T("TLA Client Demo - Corrections Complete\n"));
			break;
		}

		default:
		{
			m_ProgressScan.SetPos(lStatus);
			break;
		}
	}
}

/*
// void UpdateScanProgress(LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::UpdateScanProgress(LONG lStatus)
{
	TRACE(_T("TLA Client Demo - UpdateScanProgress - WTO_ScanProgress WTP: %d\n"), lStatus);
	switch(lStatus)
	{
		case WTP_Initialize:
		{
			SetDlgItemText(IDC_SCAN_STATUS, _T("Scanning"));
			m_ProgressScan.SetPos(WTP_ProgressStart);
			TRACE(_T("TLA Client Demo - Scan Started\n"));
			break;
		}

		case WTP_ProgressStart:
		{
//			SetDlgItemText(IDC_SCAN_STATUS, _T("Scanning"));
//			m_ProgressScan.SetPos(WTP_ProgressStart);
			SetDlgScanning(TRUE);
			break;
		}

		case WTP_ProgressComplete:
		{
			UpdateScanCounts();
			SetDlgItemText(IDC_SCAN_STATUS, _T("Finished Scanning"));
			m_ProgressScan.SetPos(WTP_ProgressEnd);
			SetDlgScanning(FALSE);
			TRACE(_T("TLA Client Demo - Scan Complete\n"));

			if(1 < m_uiScanQuantity)
			{
				if(m_uiScanQuantityCounter == 0)
					break;

				m_uiScanQuantityCounter--;
				CString str;
				str.Format(_T("%03d Done\n%03d Left"), m_uiScanQuantity - m_uiScanQuantityCounter, m_uiScanQuantityCounter);
				GetDlgItem(IDC_TEST_COUNT)->SetWindowText(str);
				TRACE(L"TLA Client Demo - ONLY %d SCANS TO GO!!!!!!!!!!!!!!!!!!!!!!!!!!\n", m_uiScanQuantityCounter);
				UpdateScanCounts();
				if(0 < m_iScanRollCount)
				{
					m_pISavePictures->DeleteRollInScanGroup(0);
					UpdateScanCounts();
				}

				CWnd *pScan = GetDlgItem(IDC_SCAN);
				::PostMessage(pScan->GetSafeHwnd(), BM_CLICK, 0, 0);
				SetDlgScanning(TRUE);
			}

			break;
		}

		default:
		{
			m_ProgressScan.SetPos(lStatus);
			break;
		}
	}
}

void CTLAClientDemoDlg::UpdateImportProgress(LONG lStatus)
{
	TRACE(_T("TLA Client Demo - UpdateImportProgress - WTO_ImportFromFileProgress WTP: %d\n"), lStatus);
	switch(lStatus)
	{
		case WTP_Initialize:
		{
			SetDlgItemText(IDC_SCAN_STATUS, _T("Importing"));
			m_ProgressScan.SetPos(WTP_ProgressStart);
			TRACE(_T("TLA Client Demo - Import Started\n"));
			break;
		}

		case WTP_ProgressStart:
		{
//			SetDlgItemText(IDC_SCAN_STATUS, _T("Scanning"));
//			m_ProgressScan.SetPos(WTP_ProgressStart);
			SetDlgScanning(TRUE);
			break;
		}

		case WTP_ProgressComplete:
		{
			UpdateScanCounts();
			SetDlgItemText(IDC_SCAN_STATUS, _T("Finished Importing"));
			m_ProgressScan.SetPos(WTP_ProgressEnd);
			SetDlgScanning(FALSE);
			TRACE(_T("TLA Client Demo - Import Complete\n"));
			break;
		}

		default:
		{
			m_ProgressScan.SetPos(lStatus);
			break;
		}
	}
}

/*
// void UpdateSaveProgress(LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::UpdateSaveProgress(LONG lStatus)
{
	TRACE(_T("TLA Client Demo - UpdateSaveProgress - WTO_SaveProgress WTP: %d\n"), lStatus);
	switch(m_iSaveMode)
	{
		case SAVE_MODE_NEW:
		{	
			UpdateDisplayNewProgress(lStatus);
			break;
		}

		case SAVE_MODE_EDIT:
		{	
			UpdateDisplayEditProgress(lStatus);
			break;
		}

//		case SAVE_MODE_EXPORT:
		default:
		{	
			UpdateExportProgress(lStatus);
			break;
		}
	}
}

/*
// void UpdateDisplayNewProgress(LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::UpdateDisplayNewProgress(LONG lStatus)
{
	switch(lStatus)
	{
		case WTP_Initialize:
		case WTP_ProgressStart:
		{
			SetDlgItemText(IDC_SAVE_STATUS, _T("Transfering Pictures"));
			m_ProgressSave.SetPos(WTP_ProgressStart);
			TRACE(_T("TLA Client Demo - Save Started\n"));
			break;
		}
		case WTP_ProgressComplete:
		{
			m_ClientBuffers.Delete();
			SetDlgItemText(IDC_SAVE_STATUS, _T("Finished Transfering Pictures"));
			m_ProgressSave.SetPos(WTP_ProgressEnd);
			SetDlgSaving(SAVE_MODE_IDLE);
			UpdatePicture();
			if (m_bResave)
			{
				TRACE0("m_bResave after NewSave was TRUE!\n");
				PostMessage(WM_RESAVE_IMAGES);
				m_bResave = FALSE;
			}
			TRACE(_T("TLA Client Demo - Save Complete\n"));
 			break;
		}
		default:
		{
			UCHAR * pDib = m_ClientBuffers.GetActiveBuffer();
			AddBitmap(pDib);
			m_ClientBuffers.bNextBuffer();
			m_ProgressSave.SetPos(lStatus);
			break;
		}
	}
}

/*
// void UpdateDisplayEditProgress(LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::UpdateDisplayEditProgress(LONG lStatus)
{
	switch(lStatus)
	{
		case WTP_Initialize:
		case WTP_ProgressStart:
		{
			SetDlgItemText(IDC_SAVE_STATUS, _T("Transfering Pictures"));
			m_ProgressSave.SetPos(WTP_ProgressStart);
			TRACE(_T("TLA Client Demo - Save Started\n"));
			break;
		}
		case WTP_ProgressComplete:
		{
			m_ClientBuffers.Delete();
			SetDlgItemText(IDC_SAVE_STATUS, _T("Finished Transfering Pictures"));
			m_ProgressSave.SetPos(WTP_ProgressEnd);
			SetDlgSaving(SAVE_MODE_IDLE);
			UpdatePicture();
			if (m_bResave)
			{
				TRACE0("m_bResave after EditSave was TRUE!\n");
				PostMessage(WM_RESAVE_IMAGES);
				m_bResave = FALSE;
			}
			TRACE(_T("TLA Client Demo - Save Complete\n"));
			break;
		}
		default:
		{
			UCHAR * pDib = m_ClientBuffers.GetActiveBuffer();
			ReplaceBitmap(pDib);
			m_ClientBuffers.bNextBuffer();
			m_ProgressSave.SetPos(lStatus);
			break;
		}
	}
}

/*
// void AddBitmap(UCHAR * pDib)
// 
// Description:
//	  Converts dib to CBitmap and adds it into first NULL bitmap
// 
// Parameters:
//	  pDib		Ptr to a DIB in memory
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::AddBitmap(UCHAR * pDib)
{
	BITMAPINFO *pBitMapInfo = (BITMAPINFO*)pDib;
	BITMAPINFOHEADER *pBitMapInfoHeader = &(pBitMapInfo->bmiHeader);

	UINT uiBitmapInfoSize;
	if(pBitMapInfoHeader->biBitCount < 16)
	{
		uiBitmapInfoSize = sizeof(BITMAPINFOHEADER) + 
									((1 << pBitMapInfoHeader->biBitCount) * sizeof(RGBQUAD));
	} 
	else
		uiBitmapInfoSize = sizeof(BITMAPINFO);

	UCHAR *pucBitmapBits = pDib + uiBitmapInfoSize;

	CDC* pDC = GetDC();
	CBitmap* pBitmap = new CBitmap();
	HBITMAP hBitmap = CreateDIBitmap(pDC->m_hDC, pBitMapInfoHeader, CBM_INIT, pucBitmapBits, pBitMapInfo, DIB_RGB_COLORS);
	if (hBitmap == NULL)
	{
		DWORD iError = GetLastError();
		ASSERT(0);
	}
	else
	{
		if (pBitmap->Attach(hBitmap))
		{
			// set the first null bitmap to this bitmap
			for (int i = 0; i < m_Pictures.GetSize(); i++)
			{
				if (m_Pictures[i]->m_PiImage.m_pBitmap == NULL)
				{
					m_Pictures[i]->m_PiImage.m_pBitmap = pBitmap;
					break;
				}
			}
		}
		else
		{
			ASSERT(0);
		}
	}
	ReleaseDC(pDC);
}

/*
// void ReplaceBitmap(int iIndex, UCHAR * pDib)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::ReplaceBitmap(UCHAR * pDib)
{
//	TRACE0("CTLAClientDemoDlg: ReplaceBitmap\n");
	BOOL bReplaced = FALSE;
	CBitmap *pNewBitmap;
	BITMAPINFO *pBitMapInfo = (BITMAPINFO*)pDib;
	BITMAPINFOHEADER *pBitMapInfoHeader = &(pBitMapInfo->bmiHeader);

	UINT uiBitmapInfoSize;
	if(pBitMapInfoHeader->biBitCount < 16)
	{
		uiBitmapInfoSize = sizeof(BITMAPINFOHEADER) + 
									((1 << pBitMapInfoHeader->biBitCount) * sizeof(RGBQUAD));
	} 
	else
		uiBitmapInfoSize = sizeof(BITMAPINFO);

	UCHAR *pucBitmapBits = pDib + uiBitmapInfoSize;

	CDC* pDC = GetDC();
	pNewBitmap = new CBitmap();
	if (pNewBitmap->Attach(CreateDIBitmap(pDC->m_hDC, pBitMapInfoHeader, 
		CBM_INIT, pucBitmapBits, pBitMapInfo, DIB_RGB_COLORS)))
	{
		for (int iIndex = 0; iIndex < m_Pictures.GetSize(); iIndex++)
		{
			if (m_Pictures[iIndex]->m_PiImage.m_bReplace)
			{
				delete (m_Pictures[iIndex]->m_PiImage.m_pBitmap);
				m_Pictures[iIndex]->m_PiImage.m_pBitmap = pNewBitmap;
				m_Pictures[iIndex]->m_PiImage.m_bReplace = FALSE;
				bReplaced = TRUE;
//				TRACE1("Bitmap %d Replaced\n", iIndex);
				break;
			}
		}
	}

	if (!bReplaced)
	{
		delete pNewBitmap;
		ASSERT(0);
	}

	ReleaseDC(pDC);
}

/*
// void PaintPicture()
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::PaintPicture()
{
//	TRACE0("CTLAClientDemoDlg: PaintPicture\n");
	// where are we going to paint
	CRect crDisplayLimit;
	CWnd *pWndPicture = GetDlgItem(IDC_PICTURE);
	CDC* pDC = pWndPicture->GetDC();
	pWndPicture->GetClientRect(crDisplayLimit);
	CPen pPen;
	pPen.CreatePen(PS_SOLID, 2, pDC->GetBkColor());
	CPen* pOldPen = pDC->SelectObject(&pPen);
	pDC->Rectangle(crDisplayLimit.left+4, crDisplayLimit.top+4, crDisplayLimit.right-4, crDisplayLimit.bottom-4);
	pDC->SelectObject(pOldPen);
	crDisplayLimit.DeflateRect(MARGIN, MARGIN);

	if (m_bDisplayFourPictures)
	{
		for (int i = m_iIndex, count = 0; count < 4; i++, count++)
		{
			if (i < m_Pictures.GetSize() && 
				 m_Pictures[m_iIndex]->m_PiImage.m_pBitmap)
			{
				pWndPicture->GetClientRect(crDisplayLimit);
				crDisplayLimit.DeflateRect(MARGIN, MARGIN);
				switch (count)
				{
				default:
				case 0:	// top left quadrant
					crDisplayLimit.right  -= ((crDisplayLimit.Width() / 2) + HALF_MARGIN);
					crDisplayLimit.bottom -= ((crDisplayLimit.Height() / 2) + HALF_MARGIN);
					break;
				case 1:	// top right quadrant
					crDisplayLimit.left   += ((crDisplayLimit.Width() / 2) + HALF_MARGIN);
					crDisplayLimit.bottom -= ((crDisplayLimit.Height() / 2) + HALF_MARGIN);
					break;
				case 2:	// bottom left quadrant
					crDisplayLimit.right  -= ((crDisplayLimit.Width() / 2) + HALF_MARGIN);
					crDisplayLimit.top    += ((crDisplayLimit.Height() / 2) + HALF_MARGIN);
					break;
				case 3:	// bottom right quadrant
					crDisplayLimit.left   += ((crDisplayLimit.Width() / 2) + HALF_MARGIN);
					crDisplayLimit.top    += ((crDisplayLimit.Height() / 2) + HALF_MARGIN);
					break;
				}
				DrawBitmap(pDC, m_Pictures[i]->m_PiImage.m_pBitmap, crDisplayLimit);
			}
		}
	}
	else
	{
		if (m_Pictures.GetSize() > m_iIndex)
		{
			PiPicture* pPicture = m_Pictures[m_iIndex];
			if (pPicture->m_PiImage.m_pBitmap)
			{
				DrawBitmap(pDC, m_Pictures[m_iIndex]->m_PiImage.m_pBitmap, crDisplayLimit);
			}
		}
	}
	ReleaseDC(pDC);
}

/*
// void DrawBitmap()
// 
// Description:
//
//   Draws a bitmap within the limiting rectangle.
//   The bitmap is automatically resized and centered.
//	  If the rectangle is bigger than the bitmap the 
//   bitmap is displayed as original size.	
//
//	Parameters:
//		pDC 					device context to draw on
//		pBitmap 				bitmap to be drawn
//		crDisplayLimit		limiting rectangle
//
*/
void CTLAClientDemoDlg::DrawBitmap(CDC *pDC,CBitmap * pBitmap,RECT crDisplayLimit)
{
//	TRACE0("CTLAClientDemoDlg: DrawBitmap\n");
	BITMAP BitmapInfo;
	CBitmap * pOldBitmap;
	CDC dcMemory;
	int XSrc,YSrc,XDest,YDest;
	int iRatio, iAspectRatioSrc, iAspectRatioDest;
	int XOffset = 0, YOffset = 0;
	BOOL bResize = TRUE;
	int Margin = 0;

	//create a compatible memory context
	dcMemory.CreateCompatibleDC(pDC);
	pOldBitmap = (CBitmap*)dcMemory.SelectObject(pBitmap);

	// get the bitmap info
	pBitmap->GetObject(sizeof(BITMAP), &BitmapInfo);

	// What is the output rect (must maintain aspect ratio)
	YSrc =  BitmapInfo.bmHeight;
	XSrc =  BitmapInfo.bmWidth;
	iAspectRatioSrc  = XSrc  * 100 / YSrc;
	YDest = crDisplayLimit.bottom - crDisplayLimit.top  - (Margin * 2);
	XDest = crDisplayLimit.right  - crDisplayLimit.left - (Margin * 2);
	iAspectRatioDest = XDest * 100 / YDest;
	if (iAspectRatioSrc < iAspectRatioDest)
	{
		// Height is limiting dimension
		iRatio = YDest * 100 / YSrc;
		XDest = XSrc * iRatio / 100;
		XOffset = (int)((crDisplayLimit.right - crDisplayLimit.left - XDest)/2);
		// if the height of the bitmap is less than the height of the rect
		// then dont resize it
		if(YSrc <= (crDisplayLimit.bottom - crDisplayLimit.top - (Margin * 2)))
		{
			XDest = XSrc;
			YDest = YSrc;
			YOffset = (int)((crDisplayLimit.bottom - crDisplayLimit.top - YDest)/2);
			XOffset = (int)((crDisplayLimit.right - crDisplayLimit.left - XDest)/2);
			bResize = FALSE;
		}
	}
	else
	{
		// Width is limiting dimension
		iRatio = XDest * 100 / XSrc;
		YDest = YSrc * iRatio / 100;
		YOffset = (int)((crDisplayLimit.bottom - crDisplayLimit.top - YDest)/2);
		// if  the width of the bitmap is less than the width of the rect
		// then dont resize it
		if(XSrc <= (crDisplayLimit.right - crDisplayLimit.left - (Margin * 2)))
		{
			XDest = XSrc;
			YDest = YSrc;
			YOffset = (int)((crDisplayLimit.bottom - crDisplayLimit.top - YDest)/2);
			XOffset = (int)((crDisplayLimit.right - crDisplayLimit.left - XDest)/2);
			bResize = FALSE;
		}
	}

	//draw the bitmap
	if(bResize){
			pDC->SetStretchBltMode(HALFTONE);
			pDC->StretchBlt(crDisplayLimit.left+Margin+XOffset,crDisplayLimit.top+Margin+YOffset,XDest,YDest,
			&dcMemory,0,0, XSrc, YSrc,SRCCOPY);
	}
	else{
		pDC->BitBlt(crDisplayLimit.left+Margin+XOffset,crDisplayLimit.top+Margin+YOffset,XDest,YDest,
			&dcMemory,0,0,SRCCOPY);
	}

	//remove the compatible memory context
	dcMemory.SelectObject(pOldBitmap);
}

/*
// void UpdateExportProgress(LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::UpdateExportProgress(LONG lStatus)
{
	switch(lStatus)
	{
		case WTP_Initialize:
		case WTP_ProgressStart:
		{
			SetDlgItemText(IDC_SAVE_STATUS, _T("Saving Pictures"));
			m_ProgressSave.SetPos(WTP_ProgressStart);
			TRACE(_T("TLA Client Demo - Save Started\n"));
			break;
		}
		case WTP_ProgressComplete:
		{
			SetDlgItemText(IDC_SAVE_STATUS, _T("Finished Saving Pictures"));
			m_ProgressSave.SetPos(WTP_ProgressEnd);
			m_ClientBuffers.Delete();
			SetDlgSaving(SAVE_MODE_IDLE);
			if (m_bResave)
			{
				TRACE0("m_bResave after ExportSave was TRUE!\n");
				PostMessage(WM_RESAVE_IMAGES);
				m_bResave = FALSE;
			}
			TRACE(_T("TLA Client Demo - Save Complete\n"));
			break;
		}
		default:
		{
			// update progress indicators
			m_ProgressSave.SetPos(lStatus);

			// if doing save to memory
			if (m_iSaveMethod != 0)
			{
				// if save to memory is a dib
				if (m_iSaveToMemoryFormat == iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8)
				{
					if(0 < m_clSaveToSharedMemoryDirAndFile.GetCount())
					{
#ifdef __AFXMT_H__
						ASSERT(0);	// FIXME - May need CCriticalSection if client is multithreaded
#endif
						SaveFileUsingSaveToSharedMemory();
					}
					else if (0 < m_clSaveToClientMemoryDirAndFile.GetCount())
					{
#ifdef __AFXMT_H__
						ASSERT(0);	// FIXME - May need CCriticalSection if client is multithreaded
#endif
						Save8BitDibUsingSaveToClientMemory();
					}
				}
				// if doing save to memory as planar
				else
				{
					SavePlanarUsingSaveToClientMemory();
				}
				m_ClientBuffers.bNextBuffer();
			}
			// else if save method is directly to disk, nothing to do
			break;
		}
	}
}

/*
// void HandleWTOError(int iShort_IID, int iOperationID, LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::HandleWTOError(int iShort_IID, int iOperationID, LONG lStatus)
{
	USES_CONVERSION;
	BSTR bstrError = NULL;
	BSTR bstrErrorNumbers = NULL;
	int iError;
	CString strCaption;
	CString strMsg;
	strCaption.LoadString(iOperationID);

	if((INT_IID_IScanPictures == iShort_IID) && !m_bScannerInitializationCompleted)
		iShort_IID = INT_IID_ITLAMain;

	HRESULT hr = m_pITLAMain->GetAndClearLastError(
		iShort_IID,
		&bstrError, 
		&bstrErrorNumbers, 
		&iError);

	if(FAILED(hr))
		::AnalyzeComError(hr, IID_ITLAMain, m_pITLAMain, NULL, FALSE);
	else if(0 == iError)
	{
		strMsg.LoadString(IDS_COM_ERROR_NO_DATA);
		MessageBox(strMsg, strCaption, MB_ICONERROR | MB_OK);
	}
	else
	{
		if(IDS_COM_ERROR_HARDWARE == iOperationID)
		{	// Note that these status bits can also change with a WTO_HardwareProgress ulOperation
			// as demonstrated in OnMsgComCallBack()
			if(HARDWARE_CB_FILM_SENSE_ENTRY & lStatus)
				TRACE(_T("TLA Client Demo - Film Sense Entry"));
			else
				TRACE(_T("TLA Client Demo - Film Absent Entry"));

			if(HARDWARE_CB_FILM_SENSE_EXIT & lStatus)
				TRACE(_T(" - Film Sense Exit"));
			else
				TRACE(_T(" - Film Absent Exit"));

			CString str(_T("   - Hardware Fault(s)\n"));
			if(HARDWARE_CB_HOST_FAULT & lStatus)
				str += _T("   -  Host Board Fault\n");

			if(HARDWARE_CB_DX_FAULT & lStatus)
				str += _T("   -  Dx Board Fault\n");

			if(HARDWARE_CB_LAMP_FAULT & lStatus)
				str += _T("   -  Lamp Board Fault\n");

			if(HARDWARE_CB_CCD_FAULT & lStatus)
				str += _T("   -  CCD Board Fault\n");

			if(HARDWARE_CB_MOTOR_FAULT & lStatus)
				str += _T("   -  Motor Board Fault\n");

			if(HARDWARE_CB_LAMP_CURRENT_HI_WARNING & lStatus)
				str += _T("   -  Lamp Current High Warning\n");

			if(HARDWARE_CB_LAMP_CURRENT_HI_ERROR & lStatus)
				str += _T("   -  Lamp Current High Error\n");

			if(HARDWARE_CB_LAMP_TEMP_HI_WARNING & lStatus)
				str += _T("   -  Lamp Temperature High Warning\n");

			if(HARDWARE_CB_LAMP_TEMP_HI_ERROR & lStatus)
				str += _T("   -  Lamp Temperature High Error\n");

			if(HARDWARE_CB_LAMP_BURN_OUT_ERROR & lStatus)
				str += _T("   -  Lamp Burn Out\n");

			if(HARDWARE_CB_LAMP_FAN_SLOW_WARNING & lStatus)
				str += _T("   -  Lamp Fan Slow Warning\n");

			if(HARDWARE_CB_LAMP_FAN_SLOW_ERROR & lStatus)
				str += _T("   -  Lamp Fan Slow Error\n");

			if(HARDWARE_CB_MOTOR_STEPPER_CCD & lStatus)
				str += _T("   -  Stepper CCD Indeterminate\n");

			if(HARDWARE_CB_MOTOR_STEPPER_LENS & lStatus)
				str += _T("   -  Stepper Lens Indeterminate\n");

			if(HARDWARE_CB_MOTOR_FILTER_WHEEL & lStatus)
				str += _T("   -  Motor Filter Wheel Indeterminate\n");

			if(HARDWARE_CB_MOTOR_FILM_GUIDE & lStatus)
				str += _T("   -  Motor Film Guide Indeterminate\n");

			if(HARDWARE_CB_FILM_EMULSION_DOWN & lStatus)
				str += _T("   -  Film Emulsion Down\n");

			if(HARDWARE_CB_FILM_TAIL_FIRST & lStatus)
				str += _T("   -  Film Tail First\n");

			if(HARDWARE_CB_CLEANING_REQUIRED & lStatus)
				str += _T("   -  Lens/Light Bar Cleaning Required\n");

			str += W2T(bstrError);
			MessageBox(str, strCaption, MB_ICONERROR | MB_OK);
		}
		else if(IDS_COM_ERROR_HARDWARE_APS == iOperationID)
		{	// Note that these status bits can also change with a WTO_HardwareAPSProgress ulOperation
			// as demonstrated in OnMsgComCallBack()
			TRACE(_T("TLA Client Demo"));
			if(HARDWARE_CB_APS_CARTRIDGE_LOADED & lStatus)
				TRACE(_T(" - APS Cartridge Loaded\n"));
			else
				TRACE(_T(" - APS Cartridge Unloaded\n"));

			CString str(_T("   - Hardware Fault(s) APS\n"));
			if(HARDWARE_CB_APS_FAULT & lStatus)
				str += _T("   - Aps Board Fault\n");

			if(HARDWARE_CB_APS_FILM_JAM_EXTRACT & lStatus)
				str += _T("   - APS Extract Film Jam\n");

			if(HARDWARE_CB_APS_FILM_JAM_SCAN & lStatus)
				str += _T("   - APS Scan Film Jam\n");

			if(HARDWARE_CB_APS_FILM_JAM_RETRACT & lStatus)
				str += _T("   - APS Retract Film Jam\n");

			if(HARDWARE_CB_APS_EJECT_ERROR & lStatus)
				str += _T("   - APS Eject Button Pressed Error\n");

			if(HARDWARE_CB_APS_UNPROCESSED_ERROR & lStatus)
				str += _T("   - APS Unprocessed Cartridge Error\n");

			if(HARDWARE_CB_APS_CART_UNPACKED_ERROR & lStatus)
				str += _T("   - APS Cartridge Unpacked Error\n");

			if(HARDWARE_CB_APS_PARK_INIT_ERROR & lStatus)
				str += _T("   - APS Park Initialization Error\n");

			if(HARDWARE_CB_APS_PARK_ERROR & lStatus)
				str += _T("   - APS Park Error\n");

			str += W2T(bstrError);
			MessageBox(str, strCaption, MB_ICONERROR | MB_OK);
		}
		else
			MessageBox(W2T(bstrError), strCaption, MB_ICONERROR | MB_OK);
	}

	::SysFreeString(bstrError);
	::SysFreeString(bstrErrorNumbers);

	switch(iShort_IID)
	{
		case INT_IID_ISavePictures:
		{
			// release buffers 
			if(FAILED(m_pISavePictures->ClientMemoryBufferDismissAll()))
				::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);

			m_ClientBuffers.Delete();

			while (m_clSaveToClientMemoryDirAndFile.GetCount())
				m_clSaveToClientMemoryDirAndFile.RemoveHead();

			SetDlgSaving(SAVE_MODE_IDLE);
			break;
		}

		case INT_IID_IScanPictures:
		{
			SetDlgScanning(FALSE);
			break;
		}

		default:
		{
			if(IDS_COM_ERROR_HARDWARE == iOperationID)
				break;

			if(IDS_COM_ERROR_HARDWARE_APS == iOperationID)
			{
				SetDlgScanning(FALSE);
				break;
			}

			strMsg.LoadString(IDS_COM_ERROR_SHUTDOWN);
			strCaption.LoadString(AFX_IDS_APP_TITLE);
			MessageBox(
				strMsg, 
				strCaption, 
				MB_ICONERROR | MB_OK);

			PostMessage(WM_CLOSE, 0, 0);	// Shut down application
			break;
		}
	}
}

/*
// void CompleteInitialization()
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::CompleteInitialization()
{
	SetDlgItemText(IDC_STATIC_PROGRESS, _T("Scanner Initialized"));
	GetDlgItem(IDC_ADVANCE_FILM)->EnableWindow(TRUE);
	GetDlgItem(IDC_SCAN)->EnableWindow(TRUE);
	GetDlgItem(IDC_CONNECT)->EnableWindow(TRUE);
	m_ProgressScan.SetPos(WTP_ProgressEnd);

	::Sleep(100);		// Allow Server's worker thread to exit

	// be sure the COM pointer is good
	ASSERT(NULL != m_pIScanPictures);

	// Check for soft errors during init
	int iInitializeWarnings;
	HRESULT hr = m_pITLAMain->GetInitializeWarnings(&iInitializeWarnings);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		return;
	}

	// check for errors
	if(iInitializeWarnings & INITIALIZEW_EEPROM_BLANK)
	{
		// EEPROM not initialized
		AfxMessageBox(_T("TLAClientDemo WARNING - EEPROM NOT INITIALIZED\n"));
	}
	
	if(iInitializeWarnings & INITIALIZEW_EEPROM_CHECKSUM_BAD)
	{
		// EEPROM read failed
		AfxMessageBox(_T("TLAClientDemo WARNING - EEPROM CHECKSUM_BAD\n"));
	}

	BSTR bstrRomVersion = NULL;
	BSTR bstrScannerModel = NULL;
	BSTR bstrSaveToSharedMemoryEventName = NULL;
	BSTR bstrSaveToSharedMemoryMapName = NULL;
	BSTR bstrTLAVersion = NULL;

	hr = m_pIScanPictures->GetScannerInfo000(
		&m_iScannerType,
		&bstrRomVersion,
		&bstrScannerModel,
		&m_iScannerSerialNumber,
		&m_iScannerVersionHardware,
		&bstrTLAVersion,
		&bstrSaveToSharedMemoryEventName,
		&bstrSaveToSharedMemoryMapName,
		&m_iDarkPointCorrectInterval,
		&m_iColorPortraitMode,
		&m_i_GetScanLineTimeOut,
		&m_iNoFilmTimeOut,
		&m_iLampSaverSec);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}

	::SysFreeString(bstrRomVersion);
	::SysFreeString(bstrScannerModel);
	::SysFreeString(bstrTLAVersion);

	// Set up shared memory for SaveDibToMemory
	if(NULL != bstrSaveToSharedMemoryEventName)
	{
		CString strSaveToSharedMemoryEventName(bstrSaveToSharedMemoryEventName);
		::SysFreeString(bstrSaveToSharedMemoryEventName);
		CString strSaveToSharedMemoryMapName(bstrSaveToSharedMemoryMapName);
		::SysFreeString(bstrSaveToSharedMemoryMapName);
							// Open event for controlling shared memory access
		m_hFileMapEventHandle = ::OpenEvent(EVENT_MODIFY_STATE, FALSE, strSaveToSharedMemoryEventName);
		if(NULL == m_hFileMapEventHandle)
		{
			::GetLastErrorDisplay(this, _T("OpenEvent Error"));
			return;
		}
							// Open file mapping object
		m_hFileMapHandle = ::OpenFileMapping(FILE_MAP_READ, FALSE, strSaveToSharedMemoryMapName);
		if(NULL == m_hFileMapHandle)
		{
			::GetLastErrorDisplay(this, _T("OpenFileMapping Error"));
			return;
		}
							// Convert the handle into a pointer.
		m_puchFileMapData = (UCHAR*)(::MapViewOfFile(m_hFileMapHandle, FILE_MAP_READ, 0, 0, 0));
		if(m_puchFileMapData == NULL) 
		{
			::GetLastErrorDisplay(this, _T("MapViewOfFile Error"));
			::CloseHandle(m_hFileMapHandle);
			m_hFileMapHandle = NULL;
			return;
		}
	}

#ifdef BOUNDS_CHECK_TEST
	int iRingTailDriverBytes;
	int iDriverTriggerBytes;
	int iRingTailProcessedBytes;
	int iProcessedTriggerBytes;
	int iMaxMemoryUsed;
	int iMinMemoryUsed;
	int iMaxFilmLength_mm;
	hr = m_pIScanPictures->GetScannerInfo001(&iRingTailDriverBytes,
													&iDriverTriggerBytes,
													&iRingTailProcessedBytes,
													&iProcessedTriggerBytes,
													&iMaxMemoryUsed,
													&iMinMemoryUsed,
													&iMaxFilmLength_mm);
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}

	iRingTailDriverBytes = 0x10000U;	// In house test
	iMaxFilmLength_mm += 400;

	hr = m_pIScanPictures->PutScannerInfo001(iRingTailDriverBytes,
													iDriverTriggerBytes,
													iRingTailProcessedBytes,
													iProcessedTriggerBytes,
													iMaxMemoryUsed,
													iMinMemoryUsed,
													iMaxFilmLength_mm);
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}
#else
	int iRingTailDriverBytes;
	int iDriverTriggerBytes;
	int iRingTailProcessedBytes;
	int iProcessedTriggerBytes;
	int iMaxMemoryUsed;
	int iMinMemoryUsed;
	int iMaxFilmLength_mm;
	hr = m_pIScanPictures->GetScannerInfo001(&iRingTailDriverBytes,
													&iDriverTriggerBytes,
													&iRingTailProcessedBytes,
													&iProcessedTriggerBytes,
													&iMaxMemoryUsed,
													&iMinMemoryUsed,
													&iMaxFilmLength_mm);
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}

	iRingTailDriverBytes = RING_TAIL_DRIVER_BYTES;
	iDriverTriggerBytes = DRIVER_TRIGGER_BYTES;
	iRingTailProcessedBytes = RING_TAIL_PROCESSED_BYTES;
	iProcessedTriggerBytes = PROCESSED_TRIGGER_BYTES;
	iMaxMemoryUsed = MAX_MEMORY_USED;
	iMinMemoryUsed = MIN_MEMORY_USED;
	iMaxFilmLength_mm = MAX_FILM_LENGTH_MM;

	hr = m_pIScanPictures->PutScannerInfo001(iRingTailDriverBytes,
		iDriverTriggerBytes,
		iRingTailProcessedBytes,
		iProcessedTriggerBytes,
		iMaxMemoryUsed,
		iMinMemoryUsed,
		iMaxFilmLength_mm);
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}
#endif
}

/*
// void SetDlgSaving(int iSaveMode)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::SetDlgSaving(int iSaveMode)
{
	m_iSaveMode = iSaveMode;
	// can't go from one active save mode to the other directly!
	ASSERT(!(  iSaveMode == SAVE_MODE_NEW && m_iSaveMode == SAVE_MODE_EXPORT));
	ASSERT(!(m_iSaveMode == SAVE_MODE_NEW &&   iSaveMode == SAVE_MODE_EXPORT));

	GetDlgItem(IDC_SAVE_CANCEL)->EnableWindow(m_iSaveMode == SAVE_MODE_EXPORT);
	if(iSaveMode > SAVE_MODE_IDLE)
	{
		// disable all user save actions except "save cancel".
		GetDlgItem(IDC_SAVE)->EnableWindow(FALSE);
		GetDlgItem(IDC_FRAMING)->EnableWindow(FALSE);
		GetDlgItem(IDC_INSERT_PICTURE)->EnableWindow(FALSE);
		GetDlgItem(IDC_DELETE_PICTURE)->EnableWindow(FALSE);
		GetDlgItem(IDC_MOVE_TO_SAVE_GROUP)->EnableWindow(FALSE);
		GetDlgItem(IDC_RELEASE_SAVE_GROUP)->EnableWindow(FALSE);
		GetDlgItem(IDC_APPLY)->EnableWindow(FALSE);
		GetDlgItem(IDC_COLOR)->EnableWindow(FALSE);
		GetDlgItem(IDC_ROTATE)->EnableWindow(FALSE);
		GetDlgItem(IDC_ROTATE_AMOUNT)->EnableWindow(FALSE);
		GetDlgItem(IDC_CONNECT)->EnableWindow(FALSE);

		GetDlgItem(IDC_PREVIOUS)->EnableWindow(FALSE);
		GetDlgItem(IDC_NEXT)->EnableWindow(FALSE);
		GetDlgItem(IDCANCEL)->EnableWindow(FALSE);
	}
	else
	{
		::Sleep(100);		// Allow Server's worker thread to exit
		// don't allow these actions until implemented and tested
		GetDlgItem(IDC_COLOR)->EnableWindow(0 < m_iSavePictureCount);
		GetDlgItem(IDC_ROTATE)->EnableWindow(0 < m_iSavePictureCount);
		GetDlgItem(IDC_ROTATE_AMOUNT)->EnableWindow(0 < m_iSavePictureCount);
		if(!m_bDisplayFourPictures && (0 < m_iSavePictureCount))
			GetDlgItem(IDC_FRAMING)->EnableWindow(TRUE);
		else 
			GetDlgItem(IDC_FRAMING)->EnableWindow(FALSE);


		GetDlgItem(IDC_INSERT_PICTURE)->EnableWindow(0 < m_iSaveRollCount);
		GetDlgItem(IDC_DELETE_PICTURE)->EnableWindow(0 < m_iSavePictureCount);
		GetDlgItem(IDC_SAVE)->EnableWindow(0 < m_iSavePictureCount);
		GetDlgItem(IDC_APPLY)->EnableWindow(0 < m_iSavePictureCount);
		GetDlgItem(IDC_MOVE_TO_SAVE_GROUP)->EnableWindow(m_iScanRollCount);
		GetDlgItem(IDC_RELEASE_SAVE_GROUP)->EnableWindow(m_iSaveRollCount);
		GetDlgItem(IDC_PREVIOUS)->EnableWindow(m_iIndex > 0);
		GetDlgItem(IDC_NEXT)->EnableWindow(m_iIndex < (m_iSavePictureCount - 1));
		GetDlgItem(IDCANCEL)->EnableWindow(!m_bScanning);

		SetDlgItemText(IDC_SAVE_STATUS, _T("Idle"));
		m_ProgressSave.SetPos(WTP_ProgressStart);
	}
}

/*
// void SetDlgScanning(BOOL bScanning)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::SetDlgScanning(BOOL bScanning)
{
	m_bScanning = bScanning;
	if(bScanning)
	{
		// disable all scan actions except "scan cancel".
		GetDlgItem(IDC_ADVANCE_FILM)->EnableWindow(FALSE);
		GetDlgItem(IDC_SCAN)->EnableWindow(FALSE);
		GetDlgItem(IDC_SCAN_CANCEL)->EnableWindow(TRUE);
		GetDlgItem(IDC_MOVE_TO_SAVE_GROUP)->EnableWindow(m_iScanRollCount);
		GetDlgItem(IDC_DELETE_ROLL)->EnableWindow(m_iScanRollCount);
		GetDlgItem(IDC_CONNECT)->EnableWindow(FALSE);
		GetDlgItem(IDCANCEL)->EnableWindow(FALSE);
	}
	else
	{
		::Sleep(100);		// Allow Server's worker thread to exit
		GetDlgItem(IDC_ADVANCE_FILM)->EnableWindow(m_iInitState == iInitStateInitialized);
		GetDlgItem(IDC_SCAN)->EnableWindow(m_iInitState == iInitStateInitialized);
		GetDlgItem(IDC_SCAN_CANCEL)->EnableWindow(FALSE);
		GetDlgItem(IDC_MOVE_TO_SAVE_GROUP)->EnableWindow(m_iScanRollCount && m_iSaveMode == SAVE_MODE_IDLE);
		GetDlgItem(IDC_DELETE_ROLL)->EnableWindow(m_iScanRollCount);
		GetDlgItem(IDCANCEL)->EnableWindow(m_iSaveMode == SAVE_MODE_IDLE);

		SetDlgItemText(IDC_SCAN_STATUS, _T("Idle"));
		m_ProgressScan.SetPos(WTP_ProgressStart);
	}
}

/*
// BOOL DestroyWindow()
// 
// Description:
//
//
// Parameters:
//
//
// Return Value
//
//
//	Remarks:
//   
*/
BOOL CTLAClientDemoDlg::DestroyWindow() 
{
	Sleep(100);
	if(m_bScanning)
		OnScanCancel();
	if(m_iSaveMode)
		OnSaveCancel();

	for (int i = 0; i < m_Pictures.GetSize(); i++)
	{
		delete (m_Pictures[i]->m_PiImage.m_pBitmap);
		delete m_Pictures[i];
	}
	m_Pictures.RemoveAll();

	m_ClientBuffers.Delete();

	if(m_puchFileMapData != NULL)
	{
		if(0 == ::UnmapViewOfFile(m_puchFileMapData))
			::GetLastErrorDisplay(this, _T("UnmapViewOfFile Error"));
	}

	if(m_hFileMapHandle != NULL)
	{
		if(!::CloseHandle(m_hFileMapHandle))
			::GetLastErrorDisplay(this, _T("CloseHandle For FileMap Error"));
	}

	if(m_hFileMapEventHandle != NULL)
	{
		if(!::CloseHandle(m_hFileMapEventHandle))
			::GetLastErrorDisplay(this, _T("CloseHandle For Event Error"));
	}

	// If we have a callback object, release it and unintialize scanner
	bInitClientCallback(FALSE);

#ifndef COMPLETE_UNLOADING

	// Release interface pointers to shut down server
	if(NULL != m_pISavePictures)
	{
		m_pISavePictures->Release();
		m_pISavePictures = NULL;
	}

	if(NULL != m_pIScanPictures)
	{
		m_pIScanPictures->Release();
		m_pIScanPictures = NULL;
	}

	if(NULL != m_pITLAMain)
	{
		m_pITLAMain->Release();
		m_pITLAMain = NULL;
	}

	// Shut down this COM apartment
	::CoUninitialize();

#endif

	return CDialog::DestroyWindow();
}

/*
// void OnPreScan(UINT nID) 
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::OnPreScan(UINT nID) 
{
	int iFilmFormat, iResolution;
	switch (nID)
	{
	case ID_PRESCAN_35MM4BASE:
		iFilmFormat = FILM_FORMAT_35MM;
		iResolution = RESOLUTION_BASE_4;
		break;
	case ID_PRESCAN_35MM8BASE:
		iFilmFormat = FILM_FORMAT_35MM;
		iResolution = RESOLUTION_BASE_8;
		break;
	case ID_PRESCAN_35MM16BASE:
		iFilmFormat = FILM_FORMAT_35MM;
		iResolution = RESOLUTION_BASE_16;
		break;
	case ID_PRESCAN_24MM4BASE:
		iFilmFormat = FILM_FORMAT_24MM;
		iResolution = RESOLUTION_BASE_4;
		break;
	case ID_PRESCAN_24MM8BASE:
		iFilmFormat = FILM_FORMAT_24MM;
		iResolution = RESOLUTION_BASE_8;
		break;
	case ID_PRESCAN_24MM16BASE:
		iFilmFormat = FILM_FORMAT_24MM;
		iResolution = RESOLUTION_BASE_16;
		break;
	}
	HRESULT hr;
	CPreScanFramingAdjust PreScanDlg(this);
	int iHeightLR;

	// get default values for reset button usage
	// These default values are for standard 35 and 24 mm
	// films
	hr = m_pIScanPictures->GetScannerInfoPreFrame(
		iResolution,
		iFilmFormat,
		&iHeightLR,
		&(PreScanDlg.m_iHeightHR_mm),
		&(PreScanDlg.m_iHeightHR),
		&(PreScanDlg.m_iWidthHR),
		&(PreScanDlg.m_iWidthUnitHR),
		&(PreScanDlg.m_crCropHR.left),
		&(PreScanDlg.m_crCropHR.top),
		&(PreScanDlg.m_crCropHR.right),
		&(PreScanDlg.m_crCropHR.bottom));
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}

	// Get the user info for display and edit.
	// The user can change the pre-frame data for non-standard
	// frame types (i.e. panoramic 35 mm).
	hr = m_pIScanPictures->GetScannerInfoPreFrameUser(
		iResolution,
		iFilmFormat,
		&(PreScanDlg.m_iWidthHR_Adj),
		&(PreScanDlg.m_iWidthUnitHR_Adj),
		&(PreScanDlg.m_crCropHR_Adj.left),
		&(PreScanDlg.m_crCropHR_Adj.top),
		&(PreScanDlg.m_crCropHR_Adj.right),
		&(PreScanDlg.m_crCropHR_Adj.bottom));
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}

	if(IDOK == PreScanDlg.DoModal())
	{
		hr = m_pIScanPictures->PutScannerInfoPreFrameUser(
			iResolution,
			iFilmFormat,
			PreScanDlg.m_iWidthHR_Adj,
			PreScanDlg.m_iWidthUnitHR_Adj,
			PreScanDlg.m_crCropHR_Adj.left,
			PreScanDlg.m_crCropHR_Adj.top,
			PreScanDlg.m_crCropHR_Adj.right,
			PreScanDlg.m_crCropHR_Adj.bottom);
		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		}
	}	
}

/*
// BOOL bComMarshallInterface(BOOL bScanPictures) 
// 
// Description:
//		MARSHALLING
//
//
// Parameters:
//
//
// Return Value
//
//
//	Remarks:
//   
*/
BOOL CTLAClientDemoDlg::bComMarshallInterface(BOOL bScanPictures)
{
	m_pIStream = NULL;
	if(bScanPictures)
	{
		IScanPictures *pIScanPictures;
		HRESULT hr = m_pILongOpsCB->QueryInterface(IID_IScanPictures, (void**)&pIScanPictures);
		if(FAILED(hr)) 
		{
			CString strResult;
			::FormatHResult(hr, strResult);
			CString str;
			str.Format(_T("QueryInterface Failed\n%s"), strResult);
			AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
			return FALSE;
		}
		
		// Initialize m_ThreadDataLongOps and marshall an interface pointer in the stream
		hr = ::CoMarshalInterThreadInterfaceInStream(									
										IID_IScanPictures,
										pIScanPictures,
										&m_pIStream);

		pIScanPictures->Release();
		if(FAILED(hr)) 
		{
			m_pIStream = NULL;
			CString strResult;
			::FormatHResult(hr, strResult);
			CString str;
			str.Format(_T("CoMarshalInterThreadInterfaceInStream Failed\n%s"), strResult);
			AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
			return FALSE;
		}
	}
	else
	{
		ISavePictures *pISavePictures;
		HRESULT hr = m_pILongOpsCB->QueryInterface(IID_ISavePictures, (void**)&pISavePictures);
		if(FAILED(hr)) 
		{
			CString strResult;
			::FormatHResult(hr, strResult);
			CString str;
			str.Format(_T("QueryInterface Failed\n%s"), strResult);
			AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
			return FALSE;
		}
		
		// Initialize m_ThreadDataLongOps and marshall an interface pointer in the stream
		hr = ::CoMarshalInterThreadInterfaceInStream(									
										IID_ISavePictures,
										pISavePictures,
										&m_pIStream);

		pISavePictures->Release();
		if(FAILED(hr)) 
		{
			m_pIStream = NULL;
			CString strResult;
			::FormatHResult(hr, strResult);
			CString str;
			str.Format(_T("CoMarshalInterThreadInterfaceInStream Failed\n%s"), strResult);
			AfxMessageBox(str, MB_OK | MB_ICONEXCLAMATION);
			return FALSE;
		}
	}

	return TRUE;
}

/*
// void OnAdvanceFilm() 
// 
// Description:
//		MARSHALLING
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::OnAdvanceFilm() 
{
	SetDlgScanning(TRUE);
	CAdvanceMotor Dlg;
	Dlg.m_iMilliseconds = m_iAdvanceFilmMilliseconds;
	Dlg.m_iSpeed = m_AdvanceFilmSpeed;
	if(IDOK == Dlg.DoModal())
	{
		m_iAdvanceFilmMilliseconds = Dlg.m_iMilliseconds;
		m_AdvanceFilmSpeed = Dlg.m_iSpeed;
		HRESULT hr = m_pIScanPictures->AdvanceFilm(m_iAdvanceFilmMilliseconds, m_AdvanceFilmSpeed);
		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
			SetDlgScanning(FALSE);
		}
/*
		else
		{
			if (m_iAdvanceFilmMilliseconds == -1)
				SetDlgScanning(TRUE);
		}
*/
	}
	else
		SetDlgScanning(FALSE);
}

/*
// void SaveFileUsingSaveToSharedMemory() 
// 
// Description:
//		This function saves the 8 bit dib in shared memory to a file.
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::SaveFileUsingSaveToSharedMemory()
{
   SiSaveToMemoryDirAndFile &rSaveToMemoryDirAndFile = 
		m_clSaveToSharedMemoryDirAndFile.RemoveHead();

	if(!m_bDibFileHeader)
	{
		// If you specified that the BITMAPFILEHEADER doesn't exist, you
		// can add your own code to do what you want with the bitmap
		// using m_ClientBuffers.GetActiveBuffer() cast as a BITMAPINFO structure
		ASSERT(FALSE);
	}
	else
	{
		// Use DIB in memory to save to disk using list of names compiled earlier
		// Create a directory (makes subdirectories along the way is required)
		if(!::bCreateDirectory(rSaveToMemoryDirAndFile.m_strDirectory, this))
			return;

		// Open file - overwrites existing file with same name
		CString strPath;
		strPath.Format(_T("%s\\%s.bmp"), rSaveToMemoryDirAndFile.m_strDirectory, rSaveToMemoryDirAndFile.m_strFileName);
		HANDLE hFile = ::CreateFile(strPath,
										 GENERIC_WRITE,
										 0,
										 NULL,
										 CREATE_ALWAYS,
										 FILE_ATTRIBUTE_NORMAL,
										 NULL);

		if(INVALID_HANDLE_VALUE == hFile)
		{
			::GetLastErrorDisplay(this, _T("CreateFile Error"));
			return;
		}

		BITMAPFILEHEADER *pBitMapFileHeader = (BITMAPFILEHEADER*)m_puchFileMapData;
		DWORD dwFileSize = pBitMapFileHeader->bfSize;

		// Write the file
		DWORD dwBytesWritten;
		if(!::WriteFile(hFile, pBitMapFileHeader, dwFileSize, &dwBytesWritten, NULL))
		{
			::GetLastErrorDisplay(this, _T("WriteFile Error"));
			::CloseHandle(hFile);
			return;
		}

		if(dwBytesWritten != dwFileSize)
		{
			::GetLastErrorDisplay(this, _T("WriteFile Error (invalid byte count)"));
			::CloseHandle(hFile);
			return;
		}

		if(!::CloseHandle(hFile))
		{
			::GetLastErrorDisplay(this, _T("CloseHandle Error"));
			return;
		}
	}

	// Let server know it can save the next picture to memory
	// Using events for handshaking ensures exclusive access to this memory
	if(!::SetEvent(m_hFileMapEventHandle))
		::GetLastErrorDisplay(this, _T("SetEvent Error"));
}

/*
// void SavePlanarUsingSaveToClientMemory() 
// 
// Description:
//		This function saves the Planar picture in client memory to a file.
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::SavePlanarUsingSaveToClientMemory()
{
	BOOL bError = FALSE;
	UCHAR *pBuffer = m_ClientBuffers.GetActiveBuffer();
	if(m_clSaveToClientMemoryDirAndFile.IsEmpty())
		return;			// Somebody hit the Cancel Save button

	if(!m_bPlanarFileHeader)
	{
		// If you specified that the header doesn't exist, you
		// can add your own code to do what you want with the planar picture
		m_clSaveToClientMemoryDirAndFile.RemoveHead();
		TRACE(_T("You planar is in the client buffer!\n"));
	}
	else
	{
		SiSaveToMemoryDirAndFile &rSaveToMemoryDirAndFile = m_clSaveToClientMemoryDirAndFile.RemoveHead();

		SiPlanarFileHeader *pPlanarFileHeader = (SiPlanarFileHeader*)pBuffer;

		DWORD dwFileSize = pPlanarFileHeader->m_uiWidth * 
									pPlanarFileHeader->m_uiHeight * 
									(pPlanarFileHeader->m_uiBitCount / 8);

		HKEY hKey = HKEY_LOCAL_MACHINE;
		DWORD dwBuffer;
		DWORD dwDataSize = sizeof(DWORD);
		DWORD dwKeyType;
		BOOL bNoHeaderSavedInPlanarFile = FALSE;

		if(ERROR_SUCCESS == ::RegOpenKeyEx(hKey, _T("Software\\Pakon\\TLA\\Scan\\Simulator"), 0L, KEY_READ, &hKey))
		{																			// Open Key
			if(ERROR_SUCCESS == ::RegQueryValueEx(hKey, _T("NoHeaderSavedInPlanarFile"), NULL, &dwKeyType, (BYTE*)(&dwBuffer), &dwDataSize))
			{																		// Get the data.
				if(REG_DWORD == dwKeyType)									// verify data type
					bNoHeaderSavedInPlanarFile = dwBuffer;
			}

			::RegCloseKey(hKey);
		}

		if(bNoHeaderSavedInPlanarFile)	// Planar header saved by server and skipped by client
			pBuffer += pPlanarFileHeader->m_uiSize;
		else
			dwFileSize += pPlanarFileHeader->m_uiSize;

		// Use DIB in memory to save to disk using list of names compiled earlier
		// Create a directory (makes subdirectories along the way is required)
		HANDLE hFile = INVALID_HANDLE_VALUE;
		if(!::bCreateDirectory(rSaveToMemoryDirAndFile.m_strDirectory, this))
			bError = TRUE;
		
		if (!bError) // if the dir is ready
		{
			// Open file - overwrites existing file with same name
			CString strPath;
			strPath.Format(_T("%s\\%s.raw"), rSaveToMemoryDirAndFile.m_strDirectory, rSaveToMemoryDirAndFile.m_strFileName);
			hFile = ::CreateFile(strPath,
											 GENERIC_WRITE,
											 0,
											 NULL,
											 CREATE_ALWAYS,
											 FILE_ATTRIBUTE_NORMAL,
											 NULL);
		}

		if(INVALID_HANDLE_VALUE == hFile)
		{
			::GetLastErrorDisplay(this, _T("CreateFile Error"));
			bError = TRUE;
		}
		else
		{
			// Write the file
			DWORD dwBytesWritten;
			if(!::WriteFile(hFile, pBuffer, dwFileSize, &dwBytesWritten, NULL))
			{
				::GetLastErrorDisplay(this, _T("WriteFile Error"));
				bError = TRUE;
			}

			if (!bError)
			{
				if(dwBytesWritten != dwFileSize)
					::GetLastErrorDisplay(this, _T("WriteFile Error (invalid byte count)"));
			}

			if(!::CloseHandle(hFile))
				::GetLastErrorDisplay(this, _T("CloseHandle Error"));
		}
	}
}

/*
// void Save8BitDibUsingSaveToClientMemory() 
// 
// Description:
//		This function saves the 8 bit dib in client memory to a file.
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::Save8BitDibUsingSaveToClientMemory()
{
	BOOL bError = FALSE;
   SiSaveToMemoryDirAndFile &rSaveToMemoryDirAndFile = 
		m_clSaveToClientMemoryDirAndFile.RemoveHead();

	if(!m_bDibFileHeader)
	{
		// If you specified that the BITMAPFILEHEADER doesn't exist, you
		// can add your own code to do what you want with the bitmap
		// using m_ClientBuffers.GetActiveBuffer() cast as a BITMAPINFO structure
		ASSERT(FALSE);
	}
	else
	{
		// Use DIB in memory to save to disk using list of names compiled earlier
		// Create a directory (makes subdirectories along the way is required)
		HANDLE hFile = INVALID_HANDLE_VALUE;
		if(!::bCreateDirectory(rSaveToMemoryDirAndFile.m_strDirectory, this))
			bError = TRUE;
		
		if (!bError) // if the dir is ready
		{
			// Open file - overwrites existing file with same name
			CString strPath;
			strPath.Format(_T("%s\\%s.bmp"), rSaveToMemoryDirAndFile.m_strDirectory, rSaveToMemoryDirAndFile.m_strFileName);
			hFile = ::CreateFile(strPath,
											 GENERIC_WRITE,
											 0,
											 NULL,
											 CREATE_ALWAYS,
											 FILE_ATTRIBUTE_NORMAL,
											 NULL);
		}

		if(INVALID_HANDLE_VALUE == hFile)
		{
			::GetLastErrorDisplay(this, _T("CreateFile Error"));
			bError = TRUE;
		}
		else
		{
			// Write the file
			BITMAPFILEHEADER *pBitMapFileHeader;
			pBitMapFileHeader = (BITMAPFILEHEADER*)m_ClientBuffers.GetActiveBuffer();
			DWORD dwFileSize = pBitMapFileHeader->bfSize;
			DWORD dwBytesWritten;
			if(!::WriteFile(hFile, pBitMapFileHeader, dwFileSize, &dwBytesWritten, NULL))
			{
				::GetLastErrorDisplay(this, _T("WriteFile Error"));
				bError = TRUE;
			}

			if(!bError)
			{
				if(dwBytesWritten != dwFileSize)
					::GetLastErrorDisplay(this, _T("WriteFile Error (invalid byte count)"));
			}

			if(!::CloseHandle(hFile))
				::GetLastErrorDisplay(this, _T("CloseHandle Error"));
		}
	}
}

/*
// void OnInsertPicture() 
// 
// Description:
//		This is a quick demonstration of inserting a picture.
//		If you want to create a dialog for your user, make sure
//		you follow the marshalling technique demonstrated in
//		CTLAClientDemoDlg::OnColor() and the CColorDlg class.
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::OnInsertPicture() 
{
	// what is the strip to insert within
	int iCurrentStipIndex = 0;
	CRect rectNew(0,0,150,100);
	if (m_Pictures.GetSize() > 0)
	{
		iCurrentStipIndex = (m_Pictures[m_iIndex])->m_PiPictureInfo.m_iStripIndex;

		// where to insert
		PiFramingUserInfo* pFraming = &((m_Pictures[m_iIndex])->m_PiFramingUserInfo);
		CRect rect(pFraming->m_lLeftHR, pFraming->m_lTopHR, pFraming->m_lRightHR, pFraming->m_lBottomHR);
		if (m_iIndex == 0)
		{
			// put picture at beginning of strip
			rectNew = CRect(0,0,rect.Width(),rect.Height());
		}
		else
		{
			rectNew = rect;
			if (rectNew.left > rect.Width())
				// put picture to left of current
				rectNew.OffsetRect(-rect.Width(),0);
		}
	}

	HRESULT hr = m_pISavePictures->InsertPicture(
//		INDEX_First,					// Insert before first picture (picture 0)
//		2,									// Insert before third picture (picture 2)
//		INDEX_InsertPictureAtEnd,	// Append after last picture
		m_iIndex,						// Insert before current picture
		iCurrentStipIndex,			// make sure you specify valid strip)
		rectNew.left,					// Specify the high res rectangle
		rectNew.top,
		rectNew.right,
		rectNew.bottom);
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		return;
	}
	UpdatePictureCounts();
	UpdatePrevNext();
	PiPicture *pPicture = new PiPicture;
	m_Pictures.InsertAt(m_iIndex, pPicture);
	pPicture->m_PiPictureInfo.m_iSelectedHidden = S_OR_H_NONE;
	// update the new picture with data from the server
	UpdateServerData(m_iIndex, FALSE);
	// update controls with new data
	UpdateData(FALSE);
	// mark something as updated so the UpdateServerData will flag 
	// this picture for a resave
	pPicture->m_PiFramingUserInfo.m_iUpdate |= UPDATE_DATA_AND_IMAGE;
	// update the bitmap
	pPicture->m_PiImage.m_bResave = TRUE;
	pPicture->m_PiImage.m_pBitmap = NULL;
	pPicture->m_PiImage.m_bReplace = FALSE;
	PostMessage(WM_RESAVE_IMAGES);
}

/*
// void OnDeletePicture() 
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::OnDeletePicture() 
{
	HRESULT hr = m_pISavePictures->DeletePicture(
//		INDEX_First						// Delete first picture (picture 0)
//		2									// Delete third picture (picture 2)
//		m_iSavePictureCount - 1		// Delete last picture
		m_iIndex							// Delete current picture
		);
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
	}
	else
	{
		delete (m_Pictures[m_iIndex]->m_PiImage.m_pBitmap);
		delete (m_Pictures[m_iIndex]);
		m_Pictures.RemoveAt(m_iIndex);
		// update controls with new data
		UpdatePictureCounts();
		if ((m_iIndex >= m_iSavePictureCount) && // if we deleted the last picture in list
			 (m_iIndex != 0))							  // but not the last picture 
		{
			m_iIndex--;
		}
		UpdatePrevNext();
		UpdateData(FALSE);
		UpdatePicture();
		SetDlgSaving(SAVE_MODE_IDLE);
	}
}

/*
// void OnUpdatePreScan(CCmdUI* pCmdUI) 
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::OnUpdatePreScan(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable(!m_bScanning);
}

void CTLAClientDemoDlg::OnHelpAbout() 
{
	CAboutDlg AboutDlg;
	AboutDlg.DoModal();
}

/*
// void OnReleaseSaveGroup() 
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::OnReleaseSaveGroup() 
{
	SetDlgSaving(SAVE_MODE_EXPORT);

	// have the scanner throw out all the rolls and pictures in the save group
	HRESULT hr = m_pISavePictures->ReleaseSaveGroup();

	if(FAILED(hr))
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);

	for (int i = 0; i < m_Pictures.GetSize(); i++)
	{
		delete (m_Pictures[i]->m_PiImage.m_pBitmap);
		delete m_Pictures[i];
	}
	m_Pictures.RemoveAll();

	m_iIndex = 0;
	m_iCurrentRollIndex = 0;

	// update controls
	UpdatePictureCounts();
	UpdateData(FALSE);
	UpdatePicture();
	SetDlgSaving(SAVE_MODE_IDLE);
}

/*
// void OnMoveToSaveGroup() 
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::OnMoveToSaveGroup() 
{
	HRESULT hr;
	// moving from scan to save group involves both the scan 
	// and save worker threads in the com server
	SetDlgSaving(SAVE_MODE_NEW);

	// deselect all pictures
	if (m_iSavePictureCount)
	{
		hr = m_pISavePictures->PutPictureSelection(INDEX_All,	S_OR_H_NONE, TRUE);
		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		}
	}

	hr = m_pISavePictures->GetPictureCountScanGroup(
		0,  // the roll we are about to move
		&m_iScanStripCount,
		&m_iScanPictureCount, 
		&m_iScanWarnings);
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
	}
	else if(0 != m_iScanWarnings)
	{
		CheckScanWarnings(m_iScanWarnings);
	}

	// add the new roll to the save group
	int iOldPictureCount = m_iSavePictureCount;
	hr = m_pISavePictures->MoveOldestRollToSaveGroup();
	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
	}
	UpdateScanCounts();							// get the new scan roll count
	UpdatePictureCounts();						// get the new save picture count
	GetDlgItem(IDC_MOVE_TO_SAVE_GROUP)->EnableWindow(m_iScanRollCount && m_iSaveMode == SAVE_MODE_IDLE);
	GetDlgItem(IDC_DELETE_ROLL)->EnableWindow(m_iScanRollCount);

	CSize sizeImageDisplay;
	CWnd *pWndPicture = GetDlgItem(IDC_PICTURE);
	CRect crPicture;
	pWndPicture->GetClientRect(crPicture);
	crPicture.DeflateRect(MARGIN, MARGIN);

	// set the save options for a display save
	int iSaveOptions = 0;
	iSaveOptions |= (
		SAV_SizeLimitForDisplay |
		SAV_TopDownDib |
		SAV_FastUpdate8BitDib |					// keep thumb in mem since we may do
   													// color correction in 1 picture mode.
		SAV_UseCurrentRotation | 
		SAV_UseLoResBuffer |	  					// Since displaying thumb size
		SAV_DoNotScaleUp |
		SAV_UseColorCorrection |				// to get 12 bit RPD
		m_uiUseKcdfs |
		SAV_UseColorSceneBalance |				// to perform scene balance
		SAV_UseColorAdjustments |				// to adjust color per color slider
		SAV_UseScratchRemovalIfAvailable );	// Always have this on to use scratch removal 

	// whats the max pixel count
	sizeImageDisplay.cx = crPicture.Width();
	sizeImageDisplay.cy = crPicture.Height();

	PiPicture* pPicture;
	PiPictureInfo* pPictureInfo;
	// for each of the new pictures
	for (int iIndex = iOldPictureCount; iIndex < m_iSavePictureCount; iIndex++)
	{
		// Set the picture selected
		pPicture = new PiPicture;
		pPictureInfo = &(pPicture->m_PiPictureInfo);
		m_Pictures.Add(pPicture);
		// update the new picture with data from the server
		UpdateServerData(iIndex, FALSE);
		// Set the picture selected
		HRESULT hr = m_pISavePictures->PutPictureSelection(
			iIndex, S_OR_H_SELECTED, TRUE);
		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		}

		pPictureInfo->m_iUpdate = UPDATE_NONE;
		pPictureInfo->m_iSelectedHidden = S_OR_H_NONE;
		pPicture->m_PiImage.m_bResave = FALSE;
		pPicture->m_PiImage.m_pBitmap = NULL;
		pPicture->m_PiImage.m_bReplace = FALSE;
	}

	if (m_iSavePictureCount > iOldPictureCount)		// if we have new pictures
	{
		m_iIndex = iOldPictureCount;		// set index to first new picture
	}
	UpdateData(FALSE);					// Update dialog controls with new picture info.
	
	// Start the save
	if (m_ClientBuffers.bAllocateBuffers(sizeImageDisplay, iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8, m_iSavePictureCount-iOldPictureCount>1?TRUE:FALSE))
	{
		HRESULT hr = m_pISavePictures->SaveToClientMemory(
			INDEX_AllSelected,
			iSaveOptions,
			sizeImageDisplay.cx,			// paint picture to fill the picture control
			sizeImageDisplay.cy,			// paint picture to fill the picture control
			0,									// ignored since iSaveOptions has SAV_UseCurrentRotation
			SCALING_METHOD_BICUBIC,		// Normal scaling method
			iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8,
			TRUE);							// UseWorkerThread 

		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pITLAMain, NULL);
			SetDlgSaving(SAVE_MODE_IDLE);
			m_ClientBuffers.Delete();
		}
	}
	// the actual bitmap creation will occur as a result of callbacks from the server
	// see the UpdateDisplayNewProgress handler function
}

/*
// void CheckScanWarnings(int iScanWarnings) 
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::CheckScanWarnings(int iScanWarnings)
{
	CString strWarnings, strDx, strProductAndSpecifier, strMof, strMotorSpeed, strMaxFilmLength;
	CString strFramingGood, strFramingAddInMiddle, strFramingAddAtEnd, strFramingAddAtBeginning, strFramingBad;

	// did we either have max film length set too short or did the film jam
	if (m_iScanWarnings & SCANW_MAX_FILM_LENGTH_EXCEEDED)
		strMaxFilmLength = _T("Film Jammed or Set Too Small");
	else
		strMaxFilmLength = _T("Good");

	// Were the DX codes read properly
	if(m_iScanWarnings & SCANW_DX_BAD)
		strDx = _T("Bad");
	else
		strDx = _T("Good");

	// Product And Specifier
	if(m_iScanWarnings & SCANW_FILM_PRODUCT_AND_SPECIFIER_BAD)
		strProductAndSpecifier = _T("Bad");
	else
		strProductAndSpecifier = _T("Good");

	// Mof
	if(SCANW_MOF_SOURCE_FILE & m_iScanWarnings)
		strMof += _T("Mof From File");
	else if(SCANW_MOF_SOURCE_F235C & m_iScanWarnings)
		strMof += _T("Mof From F235C");

	if(SCANW_MOF_FAILED_PERFORATIONS & m_iScanWarnings)
		strMof += _T("Mof Perforations Failure");

	// Motor Speed
	BOOL bMotorSpeedOk = FALSE;
	if(m_iScanWarnings & SCANW_MOTOR_SPEED_ONE_PERCENT_SLOW)
		strMotorSpeed = _T("One Percent Slow");
	else if(m_iScanWarnings & SCANW_MOTOR_SPEED_HALF_PERCENT_SLOW)
		strMotorSpeed = _T("Half Percent Slow");
	else if(m_iScanWarnings & SCANW_MOTOR_SPEED_ONE_PERCENT_FAST)
		strMotorSpeed = _T("One Percent Fast");
	else if(m_iScanWarnings & SCANW_MOTOR_SPEED_HALF_PERCENT_FAST)
		strMotorSpeed = _T("Half Percent Fast");
	else
	{
		bMotorSpeedOk = TRUE;
		strMotorSpeed = _T("Good");
	}

	// Framing
	if(m_iScanWarnings & (SCANW_FRAMING_FAIR | SCANW_FRAMING_BAD))
		strFramingGood = _T("No");
	else
		strFramingGood = _T("Yes");

	if(m_iScanWarnings & SCANW_FRAMING_IN_MIDDLE)
		strFramingAddInMiddle = _T("Yes");
	else
		strFramingAddInMiddle = _T("No");

	if(m_iScanWarnings & SCANW_FRAMING_AT_END)
		strFramingAddAtEnd = _T("Yes");
	else
		strFramingAddAtEnd = _T("No");

	if(m_iScanWarnings & SCANW_FRAMING_AT_BEGINNING)
		strFramingAddAtBeginning = _T("Yes");
	else
		strFramingAddAtBeginning = _T("No");

	if(m_iScanWarnings & SCANW_FRAMING_BAD)
		strFramingBad = _T("Yes");
	else
		strFramingBad = _T("No");

	strWarnings.Format(_T("DX Read: %s")
							_T("\nProduct And Specifier: %s")
							_T("\nMof: %s")
							_T("\nMotor Speed: %s")
							_T("\nFraming - Good: %s")
							_T("\nFraming - Add In Middle: %s")
							_T("\nFraming - Add At End: %s")
							_T("\nFraming - Add At Beginning: %s")
							_T("\nFraming - Bad: %s")
							_T("\nMaxFilmLength: %s"),
							strDx, 
							strProductAndSpecifier,
							strMof,
							strMotorSpeed, 
							strFramingGood, 
							strFramingAddInMiddle, 
							strFramingAddAtEnd, 
							strFramingAddAtBeginning, 
							strFramingBad,
							strMaxFilmLength);

	MessageBox(strWarnings, _T("Scan Warnings"), MB_OK | MB_ICONEXCLAMATION);

	if(!bMotorSpeedOk)
	{
		if(IDYES == MessageBox(_T("Adjust Motor Speed?"), _T("Scan Warnings"), MB_YESNO | MB_ICONEXCLAMATION))
		{
			HRESULT hr = m_pIScanPictures->AdjustMotorSpeed();
			if(FAILED(hr))
				::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
		}
	}
}

/*
// void OnRotate() 
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::OnRotate() 
{
//	TRACE0("\nHandling Rotate Button Push\n");
	// How much to rotate?
	int iRotateAmount = ((CComboBox*)GetDlgItem(IDC_ROTATE_AMOUNT))->GetCurSel() + 1;
//	TRACE1("Rotate amount: %d\n", iRotateAmount);
	int *piRotation, iIndex;

	// for each of the displayed pictures
	int iNumberPicturesToRotate = m_bDisplayFourPictures?4:1;
	do 
	{
		iIndex = m_iIndex + iNumberPicturesToRotate - 1;
		if(m_iSavePictureCount > iIndex)
		{
			piRotation = &(m_Pictures[iIndex]->m_PiPictureInfo.m_iRotation);
//			TRACE1("Initial Rotation amount: %d\n", *piRotation);
			*piRotation = AddRotation(*piRotation, iRotateAmount);
//			TRACE1("The new Rotation amount: %d\n", *piRotation);
			m_Pictures[iIndex]->m_PiPictureInfo.m_iUpdate = UPDATE_DATA_AND_IMAGE;
		}
		iNumberPicturesToRotate--;
	} while (iNumberPicturesToRotate);
//	TRACE0("\n");
	PostMessage(WM_RESAVE_IMAGES);
}

/*
// int AddRotation(int iStartRotation, int iAdditionalRotation)
// 
// Description:
//
//
// Parameters:
//
//
// Return Value
//
//
//	Remarks:
//   
*/
int AddRotation(int iStartRotation, int iAdditionalRotation)
{
	int iNewRotation = 0;
	int iStartRotationOnly = iStartRotation & ROTATE_90R;
	int iAdditionalRotationOnly = iAdditionalRotation & ROTATE_90R;
	int iStartRotationMirror = iStartRotation & MIRROR_L_R;
	int iAdditionalRotationMirror = iAdditionalRotation & MIRROR_L_R;
	
	switch (iAdditionalRotationOnly)
	{
	case ROTATE_0:  // no rotation added
	default:
		iNewRotation |= iStartRotationOnly;
		break;
	case ROTATE_90L:
		switch (iStartRotationOnly)
		{ 
		case ROTATE_90L:
			iNewRotation |= ROTATE_180;
			break;
		case ROTATE_90R:	
			iNewRotation |= ROTATE_0;
			break;
		case ROTATE_180:	
			iNewRotation |= ROTATE_90R;
			break;
		case ROTATE_0:	
		default:
			iNewRotation |= ROTATE_90L;
			break;
		}
		break;
	case ROTATE_180: 
		switch (iStartRotationOnly)
		{ 
		case ROTATE_90L:
			iNewRotation |= ROTATE_90R;
			break;
		case ROTATE_90R:	
			iNewRotation |= ROTATE_90L;
			break;
		case ROTATE_180:	
			iNewRotation |= ROTATE_0;
			break;
		case ROTATE_0:	
		default:
			iNewRotation |= ROTATE_180;
			break;
		}
		break;
	case ROTATE_90R:
		switch (iStartRotationOnly)
		{ 
		case ROTATE_90L:
			iNewRotation |= ROTATE_0;
			break;
		case ROTATE_90R:	
			iNewRotation |= ROTATE_180;
			break;
		case ROTATE_180:	
			iNewRotation |= ROTATE_90L;
			break;
		case ROTATE_0:	
		default:
			iNewRotation |= ROTATE_90R;
			break;
		}
	}

	if (iAdditionalRotationMirror)
	{
		if (!iStartRotationMirror)
			iNewRotation |= MIRROR_L_R;
	}
	else
	{
		iNewRotation |= iStartRotationMirror;
	}

	return iNewRotation;
}


void CTLAClientDemoDlg::UpdatePrevNext()
{
	if (m_iSaveMode == SAVE_MODE_IDLE)
	{
		GetDlgItem(IDC_PREVIOUS)->EnableWindow(m_iIndex > 0);
		GetDlgItem(IDC_NEXT)->EnableWindow(m_iIndex < (m_iSavePictureCount - 1));
		GetDlgItem(IDC_DELETE_PICTURE)->EnableWindow(m_iSavePictureCount);
	}
}

/*
// afx_msg void OnMsgResaveImages(UINT ulOperation, LONG lStatus)
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
LRESULT CTLAClientDemoDlg::OnMsgResaveImages(UINT ulOperation, LONG lStatus)
{
//	TRACE0("CTLAClientDemoDlg: ResaveImages\n");
	// clear message cue of all queued resave messages
	MSG msg;  	
   while (PeekMessage(&msg, GetSafeHwnd(),  WM_RESAVE_IMAGES, WM_RESAVE_IMAGES, PM_REMOVE))
		;
	if (m_iSaveMode)
	{
//		TRACE0("ResaveImages: ServerBusy, repaint after save\n");
		m_bResave = TRUE;
		return 0;
	}

	SetDlgSaving(SAVE_MODE_EDIT);

	// set the save options for a display save
	CSize sizeImageDisplay;
	CWnd *pWndPicture = GetDlgItem(IDC_PICTURE);
	CRect crPicture;
	pWndPicture->GetClientRect(crPicture);
	crPicture.DeflateRect(MARGIN,MARGIN);
	int iSaveOptions = 0;
	iSaveOptions |= (
		SAV_SizeLimitForDisplay |
		SAV_TopDownDib |
		SAV_FastUpdate8BitDib |					// keep thumb in mem since we may do
   													// color correction in 1 picture mode.
		SAV_UseCurrentRotation | 
		SAV_UseLoResBuffer |						// Since displaying thumb size
		SAV_DoNotScaleUp |
		SAV_UseColorCorrection |				// to get 12 RPD
		m_uiUseKcdfs |
		SAV_UseColorSceneBalance |				// to perform scene balance
		SAV_UseColorAdjustments |				// to adjust color per color slider
		SAV_UseScratchRemovalIfAvailable );	// Always have this on to use scratch removal 

	// whats the max pixel count
	sizeImageDisplay.cx = crPicture.Width();
	sizeImageDisplay.cy = crPicture.Height();

	PiImage* pImage;
	HRESULT hr;
	int iNumberToResave = 0;
	// for each of the pictures requiring re save set them selected
	for (int iIndex = 0; iIndex < m_Pictures.GetSize(); iIndex++)
	{
		UpdateServerData(iIndex);
		pImage = &(m_Pictures[iIndex]->m_PiImage);
		if (pImage->m_bResave)
		{
//			TRACE1("Image %d requires resave\n", iIndex);
			pImage->m_bResave = FALSE;
			pImage->m_bReplace = TRUE;
			iNumberToResave++;
//			TRACE1("ResaveImages: found something to resave %d\n", iIndex);
			hr = m_pISavePictures->PutPictureSelection(iIndex,	S_OR_H_SELECTED, TRUE);
			if(FAILED(hr))
			{
				::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			}
		}
		else
		{
			hr = m_pISavePictures->PutPictureSelection(iIndex,S_OR_H_NONE,TRUE);
			if(FAILED(hr))
			{
				::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			}
		}
	}
	
	// Start the save
	if (iNumberToResave &&
		m_ClientBuffers.bAllocateBuffers(sizeImageDisplay, iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8, iNumberToResave>1?TRUE:FALSE))
	{
//		TRACE0("ResaveImages: starting save operation\n");
		HRESULT hr = m_pISavePictures->SaveToClientMemory(
			INDEX_AllSelected,
			iSaveOptions,
			sizeImageDisplay.cx,			// paint picture to fill the picture control
			sizeImageDisplay.cy,			// paint picture to fill the picture control
			0,									// ignored since iSaveOptions has SAV_UseCurrentRotation
			SCALING_METHOD_BICUBIC,	   // Normal scaling method
			iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8,
			TRUE);							// UseWorkerThread 

		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			SetDlgSaving(SAVE_MODE_IDLE);
			m_ClientBuffers.Delete();
		}
	}
	else
	{
		SetDlgSaving(SAVE_MODE_IDLE);
	}
	// the actual bitmap creation will occur as a result of callbacks from the server
	// see the UpdateDisplayNewProgress handler function

	return 0;
}

void CTLAClientDemoDlg::UpdatePicture()
{
//	TRACE0("CTLAClientDemoDlg: Invalidating Picture\n");
	CWnd* pWndPicture = GetDlgItem(IDC_PICTURE);
	CRect rect;
	pWndPicture->GetClientRect(&rect);
	pWndPicture->MapWindowPoints(this,  &rect);
	InvalidateRect(&rect);
	UpdateWindow();
}


/*
// void AdjustColor() 
// 
// Description:
//
//
// Parameters:
//
//
//	Remarks:
//   
*/
void CTLAClientDemoDlg::AdjustColor(int iRed, int iGreen, int iBlue, int iBright, int iContrast, int iSharpness)
{
	PiColorSetting *pColorSetting;
	int iIndex;

	// for each of the displayed pictures
	int iNumberPicturesToAdjust = m_bDisplayFourPictures ? 4 : 1;
	do 
	{
		iIndex = m_iIndex + iNumberPicturesToAdjust - 1;
		// if we're not past the end of the strip
		if(m_iSavePictureCount > iIndex)
		{
			pColorSetting = &(m_Pictures[iIndex]->m_PiColorSetting);
			if(m_bDisplayFourPictures)
			{
				int i;

				// do differential color adjust
				i = pColorSetting->m_iRed;
				pColorSetting->m_iRed += iRed;
				if (pColorSetting->m_iRed < -1000)
					pColorSetting->m_iRed = -1000;
				else if (pColorSetting->m_iRed > 1000)
					pColorSetting->m_iRed = 1000;

				if (i != pColorSetting->m_iRed)
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;

				i = pColorSetting->m_iGreen;
				pColorSetting->m_iGreen += iGreen;
				if (pColorSetting->m_iGreen < -1000)
					pColorSetting->m_iGreen = -1000;
				else if (pColorSetting->m_iGreen > 1000)
					pColorSetting->m_iGreen = 1000;

				if (i != pColorSetting->m_iGreen)
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;

				i = pColorSetting->m_iBlue;
				pColorSetting->m_iBlue += iBlue;
				if (pColorSetting->m_iBlue < -1000)
					pColorSetting->m_iBlue = -1000;
				else if (pColorSetting->m_iBlue > 1000)
					pColorSetting->m_iBlue = 1000;

				if (i != pColorSetting->m_iBlue)
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
				
				i = pColorSetting->m_iBrightness;
				pColorSetting->m_iBrightness += iBright;
				if (pColorSetting->m_iBrightness < -1000)
					pColorSetting->m_iBrightness = -1000;
				else if (pColorSetting->m_iBrightness > 1000)
					pColorSetting->m_iBrightness = 1000;

				if (i != pColorSetting->m_iBrightness)
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
				
				i = pColorSetting->m_iContrast;
				pColorSetting->m_iContrast += iContrast;
				if (pColorSetting->m_iContrast < -1000)
					pColorSetting->m_iContrast = -1000;
				else if (pColorSetting->m_iContrast > 1000)
					pColorSetting->m_iContrast = 1000;

				if (i != pColorSetting->m_iContrast)
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;

				i = pColorSetting->m_iSharpness;
				pColorSetting->m_iSharpness += iSharpness;
				if (pColorSetting->m_iSharpness < -1000)
					pColorSetting->m_iSharpness = -1000;
				else if (pColorSetting->m_iSharpness > 1000)
					pColorSetting->m_iSharpness = 1000;

				if (i != pColorSetting->m_iSharpness)
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
			}
			else
			{
				// set the color adjust
				if (pColorSetting->m_iRed != iRed)
				{
					pColorSetting->m_iRed = iRed;
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
				}

				if (pColorSetting->m_iGreen != iGreen)
				{
					pColorSetting->m_iGreen = iGreen;
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
				}

				if (pColorSetting->m_iBlue != iBlue)
				{
					pColorSetting->m_iBlue = iBlue;
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
				}

				if (pColorSetting->m_iBrightness != iBright)
				{
					pColorSetting->m_iBrightness = iBright;
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
				}

				if (pColorSetting->m_iContrast != iContrast)
				{
					pColorSetting->m_iContrast = iContrast;
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
				}

				if (pColorSetting->m_iSharpness != iSharpness)
				{
					pColorSetting->m_iSharpness = iSharpness;
					pColorSetting->m_iUpdate = UPDATE_DATA_AND_IMAGE;
				}
			}
		}

		iNumberPicturesToAdjust--;
	} while (iNumberPicturesToAdjust);

	PostMessage(WM_RESAVE_IMAGES);
}

void CTLAClientDemoDlg::OnConnect() 
{
#ifdef COMPLETE_UNLOADING
	switch(m_iInitState)
	{
		case iInitStateBegin:
		{
			GetDlgItem(IDC_CONNECT)->EnableWindow(FALSE);
			if(!bInitCom())
			{
				PostMessage(WM_CLOSE, 0, 0);	// Close down the application on failure
				break;
			}

			if(!bInitClientCallback(TRUE))
			{
				PostMessage(WM_CLOSE, 0, 0);	// Close down the application on failure
				break;
			}

			if(!bInitScanner())
			{
				PostMessage(WM_CLOSE, 0, 0);	// Close down the application on failure
				break;
			}

			SetDlgItemText(IDC_CONNECT, _T("Disconnect"));
			m_ClientBuffers.Init(m_pISavePictures);
			UpdateScanCounts();
			SetDlgSaving(0);
			SetDlgScanning(FALSE);
			break;
		}

		case iInitStateConnected:
		{
			ASSERT(0);
			break;
		}

//		case iInitStateCallBack:
//		case iInitStateInitialized:
		default:
		{
			OnReleaseSaveGroup();
			while (m_iScanRollCount)
			{
				HRESULT hr = m_pISavePictures->DeleteRollInScanGroup(0);
				if(FAILED(hr))
				{
					::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
				}
				UpdateScanCounts();
			};

			bInitClientCallback(FALSE);

			// Release interface pointers to shut down server
			if(NULL != m_pISavePictures)
			{
				m_pISavePictures->Release();
				m_pISavePictures = NULL;
			}

			if(NULL != m_pIScanPictures)
			{
				m_pIScanPictures->Release();
				m_pIScanPictures = NULL;
			}

			if(NULL != m_pITLAMain)
			{
				m_pITLAMain->Release();
				m_pITLAMain = NULL;
			}

			// Shut down this COM apartment
			::CoUninitialize();

			m_iInitState = iInitStateBegin;
			m_ClientBuffers.Init(NULL);

			SetDlgItemText(IDC_CONNECT, _T("Connect"));
			GetDlgItem(IDC_CONNECT)->EnableWindow(TRUE);
			SetDlgSaving(0);
			SetDlgScanning(FALSE);
			break;
		}
	}
#else
	switch(m_iInitState)
	{
		case iInitStateBegin:
		{
			ASSERT(0);
			break;
		}

		case iInitStateConnected:
		{
			if(!bInitClientCallback(TRUE))
			{
				PostMessage(WM_CLOSE, 0, 0);	// Close down the application on failure
				break;
			}

			if(!bInitScanner())
			{
				PostMessage(WM_CLOSE, 0, 0);	// Close down the application on failure
				break;
			}

			SetDlgItemText(IDC_CONNECT, _T("Disconnect"));
			break;
		}

//		case iInitStateCallBack:
//		case iInitStateInitialized:
		default:
		{
			bInitClientCallback(FALSE);
			SetDlgItemText(IDC_CONNECT, _T("Connect"));
			break;
		}
	}
#endif
}

void CTLAClientDemoDlg::OnRunAid() 
{
#ifndef KODAK_AID
	return;
#else
	if((1 != m_iSaveStripCount) ||	// Makes it easier if only one roll in save group
		(m_iSavePictureCount <= 0) ||	// No pictures
		(2 != m_iFilmFormat))			// Not APS
	{
		MessageBox(_T("Oops!"), _T("Having a bad Aid day"), MB_ICONERROR | MB_OK);
	}

	HRESULT hr;
	if(!bRunAid(m_pISavePictures, &hr, m_iSavePictureCount))
	{
		int iErrorNumber;
		if(EC_WorkerThreadExists == (iErrorNumber = ::iGetComErrorNumber(IID_ISavePictures, m_pISavePictures)))
			MessageBox(_T("Worker Thread Busy"), _T("Having a bad Aid day"), MB_ICONERROR | MB_OK);
		else
			::AnalyzeComError2(hr, IID_ISavePictures, m_pISavePictures, iErrorNumber, NULL);
	}
#endif
}

void CTLAClientDemoDlg::OnDeleteRoll() 
{
	int i;
	HRESULT hr;
	CDeleteRollDlg Dlg;

	SetDlgScanning(FALSE);
	Dlg.m_iRollCount = m_iScanRollCount;
	i = Dlg.DoModal();
	while (i != IDCANCEL)
	{
		hr = m_pISavePictures->DeleteRollInScanGroup(Dlg.m_iRollIndex);

		if(FAILED(hr))
		{
			::AnalyzeComError(hr, IID_ISavePictures, m_pISavePictures, NULL);
			// SetDlgSaving(SAVE_MODE_IDLE);
			return;
		}
		UpdateScanCounts();
		Dlg.m_iRollCount = m_iScanRollCount;
		if (m_iScanRollCount > 0)
			i = Dlg.DoModal();
		else
			i = IDCANCEL;
	}
	SetDlgScanning(FALSE);
}

void CTLAClientDemoDlg::OnControlLampOff() 
{
	HRESULT hr;

	hr = m_pIScanPictures->LampManualControl(FILM_COLOR_LAMP_OFF);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}
}

void CTLAClientDemoDlg::OnControlLampDim() 
{
	HRESULT hr;

	hr = m_pIScanPictures->LampManualControl(FILM_COLOR_LAMP_STANDBY);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}
}

void CTLAClientDemoDlg::OnControlLampOn() 
{
	HRESULT hr;

	hr = m_pIScanPictures->LampManualControl(FILM_COLOR_NEGATIVE);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}
}

void CTLAClientDemoDlg::OnControlRollersRelease() 
{
	HRESULT hr;

	hr = m_pIScanPictures->PutFilmPressureRollerPosition(FALSE);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}
}

void CTLAClientDemoDlg::OnControlApsManualRetract() 
{
	HRESULT hr;

	SetDlgScanning(TRUE);

	hr = m_pIScanPictures->ApsManualRetract();

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}
}

void CTLAClientDemoDlg::OnUpdateControlApsManualRetract(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable(!m_bScanning && (SCANNER_TYPE_F_235C == m_iScannerType));
}

void CTLAClientDemoDlg::OnControlDeleteOldestMofRoll() 
{
	HRESULT hr;

	int iFilesRemaining;
	hr = m_pIScanPictures->CountRemainingMofFiles(TRUE, &iFilesRemaining);

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}

	CString str;
	str.Format(L"%d MOF Files Remain", iFilesRemaining);
	MessageBox(str, _T("MOF Files Remaining"));
}

void CTLAClientDemoDlg::OnResetLeds() 
{
	HRESULT hr = m_pIScanPictures->ResetStatusLeds();

	if(FAILED(hr))
	{
		::AnalyzeComError(hr, IID_IScanPictures, m_pIScanPictures, NULL);
		return;
	}
}

void CTLAClientDemoDlg::OnLoadFirmwareAtStartup() 
{
	CButton *pChk = (CButton*)GetDlgItem(IDC_LOAD_FIRMWARE_AT_STARTUP);

	if(BST_CHECKED == pChk->GetCheck())
		m_bLoadFirmwareAtStartup = TRUE;
	else
		m_bLoadFirmwareAtStartup = FALSE;

	bPutField_dw(HKEY_LOCAL_MACHINE, _T("Software\\Pakon\\TLAClientDemo\\General"), _T("LoadFirmwareAtStartup"), m_bLoadFirmwareAtStartup);
}

void CTLAClientDemoDlg::OnSelfTestAtStartup() 
{
	CButton *pChk = (CButton*)GetDlgItem(IDC_SELF_TEST_AT_STARTUP);

	if(BST_CHECKED == pChk->GetCheck())
		m_bMotorSelfTestAtStartup = TRUE;
	else
		m_bMotorSelfTestAtStartup = FALSE;

	bPutField_dw(HKEY_LOCAL_MACHINE, _T("Software\\Pakon\\TLAClientDemo\\General"), _T("MotorSelfTestAtStartup"), m_bMotorSelfTestAtStartup);
}
