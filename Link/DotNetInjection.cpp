#include "stdafx.h"
#include "DotNetInjection.h"
#include "Bolter.h"

using namespace mscorlib;

DotNetInjection::DotNetInjection()
{
}

_Assembly *assembly = NULL;
VARIANT *launcher = new VARIANT();
ICorRuntimeHost *pHost = NULL;
_AppDomain *appDomain = NULL;
ICLRMetaHost *pMetaHost = NULL;
ICLRRuntimeInfo *pRuntimeInfo = NULL;
IUnknown *pUnk = NULL;

unsigned int DotNetInjection::Launch(const char * classtoInstance, VARIANTARG FAR * args)
{
	HRESULT hr;

	//Create instance of the Common Language Runtime
	if (pHost == NULL)
	{
		hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost));

		//Get latest runtime
		hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));
		//pRuntimeInfo->BindAsLegacyV2Runtime();
		//Get host interface
		hr = pRuntimeInfo->GetInterface(CLSID_CorRuntimeHost, IID_PPV_ARGS(&pHost));

		//Start CLR Host
		hr = pHost->Start();
	}
	//Create AppDomain, give it a friendly name
	hr = pHost->CreateDomain(L"Mr.Rogers",NULL,&pUnk);
	hr = pUnk->QueryInterface(&appDomain);

	// Set SafeArray bounds.
	SAFEARRAYBOUND sabdBounds[1] = {BolterSize, 0};

	// Create pointer for the SafeArray's data.
	unsigned char *arrayData = NULL;

	// Create SafeArray.
	SAFEARRAY *BolterSA = SafeArrayCreate(VT_UI1,1,sabdBounds);

	// Expose the SafeArray's data pointer.
	SafeArrayAccessData(BolterSA,(void **)&arrayData);

	// Copy the Bolter assembly, directly into the SafeArray's data.
	memcpy(arrayData,Bolter,BolterSize);

	// Close it all up.
	SafeArrayUnaccessData(BolterSA);

	// Load raw assembly data into the AppDomain
	hr = appDomain->Load_3(BolterSA, &assembly);

	// Destroy SafeArray
	SafeArrayDestroy(BolterSA);

	//Instantiate Main Bolter-XIV Class 
	hr = assembly->CreateInstance(_bstr_t(classtoInstance), launcher);

	IDispatch *disp = NULL;
	//Setup dispatcher to pass the unmanaged data
	disp = launcher->pdispVal;
	DISPID dispid;

	//Get Id of PassInfo() method.
	OLECHAR FAR* methodName = L"PassInfo";
	hr = disp->GetIDsOfNames(IID_NULL, &methodName, 1, LOCALE_SYSTEM_DEFAULT, &dispid);

	//Set as dispatcher parameters
	DISPPARAMS _disArgs = {args, NULL, 13, 0};

	//Invoke PassInfo() and pass the unmanaged data, to the managed library.
	hr = disp->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_METHOD, &_disArgs, NULL, NULL, NULL);
	disp->Release();
	return 0;
}
unsigned int DotNetInjection::Unload()
{
	pHost->UnloadDomain(appDomain);
	return 0;
}

BOOL DotNetInjection::IsDomainLoaded()
{

	if (pHost == NULL)
		return false;

	HDOMAINENUM enumHandle = new HDOMAINENUM();

	pHost->EnumDomains(&enumHandle);



	while (true)
	{
		IUnknown* dUnk = NULL;

		_AppDomain *localAppDomain = NULL;

		pHost->NextDomain(enumHandle,&dUnk);

		if (dUnk == NULL) break;

		dUnk->QueryInterface(&localAppDomain);

		BSTR localDomainName = NULL;

		localAppDomain->get_FriendlyName(&localDomainName);

		if (0 == wcscmp(_bstr_t("Mr.Rogers"),localDomainName))
		{
			pHost->CloseEnum(enumHandle);
			SysFreeString(localDomainName);
			return true;
		}

		SysFreeString(localDomainName);
	}

	pHost->CloseEnum(enumHandle);

	return false;
}

