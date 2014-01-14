#include "Source.h"
#include "stdafx.h"
#include "DotNetInjection.h"

TCHAR path[200];
DotNetInjection *BolterBytes = NULL;

INT APIENTRY DllMain(HMODULE hDLL, DWORD Reason, LPVOID Reserved) {

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
	DECLDIR void LoadIt(const char * xmlpath) //const char * xmlpath
	{
		for (unsigned int i = 0;i < strlen(xmlpath); i++)
		{
			path[i] = xmlpath[i];
		}
		if (BolterBytes == NULL)
		{
			BolterBytes = new DotNetInjection();
		}
		void * p = &UnloadIt;
		BolterBytes->Launch("Bolter_XIV.STAThread",path,(unsigned int)p);
		
	}
	DECLDIR void UnloadIt() //const char * xmlpath
	{
		Sleep(2000);
		BolterBytes->Unload();
		//try{delete[] BolterBytes;}catch (...){}
	}
}