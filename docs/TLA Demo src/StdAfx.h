// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#if !defined(AFX_STDAFX_H__4FF9ECB8_08D9_11D5_A806_00D0B72541DC__INCLUDED_)
#define AFX_STDAFX_H__4FF9ECB8_08D9_11D5_A806_00D0B72541DC__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#ifndef WINVER
	#define WINVER 0x0500	// At least Windows 2000
#elif (WINVER < 0x0500)
	#undef WINVER
	#define WINVER 0x0500	// At least Windows 2000
#endif

#ifndef _WIN32_DCOM
	#define _WIN32_DCOM 0x500	// We are using at least Windows 2000
#endif

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions
#include <afxdisp.h>        // MFC Automation classes
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

//#define STA_THREADED_EXAMPLE	// Comment this out if using the COM Free Threading (MTS) Model

// modify these for your environment and save the file.  When you get an update to the demo, copy
// your file back into the project to restore your settings
#define SERVER_CONSTANTS						"..\\TLA\\TLA.h"
#define SERVER_GUIDS							"..\\TLA\\TLA_i.c"
#define INITIALIZE_CONTROL						INITIALIZE_ProgressUpdatesAsPercent

//#define PAKON_CAL
//#define PAKON_JEFF
//#define PAKON_JOHN
//#define PAKON_WAYNE

#if defined PAKON_CAL
//	#define RING_TAIL_DRIVER_BYTES				8388608			// Default 0x200000
	#define RING_TAIL_DRIVER_BYTES				16777216		// Default 0x200000
	#define DRIVER_TRIGGER_BYTES				(RING_TAIL_DRIVER_BYTES / 8)
	#define RING_TAIL_PROCESSED_BYTES			23068672		// Default 0x1600000
	#define PROCESSED_TRIGGER_BYTES				8388608			// Default 0x400000
	#define MAX_MEMORY_USED						0x2600000		// Default 0x2600000
	#define MIN_MEMORY_USED						0x300000		// Default 0x300000
	#define MAX_FILM_LENGTH_MM					1600			// Default 1600
#elif defined PAKON_JEFF
	#define RING_TAIL_DRIVER_BYTES				0x800000		// Default 0x200000
	#define DRIVER_TRIGGER_BYTES				(RING_TAIL_DRIVER_BYTES / 8)
	#define RING_TAIL_PROCESSED_BYTES			0x1600000		// Default 0x1600000
	#define PROCESSED_TRIGGER_BYTES				0x800000		// Default 0x400000
	#define MAX_MEMORY_USED						0x2600000		// Default 0x2600000
	#define MIN_MEMORY_USED						0x300000		// Default 0x300000
	#define MAX_FILM_LENGTH_MM					1600			// Default 1600
#elif defined PAKON_JOHN
	#define RING_TAIL_DRIVER_BYTES				0x800000		// Default 0x200000
	#define DRIVER_TRIGGER_BYTES				(RING_TAIL_DRIVER_BYTES / 8)
	#define RING_TAIL_PROCESSED_BYTES			0x1600000		// Default 0x1600000
	#define PROCESSED_TRIGGER_BYTES				0x800000		// Default 0x400000
	#define MAX_MEMORY_USED						0x2600000		// Default 0x2600000
	#define MIN_MEMORY_USED						0x300000		// Default 0x300000
	#define MAX_FILM_LENGTH_MM					1600			// Default 1600
#elif defined PAKON_WAYNE
	#define RING_TAIL_DRIVER_BYTES				0x800000		// Default 0x200000
	#define DRIVER_TRIGGER_BYTES				(RING_TAIL_DRIVER_BYTES / 8)
	#define RING_TAIL_PROCESSED_BYTES			0x1600000		// Default 0x1600000
	#define PROCESSED_TRIGGER_BYTES				0x800000		// Default 0x400000
	#define MAX_MEMORY_USED						0x2600000		// Default 0x2600000
	#define MIN_MEMORY_USED						0x300000		// Default 0x300000
	#define MAX_FILM_LENGTH_MM					1600			// Default 1600
#else
	#define RING_TAIL_DRIVER_BYTES				0x800000		// Default 0x200000
	#define DRIVER_TRIGGER_BYTES				0x100000		// 262144
	#define RING_TAIL_PROCESSED_BYTES			0x800000		// Default 0x1600000
	#define PROCESSED_TRIGGER_BYTES				0x200000	// 262144
	#define MAX_MEMORY_USED						0x2600000		// Default 0x2600000
	#define MIN_MEMORY_USED						0x300000		// Default 0x300000
	#define MAX_FILM_LENGTH_MM					1600			// Default 1600
#endif

#define SAVE_TO_MEMORY_TIMEOUT					20000			// time out 20 secs 
#define SAVE_TO_SHARED_MEMORY_SIZE				0				// No SaveToSharedMemory
//#define SAVE_TO_SHARED_MEMORY_SIZE		36004000			// 2 * 3 * 2000 * 3000 + Btmp header stuff

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STDAFX_H__4FF9ECB8_08D9_11D5_A806_00D0B72541DC__INCLUDED_)
