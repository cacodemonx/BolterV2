#ifndef _SOURCE_H_
#define _SOURCE_H_
#include <iostream>
#include "stdafx.h"

#if defined DLL_EXPORT
#define DECLDIR __declspec(dllexport)
#else
#define DECLDIR __declspec(dllimport)
#endif

const INT32 FuncCount = 15;

LPVOID FuncPoints[FuncCount];

struct LoadParas
{
	char pathptr[260];
	char domainName[260];
	char nameSpace[70];
	int raw;
};

extern "C"
{
	DECLDIR void LoadIt(LoadParas * paras);
}

void UnloadIt(const char * domainName);

#endif