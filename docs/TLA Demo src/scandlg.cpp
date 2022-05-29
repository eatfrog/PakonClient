// ScanDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TLAClientDemo.h"
#include "ScanDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CScanDlg dialog


CScanDlg::CScanDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CScanDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CScanDlg)
	m_iResolution = -1;
	m_iFilmFormat = -1;
	m_iFilmColor = -1;
	m_iFramesPerStrip = -1;
	m_bReelToReelMode = FALSE;
	m_bRead_DX = TRUE;
	m_bScratchRemoval = FALSE;
	m_bAggressiveFraming = FALSE;
	m_bLampWarmUp = FALSE;
	m_bExerciseSteppers = FALSE;
	m_bHasFilmDrag = FALSE;
	//}}AFX_DATA_INIT

	m_iOperation = 0;
	m_bF235C = FALSE;
}


void CScanDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CScanDlg)
	DDX_Radio(pDX, IDC_RESOLUTION_LO, m_iResolution);
	DDX_Radio(pDX, IDC_FILM_FORMAT_35MM, m_iFilmFormat);
	DDX_Radio(pDX, IDC_FILM_COLOR_NEGATIVE, m_iFilmColor);
	DDX_Radio(pDX, IDC_FRAMES_PER_STRIP_0, m_iFramesPerStrip);
	DDX_Check(pDX, IDC_REEL_TO_REEL, m_bReelToReelMode);
	DDX_Check(pDX, IDC_READ_DX, m_bRead_DX);
	DDX_Check(pDX, IDC_SCRATCH_REMOVAL, m_bScratchRemoval);
	DDX_Check(pDX, IDC_AGGRESSIVE_FRAMING, m_bAggressiveFraming);
	DDX_Check(pDX, IDC_LAMP_WARM_UP, m_bLampWarmUp);
	DDX_Check(pDX, IDC_EXERCISE_STEPPERS, m_bExerciseSteppers);
	DDX_Check(pDX, IDC_FILM_DRAG, m_bHasFilmDrag);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CScanDlg, CDialog)
	//{{AFX_MSG_MAP(CScanDlg)
	ON_BN_CLICKED(IDC_GAIN_CORRECTIONS, OnGainCorrections)
	ON_BN_CLICKED(IDC_FP_CORRECTIONS, OnFpCorrections)
	ON_BN_CLICKED(IDC_FILM_TRACK, OnSetFilmTrack)
	ON_BN_CLICKED(IDC_CORRECTIONS_FOCUS, OnCorrectionsFocus)
	ON_BN_CLICKED(IDC_PRE_SCAN, OnPreScan)
	ON_BN_CLICKED(IDOK, OnScan)
	ON_BN_CLICKED(IDC_CORRECTIONS_FOCUS_ADVANCE_FILM, OnCorrectionsFocusAdvanceFilm)
	ON_BN_CLICKED(IDC_DO_FILM_TRACK_TEST, OnDoFilmTrackTest)
	ON_BN_CLICKED(IDC_IMPORT, OnImport)
	ON_BN_CLICKED(IDC_RESET_MOTOR_SPEED_ADJUST, OnResetMotorSpeedAdjust)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CScanDlg message handlers

BOOL CScanDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	if(!m_bF235C)
	{
		GetDlgItem(IDC_FILM_FORMAT_24MM_CART)->EnableWindow(FALSE);
		GetDlgItem(IDC_FILM_FORMAT_24MM_CART_MOF_READER)->EnableWindow(FALSE);
		GetDlgItem(IDC_FILM_FORMAT_24MM_CART_MOF_FILE)->EnableWindow(FALSE);
		GetDlgItem(IDC_FILM_FORMAT_24MM_CART_MOF_FILE_OR_READER)->EnableWindow(FALSE);
	}
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CScanDlg::OnGainCorrections() 
{
	m_iOperation = iSCAN_OP_CORRECTION_GAIN;
	CDialog::OnOK();
}

void CScanDlg::OnFpCorrections() 
{
	m_iOperation = iSCAN_OP_CORRECTION_FP;
	CDialog::OnOK();
}

void CScanDlg::OnCorrectionsFocusAdvanceFilm() 
{
	m_iOperation = iSCAN_OP_CORRECTION_FOCUS_ADVANCE_FILM;
	CDialog::OnOK();
}

void CScanDlg::OnCorrectionsFocus() 
{
	m_iOperation = iSCAN_OP_CORRECTION_FOCUS;
	CDialog::OnOK();
}

void CScanDlg::OnSetFilmTrack() 
{
	m_iOperation = iSCAN_OP_SET_FILM_TRACK;
	CDialog::OnOK();
}

void CScanDlg::OnPreScan() 
{
	m_iOperation = iSCAN_OP_PRE_SCAN;
	CDialog::OnOK();
}

void CScanDlg::OnScan() 
{
	m_iOperation = iSCAN_OP_SCAN;
	CDialog::OnOK();
}

void CScanDlg::OnDoFilmTrackTest() 
{
	m_iOperation = iSCAN_OP_DO_FILM_TRACK_TEST;
	CDialog::OnOK();
}

void CScanDlg::OnImport() 
{
	m_iOperation = iSCAN_OP_IMPORT;
	CDialog::OnOK();
}

void CScanDlg::OnResetMotorSpeedAdjust() 
{
	m_iOperation = iSCAN_OP_RESET_MOTOR_SPEED_ADJUST;
	CDialog::OnOK();
}
