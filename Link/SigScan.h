#pragma once
#include "stdafx.h"
#include <Psapi.h>
#pragma comment(lib, "Psapi.lib")
#define THREADCOUNT 8
#define FUNC LPTHREAD_START_ROUTINE

HANDLE scanThreads[THREADCOUNT];
DWORD sigPoints[10];
MODULEINFO modinfo = { 0 };
DWORD retData[THREADCOUNT];

DWORD clearData[THREADCOUNT];

VARIANTARG FAR ManagedData[12];

struct PatternParas
{
	const unsigned char* lpPattern;
	const char* pszMask;
	DWORD startIndex;
	DWORD stopIndex;
	int sigNumber;
};
PatternParas patternParas[THREADCOUNT];

DWORD FindPatternEx(unsigned char * lpPattern, const char * lpMask); 
bool __stdcall MaskCompare( const unsigned char* lpDataPtr, const unsigned char* lpPattern, const char* pszMask );
DWORD __stdcall FindPattern(PatternParas * paras);
DWORD GetFirstAddress();