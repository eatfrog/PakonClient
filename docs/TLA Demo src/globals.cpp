#include "stdafx.h"
#include "AfxPriv.h"	// for string conversion macros (W2T, etc.)
#include "Globals.h"
#include "Constants.h"
#include "resource.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Get definitions from the server -- the server must be built first
#include SERVER_CONSTANTS

void FormatHResult(HRESULT hr, CString &rstrResult)
{
	LPVOID lpMsgBuf;
	::FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
						NULL,
						hr,
						MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
						(LPTSTR)(&lpMsgBuf),
						0,
						NULL);

	rstrResult = (TCHAR*)lpMsgBuf;
	LocalFree(lpMsgBuf);				// Free the buffer.
}

int iGetComErrorNumber(REFIID riid, IUnknown *pIUnknown)
{
	USES_CONVERSION;
	int iErrorNumber = 0;

	// Check that the current interface supports error objects.
	ISupportErrorInfo *pISupportErrorInfo;
	if(SUCCEEDED(pIUnknown->QueryInterface(IID_ISupportErrorInfo, (void**)&pISupportErrorInfo)))
	{
		if(SUCCEEDED(pISupportErrorInfo->InterfaceSupportsErrorInfo(riid)))
		{
			IErrorInfo *pIErrorInfo = NULL;
			if(SUCCEEDED(::GetErrorInfo(0, &pIErrorInfo)))
			{
				if(NULL != pIErrorInfo)
				{	// we got the error number from the server
					BSTR bstrDescription = NULL;
					HRESULT hr1 = pIErrorInfo->GetDescription(&bstrDescription);
					ASSERT(SUCCEEDED(hr1));
					iErrorNumber = _ttoi(W2T(bstrDescription));
					::SysFreeString(bstrDescription);
				}
			}
		}
	}

	return iErrorNumber;
}

void AnalyzeComError(HRESULT hr, 
								REFIID riid, 
								IUnknown *pIUnknown,
								CString *pstrError,
								BOOL bCallGetAndClearLastError)
{
	int iErrorNumber = ::iGetComErrorNumber(riid, pIUnknown);
	::AnalyzeComError2(hr, 
								riid, 
								pIUnknown,
								iErrorNumber,
								pstrError,
								bCallGetAndClearLastError);
}

// Use this if iGetComErrorNumber has already been called
void AnalyzeComError2(HRESULT hr, 
								REFIID riid, 
								IUnknown *pIUnknown,
								int iErrorNumber,
								CString *pstrError,
								BOOL bCallGetAndClearLastError)
{

	// This is the list of error numbers that can occur without an error being
	// sent for us to retrieve using GetAndClearLastError (in ERROR_CODES_000).
	// You should still use GetAndClearLastError to see if there is more information
	// about the error.
	CString strErrorObject;
	if((EC_InvalidPtrToClientCallback <= iErrorNumber) &&
		(iErrorNumber <= EC_NotAllowedWithAps) &&
		(EC_PreviousError != iErrorNumber))
	{
		strErrorObject.LoadString(IDS_COM_ERROR01 + iErrorNumber - EC_InvalidPtrToClientCallback);
	}

	// Calling GetAndClearLastError will give us a stack trace of the error.
	// If the error occurred within a worker thread function, after the initiating
	// call has returned control to the client, this is the only method to get
	// error info.
	CString strErrorGetAndClearLastError;
	if(bCallGetAndClearLastError)
	{
		BSTR bstrError = NULL;
		BSTR bstrErrorObjects = NULL;
		BOOL bErrorOccurred;
		int iShort_IID;
		
		ITLAMain *pITLAMain = NULL;
		HRESULT hr2 = S_OK;
		
		if(IID_ITLAMain == riid)
		{
			pITLAMain = (ITLAMain*)pIUnknown;
			iShort_IID = INT_IID_ITLAMain;
		}
		else
		{	// get an interface pointer to GetAndClearLastError
			hr2 = pIUnknown->QueryInterface(IID_ITLAMain, (void**)&pITLAMain);
			if(FAILED(hr2))
			{
				CString strHResult;
				::FormatHResult(hr2, strHResult);
				strErrorGetAndClearLastError.Format(_T("QueryInterface for Error Function Interface\n  %s"), strHResult);
			}
			if (IID_IScanPictures == riid)
				iShort_IID = INT_IID_IScanPictures;
			else
				iShort_IID = INT_IID_ISavePictures;
		}

		if(SUCCEEDED(hr2))
		{	// call GetAndClearLastError
			hr2 = pITLAMain->GetAndClearLastError(
				iShort_IID,
				&bstrError, 
				&bstrErrorObjects, 
				&bErrorOccurred);
			if(FAILED(hr2))
			{
				CString strHResult1;
				::FormatHResult(hr2, strHResult1);
				strErrorGetAndClearLastError.Format(_T("Error Function\n  %s"), strHResult1);
			}
			else if(bErrorOccurred)
				strErrorGetAndClearLastError = bstrError;	// GetAndClearLastError had error info

			::SysFreeString(bstrError);
			::SysFreeString(bstrErrorObjects);
		}
		
		// release interface pointer to GetAndClearLastError
		if((IID_ITLAMain != riid) && (NULL != pITLAMain))
			pITLAMain->Release();
	}

	CString *pstrErrorLocal, strErrorLocal;
	BOOL bMessageBox;
	if(NULL == pstrError)
	{	// displaying message box
		bMessageBox = TRUE;
		pstrErrorLocal = &strErrorLocal;
	}
	else
	{	// returning string to caller
		bMessageBox = FALSE;
		pstrErrorLocal = pstrError;
	}

	// Use hr passed in at top of this function
	CString strHResult;
	if(E_FAIL == hr)			// Ignore "Unspecified Error" hresult
		strHResult = strErrorObject;
	else
	{	// hr error text
		::FormatHResult(hr, strHResult);
		if(!strErrorObject.IsEmpty())
		{	// Add error code text from switch statement above
			strHResult += _T("\n");
			strHResult += strErrorObject;
		}
	}

	if(strHResult.IsEmpty())
	{
		if(strErrorGetAndClearLastError.IsEmpty())	// Absolutely nothing - this shouldn't happen
			*pstrErrorLocal = _T("Error reported improperly");
		else							// Error from GetAndClearLastError
			*pstrErrorLocal = strErrorGetAndClearLastError;
	}
	else if(!strErrorGetAndClearLastError.IsEmpty())	// Adding everything together
		pstrErrorLocal->Format(_T("%s\n%s"), strHResult, strErrorGetAndClearLastError);
	else
		*pstrErrorLocal = strHResult;

	if(bMessageBox)
	{
		CString strTitle;
		strTitle.LoadString(IDS_COM_ERROR_TITLE);
		AfxMessageBox(*pstrErrorLocal, MB_ICONERROR | MB_OK);
	}
}

void GetLastErrorDisplay(CWnd *pWnd, LPCTSTR szCaption)
{
	LPVOID lpMsgBuf;					// It didn't work - here's why
	DWORD dwLastError = GetLastError();
	FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
						NULL,
						dwLastError,
						MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
						(LPTSTR)(&lpMsgBuf),
						0,
						NULL);
 
	CString str;
	str.Format(_T("TLAClientDemo - %s"), szCaption);
	if(NULL == pWnd)
	{
		::MessageBox(NULL, (TCHAR*)(lpMsgBuf),
							str,
							MB_OK | MB_ICONERROR | MB_TOPMOST);
	}
	else
	{
		pWnd->MessageBox((TCHAR*)(lpMsgBuf),
							str,
							MB_OK | MB_ICONERROR | MB_TOPMOST);
	}

	LocalFree(lpMsgBuf);				// Free the buffer.
}

/*
	This function creates the directory in szDir.
	If the subdirectories under this directory do not exist, they are created.
	Both "C:\" and "\\Scapegoat\" forms are accepted.
	If there is an error and NULL != pWnd, an error message will be displayed.
	Returns TRUE if OK, FALSE if error.
*/
BOOL bCreateDirectory(LPCTSTR szDir, CWnd *pWnd)
{
	_TCHAR *szDirTemp, *pchCurr;
	BOOL bDone;

	if(bFileOrDirectoryExists(szDir))
		return TRUE;							// Directory already exists

	szDirTemp = new _TCHAR[_tcslen(szDir) + 1];
	_tcscpy(szDirTemp, szDir);

												// verify proper form
	if(0 == _tcsncmp(_T("\\\\"), szDirTemp, 2))
	{											// universal naming convention
		if(6 > _tcslen(szDirTemp))
			goto DammitJim;

		if('\\' == *(szDirTemp + 2))
			goto DammitJim;

		pchCurr = szDirTemp + 2;
		while(('\0' != *pchCurr) &&('\\' != *pchCurr))
			pchCurr++;

		if('\0' == *pchCurr)
			goto DammitJim;

		pchCurr++;
	}
	else
	{											// drive letter
		if(4 > _tcslen(szDirTemp))
			goto DammitJim;

		if(!_istalpha(*szDirTemp) ||(':' != *(szDirTemp + 1)) ||('\\' != *(szDirTemp + 2)))
			goto DammitJim;

		pchCurr = szDirTemp + 3;
	}
												// build directories from root
	bDone = FALSE;
	while(!bDone)
	{
		while(('\0' != *pchCurr) &&('\\' != *pchCurr))
			pchCurr++;

		if('\0' == *pchCurr)
			bDone = TRUE;
		else
			*pchCurr = '\0';

		if(bFileOrDirectoryExists(szDirTemp))
		{												// Sub-Directory already exists
			*pchCurr++ = '\\';
			continue;
		}

		if(::CreateDirectory(szDirTemp, NULL))
		{												// created Sub-Directory	
			*pchCurr++ = '\\';
			continue;
		}

		CString strError("Error Creating Directory: ");
		strError += szDirTemp;
		::GetLastErrorDisplay(pWnd, strError);

		delete[] szDirTemp;
		return FALSE;
	}

	delete[] szDirTemp;
	return TRUE;

DammitJim:
	if(NULL == pWnd)
	{
		::MessageBox(NULL, szDirTemp,
						_T("F-12 Client Demo - Error Creating Directory - Invalid Form"),
						 MB_OK | MB_ICONINFORMATION);
	}
	else
	{
		pWnd->MessageBox(szDirTemp,
						_T("F-12 Client Demo - Error Creating Directory - Invalid Form"),
						 MB_OK | MB_ICONINFORMATION);
	}

	delete[] szDirTemp;
	return FALSE;
}

