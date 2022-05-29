// AdvanceMotor.cpp : implementation file
//

#include "stdafx.h"
#include "tlaclientdemo.h"
#include "AdvanceMotor.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CAdvanceMotor dialog


CAdvanceMotor::CAdvanceMotor(CWnd* pParent /*=NULL*/)
	: CDialog(CAdvanceMotor::IDD, pParent)
{
	//{{AFX_DATA_INIT(CAdvanceMotor)
	m_iMilliseconds = 0;
	m_iSpeed = 0;
	//}}AFX_DATA_INIT
}


void CAdvanceMotor::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAdvanceMotor)
	DDX_Control(pDX, IDC_SPIN_SPEED, m_spinSpeed);
	DDX_Text(pDX, IDC_MILLISECONDS, m_iMilliseconds);
	DDV_MinMaxInt(pDX, m_iMilliseconds, -1, 60000);
	DDX_Text(pDX, IDC_SPEED, m_iSpeed);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CAdvanceMotor, CDialog)
	//{{AFX_MSG_MAP(CAdvanceMotor)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CAdvanceMotor message handlers

BOOL CAdvanceMotor::OnInitDialog() 
{
	CDialog::OnInitDialog();
	m_spinSpeed.SetRange32(-2200, 2200);
	m_spinSpeed.SetPos(m_iSpeed);
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
