#pragma once
#pragma warning(disable : 4005)
#include <iostream>
#include <fstream>
#include <mscoree.h>
#include <metahost.h>
#include <vector>
#pragma warning(disable : 4278)
#import <mscorlib.tlb> raw_interfaces_only
#pragma warning(default : 4278)
#pragma comment(lib, "mscoree.lib")

struct IsLoadedCallerData
{
	char name[24];
	INT32 retData;
};



struct PassInfo
{
	const char* pathptr;
	DWORD* sigs;
	LPVOID strptr;
	LPVOID* Funcs;
};

class DotNetInjection
{

public:
	DotNetInjection();
	unsigned int Launch(const char * assPath, const char * domainName, const char * classtoInstance, VARIANTARG FAR * args, BOOL raw);
	unsigned int Unload(const char * domainName);
	BOOL IsDomainLoaded(const char * domainName);
	std::vector<char> ReadAllBytes(char const* filename);
};
