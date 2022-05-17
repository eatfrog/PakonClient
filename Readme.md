_What we know_
- TLA, TLB, TLC, TLX are COM DLL:s that TLXClientDemo interfaces with
- TLXClientDemo is a demonstration app that you can run the Pakon without using the official software
- But development for Windows Xp is tricky today, best to run an old version of VS2010 on the same virtual machine as the Pakon is running on

_What we must find out_
- What do the DLL:s contain and what does TLXClientDemo actually do
- TLX contains functions like FN_AdvanceFilm or FN_ScanPictures which sounds promising, how can we call these functions? Preferrably from some other language than old vc++
- What does TL(A-C) contain? Looks like TLX pulls these modules in and calls them itself
- Visual Studio can generate an interop dll so that you can call on the com methods, but I don't know how to inialize it yet


_Parameter naming_
Are there some clues in the naming of parameters?
iInitializeControl
iSaveToMemoryTimeout
iAdvanceMilliseconds
iAdvanceSpeed
piInitializeWarnings (ref)
pbstrError (ref)
pbstrErrorNumbers (ref)
bEngage
i_uiScanPacketReadTimeOut
i_uiNoFilmTimeOut

_Memory locations and reverse engineering_
10013705 - tlx.dll entry
Ghidra reverses the signature to:
int InitializeScanner(HMODULE baseAddress,int iInitializeControl,undefined4 iSaveToMemoryTimeout)