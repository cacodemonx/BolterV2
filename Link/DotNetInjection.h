#pragma once
#pragma warning(disable : 4005)
#include <iostream>
#include <mscoree.h>
#include <metahost.h>

#pragma warning(disable : 4278)
#import <mscorlib.tlb> raw_interfaces_only
#pragma warning(default : 4278)
#pragma comment(lib, "mscoree.lib")

class DotNetInjection
{
public:
	DotNetInjection();
	unsigned int Launch(const char * classtoInstance, VARIANTARG FAR * args);
	unsigned int Unload();
};