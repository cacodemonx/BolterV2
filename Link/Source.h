#ifndef _SOURCE_H_
#define _SOURCE_H_
#include <iostream>
 
#if defined DLL_EXPORT
#define DECLDIR __declspec(dllexport)
#else
#define DECLDIR __declspec(dllimport)
#endif
 
extern "C"
{
	DECLDIR void LoadIt(const char * xmlpath);
	DECLDIR void UnloadIt();
}
 
#endif