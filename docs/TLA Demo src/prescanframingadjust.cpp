// PreScanFramingAdjust.cpp : implementation file
//

#include "stdafx.h"
#include "TLAclientdemo.h"
#include "PreScanFramingAdjust.h"
#include "Globals.h"
#include "TLAClientDemoDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CPreScanFramingAdjust dialog


CPreScanFramingAdjust::CPreScanFramingAdjust(CWnd* pParent /*=NULL*/)
	: CDialog(CPreScanFramingAdjust::IDD, pParent)
{
	//{{AFX_DATA_INIT(CPreScanFramingAdjust)
	m_iHeightHR = 0;
	m_iHeightHR_mm = 0;
	m_iWidthHR_Adj = 0;
	m_iWidthUnitHR_Adj = 0;
	//}}AFX_DATA_INIT
	m_crCropHR.SetRectEmpty();
	m_crCropHR_Adj.SetRectEmpty();
}

void CPreScanFramingAdjust::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

	// Enforce the same rules the server enforces

	//{{AFX_DATA_MAP(CPreScanFramingAdjust)
	DDX_Text(pDX, IDC_EDIT_HIGH_RES_HEIGHT, m_iHeightHR);
	DDX_Text(pDX, IDC_EDIT_HIGH_RES_HEIGHT_MM, m_iHeightHR_mm);
	DDX_Text(pDX, IDC_EDIT_WIDTH, m_iWidthHR_Adj);
	DDX_Text(pDX, IDC_EDIT_WIDTH_UNIT, m_iWidthUnitHR_Adj);
	//}}AFX_DATA_MAP
	DDX_Text(pDX, IDC_EDIT_LEFT, m_crCropHR_Adj.left);
	DDX_Text(pDX, IDC_EDIT_RIGHT, m_crCropHR_Adj.right);
	DDX_Text(pDX, IDC_EDIT_TOP, m_crCropHR_Adj.top);
	DDX_Text(pDX, IDC_EDIT_BOTTOM, m_crCropHR_Adj.bottom);
}


BEGIN_MESSAGE_MAP(CPreScanFramingAdjust, CDialog)
	//{{AFX_MSG_MAP(CPreScanFramingAdjust)
	ON_BN_CLICKED(ID_RESET, OnReset)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CPreScanFramingAdjust message handlers


BOOL CPreScanFramingAdjust::OnInitDialog() 
{
	CDialog::OnInitDialog();

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CPreScanFramingAdjust::OnReset() 
{
	CString str;
	str.Format(_T("%d"), m_iHeightHR);
	GetDlgItem(IDC_EDIT_HIGH_RES_HEIGHT)->SetWindowText(str);

	str.Format(_T("%d"), m_iHeightHR_mm);
	GetDlgItem(IDC_EDIT_HIGH_RES_HEIGHT_MM)->SetWindowText(str);

	str.Format(_T("%d"), m_iWidthHR);
	GetDlgItem(IDC_EDIT_WIDTH)->SetWindowText(str);

	str.Format(_T("%d"), m_iWidthUnitHR);
	GetDlgItem(IDC_EDIT_WIDTH_UNIT_35)->SetWindowText(str);

	str.Format(_T("%d"), m_crCropHR.left);
	GetDlgItem(IDC_EDIT_LEFT)->SetWindowText(str);

	str.Format(_T("%d"), m_crCropHR.top);
	GetDlgItem(IDC_EDIT_TOP)->SetWindowText(str);

	str.Format(_T("%d"), m_crCropHR.right);
	GetDlgItem(IDC_EDIT_RIGHT)->SetWindowText(str);

	str.Format(_T("%d"), m_crCropHR.bottom);
	GetDlgItem(IDC_EDIT_BOTTOM)->SetWindowText(str);
}

BOOL CPreScanFramingAdjust::bCheckRect(CRect &rcrRect, CRect &rcrBoundary, CSize &rcsMinSize)
{
	if(rcrRect.left < rcrBoundary.left)
		return FALSE;

	if(rcrRect.top < rcrBoundary.top)
		return FALSE;

	if(rcrRect.right > rcrBoundary.right)
		return FALSE;

	if(rcrRect.bottom > rcrBoundary.bottom)
		return FALSE;

	if(rcrRect.Height() < rcsMinSize.cy)
		return FALSE;

	if(rcrRect.Width() < rcsMinSize.cx)
		return FALSE;

	return TRUE;
}


void CPreScanFramingAdjust::OnOK() 
{
	// bounding rectangle for crop
	CRect crBoundary = CRect(0, 0, m_iWidthHR_Adj - 1, m_iHeightHR - 1);

	// min sizes of crop
	int iMinWidth = m_iWidthHR / 4;
	int iMinHeight = m_iHeightHR / 4;

	if(!bCheckRect(m_crCropHR_Adj, crBoundary, CSize(iMinWidth, iMinHeight)))
	{
		// inform user of error
		AfxMessageBox(_T("Cropping Invalid"));
		return;
	}

	if(m_iWidthUnitHR_Adj < m_iWidthUnitHR / 4 ||
		m_iWidthUnitHR_Adj < m_iWidthHR_Adj + 25 || 
		m_iWidthUnitHR_Adj > m_iWidthUnitHR * 4)
	{
		// inform user of error
		AfxMessageBox(_T("Framing Invalid"));
		return;
	}

	CDialog::OnOK();
}
