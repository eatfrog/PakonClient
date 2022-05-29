// AidInterface.cpp : implementation file
//
#include "stdafx.h"
#include "Constants.h"
/*
#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif
*/
#ifdef KODAK_AID

// Get definitions from the server -- the server must be built first
#include SERVER_CONSTANTS

#include "AidInterface.h"
#include "MofHeader.h"
#include "..\\TLA\\AidToolkit\\inc\\AidRoll.h"

/*
This is a rather odd little bit of code, because GetPictureInfoAid() acts a little strangly.
If you have junk pictures on the leader, they are going the have the same value returned
to auiFrameNumbersAid[i] as the first valid frame (probably 1).  This is because we decided that
all APS frames should be associated with MOF and optical data.  We could change GetPictureInfoAid()
if you feel this is too odd.

The question is whether we should change how TLA behaves - you would have to modify the code
you base off this sample if we did.  I am a little bit concerned about whether other users of TLA
will find the current usage confusing.

The uiHighestFrameNumber and iFirstGoodIndex variables help to deal with this behavior by finding
the first valid frame in the "for(i = iFirstGoodIndex; i < iSavePictureCount; i++)" statement.

If the operator has deleted one or more frames, you must be careful to maintain coordination
of Aid data with the pictures.  Use auiFrameNumbersAid[i] for the frame number associated with
the Aid data and you should never have a problem.

The next version of TLA will have the InsertPicture() function disabled for APS.  It isn't really
all that necessary and way too easy to mess up the coordination.  PutPictureInfo() and PutPictureInfo1()
will not allow changing of the iFrameNumber argument from the orginal value when using APS
for the same reason.

As the schedule permits, we will be using the perforations to verify valid picture locations.
This should reduce the likelyhood of junk pictures being displayed.
*/

BOOL bRunAid(ISavePictures *pISavePictures,
					HRESULT *phr,
					int iSavePictureCount)
{
	*phr = S_OK;

	tagPAKON_MOF_HEADER MofHeader;
																// Create AID Roll
	AIDRoll *pAIDRoll = new AIDRoll(true, true);
	if(NULL == pAIDRoll)
	{
		::MessageBox(NULL, _T("Error allocating pAIDRoll"), _T("Having a bad Aid day"), MB_ICONERROR | MB_OK);
		return FALSE;
	}
																// Get leader data
	AID_LeaderDataIn LeaderDataIn;
	AID_LeaderDataIn *pLeaderDataIn = &LeaderDataIn;
	AID_OpticalLeaderDataIn OpticalLeaderDataIn;
	AID_OpticalLeaderDataIn *pOpticalLeaderDataIn = &OpticalLeaderDataIn;

	*phr = pISavePictures->GetStripInfoAid(0,
											(int)(&MofHeader),
											sizeof(tagPAKON_MOF_HEADER),
											(int)(pLeaderDataIn),
											sizeof(AID_LeaderDataIn),
											(int)(pOpticalLeaderDataIn),
											sizeof(AID_OpticalLeaderDataIn));

	if(FAILED(*phr))
	{
		delete pAIDRoll;
		return FALSE;
	}

	pAIDRoll->setLeaderData(pLeaderDataIn, pOpticalLeaderDataIn);

	TRACE(_T("AID Test Roll Good Reads      %02u, %02u, %02u, %02u\n"), MofHeader.m_FieldCounters.good[RD_TRK_CM1],
																		MofHeader.m_FieldCounters.good[RD_TRK_CM2],
																		MofHeader.m_FieldCounters.good[RD_TRK_PF1],
																		MofHeader.m_FieldCounters.good[RD_TRK_PF2]);

	TRACE(_T("AID Test Roll Corrected Reads %02u, %02u, %02u, %02u\n"), MofHeader.m_FieldCounters.corrected[RD_TRK_CM1],
																		MofHeader.m_FieldCounters.corrected[RD_TRK_CM2],
																		MofHeader.m_FieldCounters.corrected[RD_TRK_PF1],
																		MofHeader.m_FieldCounters.corrected[RD_TRK_PF2]);

	TRACE(_T("AID Test Roll Bad Reads       %02u, %02u, %02u, %02u\n"), MofHeader.m_FieldCounters.bad[RD_TRK_CM1],
																		MofHeader.m_FieldCounters.bad[RD_TRK_CM2],
																		MofHeader.m_FieldCounters.bad[RD_TRK_PF1],
																		MofHeader.m_FieldCounters.bad[RD_TRK_PF2]);

	TRACE(_T("AID Test Roll Dropped Reads   %02u, %02u, %02u, %02u\n"), MofHeader.m_FieldCounters.drop[RD_TRK_CM1],
																		MofHeader.m_FieldCounters.drop[RD_TRK_CM2],
																		MofHeader.m_FieldCounters.drop[RD_TRK_PF1],
																		MofHeader.m_FieldCounters.drop[RD_TRK_PF2]);
	for(int i = 0; i < 2; i++)
	{					// Did the edc library get a good read for the frame on each leader?
		if(MofHeader.m_aTrackResultsLeader[i].m_dwC1)	// Camera Track 1
			TRACE(_T("AID Test Leader %d, GOOD, "), i + 1);
		else
			TRACE(_T("AID Test Leader %d, BAD,  "), i + 1);

		if(MofHeader.m_aTrackResultsLeader[i].m_dwC2)	// Camera Track 2
			TRACE(_T("GOOD, "), i);
		else
			TRACE(_T("BAD,  "), i);

		if(MofHeader.m_aTrackResultsLeader[i].m_dwP1)	// Processor Track 1
			TRACE(_T("GOOD, "), i);
		else
			TRACE(_T("BAD,  "), i);

		if(MofHeader.m_aTrackResultsLeader[i].m_dwP2)	// Processor Track 2
			TRACE(_T("GOOD\n"), i);
		else
			TRACE(_T("BAD\n"), i);
	}

	AID_FrameDataIn FrameDataIn;						// Get frame data
	AID_FrameDataIn *pFrameDataIn = &FrameDataIn;
	AID_OpticalFrameDataIn OpticalFrameDataIn;
	AID_OpticalFrameDataIn *pOpticalFrameDataIn = &OpticalFrameDataIn;

	UINT auiFrameNumbersAid[50];						// Add ten extra, to be safe
	if(50 < iSavePictureCount)
		iSavePictureCount = 50;

	UINT uiHighestFrameNumber = 0;
	int iFirstGoodIndex = 0;
	int iMagneticDataStatus;
	for(i = 0; i < iSavePictureCount; i++)
	{														// Get picture data
		*phr = pISavePictures->GetPictureInfoAid(i,
												(int*)(&(auiFrameNumbersAid[i])),
												&iMagneticDataStatus,
												(int)(pFrameDataIn),
												sizeof(AID_FrameDataIn),
												(int)(pOpticalFrameDataIn),
												sizeof(AID_OpticalFrameDataIn));

		if(FAILED(*phr))
		{
			delete pAIDRoll;
			return FALSE;
		}

		if(40 < auiFrameNumbersAid[i])
			break;											// Invalid frame number

		if(auiFrameNumbersAid[i] <= uiHighestFrameNumber)
			continue;										// Don't count garbage on leader

		if((0 == iFirstGoodIndex) && (0 < uiHighestFrameNumber))
			iFirstGoodIndex = i - 1;						// Finds first frame for loop below

		uiHighestFrameNumber = auiFrameNumbersAid[i];

		pAIDRoll->AddFrame(auiFrameNumbersAid[i], pFrameDataIn, pOpticalFrameDataIn);
	}

	for(i = iFirstGoodIndex; i < iSavePictureCount; i++)	// Skip garbage on leader
	{
		if(40 < auiFrameNumbersAid[i])
			break;											// Invalid frame number

					// Did the edc library get a good read for the frame on each track?
		if(MofHeader.m_aTrackResultsFrame[auiFrameNumbersAid[i] - 1].m_dwC1)	// Camera Track 1
			TRACE(_T("AID Test Frame %u, GOOD, "), auiFrameNumbersAid[i]);
		else
			TRACE(_T("AID Test Frame %u, BAD,  "), auiFrameNumbersAid[i]);

		if(MofHeader.m_aTrackResultsFrame[auiFrameNumbersAid[i] - 1].m_dwC2)	// Camera Track 2
			TRACE(_T("GOOD, "));
		else
			TRACE(_T("BAD,  "));

		if(MofHeader.m_aTrackResultsFrame[auiFrameNumbersAid[i] - 1].m_dwP1)	// Processor Track 1
			TRACE(_T("GOOD, "));
		else
			TRACE(_T("BAD,  "));

		if(MofHeader.m_aTrackResultsFrame[auiFrameNumbersAid[i] - 1].m_dwP2)	// Processor Track 2
			TRACE(_T("GOOD, "));
		else
			TRACE(_T("BAD,  "));
																	// Use AID for frame data
		CAID_ProcessedFrame *pCAID_ProcessedFrame = NULL;
		UINT uiAidError = pAIDRoll->getProcessedFrame(auiFrameNumbersAid[i], pCAID_ProcessedFrame);
		if(NULL != pCAID_ProcessedFrame)
		{
			if(ER_SUCCESS == uiAidError)
			{
				switch(pCAID_ProcessedFrame->par)
				{
					case PAR_P:
					{
						TRACE(_T("Panoramic  "));
						break;
					}

					case PAR_C:
					{
						TRACE(_T("Classic    "));
						break;
					}

//					case PAR_H:
					default:
					{
						TRACE(_T("High Vision"));
						break;
					}
				}

				AID_CameraExpDateAndTime *pCameraExpDateAndTime = &(pCAID_ProcessedFrame->cameraExpDateAndTime);
				TRACE(_T("  %02u:%02u %02u/%02u/%04u\n"), pCameraExpDateAndTime->hour,
															pCameraExpDateAndTime->minutes,
															pCameraExpDateAndTime->month,
															pCameraExpDateAndTime->day,
															pCameraExpDateAndTime->year);
			}

			delete pCAID_ProcessedFrame;
		}
	}

	delete pAIDRoll;
	return TRUE;
}

#endif // KODAK_AID
