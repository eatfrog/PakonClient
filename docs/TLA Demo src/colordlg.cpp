// ColorDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TLAclientdemo.h"
#include "ColorDlg.h"
#include SERVER_GUIDS

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CColorDlg dialog


CColorDlg::CColorDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CColorDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CColorDlg)
	m_iContrast = 0;
	m_iBrightness = 0;
	m_iGreen = 0;
	m_iBlue = 0;
	m_iRed = 0;
	m_iSharpness = 0;
	//}}AFX_DATA_INIT

	m_pParent = (CTLAClientDemoDlg*)pParent;
	m_bDisplayFourPictures = FALSE;
	m_iIndex = 0;

	m_iRedUndo = 0;
	m_iGreenUndo = 0;
	m_iBlueUndo = 0;
	m_iBrightnessUndo = 0;
	m_iContrastUndo = 0;
	m_iSharpnessUndo = 0;
}


void CColorDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CColorDlg)
	DDX_Slider(pDX, IDC_SLIDER_C, m_iContrast);
	DDX_Slider(pDX, IDC_SLIDER_D, m_iBrightness);
	DDX_Slider(pDX, IDC_SLIDER_G, m_iGreen);
	DDX_Slider(pDX, IDC_SLIDER_B, m_iBlue);
	DDX_Slider(pDX, IDC_SLIDER_R, m_iRed);
	DDX_Slider(pDX, IDC_SLIDER_SHARPNESS, m_iSharpness);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CColorDlg, CDialog)
	//{{AFX_MSG_MAP(CColorDlg)
	ON_WM_HSCROLL()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CColorDlg message handlers

BOOL CColorDlg::OnInitDialog() 
{
	if(!m_bDisplayFourPictures)
	{
		// save the absolute settings for the picuture
		// so we can undo if the user cancels.
		m_iRedUndo = m_iRed;
		m_iGreenUndo = m_iGreen;
		m_iBlueUndo = m_iBlue;
		m_iBrightnessUndo = m_iBrightness;
		m_iContrastUndo = m_iContrast;
		m_iSharpnessUndo = m_iSharpness;
	}

	// else
		// If we are doing differential we will keep track
		// as we go.

	CDialog::OnInitDialog();	

	// setup the sliders
	CSliderCtrl* pSlide = (CSliderCtrl*)GetDlgItem(IDC_SLIDER_R);
	pSlide->SetRange(0, 2000);
	pSlide->SetPos(m_iRed + 1000);

	pSlide = (CSliderCtrl*)GetDlgItem(IDC_SLIDER_G);
	pSlide->SetRange(0, 2000);
	pSlide->SetPos(m_iGreen + 1000);

	pSlide = (CSliderCtrl*)GetDlgItem(IDC_SLIDER_B);
	pSlide->SetRange(0, 2000);
	pSlide->SetPos(m_iBlue + 1000);

	pSlide = (CSliderCtrl*)GetDlgItem(IDC_SLIDER_D);
	pSlide->SetRange(0, 2000);
	pSlide->SetPos(m_iBrightness + 1000);

	pSlide = (CSliderCtrl*)GetDlgItem(IDC_SLIDER_C);
	pSlide->SetRange(0, 2000);
	pSlide->SetPos(m_iContrast + 1000);

	pSlide = (CSliderCtrl*)GetDlgItem(IDC_SLIDER_SHARPNESS);
	pSlide->SetRange(0, 2000);
	pSlide->SetPos(m_iSharpness + 1000);

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CColorDlg::OnCancel() 
{
	CDialog::OnCancel();
}

void CColorDlg::OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar) 
{
	UINT SliderID = pScrollBar->GetDlgCtrlID();
	CSliderCtrl* pSlide = (CSliderCtrl*)pScrollBar;

	if(m_bDisplayFourPictures)
	{
		int iRedDiff = 0;
		int iGreenDiff = 0;
		int iBlueDiff = 0;
		int iBrightnessDiff = 0;
		int iContrastDiff = 0;
		int iSharpnessDiff = 0;

		switch (SliderID)	
		{
			case IDC_SLIDER_R:
			{
				iRedDiff = (pSlide->GetPos() - 1000) - m_iRed;
				m_iRed += iRedDiff; 
				break;
			}

			case IDC_SLIDER_G:
			{
				iGreenDiff = (pSlide->GetPos() - 1000) - m_iGreen;
				m_iGreen += iGreenDiff; 
				break;
			}

			case IDC_SLIDER_B:
			{
				iBlueDiff = (pSlide->GetPos() - 1000) - m_iBlue;
				m_iBlue += iBlueDiff; 
				break;
			}

			case IDC_SLIDER_D:
			{
				iBrightnessDiff = (pSlide->GetPos() - 1000) - m_iBrightness;
				m_iBrightness += iBrightnessDiff; 
				break;
			}

			case IDC_SLIDER_C:
			{
				iContrastDiff = (pSlide->GetPos() - 1000) - m_iContrast;
				m_iContrast += iContrastDiff;
				break;
			}

//			case IDC_SLIDER_SHARPNESS:
			default:
			{
				iSharpnessDiff = (pSlide->GetPos() - 1000) - m_iSharpness;
				m_iSharpness += iSharpnessDiff; 
				break;
			}
		}

		m_pParent->AdjustColor(iRedDiff,
										iGreenDiff,
										iBlueDiff,
										iBrightnessDiff,
										iContrastDiff,
										iSharpnessDiff);
	}
	else
	{
		switch (SliderID)	
		{
			case IDC_SLIDER_R:
			{
				m_iRed = (pSlide->GetPos() - 1000);
				break;
			}

			case IDC_SLIDER_G:
			{
				m_iGreen = (pSlide->GetPos() - 1000);
				break;
			}

			case IDC_SLIDER_B:
			{
				m_iBlue = (pSlide->GetPos() - 1000);
				break;
			}

			case IDC_SLIDER_D:
			{
				m_iBrightness = (pSlide->GetPos() - 1000);
				break;
			}

			case IDC_SLIDER_C:
			{
				m_iContrast = (pSlide->GetPos() - 1000);
				break;
			}

//			case IDC_SLIDER_SHARPNESS:
			default:
			{
				m_iSharpness = (pSlide->GetPos() - 1000);
				break;
			}
		}

		m_pParent->AdjustColor(m_iRed,
										m_iGreen,
										m_iBlue,
										m_iBrightness,
										m_iContrast,
										m_iSharpness);
	}
}
