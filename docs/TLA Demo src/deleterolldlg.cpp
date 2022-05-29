// deleterolldlg.cpp : implementation file
//

#include "stdafx.h"
#include "tlaclientdemo.h"
#include "deleterolldlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CDeleteRollDlg dialog


CDeleteRollDlg::CDeleteRollDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CDeleteRollDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDeleteRollDlg)
	m_iRollIndex = 0;
	//}}AFX_DATA_INIT
}


void CDeleteRollDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDeleteRollDlg)
	DDX_Text(pDX, IDC_DELETE_ROLL_INDEX, m_iRollIndex);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CDeleteRollDlg, CDialog)
	//{{AFX_MSG_MAP(CDeleteRollDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CDeleteRollDlg message handlers

BOOL CDeleteRollDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	CSpinButtonCtrl* pSpin = (CSpinButtonCtrl*)GetDlgItem(IDC_SPIN_ROLL);
	pSpin->SetRange(0, m_iRollCount - 1);
	pSpin->SetPos(0);
	
	return TRUE;
}
