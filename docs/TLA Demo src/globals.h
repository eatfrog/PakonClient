#if !defined(AFX_GLOBALS_H__63D2650D_0DF9_48ca_8A8C_D71167B5CB7A__INCLUDED_)
#define AFX_GLOBALS_H__63D2650D_0DF9_48ca_8A8C_D71167B5CB7A__INCLUDED_

#include <io.h>

#define bFileOrDirectoryExists(szName) (0 == _taccess(szName, 0))

extern "C"
{
	void FormatHResult(HRESULT hr, CString &rstrResult);
	int iGetComErrorNumber(REFIID riid, IUnknown *pIUnknown);
	void AnalyzeComError(HRESULT hr, 
									REFIID riid,
									IUnknown *pIUnknown,
									CString *pstrError,
									BOOL bCallGetAndClearLastError = TRUE);

	void AnalyzeComError2(HRESULT hr, 
									REFIID riid, 
									IUnknown *pIUnknown,
									int iErrorNumber,
									CString *pstrError,
									BOOL bCallGetAndClearLastError = TRUE);

	void GetLastErrorDisplay(CWnd *pWnd, LPCTSTR szCaption);
	BOOL bCreateDirectory(LPCTSTR szDir, CWnd *pWnd);
};

#endif // !defined(AFX_GLOBALS_H__63D2650D_0DF9_48ca_8A8C_D71167B5CB7A__INCLUDED_)
