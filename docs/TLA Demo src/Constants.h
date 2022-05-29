#if !defined(AFX_CONSTANTS_H__E98AE429_C86D_4ad8_A58B_17367107B78E__INCLUDED_)
#define AFX_CONSTANTS_H__E98AE429_C86D_4ad8_A58B_17367107B78E__INCLUDED_

//----------------  WM_APP_MESSAGE definition  -----------------
///////////////////////////////////////////////////////////////////////////////////////////////////

#define WM_COM_CALLBACK				(WM_APP + 1)
#define WM_COM_MARSHALL_INTERFACE	(WM_COM_CALLBACK + 1)
#define WM_RESAVE_IMAGES			(WM_COM_MARSHALL_INTERFACE + 1)

#define CALIBRATION_OFFSET 6U
#define CALIBRATION_HEIGHT 2114U

//#define KODAK_AID

typedef struct
{
	CString m_strDirectory;
	CString m_strFileName;
} SiSaveToMemoryDirAndFile;

enum SAVE_MODES
{
	SAVE_MODE_IDLE = 0,
	SAVE_MODE_EXPORT = 1,
	SAVE_MODE_NEW = 2,
	SAVE_MODE_EDIT = 3,
};

enum PICTURE_UPDATE_TYPES
{
	UPDATE_NONE = 0x00,
	UPDATE_DATA = 0x01,
	UPDATE_DATA_AND_IMAGE = 0x02,
};

typedef struct
{
	int		m_iRollIndex;
	int		m_iStripIndex;
	int		m_iFilmProduct;
	int		m_iFilmSpecifier;
	int		m_iIndex;
	int		m_iFrameNumber;
	CString	m_strFrameName;
	CString	m_strFileName;
	CString	m_strDirectory;
	int		m_iRotation;
	int		m_iSelectedHidden;
	BOOL		m_iUpdate;				// Set this flag if data is to be updated.
} PiPictureInfo;

typedef struct
{
	int		m_iStripIndex;
	int		m_iFilmProduct;
	int		m_iFilmSpecifier;
	BOOL	m_iUpdate;
} PiStripInfo;

typedef struct
{
	int		m_iRed;
	int		m_iGreen;
	int		m_iBlue;
	int		m_iBrightness;
	int		m_iContrast;
	int		m_iSharpness;
	BOOL	m_iUpdate;
} PiColorSetting;

typedef struct
{
	long		m_lLeftHR;
	long		m_lTopHR;
	long		m_lRightHR;
	long		m_lBottomHR;
	long		m_lLeftLR;
	long		m_lTopLR;
	long		m_lRightLR;
	long		m_lBottomLR;
	BOOL		m_iUpdate;
} PiFramingUserInfo;

typedef struct
{
	CBitmap*		m_pBitmap;
	BOOL			m_bResave;
	BOOL			m_bReplace;
}	PiImage;

typedef struct
{
	PiPictureInfo		m_PiPictureInfo;
	PiColorSetting		m_PiColorSetting;
	PiFramingUserInfo	m_PiFramingUserInfo;
	PiImage				m_PiImage;
} PiPicture;

#define MARGIN 8
#define HALF_MARGIN (MARGIN / 2)

#endif // !defined(AFX_CONSTANTS_H__E98AE429_C86D_4ad8_A58B_17367107B78E__INCLUDED_)
