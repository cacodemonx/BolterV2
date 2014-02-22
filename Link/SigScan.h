#pragma once
#include "stdafx.h"
#include <Psapi.h>
#pragma comment(lib, "Psapi.lib")
#define THREADCOUNT 8
#define FUNC LPTHREAD_START_ROUTINE

HANDLE scanThreads[THREADCOUNT];
DWORD sigPoints[10];
MODULEINFO modinfo = { 0 };
INT32 retData[THREADCOUNT];

INT32 clearData[THREADCOUNT];

VARIANTARG FAR ManagedData[1];

struct PatternParas
{
	const unsigned char* lpPattern;
	const char* pszMask;
	INT32 startIndex;
	INT32 stopIndex;
	int sigNumber;
};
PatternParas patternParas[THREADCOUNT];

INT32 FindPatternEx(unsigned char * lpPattern, const char * lpMask); 
bool __stdcall MaskCompare( const unsigned char* lpDataPtr, const unsigned char* lpPattern, const char* pszMask );
INT32 __stdcall FindPattern(PatternParas * paras);
INT32 GetFirstAddress();