// FrameDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TLAClientDemo.h"
#include "FrameDlg.h"
#include "TLAClientDemoDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CFrameDlg dialog


CFrameDlg::CFrameDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CFrameDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CFrameDlg)
	m_iFrameOrCrop = -1;
	m_iBottom = 0;
	m_iLeft = 0;
	m_iRight = 0;
	m_iTop = 0;
	m_iFramingRisk = 0;
	//}}AFX_DATA_INIT
	m_pFramingUserInfo = NULL;
	m_pImage = NULL;
	m_pParent = (CTLAClientDemoDlg*)pParent;
}


void CFrameDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

	//{{AFX_DATA_MAP(CFrameDlg)
	DDX_Radio(pDX, IDC_FRAME_OP_ADJUST, m_iFrameOrCrop);
	DDX_Text(pDX, IDC_EDIT1, m_iFramingRisk);
	//}}AFX_DATA_MAP
	DDX_Text(pDX, IDC_HR_BOTTOM, m_pFramingUserInfo->m_lBottomHR);
	DDX_Text(pDX, IDC_HR_LEFT, m_pFramingUserInfo->m_lLeftHR);
	DDX_Text(pDX, IDC_HR_RIGHT, m_pFramingUserInfo->m_lRightHR);
	DDX_Text(pDX, IDC_HR_TOP, m_pFramingUserInfo->m_lTopHR);
}


BEGIN_MESSAGE_MAP(CFrameDlg, CDialog)
	//{{AFX_MSG_MAP(CFrameDlg)
	ON_BN_CLICKED(IDC_FRAME_OP_ADJUST, OnFrameOpAdjust)
	ON_BN_CLICKED(IDC_FRAME_OP_CROP, OnFrameOpCrop)
	ON_BN_CLICKED(IDC_APPLY, OnApply)
	ON_BN_CLICKED(IDC_RESET, OnReset)
	ON_EN_CHANGE(IDC_HR_LEFT, OnChangeHrLeft)
	ON_EN_CHANGE(IDC_HR_BOTTOM, OnChangeHrBottom)
	ON_EN_CHANGE(IDC_HR_RIGHT, OnChangeHrRight)
	ON_EN_CHANGE(IDC_HR_TOP, OnChangeHrTop)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CFrameDlg message handlers

BOOL CFrameDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	m_iFrameWidth = m_pFramingUserInfo->m_lRightHR - m_pFramingUserInfo->m_lLeftHR;

	if (m_iFrameOrCrop == 0)
		OnFrameOpAdjust();

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

// Saves the current widths and heights of the rectangles
// Disables the Right and Bottom edit fields
void CFrameDlg::OnFrameOpAdjust() 
{
	m_iFrameOrCrop = 0;
	m_iFrameWidth = m_pFramingUserInfo->m_lRightHR - m_pFramingUserInfo->m_lLeftHR;
	GetDlgItem(IDC_HR_RIGHT)->EnableWindow(FALSE);
	GetDlgItem(IDC_HR_BOTTOM)->EnableWindow(FALSE);
	GetDlgItem(IDC_HR_TOP)->EnableWindow(FALSE);
}

// Enables the Right and Bottom edit fields
void CFrameDlg::OnFrameOpCrop() 
{
	m_iFrameOrCrop = 1;
	GetDlgItem(IDC_HR_RIGHT)->EnableWindow(TRUE);
	GetDlgItem(IDC_HR_BOTTOM)->EnableWindow(TRUE);
	GetDlgItem(IDC_HR_TOP)->EnableWindow(TRUE);
}

// Sets values of other edit fields which depend on this one
void CFrameDlg::OnChangeHrLeft() 
{
	if(m_iFrameOrCrop == 0)
	{
		BOOL bTrans;
		int iRight;
		iRight = GetDlgItemInt(IDC_HR_LEFT, &bTrans) + m_iFrameWidth;
		SetDlgItemInt(IDC_HR_RIGHT, iRight);
	}

	GetDlgItem(IDC_APPLY)->EnableWindow(TRUE);
}

void CFrameDlg::OnChangeHrBottom() 
{
	GetDlgItem(IDC_APPLY)->EnableWindow(TRUE);
}

void CFrameDlg::OnChangeHrRight() 
{
	GetDlgItem(IDC_APPLY)->EnableWindow(TRUE);
}

void CFrameDlg::OnChangeHrTop() 
{
	GetDlgItem(IDC_APPLY)->EnableWindow(TRUE);
}

// Use the current edit field values to see where you're at
void CFrameDlg::OnApply() 
{
	if(bValidData())
	{
		UpdateData();
		m_pFramingUserInfo->m_iUpdate |= UPDATE_DATA_AND_IMAGE;
		m_pParent->PostMessage(WM_RESAVE_IMAGES);
	}

	GetDlgItem(IDC_APPLY)->EnableWindow(FALSE);
}

void CFrameDlg::OnOK() 
{
	if(bValidData())
	{
		if(GetDlgItem(IDC_APPLY)->IsWindowEnabled())
		{
			UpdateData();
			m_pFramingUserInfo->m_iUpdate |= UPDATE_DATA_AND_IMAGE;
			m_pParent->PostMessage(WM_RESAVE_IMAGES);
		}

		CDialog::OnOK();
	}
	else // bad framing data
		GetDlgItem(IDC_APPLY)->EnableWindow(FALSE);
}

// Use the rectangles which the framing algorithm found
void CFrameDlg::OnReset() 
{
	SetDlgItemInt(IDC_HR_LEFT, m_crDefault.left);
	SetDlgItemInt(IDC_HR_TOP, m_crDefault.top);
	SetDlgItemInt(IDC_HR_RIGHT, m_crDefault.right);
	SetDlgItemInt(IDC_HR_BOTTOM, m_crDefault.bottom);
	GetDlgItem(IDC_APPLY)->EnableWindow(TRUE);
}

BOOL CFrameDlg::bValidData()
{
	BOOL bTrans;
	CRect crBoundary(0, 0, m_csStrip.cx - 1, m_csStrip.cy - 1);
	CRect crFrame(GetDlgItemInt(IDC_HR_LEFT, &bTrans), 
					  GetDlgItemInt(IDC_HR_TOP, &bTrans), 
					  GetDlgItemInt(IDC_HR_RIGHT, &bTrans), 
					  GetDlgItemInt(IDC_HR_BOTTOM, &bTrans));
	CSize csMin(128, 128);
	return bCheckRect(crFrame, crBoundary, csMin);
}

BOOL CFrameDlg::bCheckRect(CRect &rcrRect, CRect &rcrBoundary, CSize &rcsMinSize)
{
	CString strMsg;
	BOOL bReturn;

	if (rcrRect.right < rcrRect.left)
	{
		strMsg = _T("Right must be greater than Left");
		ErrorMsg(IDC_HR_RIGHT, strMsg);
	}
	else if (rcrRect.bottom < rcrRect.top)
	{
		strMsg = _T("Bottom must be greater than Top");
		ErrorMsg(IDC_HR_BOTTOM, strMsg);
	}
	else if(rcrRect.left < rcrBoundary.left)
	{
		strMsg.Format(_T("Left cannot be less than %d"), rcrBoundary.left);
		ErrorMsg(IDC_HR_LEFT, strMsg);
	}
	else if(rcrRect.top < rcrBoundary.top)
	{
		strMsg.Format(_T("Top cannot be less than %d"), rcrBoundary.top);
		ErrorMsg(IDC_HR_TOP, strMsg);
	}
	else if(rcrRect.right > rcrBoundary.right)
	{
		strMsg.Format(_T("Right cannot be greater than %d"), rcrBoundary.right);
		ErrorMsg(IDC_HR_RIGHT, strMsg);
	}
	else if(rcrRect.bottom > rcrBoundary.bottom)
	{
		strMsg.Format(_T("Bottom cannot be greater than %d"), rcrBoundary.bottom);
		ErrorMsg(IDC_HR_BOTTOM, strMsg);
	}
	else if(rcrRect.Height() < rcsMinSize.cy)
	{
		strMsg.Format(_T("Bottom - Top must be greater than %d"), rcsMinSize.cy);
		ErrorMsg(IDC_HR_BOTTOM, strMsg);
	}
	else if(rcrRect.Width() < rcsMinSize.cx)
	{
		strMsg.Format(_T("Right - Left must be greater than %d"), rcsMinSize.cx);
		ErrorMsg(IDC_HR_RIGHT, strMsg);
	}

	if (strMsg.IsEmpty())
		bReturn = TRUE;
	else
		bReturn = FALSE;

	return bReturn;
}

void CFrameDlg::ErrorMsg(UINT iEditControl, CString &strMsg)
{
	CEdit* pEdit = (CEdit*)GetDlgItem(iEditControl);
	pEdit->SetFocus();
	pEdit->SetSel(0, -1);
	CString strCaption = _T("Invalid Frame Data");
	MessageBox(strMsg, strCaption);
}
