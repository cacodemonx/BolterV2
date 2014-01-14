#include "stdafx.h"
#include "DotNetInjection.h"
#include "Bolter.h"

using namespace mscorlib;

DotNetInjection::DotNetInjection()
{
}

_AppDomain *appDomain = NULL;
ICorRuntimeHost *pHost = NULL;
ICLRMetaHost *pMetaHost = NULL;
ICLRRuntimeInfo *pRuntimeInfo = NULL;
IUnknown *pUnk = NULL;
_Assembly *assembly = NULL;
VARIANT *launcher = new VARIANT();

unsigned int DotNetInjection::Launch(const char * classtoInstance, TCHAR * szPath, unsigned int Unloader)
{
	HRESULT hr;
	if (pHost == NULL)
	{
		//Create instance of the Common Language Runtime
		hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost));

		//Get latest runtime
		hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));

		//Get host interface
		hr = pRuntimeInfo->GetInterface(CLSID_CorRuntimeHost, IID_PPV_ARGS(&pHost));

		//Start CLR Host
		hr = pHost->Start();
		Reload(classtoInstance,szPath,Unloader);
		return 0;
	}
	Reload(classtoInstance,szPath,Unloader);
	return 0;
}
unsigned int DotNetInjection::Unload()
{
	pHost->UnloadDomain(appDomain);

	return 0;
}

unsigned int DotNetInjection::Reload(const char * classtoInstance, TCHAR * szPath, unsigned int Unloader)
{
	HRESULT hr;
	//Get Defualt AppDomain
	hr = pHost->CreateDomain(L"Mr.Rogers",NULL,&pUnk);
	hr = pUnk->QueryInterface(&appDomain);

	//Create SafeArray 
	SAFEARRAYBOUND sabdBounds[1] = {BolterSize, 0}; 
	LPSAFEARRAY lpsaArray = SafeArrayCreate(VT_UI1, 1, sabdBounds);

	//Add the Bolter-XIV managed library (stored as byte array in Bolter.h)
	for( long lIndex = 0; lIndex < BolterSize; ++lIndex )
	{
		SafeArrayPutElement(lpsaArray,&lIndex,&Bolter[lIndex]);
	}

	//Load Bolter-XIV SafeArray in Mr.Rogers AppDomain
	hr = appDomain->Load_3(lpsaArray, &assembly);
	
	//Instantiate Main Bolter-XIV Class (Starts new thread with STA attribute to satisfy WPF) 
	hr = assembly->CreateInstance(_bstr_t(classtoInstance), launcher);
	IDispatch *disp = NULL;
	//Setup dispather to pass the config file path 
	disp = launcher->pdispVal;
	DISPID dispid;

	//Get Id of GetPath() method.
	OLECHAR FAR* methodName = L"GetPath";
	hr = disp->GetIDsOfNames(IID_NULL, &methodName, 1, LOCALE_SYSTEM_DEFAULT, &dispid);
	
	//Create array of the arguments
	

	VARIANTARG *path = new VARIANT();
	VARIANTARG *AssAddress = new VARIANT();
	VARIANTARG *asize = new VARIANT();
	//Set path argument
	BSTR bstr = SysAllocString(szPath);
	path->bstrVal = bstr;
	path->vt = VT_BSTR;

	AssAddress->uintVal = Unloader;
	AssAddress->vt = VT_UINT;

	asize->uintVal = 0;
	asize->vt = VT_UINT;
	VARIANTARG FAR args[] = {*asize,*AssAddress,*path};

	//Set as dispatcher parameters
	DISPPARAMS _disArgs = {args, NULL, 3, 0};
	
	//Invoke GetPath() and pass the config path string, to the managed library.
	hr = disp->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_METHOD, &_disArgs, NULL, NULL, NULL);
	
	SafeArrayDestroy(lpsaArray);
	return 0;
}

void DotNetInjection::KillCLR(DWORD targetProcessId, DWORD targetThreadId)
{
    HANDLE h = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
    if (h != INVALID_HANDLE_VALUE)
    {
        THREADENTRY32 te;
        te.dwSize = sizeof(te);
        if (Thread32First(h, &te))
        {
            do
            {
                if (te.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) + sizeof(te.th32OwnerProcessID)) 
                {
                    if(te.th32ThreadID != targetThreadId && te.th32OwnerProcessID == targetProcessId)
                    {
						
                        HANDLE thread = ::OpenThread(THREAD_ALL_ACCESS, FALSE, te.th32ThreadID);
                        if(thread != NULL)
                        {
							MEMORY_BASIC_INFORMATION *meminfo = new MEMORY_BASIC_INFORMATION();
							VirtualQuery((LPCVOID)GetThreadStartAddress(thread),meminfo,sizeof(MEMORY_BASIC_INFORMATION));
							if (((DWORD)meminfo->BaseAddress == (DWORD)GetModuleHandle(L"clr.dll") + 0xE0000) || ((DWORD)meminfo->BaseAddress == (DWORD)GetModuleHandle(L"clr.dll") + 0xBF000))
							{
								SuspendThread(thread);
								TerminateThread(thread,0);
							}
							CloseHandle(thread);
							delete meminfo;
                        }
                    }
                }
                te.dwSize = sizeof(te);
            } while (Thread32Next(h, &te));
        }
        CloseHandle(h);    
    }
}

DWORD WINAPI DotNetInjection::GetThreadStartAddress(HANDLE hThread)
{

     NTSTATUS ntStatus;

     HANDLE hDupHandle;

     DWORD dwStartAddress;

    if(NtQueryInformationThread == NULL) return 0;

   
    HANDLE hCurrentProcess = GetCurrentProcess();

    if(!DuplicateHandle(hCurrentProcess, hThread, hCurrentProcess, &hDupHandle, THREAD_QUERY_INFORMATION, FALSE, 0)){

        SetLastError(ERROR_ACCESS_DENIED);

        return 0;

    }

    ntStatus = NtQueryInformationThread(hDupHandle, (THREADINFOCLASS)ThreadQuerySetWin32StartAddress, &dwStartAddress, sizeof(DWORD), NULL);

    CloseHandle(hDupHandle);

    

    if (ntStatus != STATUS_SUCCESS) return 0;

    return dwStartAddress;

}

NTSTATUS DotNetInjection::LdrUnloadDll(HANDLE hModule)
{
	HMODULE modNtDll = GetModuleHandle(L"ntdll.dll");

	LPFUN_LdrUnloadDll funLdrUnloadDll = (LPFUN_LdrUnloadDll) GetProcAddress(modNtDll, "LdrUnloadDll");

	NTSTATUS status = funLdrUnloadDll(hModule);
	
	return status;
}

HANDLE DotNetInjection::NtCreateThreadEx(HANDLE process, LPTHREAD_START_ROUTINE Start, LPVOID lpParameter){
     
    HMODULE modNtDll = GetModuleHandle(L"ntdll.dll");
 
    LPFUN_NtCreateThreadEx funNtCreateThreadEx = (LPFUN_NtCreateThreadEx) GetProcAddress(modNtDll, "NtCreateThreadEx");
 
    NtCreateThreadExBuffer ntbuffer;
 
    memset (&ntbuffer,0,sizeof(NtCreateThreadExBuffer));
    DWORD temp1 = 0;
    DWORD temp2 = 0;
 
    ntbuffer.Size = sizeof(NtCreateThreadExBuffer);
    ntbuffer.Unknown1 = 0x10003;
    ntbuffer.Unknown2 = 0x8;
    ntbuffer.Unknown3 = &temp2;
    ntbuffer.Unknown4 = 0;
    ntbuffer.Unknown5 = 0x10004;
    ntbuffer.Unknown6 = 4;
    ntbuffer.Unknown7 = &temp1;
   // ntbuffer.Unknown8 = 0;
 
    HANDLE hThread;  
    NTSTATUS status = funNtCreateThreadEx(
                        &hThread,
                        0x1FFFFF,
                        NULL,
                        process,
                        (LPTHREAD_START_ROUTINE) Start,
                        lpParameter,
                        FALSE, //start instantly
                        0, //null
                        0, //null
                        0, //null
                        &ntbuffer
                        );
                         
       
    return hThread;
 
}