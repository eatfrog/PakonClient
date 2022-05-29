// SaveDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TLAClientDemo.h"
#include "SaveDlg.h"
#include "constants.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CSaveDlg dialog


CSaveDlg::CSaveDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CSaveDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CSaveDlg)
	m_iFileFormat = -1;
	m_iDPI = 0;
	m_iCompression = 0;
	m_iRotateType = -1;
	m_iRotation = 0;
	m_bSavUseLoRes = FALSE;
	m_bUseColorCorrection = FALSE;
	m_iIndex = -1;
	m_bFastUpdates = FALSE;
	m_bTopDownDIB = FALSE;
	m_iSaveToMemoryFormat = -1;
	m_iSaveMethod = -1;
	m_iPictureBoundaryHeight = 0;
	m_iPictureBoundaryWidth = 0;
	m_iSizeType = -1;
	m_bUseColorAdjustments = FALSE;
	m_bUseScratchRemoval = FALSE;
	m_bPlanarFileHeader = FALSE;
	m_bMirror = FALSE;
	m_bDibFileHeader = FALSE;
	m_bUseColorSceneBalance = FALSE;
	m_bUseWorkerThread = FALSE;
	//}}AFX_DATA_INIT
}


void CSaveDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CSaveDlg)
	DDX_Control(pDX, IDC_SPIN_DPI, m_SpinDpi);
	DDX_Control(pDX, IDC_SPIN_COMPRESSION, m_SpinCompression);
	DDX_CBIndex(pDX, IDC_FILE_FORMAT_TYPE, m_iFileFormat);
	DDX_Text(pDX, IDC_DPI, m_iDPI);
	DDV_MinMaxInt(pDX, m_iDPI, 50, 6000);
	DDX_Text(pDX, IDC_COMPRESSION, m_iCompression);
	DDV_MinMaxInt(pDX, m_iCompression, 0, 100);
	DDX_Radio(pDX, IDC_ROTATE_CURRENT, m_iRotateType);
	DDX_CBIndex(pDX, IDC_ROTATE_NEW_AMOUNT, m_iRotation);
	DDX_Check(pDX, IDC_SAV_USE_LO_RES_BUFFER, m_bSavUseLoRes);
	DDX_Check(pDX, IDC_SAV_USE_COLOR_CORRECTION, m_bUseColorCorrection);
	DDX_Radio(pDX, IDC_INDEX_CURRENT, m_iIndex);
	DDX_Check(pDX, IDC_SAV_FAST_UPDATES, m_bFastUpdates);
	DDX_Check(pDX, IDC_SAV_TOP_DOWN_DIB, m_bTopDownDIB);
	DDX_Radio(pDX, IDC_PLANAR, m_iSaveToMemoryFormat);
	DDX_Radio(pDX, IDC_SAV_METHOD_DISK, m_iSaveMethod);
	DDX_Text(pDX, IDC_PICTURE_HEIGHT, m_iPictureBoundaryHeight);
	DDV_MinMaxInt(pDX, m_iPictureBoundaryHeight, 16, 3000);
	DDX_Text(pDX, IDC_PICTURE_WIDTH, m_iPictureBoundaryWidth);
	DDV_MinMaxInt(pDX, m_iPictureBoundaryWidth, 16, 3000);
	DDX_Radio(pDX, IDC_SAV_SIZE_ORIGINAL, m_iSizeType);
	DDX_Check(pDX, IDC_SAV_USE_COLOR_ADJUSTMENTS, m_bUseColorAdjustments);
	DDX_Check(pDX, IDC_SAV_USE_SCRATCH_REMOVAL, m_bUseScratchRemoval);
	DDX_Check(pDX, IDC_SAV_PLANAR_FILE_HEADER, m_bPlanarFileHeader);
	DDX_Check(pDX, IDC_MIRROR, m_bMirror);
	DDX_Check(pDX, IDC_SAV_DIB_FILE_HEADER, m_bDibFileHeader);
	DDX_Check(pDX, IDC_SAV_USE_COLOR_SCENE_BALANCE, m_bUseColorSceneBalance);
	DDX_Check(pDX, IDC_USE_WORKER_THREAD, m_bUseWorkerThread);
	//}}AFX_DATA_MAP
}

void CSaveDlg::EnableRotateAmount(BOOL bEnable)
{
	GetDlgItem(IDC_ROTATE_NEW_AMOUNT)->EnableWindow(bEnable);
	GetDlgItem(IDC_MIRROR)->EnableWindow(bEnable);
}

void CSaveDlg::EnableCompressionAmount(BOOL bEnable)
{
	GetDlgItem(IDC_COMPRESSION)->EnableWindow(bEnable);
}

void CSaveDlg::EnableHeightAndWidth(BOOL bEnable)
{
	GetDlgItem(IDC_PICTURE_HEIGHT)->EnableWindow(bEnable);
	GetDlgItem(IDC_PICTURE_WIDTH)->EnableWindow(bEnable);
}

BEGIN_MESSAGE_MAP(CSaveDlg, CDialog)
	//{{AFX_MSG_MAP(CSaveDlg)
	ON_CBN_SELCHANGE(IDC_FILE_FORMAT_TYPE, OnSelchangeFileFormatType)
	ON_BN_CLICKED(IDC_ROTATE_CURRENT, OnRotateCurrent)
	ON_BN_CLICKED(IDC_ROTATE_NEW, OnRotateNew)
	ON_BN_CLICKED(IDC_8BIT_DIB, On8bitDib)
	ON_BN_CLICKED(IDC_PLANAR, OnPlanar)
	ON_BN_CLICKED(IDC_SAV_METHOD_DISK, OnSavMethodDisk)
	ON_BN_CLICKED(IDC_SAV_METHOD_MEMORY_CLIENT, OnSavMethodMemoryClient)
	ON_BN_CLICKED(IDC_SAV_METHOD_MEMORY_SHARED, OnSavMethodMemoryShared)
	ON_BN_CLICKED(IDC_SAV_SIZE_DISPLAY, OnSavSizeDisplay)
	ON_BN_CLICKED(IDC_SAV_SIZE_SAVE, OnSavSizeSave)
	ON_BN_CLICKED(IDC_SAV_SIZE_ORIGINAL, OnSavSizeOriginal)
	ON_BN_CLICKED(IDC_SAV_USE_COLOR_CORRECTION, OnSavUseColorCorrection)
	ON_BN_CLICKED(IDC_SAV_FAST_UPDATES, OnSavFastUpdates)
	ON_BN_CLICKED(IDC_SAV_USE_COLOR_SCENE_BALANCE, OnSavUseColorSceneBalance)
	ON_BN_CLICKED(IDC_INDEX_CURRENT, OnIndexCurrent)
	ON_BN_CLICKED(IDC_INDEX_ALL, OnIndexAll)
	ON_BN_CLICKED(IDC_INDEX_SELECTED, OnIndexSelected)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CSaveDlg message handlers

BOOL CSaveDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	// do not allow rotation input if use current or always landscape is true
	if(m_iRotateType == 0)
		EnableRotateAmount(FALSE);

	// do not allow dimension input if save as original
	if(m_iSizeType == 0)
		EnableHeightAndWidth(FALSE);

	if (0 == m_iSaveMethod)
		OnSavMethodDisk();
	else if (1 == m_iSaveMethod)
		OnSavMethodMemoryShared();
	else
		OnSavMethodMemoryClient();
	
	if (0 == m_iSaveToMemoryFormat)
		OnPlanar();
	else
		On8bitDib();

	// if we may be saving more than one pic
	if (m_iIndex)
	{
		// disable "use worker thread" option and set it true
		OnIndexAll();
	}

	// intialize the spin controls
	m_SpinDpi.SetRange(50, 6000);
	m_SpinCompression.SetRange(0, 100);

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CSaveDlg::OnSavSizeDisplay() 
{
	EnableHeightAndWidth(TRUE);	
}

void CSaveDlg::OnSavSizeSave() 
{
	EnableHeightAndWidth(TRUE);	
}

void CSaveDlg::OnSavSizeOriginal() 
{
	EnableHeightAndWidth(FALSE);	
}

void CSaveDlg::OnSelchangeFileFormatType() 
{
	// can only adjust compression if jpeg
	int iFileType = ((CComboBox*)GetDlgItem(IDC_FILE_FORMAT_TYPE))->GetCurSel();
	if(iFileType == 0 /*|| iFileType == 1*/)
		EnableCompressionAmount(TRUE);
	else
		EnableCompressionAmount(FALSE);
}

void CSaveDlg::OnRotateCurrent() 
{
	EnableRotateAmount(FALSE);
}

void CSaveDlg::OnRotateNew() 
{
	EnableRotateAmount(TRUE);
}

void CSaveDlg::OnPlanar() 
{
	GetDlgItem(IDC_SAV_PLANAR_FILE_HEADER)->EnableWindow(TRUE);
	GetDlgItem(IDC_SAV_DIB_FILE_HEADER)->EnableWindow(FALSE);
	GetDlgItem(IDC_SAV_TOP_DOWN_DIB)->EnableWindow(FALSE);
	GetDlgItem(IDC_SAV_FAST_UPDATES)->EnableWindow(FALSE);
	EnableColorSettings();
	OnSavUseColorSceneBalance(); // enable/disable color adjust
	OnSavUseColorCorrection(); // enable/disable image correction
}

void CSaveDlg::On8bitDib() 
{
	GetDlgItem(IDC_SAV_PLANAR_FILE_HEADER)->EnableWindow(FALSE);
	GetDlgItem(IDC_SAV_DIB_FILE_HEADER)->EnableWindow(TRUE);
	GetDlgItem(IDC_SAV_TOP_DOWN_DIB)->EnableWindow(TRUE);	
	GetDlgItem(IDC_SAV_FAST_UPDATES)->EnableWindow(TRUE);
	OnSavFastUpdates();
}

void CSaveDlg::OnSavMethodDisk() 
{
	EnableDiskControls();
	EnableMemoryControls(FALSE);
}

void CSaveDlg::OnSavMethodMemoryClient() 
{
	EnableDiskControls(FALSE);
	EnableMemoryControls();
}

void CSaveDlg::OnSavMethodMemoryShared() 
{
	EnableDiskControls(FALSE);
	EnableMemoryControls();
}

void CSaveDlg::EnableDiskControls(BOOL bEnable)
{
	GetDlgItem(IDC_FILE_FORMAT_TYPE)->EnableWindow(bEnable);
	if (bEnable)
	{
		GetDlgItem(IDC_DPI)->EnableWindow(bEnable);
		OnSelchangeFileFormatType();
	}
	else
		GetDlgItem(IDC_COMPRESSION)->EnableWindow(bEnable);
}

void CSaveDlg::EnableMemoryControls(BOOL bEnable)
{
	CButton *pPlanarChk = (CButton*)GetDlgItem(IDC_PLANAR);	
	CButton *pDIB8Chk   = (CButton*)GetDlgItem(IDC_8BIT_DIB);	
	
	pPlanarChk->EnableWindow(bEnable);
	pDIB8Chk->EnableWindow(bEnable);

	if (bEnable)
	{
		if (pDIB8Chk->GetCheck())
			On8bitDib();
		else
			OnPlanar();
	}
	else
	{
		GetDlgItem(IDC_SAV_PLANAR_FILE_HEADER)->EnableWindow(FALSE);
		GetDlgItem(IDC_SAV_DIB_FILE_HEADER)->EnableWindow(FALSE);
		GetDlgItem(IDC_SAV_TOP_DOWN_DIB)->EnableWindow(FALSE);
		GetDlgItem(IDC_SAV_FAST_UPDATES)->EnableWindow(FALSE);
		EnableColorSettings();
		OnSavUseColorSceneBalance(); // enable/disable color adjust
		OnSavUseColorCorrection(); // enable/disable image correction
	}
}

void CSaveDlg::OnSavUseColorCorrection() 
{
	CButton *pUseColorCorrectionChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_CORRECTION);	
	CButton *pUseColorAdjustmentsChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_ADJUSTMENTS);	
	CButton *pUseColorSceneBalancesChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_SCENE_BALANCE);	

	if(pUseColorCorrectionChk->GetCheck())
	{
		pUseColorSceneBalancesChk->EnableWindow(TRUE);
		OnSavUseColorSceneBalance();
	}
	else
	{
		pUseColorSceneBalancesChk->SetCheck(0);
		pUseColorSceneBalancesChk->EnableWindow(FALSE);
		pUseColorAdjustmentsChk->SetCheck(0);
		pUseColorAdjustmentsChk->EnableWindow(FALSE);
	}
}


void CSaveDlg::OnSavUseColorSceneBalance() 
{
	CButton *pUseColorCorrectionChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_CORRECTION);	
	CButton *pUseColorAdjustmentsChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_ADJUSTMENTS);	
	CButton *pUseColorSceneBalancesChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_SCENE_BALANCE);	

	if(pUseColorSceneBalancesChk->GetCheck())
	{
		pUseColorAdjustmentsChk->EnableWindow(TRUE);
	}
	else
	{
		pUseColorAdjustmentsChk->SetCheck(0);
		pUseColorAdjustmentsChk->EnableWindow(FALSE);
	}
}

void CSaveDlg::OnSavFastUpdates() 
{
	/*
	if doing fast update 8 bit dib you must be doing full
	color correction (i.e. you must be applying color correction,
	image correction and color adjustments) and you must be saving
	an 8 bit dib format
	*/

	CButton *pFastUpdateChk = (CButton*)GetDlgItem(IDC_SAV_FAST_UPDATES);

	// if user checked the fast update check box
	if(pFastUpdateChk->GetCheck())
	{
		// check all color check boxes
		SetColorSettings();
		// disable all color check boxes
		EnableColorSettings(FALSE);
	}
	else // fast update unchecked
	{
		// enable the color check boxes
		EnableColorSettings();
		OnSavUseColorSceneBalance(); // enable/disable color adjust
		OnSavUseColorCorrection(); // enable/disable image correction
	}
	
}

void CSaveDlg::SetColorSettings(BOOL bCheck)
{
	CButton *pUseColorCorrectionChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_CORRECTION);	
	CButton *pUseColorAdjustmentsChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_ADJUSTMENTS);	
	CButton *pUseColorSceneBalanceChk = (CButton*)GetDlgItem(IDC_SAV_USE_COLOR_SCENE_BALANCE);	

	if (bCheck)
	{
		pUseColorAdjustmentsChk->SetCheck(1);
		pUseColorCorrectionChk->SetCheck(1);
		pUseColorSceneBalanceChk->SetCheck(1);
	}
	else
	{
		pUseColorAdjustmentsChk->SetCheck(0);
		pUseColorCorrectionChk->SetCheck(0);
		pUseColorSceneBalanceChk->SetCheck(0);
	}
}

void CSaveDlg::EnableColorSettings(BOOL bEnable)
{
	GetDlgItem(IDC_SAV_USE_COLOR_CORRECTION)->EnableWindow(bEnable);	
	GetDlgItem(IDC_SAV_USE_COLOR_ADJUSTMENTS)->EnableWindow(bEnable);	
	GetDlgItem(IDC_SAV_USE_COLOR_SCENE_BALANCE)->EnableWindow(bEnable);	
}

void CSaveDlg::OnIndexCurrent() 
{
	// enable "use worker thread" option since we know only one picture 
	// will be saved
	GetDlgItem(IDC_USE_WORKER_THREAD)->EnableWindow();	
}

void CSaveDlg::OnIndexAll() 
{
	// disable "use worker thread" option since we don't know only one picture 
	// will be saved
	((CButton*)GetDlgItem(IDC_USE_WORKER_THREAD))->SetCheck(1);	
	GetDlgItem(IDC_USE_WORKER_THREAD)->EnableWindow(FALSE);	
}

void CSaveDlg::OnIndexSelected() 
{
	// disable "use worker thread" option since we don't know only one picture 
	// will be saved
	((CButton*)GetDlgItem(IDC_USE_WORKER_THREAD))->SetCheck(1);	
	GetDlgItem(IDC_USE_WORKER_THREAD)->EnableWindow(FALSE);	
}
