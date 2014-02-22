#include "Source.h"
#include "DotNetInjection.h"
#include "SigScan.h"
#include "GameStructs.h"

INT32 PastAllocation = NULL;

DotNetInjection *BolterBytes = NULL;



unsigned char lockBuff[12] = 
{0x89, 0x55, 0xA8, 0x89, 0x4D, 0xB0
,0x83, 0xF8, 0x1E, 0x73, 0x3A, 0x0F};

unsigned char lockAxisC[12] = 
{0x8B, 0x86, 0xEC, 0x00, 0x00, 0x00
,0xF3, 0x0F, 0x10, 0x45, 0xE0, 0xF3};

unsigned char lockAxisS[15] =
{0x7B, 0x12, 0xF3, 0x0F, 0x10
,0x45, 0x08, 0xF3, 0x0F, 0x11
,0x81, 0xE0, 0x00, 0x00, 0x00};

unsigned char hideBuffSig[12] = 
{0x8B, 0x45, 0x08, 0x83, 0xF8, 0x1E
,0x72, 0x06, 0x33, 0xC0, 0x5D, 0xC2};

unsigned char playerStructSig[18] =
{0x56, 0x8B, 0xDF, 0x89, 0x45, 0xF8
,0x90, 0x8B, 0x4D, 0x0C, 0x8B, 0x33
,0x8B,0x01, 0x8B, 0x50, 0x04, 0x56};

unsigned char movementSig[11] =
{0x84, 0xC0, 0x74, 0x09, 0x80 ,
0x3D, 0x00, 0x00, 0x00, 0x00, 0x00};

unsigned char targetSig[14] = 
{0x00, 0x51, 0x8B, 0x4E, 0x10, 0xD9, 0x1C, 
0x24, 0x51, 0x8D, 0x55, 0xEC, 0x52, 0xB9};

unsigned char zoneSig[17] = 
{0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 
0x55, 0x8B, 0xEC, 0x53, 0x56, 0x8B, 
0x35, 0x00, 0x00, 0x00, 0x00, 0x57};

unsigned char collision[15] =
{0x89, 0x45, 0xFC, 0x3B, 0xC1, 
0x74, 0x6C, 0x38, 0x8E, 0xCC, 
0x01, 0x00, 0x00, 0x74, 0x38};

unsigned char masterSig[13] = 
{0x84, 0xC0, 0x0F, 0x95, 0xC0, 0x88, 
0x06, 0x8B, 0x46, 0x04, 0x48, 0x75, 0x2E};

unsigned char menuSig[12] = 
{0x00, 0x57, 0x8B, 0xCE, 0xFF, 0xD2, 
0x84, 0xC0, 0x74, 0x6A, 0x84, 0x1D};


INT APIENTRY DllMain(HMODULE hDLL, INT32 Reason, LPVOID Reserved) {

	switch(Reason) {
	case DLL_PROCESS_ATTACH:		
		break;
	case DLL_PROCESS_DETACH:
		break;
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	}
	return TRUE;
}

extern "C"
{
	DECLDIR void LoadIt(LoadParas * paras) //const char * xmlpath
	{
		if (BolterBytes == NULL)
		{
			// Instantiate the CLR hosting class.
			BolterBytes = new DotNetInjection();
		}

		// Is our AppDomain loaded
		if (BolterBytes->IsDomainLoaded(paras->domainName))
		{
			// Unload it, the user has tried to open 2 instances.
			BolterBytes->Unload(paras->domainName);
		}

		/* Sig scan stuff. */
		// Get base module information and save it to the global variable. The scanner threads will share this.
		GetModuleInformation( GetCurrentProcess(), GetModuleHandleW( L"ffxiv.exe" ), &modinfo, sizeof( MODULEINFO ) );
		// Scan for all the sigs.
		sigPoints[0] = FindPatternEx(zoneSig,"xxxxxxxxxxxx????x");
		sigPoints[1] = FindPatternEx(lockBuff,"xxxxxxxxxxxx");
		sigPoints[2] = FindPatternEx(lockAxisC,"xxxxxxxxxxxx");
		sigPoints[3] = FindPatternEx(lockAxisS,"xxxxxxxxxxxxxxx");
		sigPoints[4] = FindPatternEx(hideBuffSig,"xxxxxxxxxxxx");
		sigPoints[5] = FindPatternEx(playerStructSig,"xxxxxxxxxxxxxxxxxx");
		sigPoints[6] = FindPatternEx(movementSig,"xxxxxx????x");
		sigPoints[7] = FindPatternEx(collision,"xxxxxxxxxxxxxxx");
		sigPoints[8] = FindPatternEx(masterSig,"xxxxxxxxxxxxx");
		sigPoints[9] = FindPatternEx(menuSig,"xxxxxxxxxxxx");

		// Close all the handles.
		for (int i = 0;i < THREADCOUNT;i++)
			CloseHandle(scanThreads[i]);

		AllocConsole();

		/* Setup all the data that is to be passed to our managed side. */
		// The path to our configuration doc.

		MasterPtr = (MasterPointer*)(sigPoints[8] + (INT32)modinfo.lpBaseOfDll + 0x3C);

		MovPtr = (Movment**)(sigPoints[6] + (INT32)modinfo.lpBaseOfDll + 0x6);

		PassInfo* pInfo = new PassInfo();
		pInfo->sigs = sigPoints;
		pInfo->pathptr = paras->pathptr;

		FuncPoints[0] = &UnloadIt;
		FuncPoints[1] = &GetPOS;
		FuncPoints[2] = &GetName;
		FuncPoints[3] = &SetPOS;
		FuncPoints[4] = &SetHeading;
		FuncPoints[5] = &GetHeading;
		FuncPoints[6] = &Set3DVector;
		FuncPoints[7] = &Get3DVector;
		FuncPoints[8] = &GetBuff;
		FuncPoints[9] = &GetMovement;
		FuncPoints[10] = &SetMovement;
		FuncPoints[11] = &GetMoveStatus;
		FuncPoints[12] = &SetMoveStatus;
		FuncPoints[13] = &GetEntityID;
		FuncPoints[14] = &GetTargetEntityID;

		pInfo->strptr = paras;
		pInfo->Funcs = FuncPoints;
		

		ManagedData[0].intVal = (int)pInfo;

		ManagedData[0].vt = VT_INT;

		/*Start up the CLR and instantiate the main Bolter class 
		(starts a new thread with the STA attribute, to satisfy WPF)*/
		BolterBytes->Launch(paras->pathptr, paras->domainName, paras->nameSpace, ManagedData, paras->raw == 1);

		

		//delete pInfo;

	}
	
}

void UnloadIt(const char * domainName)
{
	Sleep(2000);
	BolterBytes->Unload(domainName);
}

bool __stdcall MaskCompare( const unsigned char* lpDataPtr, const unsigned char* lpPattern, const char* pszMask )
{
	for (; *pszMask; ++pszMask, ++lpDataPtr, ++lpPattern)
	{
		if (*pszMask == 'x' && *lpDataPtr != *lpPattern)
			return false;
	}
	return (*pszMask) == NULL;
}

INT32 __stdcall FindPattern(PatternParas * paras)
{
	for (INT32 x = paras->startIndex; x < paras->stopIndex; x++)
	{
		if (MaskCompare( reinterpret_cast< unsigned char* >( (INT32)modinfo.lpBaseOfDll + x ), paras->lpPattern, paras->pszMask ))
		{
			retData[paras->sigNumber] = ( x );
			return 0;
		}
	}
	return 0;
}
INT32 FindPatternEx(unsigned char * lpPattern, const char * lpMask) 
{
	int i;
	DWORD dwThreadID;
	INT32 dataChuck = (modinfo.SizeOfImage / THREADCOUNT);

	memcpy(retData,clearData,sizeof(retData));

	for(i = 0; i < THREADCOUNT-1; i++) 
	{
		patternParas[i].lpPattern = lpPattern;
		patternParas[i].pszMask = lpMask;
		patternParas[i].sigNumber = i;
		patternParas[i].startIndex = dataChuck * i;
		patternParas[i].stopIndex = (dataChuck * (i+1)) + 20;
		scanThreads[i] = CreateThread(NULL,0,(FUNC)&FindPattern,&patternParas[i],0,&dwThreadID); 
	}
	patternParas[i].lpPattern = lpPattern;
	patternParas[i].pszMask = lpMask;
	patternParas[i].sigNumber = i;
	patternParas[i].startIndex = dataChuck * i;
	patternParas[i].stopIndex = dataChuck * (i+1) - 20;
	scanThreads[i] = CreateThread(NULL,0,(FUNC)&FindPattern,&patternParas[i],0,&dwThreadID);

	WaitForMultipleObjects(THREADCOUNT,scanThreads,TRUE,1000);

	return GetFirstAddress();
}
INT32 GetFirstAddress()
{

	for (int i = 0;i < THREADCOUNT;i++)
	{
		if (retData[i] != 0)
		{
			return retData[i];
		}
	}
	return 0;
}

