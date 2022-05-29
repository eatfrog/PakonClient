// MofHeader.h
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_MOF_HEADER_H__702D8D17_86E9_4040_80F9_D98274FFF48B__INCLUDED_)
#define AFX_MOF_HEADER_H__702D8D17_86E9_4040_80F9_D98274FFF48B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "stdafx.h"
#include "Constants.h"	// Pakon TLA file which has #define __TLA_DLL_h__
								// Create your own Constants.h without this #define

/*
The use of the pragma below ensures that each element of every structure is placed on
QWORD boundaries.

The file contains the PAKON_MOF_HEADER structure followed immediately by the PAKON_MOF structure.

PAKON_MOF elements can be found by starting at the beginning of the file and adding
PAKON_MOF_HEADER.m_dwPakonMofHeaderBytes bytes.
*/

#pragma pack(push, PAKON_MOF_PACK, 8)

typedef struct tagPAKON_MOF_TRACKS
{ 
	DWORD					m_dwC1;								// Camera Track 1
	DWORD					m_dwC2;								// Camera Track 2
	DWORD					m_dwP1;								// Processor Track 1
	DWORD					m_dwP2;								// Processor Track 2
} PAKON_MOF_TRACKS;

#if defined(__TLA_DLL_h__)

	#include ".\\MofEdc\\edc.h"

	typedef struct tagPAKON_MOF_HEADER
	{ 
		DWORD					m_dwPakonMofHeaderBytes;		// sizeof(PAKON_MOF_HEADER)
		DWORD					m_dwVersion;						// 1
		DWORD					m_dwFrameCount;					// 15, 25 or 40 frames
		PAKON_MOF_TRACKS	m_aTrackResultsLeader[2];		// TRUE or FALSE result from edc_track() for leader
		PAKON_MOF_TRACKS	m_aTrackResultsFrame[40];		// TRUE or FALSE result from edc_track() for frames
		FIELD_COUNTERS		m_FieldCounters;					// Filled in by edc_read_counters()
	} PAKON_MOF_HEADER;

#else

	/* trackId for edc functions */

	#define RD_TRK_PF1 0  /* ID for read data for photofinishing track 1 */
	#define RD_TRK_PF2 1  /* ID for read data for photofinishing track 2 */
	#define RD_TRK_CM1 2  /* ID for read data for camera track 1 */
	#define RD_TRK_CM2 3  /* ID for read data for camera track 2 */

	#define MAX_READ_TRACKS 4

	typedef struct tagPAKON_EDC_FIELD_COUNTERS
	{
		 unsigned long good[MAX_READ_TRACKS];
		 unsigned long corrected[MAX_READ_TRACKS];
		 unsigned long bad[MAX_READ_TRACKS];
		 unsigned long drop[MAX_READ_TRACKS];
	} PAKON_EDC_FIELD_COUNTERS;

	typedef struct tagPAKON_MOF_HEADER
	{ 
		DWORD					m_dwPakonMofHeaderBytes;		// sizeof(PAKON_MOF_HEADER)
		DWORD					m_dwVersion;						// 1
		DWORD					m_dwFrameCount;					// 15, 25 or 40 frames
		PAKON_MOF_TRACKS	m_aTrackResultsLeader[2];		// TRUE or FALSE result from edc_track() for leader
		PAKON_MOF_TRACKS	m_aTrackResultsFrame[40];		// TRUE or FALSE result from edc_track() for frames
		PAKON_EDC_FIELD_COUNTERS m_FieldCounters;			// Filled in by edc_read_counters()
	} PAKON_MOF_HEADER;

#endif // defined(__TLA_DLL_h__)

//#ifndef __TLA_DLL_h__

typedef struct tagPAKON_MOF_AID_TRACK
{
	enum					SizeArray { SIZE = 121 };
	unsigned char		versionIdentifier;				// Data sent to *version_ptr in edc_track()
	unsigned int		numOfValidBytes;					// Data sent to *track_count in edc_track()
	unsigned char		data[SIZE];							// Data sent to *optr in edc_track()
} PAKON_MOF_AID_TRACK;

typedef struct tagPAKON_MOF_AID_DATA
{
	PAKON_MOF_AID_TRACK	c1;
	PAKON_MOF_AID_TRACK	c2;
	PAKON_MOF_AID_TRACK	p1;
	PAKON_MOF_AID_TRACK	p2;
} PAKON_MOF_AID_DATA;

typedef struct tagPAKON_MOF_AID_LEADER_DATA
{
	PAKON_MOF_AID_DATA	leader1;
	PAKON_MOF_AID_DATA	leader2;
} PAKON_MOF_AID_LEADER_DATA;

typedef struct tagPAKON_MOF
{ 
	PAKON_MOF_AID_LEADER_DATA	m_LeaderData;
	PAKON_MOF_AID_DATA			m_aFrameData[40];
} PAKON_MOF;

typedef struct tagPAKON_OPTICAL_AID_LEADER_DATA
{ 
	int lot_number;
	unsigned char filmProductClass;
	unsigned char filmSpecifier;
	unsigned int fid;
	unsigned char numOfExposures;
	unsigned char chol;
	unsigned char ssu;
} PAKON_OPTICAL_AID_LEADER_DATA;

typedef struct tagPAKON_OPTICAL_AID_FRAME_DATA
{ 
	unsigned int frameNum;
	unsigned char par;
} PAKON_OPTICAL_AID_FRAME_DATA;

//#endif // __TLA_DLL_h__

#pragma pack(pop, PAKON_MOF_PACK)

#endif // !defined(AFX_MOF_HEADER_H__702D8D17_86E9_4040_80F9_D98274FFF48B__INCLUDED_)
