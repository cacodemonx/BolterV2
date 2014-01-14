#pragma once
#pragma warning(disable : 4005)
#include <stdio.h>
#include <tlhelp32.h>
#include <iostream>
#include <mscoree.h>
#include <metahost.h>
#include <Winternl.h>
#include <ntstatus.h>
#pragma warning(disable : 4278)
#import <mscorlib.tlb> raw_interfaces_only
#pragma warning(default : 4278)
#pragma comment(lib, "ntdll.lib")
#pragma comment(lib, "mscoree.lib")

typedef NTSTATUS (WINAPI *LPFUN_NtCreateThreadEx)
(
  OUT PHANDLE hThread,
  IN ACCESS_MASK DesiredAccess,
  IN LPVOID ObjectAttributes,
  IN HANDLE ProcessHandle,
  IN LPTHREAD_START_ROUTINE lpStartAddress,
  IN LPVOID lpParameter,
  IN BOOL CreateSuspended,
  IN DWORD StackZeroBits,
  IN DWORD SizeOfStackCommit,
  IN DWORD SizeOfStackReserve,
  OUT LPVOID lpBytesBuffer
);
typedef NTSYSAPI NTSTATUS (NTAPI *LPFUN_LdrUnloadDll)
(
  IN HANDLE	ModuleHandle 
);
struct NtCreateThreadExBuffer
{
  ULONG Size;
  ULONG Unknown1;
  ULONG Unknown2;
  PULONG Unknown3;
  ULONG Unknown4;
  ULONG Unknown5;
  ULONG Unknown6;
  PULONG Unknown7;
  ULONG Unknown8;
};
#define ThreadQuerySetWin32StartAddress 9

class DotNetInjection
{
public:
	DotNetInjection();
	unsigned int Launch(const char * classtoInstance, TCHAR * szPath, unsigned int Unloader);
	unsigned int Unload();
	unsigned int Reload(const char * classtoInstance, TCHAR * szPath, unsigned int Unloader);
	NTSTATUS LdrUnloadDll(HANDLE hModule);
	HANDLE NtCreateThreadEx(HANDLE process, LPTHREAD_START_ROUTINE Start, LPVOID lpParameter);
	DWORD WINAPI GetThreadStartAddress(HANDLE hThread);
	void KillCLR(DWORD targetProcessId, DWORD targetThreadId);
};