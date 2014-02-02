/*
    InjectionMethod.cs
    Copyright (C) 2012 Jason Larke

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
 
    In addition to the above disclaimers, I am also not responsible for how
    you decide to use software resulting from this library.

    For a full specification of the GNU GPL license, see <http://www.gnu.org/copyleft/gpl.html>
 
    This license notice should be left in tact in all future works
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace BolterV2
{
    /*
      Core class that every injection method must conform to. Also acts
      as a factory class to avoid exposing each different injection method
      publically.
     */
    /// <summary>
    /// An DLL-injection method capable of executing user code in a remote process.
    /// </summary>
    public abstract class InjectionMethod : ErrorBase
    {
        // Maintain knowledge about the current injection method.
        public InjectionMethodType Type { get; protected set; }

        /// <summary>
        /// Inject a file into a process using a valid existing process handle
        /// </summary>
        /// <param name="dllPath">Path to the file to inject</param>
        /// <param name="hProcess">Handle to the remote process</param>
        /// <returns>A valid module handle if the function is successful, or IntPtr.Zero otherwise</returns>
        public abstract IntPtr Inject(string dllPath, IntPtr hProcess);

        /// <summary>
        /// Inject a collection of files into a process using a valid existing process handle
        /// </summary>
        /// <param name="dllPaths">An array listing which files to inject</param>
        /// <param name="hProcess">Handle to the remote process</param>
        /// <returns>An array of the same length as the 'dllPaths' parameter containing the module handles for each file</returns>
        public abstract IntPtr[] InjectAll(string[] dllPaths, IntPtr hProcess);


        /// <summary>
        /// Inject an existing in-memory PortableExecutable image into a process using a valid existing process handle
        /// </summary>
        /// <param name="image">Any valid existing PortableExecutable instance</param>
        /// <param name="hProcess">Handle to the remote process</param>
        /// <returns>A valid module handle if the function is successful, or IntPtr.Zero otherwise</returns>
        public abstract IntPtr Inject(PortableExecutable image, IntPtr hProcess);

        /// <summary>
        /// Inject an existing in-memory PortableExecutable image into a process using a unique process id as an identifier
        /// </summary>
        /// <param name="image">Any valid existing PortableExecutable instance</param>
        /// <param name="processId">Unique process identifier</param>
        /// <returns>A valid module handle if the function is successful, or IntPtr.Zero otherwise</returns>
        public virtual IntPtr Inject(PortableExecutable image, int processId)
        {
            ClearErrors();
            var hProcess = //WinAPI.OpenProcess(0x043A, false, processId);
            Process.GetProcessById(processId).Handle;
            var hModule = Inject(image, hProcess);
            return hModule;
        }

      
        /// <summary>
        /// Create a new InjectionMethod based on the specified MethodType
        /// </summary>
        /// <param name="type">Type of injection method to be created</param>
        /// <returns>A valid InjectionMethod instance if the 'type' parameter was valid, null otherwise</returns>
        public static InjectionMethod Create(InjectionMethodType type)
        {
            InjectionMethod method;
            switch (type)
            {
                case InjectionMethodType.ManualMap:
                    method = new ManualMap(); break;
                default:
                    return null;
            }
            if (method != null)
                method.Type = type;
            return method;
        }

        private bool disposed;

        /// <summary>
        /// Destructor
        /// </summary>
        ~InjectionMethod()
        {
            Dispose(false);
        }

        /// <summary>
        /// The dispose method that implements IDisposable.
        /// </summary>
        public new virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The virtual dispose method that allows
        /// classes inherited from this one to dispose their resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here.
                }

                // Dispose unmanaged resources here.
            }

            disposed = true;
        }
    }

    public enum InjectionMethodType
    {
        Standard,
        ManualMap
    };

    internal class ManualMap : InjectionMethod
    {
        #region Path Injection Implementations
        public override IntPtr Inject(string dllPath, IntPtr hProcess)
        {
            ClearErrors();
            try
            {
                using (var img = new PortableExecutable(dllPath))
                    return Inject(img, hProcess);
            }
            catch (Exception e)
            {
                SetLastError(e);
                return IntPtr.Zero;
            }
        }

        public override IntPtr[] InjectAll(string[] dllPaths, IntPtr hProcess)
        {
            ClearErrors();
            return Array.ConvertAll(dllPaths, dp => Inject(dp, hProcess));
        }
        #endregion

        public override IntPtr Inject(PortableExecutable image, IntPtr hProcess)
        {
            ClearErrors();
            try
            {
                return MapModule(Utils.DeepClone(image), hProcess, true);
            }
            catch (Exception e)
            {
                SetLastError(e);
                MessageBox.Show(e.Message);
                return IntPtr.Zero;
            }
        }

        #region Codebase
        // Most efficient version, this will be the work horse.
        private static IntPtr MapModule(PortableExecutable image, IntPtr hProcess, bool preserveHeaders = false)
        {
            if (hProcess.IsNull() || hProcess.Compare(-1))
                throw new ArgumentException("Invalid process handle.", "hProcess");

            if (image == null)
                throw new ArgumentException("Cannot map a non-existant PE Image.", "image");

            var processId = WinAPI.GetProcessId(hProcess);

            if (processId == 0)
                throw new ArgumentException("Provided handle doesn't have sufficient permissions to inject", "hProcess");

            var hModule = IntPtr.Zero;
            var pStub = IntPtr.Zero;
            uint nBytes = 0;

            try
            {
                //allocate memory for the image to load into the remote process.
                hModule = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, image.NTHeader.OptionalHeader.SizeOfImage, 0x1000 | 0x2000, 0x04);
                if (hModule.IsNull())
                    throw new InvalidOperationException("Unable to allocate memory in the remote process.");

                PatchRelocations(image, hModule);
                LoadDependencies(image, hProcess, processId);
                PatchImports(image, hProcess, processId);

                if (preserveHeaders)
                {
                    var szHeader = (image.DOSHeader.e_lfanew + Marshal.SizeOf(typeof(IMAGE_FILE_HEADER)) + sizeof(uint) + image.NTHeader.FileHeader.SizeOfOptionalHeader);
                    var header = new byte[szHeader];
                    if (image.Read(0, SeekOrigin.Begin, header))
                        WinAPI.WriteProcessMemory(hProcess, hModule, header, header.Length, out nBytes);
                }

                MapSections(image, hProcess, hModule);

                // some modules don't have an entry point and are purely libraries, mapping them and keeping the handle is just fine
                // an unlikely scenario with forced injection, but you never know.
                if (image.NTHeader.OptionalHeader.AddressOfEntryPoint > 0)
                {
                    var stub = (byte[])DLLMAIN_STUB.Clone();
                    BitConverter.GetBytes(hModule.ToInt32()).CopyTo(stub, 0x0B);

                    pStub = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)DLLMAIN_STUB.Length, 0x1000 | 0x2000, 0x40);
                    if (pStub.IsNull() || (!WinAPI.WriteProcessMemory(hProcess, pStub, stub, stub.Length, out nBytes) || nBytes != (uint)stub.Length))
                        throw new InvalidOperationException("Unable to write stub to the remote process.");

                    var hStubThread = new IntPtr();
                    WinAPI.NtCreateThreadEx(ref hStubThread, 0x1FFFFF, null, hProcess, pStub,
                        hModule.Add(image.NTHeader.OptionalHeader.AddressOfEntryPoint));
                    //= WinAPI.CreateRemoteThread(hProcess, 0, 0, pStub, (IntPtr)(hModule.Add(image.NTHeader.OptionalHeader.AddressOfEntryPoint).ToInt32()), 0, 0);
                    if (WinAPI.WaitForSingleObject(hStubThread, 5000) == 0x0L)
                    {
                        WinAPI.GetExitCodeThread(hStubThread, out nBytes);
                        if (nBytes == 0)
                        {
                            WinAPI.VirtualFreeEx(hProcess, hModule, 0, 0x8000);
                            throw new Exception("Entry method of module reported a failure " + Marshal.GetLastWin32Error().ToString());
                        }
                        WinAPI.VirtualFreeEx(hProcess, pStub, 0, 0x8000);
                        WinAPI.CloseHandle(hStubThread);
                    }
                }
            }
            catch (Exception e)
            {
                if (!hModule.IsNull())
                    WinAPI.VirtualFreeEx(hProcess, hModule, 0, 0x8000);
                if (!pStub.IsNull())
                    WinAPI.VirtualFreeEx(hProcess, hModule, 0, 0x8000);

                hModule = IntPtr.Zero;
                throw e;
            }
            return hModule;
        }


        private static void MapSections(PortableExecutable image, IntPtr hProcess, IntPtr pModule)
        {
            //very straightforward really. Just iterate through all the sections and map them to their desired virtual addresses in the remote process.
            //I'm not 100% sure about how well masking the section header characteristics and passing them off as memory protection constants goes. But
            //so far I haven't hit any issues. (i.e a section header with characteristics "IMAGE_SCN_TYPE_NO_PAD" will set "PAGE_WRITECOPY" memory protection.
            byte[] databuffer;
            uint n;

            foreach (var pSecHd in image.EnumSectionHeaders())
            {
                databuffer = new byte[pSecHd.SizeOfRawData];
                if (image.Read(pSecHd.PointerToRawData, SeekOrigin.Begin, databuffer))
                {
                    if ((pSecHd.Characteristics & 0x02000000) == 0) //can actually ignore this section (usually the reloc section)
                    {
                        WinAPI.WriteProcessMemory(hProcess, pModule.Add(pSecHd.VirtualAddress), databuffer, databuffer.Length, out n);
                        WinAPI.VirtualProtectEx(hProcess, pModule.Add(pSecHd.VirtualAddress), pSecHd.SizeOfRawData, pSecHd.Characteristics & 0x00FFFFFF, out n);
                    }
                }
                else
                {
                    throw image.GetLastError();
                }
            }
        }

        private static void PatchRelocations(PortableExecutable image, IntPtr pAlloc)
        {
            // Base relocations are essentially Microsofts ingenious way of preserving portability in images.
            // for all absolute address calls/jmps/references...etc, an entry is made into the base relocation
            // table telling the loader exactly where an "absolute" address is being used. This allows the loader
            // to iterate through the relocations and patch these absolute values to ensure they are correct when
            // the image is loaded somewhere that isn't its preferred base address.
            var relocDir = image.NTHeader.OptionalHeader.DataDirectory[(int)DATA_DIRECTORIES.BaseRelocTable];
            if (relocDir.Size > 0) //check if there are in fact any relocations.
            {
                uint n = 0;
                var delta = (uint)(pAlloc.ToInt32() - image.NTHeader.OptionalHeader.ImageBase); //The difference in loaded/preferred addresses.
                var pReloc = image.GetPtrFromRVA(relocDir.VirtualAddress);
                var szReloc = (uint)Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION));
                IMAGE_BASE_RELOCATION reloc;

                while (n < relocDir.Size && image.Read(pReloc, SeekOrigin.Begin, out reloc))
                {
                    // A relocation block consists of an IMAGE_BASE_RELOCATION, and an array of WORDs.
                    // To calculate the number of relocations (represented by WORDs), just do some simple math.
                    var nrelocs = (int)((reloc.SizeOfBlock - szReloc) / sizeof(ushort));
                    var pageVa = image.GetPtrFromRVA(reloc.VirtualAddress); //The Page RVA for this set of relocations (usually a 4K boundary).
                    ushort vreloc;
                    uint old;

                    for (var i = 0; i < nrelocs; i++)
                    {
                        // There are only 2 types of relocations on Intel machines: ABSOLUTE (padding, nothing needs to be done) and HIGHLOW (0x03)
                        // Highlow means that all 32 bits of the "delta" value need to be added to the relocation value.
                        if (image.Read(pReloc + szReloc + (i << 1), SeekOrigin.Begin, out vreloc) && (vreloc >> 12 & 3) != 0)
                        {
                            var vp = (uint)(pageVa + (vreloc & 0x0FFF));
                            if (image.Read(vp, SeekOrigin.Begin, out old))
                                image.Write(-4, SeekOrigin.Current, (uint)(old + delta));
                            else
                                throw image.GetLastError(); //unlikely, but I hate crashing targets because something in the PE was messed up.
                        }
                    }
                    n += reloc.SizeOfBlock;
                    pReloc += reloc.SizeOfBlock;
                }
            }
        }

        private static void PatchImports(PortableExecutable image, IntPtr hProcess, int processId)
        {
            var module = string.Empty;
            var fname = string.Empty;

            foreach (var desc in image.EnumImports())
            {
                if (image.ReadString(image.GetPtrFromRVA(desc.Name), SeekOrigin.Begin, out module))
                {
                    var pModule = IntPtr.Zero;
                    var tModule = IntPtr.Zero;
                    //Thanks to FastLoadDependencies, all dependent modules *should* be loaded into the remote process already.
                    pModule = GetRemoteModuleHandle(module, processId);

                    if (pModule.IsNull())
                        throw new FileNotFoundException(string.Format("Unable to load dependent module '{0}'.", module));

                    //now have a supposedly valid module handle remote process, all that remains to be done is patch the info.
                    var pThunk = image.GetPtrFromRVA(desc.FirstThunkPtr); //despite the fact FirstThunk and OriginalFirstThunk are identical within an unmapped PE, only FirstThunk is looked up after mapping.
                    var szThunk = (uint)Marshal.SizeOf(typeof(IMAGE_THUNK_DATA));
                    IMAGE_THUNK_DATA thunk;

                    while (image.Read(pThunk, SeekOrigin.Begin, out thunk) && thunk.u1.AddressOfData > 0) //final thunk is signified by a null-filled IMAGE_THUNK_DATA structure.
                    {
                        var remote = IntPtr.Zero;
                        object procVal = null;
                        if ((thunk.u1.Ordinal & 0x80000000) == 0) //import by name
                        {
                            if (image.ReadString(image.GetPtrFromRVA(thunk.u1.AddressOfData) + 2, SeekOrigin.Begin, out fname)) //get the function name.
                                procVal = fname;
                            else
                                throw image.GetLastError(); //error occurred during memory iteration, this is only a safeguard and shouldn't really ever occur, but the universe loves proving me wrong.
                        }
                        else //import by ordinal.
                        {
                            procVal = (ushort)(thunk.u1.Ordinal & 0xFFFF);
                        }

                        // the following section of code simply aims to reduce overhead. A check is first performed to see if the current module
                        // is loaded in the current process first, if so it simply uses relative addresses to calculate the function address in the remote
                        // process. If the module isn't found in our process, a call to GetProcAddressEx is used to find the remote address. Of course, you
                        // could simply guarantee the first case by calling LoadLibrary internally, but I find that can have unwanted side effects.
                        if (!(remote = WinAPI.GetModuleHandleA(module)).IsNull())
                        {
                            var local = procVal is string
                                            ? WinAPI.GetProcAddress(remote, (string)procVal)
                                            : WinAPI.GetProcAddress(remote, (uint)((ushort)procVal) & 0x0000FFFF);
                            if (!local.IsNull())
                                remote = pModule.Add(local.Subtract(remote.ToInt32()).ToInt32());
                        }
                        else
                        {
                            remote = WinAPI.GetProcAddressEx(hProcess, pModule, procVal);
                        }

                        if (remote.IsNull()) //alas, couldn't find the function.
                            throw new EntryPointNotFoundException(string.Format("Unable to locate imported function '{0}' from module '{1}' in the remote process.", fname, module));

                        image.Write(pThunk, SeekOrigin.Begin, remote.ToInt32()); //overwrite the thunk and continue on our merry way.
                        pThunk += szThunk;
                    }
                }
            }
        }

        /*
         * Handles loading of all dependent modules. Iterates the IAT entries and attempts to load (using LoadLibrary) all 
         * of the necessary modules for the main module to function. The manifest is extracted and activation contexts used to
         * ensure correct loading of Side-By-Side dependencies.
         */
        private static bool LoadDependencies(PortableExecutable image, IntPtr hProcess, int processId)
        {
            var neededDependencies = new List<string>();
            var curdep = string.Empty;
            var success = false;

            foreach (var desc in image.EnumImports())
            {
                if (image.ReadString(image.GetPtrFromRVA(desc.Name), SeekOrigin.Begin, out curdep) && !string.IsNullOrEmpty(curdep))
                {
                    if (GetRemoteModuleHandle(curdep, processId).IsNull())
                        neededDependencies.Add(curdep);
                }
            }

            if (neededDependencies.Count > 0) //do we actually need to load any new modules?
            {
                var bManifest = ExtractManifest(image);
                var pathManifest = string.Empty;

                if (bManifest == null) // no internal manifest, may be an external manifest or none at all?
                {
                    if (!string.IsNullOrEmpty(image.FileLocation) && File.Exists(Path.Combine(Path.GetDirectoryName(image.FileLocation), Path.GetFileName(image.FileLocation) + ".manifest")))
                    {
                        pathManifest = Path.Combine(Path.GetDirectoryName(image.FileLocation), Path.GetFileName(image.FileLocation) + ".manifest");
                    }
                    else // no internal or external manifest, presume no side-by-side dependencies.
                    {
                        var standard = Create(InjectionMethodType.Standard);
                        var results = standard.InjectAll(neededDependencies.ToArray(), hProcess);

                        foreach (var result in results)
                            if (result.IsNull())
                                return false; // failed to inject a dependecy, abort mission.

                        return true; // done loading dependencies.
                    }
                }
                else
                {
                    pathManifest = Utils.WriteTempData(bManifest);
                }

                if (string.IsNullOrEmpty(pathManifest))
                    return false;

                var pResolverStub = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)RESOLVER_STUB.Length, 0x1000 | 0x2000, 0x40);
                var pManifest = WinAPI.CreateRemotePointer(hProcess, Encoding.ASCII.GetBytes(pathManifest + "\0"), 0x04);
                var pModules = WinAPI.CreateRemotePointer(hProcess, Encoding.ASCII.GetBytes(string.Join("\0", neededDependencies.ToArray()) + "\0"), 0x04);

                if (!pResolverStub.IsNull())
                {
                    var resolverStub = (byte[])RESOLVER_STUB.Clone();
                    uint nBytes = 0;

                    // Call patching. Patch the empty function addresses with the runtime addresses.
                    BitConverter.GetBytes(FN_CREATEACTCTXA.Subtract(pResolverStub.Add(0x3F)).ToInt32()).CopyTo(resolverStub, 0x3B);
                    BitConverter.GetBytes(FN_ACTIVATEACTCTX.Subtract(pResolverStub.Add(0x58)).ToInt32()).CopyTo(resolverStub, 0x54);
                    BitConverter.GetBytes(FN_GETMODULEHANDLEA.Subtract(pResolverStub.Add(0x84)).ToInt32()).CopyTo(resolverStub, 0x80);
                    BitConverter.GetBytes(FN_LOADLIBRARYA.Subtract(pResolverStub.Add(0x92)).ToInt32()).CopyTo(resolverStub, 0x8E);
                    BitConverter.GetBytes(FN_DEACTIVATEACTCTX.Subtract(pResolverStub.Add(0xC8)).ToInt32()).CopyTo(resolverStub, 0xC4);
                    BitConverter.GetBytes(FN_RELEASEACTCTX.Subtract(pResolverStub.Add(0xD1)).ToInt32()).CopyTo(resolverStub, 0xCD);

                    // Parameter patching
                    BitConverter.GetBytes(pManifest.ToInt32()).CopyTo(resolverStub, 0x1F);
                    BitConverter.GetBytes(neededDependencies.Count).CopyTo(resolverStub, 0x28);
                    BitConverter.GetBytes(pModules.ToInt32()).CopyTo(resolverStub, 0x31);

                    if (WinAPI.WriteProcessMemory(hProcess, pResolverStub, resolverStub, resolverStub.Length, out nBytes) && nBytes == (uint)resolverStub.Length)
                    {
                        var result = WinAPI.RunThread(hProcess, pResolverStub, 0, 5000);
                        success = (result != uint.MaxValue && result != 0);
                    }

                    // Cleanup
                    WinAPI.VirtualFreeEx(hProcess, pModules, 0, 0x8000);
                    WinAPI.VirtualFreeEx(hProcess, pManifest, 0, 0x8000);
                    WinAPI.VirtualFreeEx(hProcess, pResolverStub, 0, 0x8000);
                }
            }

            return success;
        }

        /**
         * Extract an application manifest from
         * an executable file's resources.
         * Returns null if no manifest was found in the executable.
         */
        private static byte[] ExtractManifest(PortableExecutable image)
        {
            byte[] manifest = null;
            var walker = new ResourceWalker(image);
            ResourceWalker.ResourceDirectory manifestDir = null;
            for (var i = 0; i < walker.Root.Directories.Length && manifestDir == null; i++)
                if (walker.Root.Directories[i].Id == Constants.RT_MANIFEST)
                    manifestDir = walker.Root.Directories[i];

            if (manifestDir != null && manifestDir.Directories.Length > 0)
                if (IsManifestResource(manifestDir.Directories[0].Id) && manifestDir.Directories[0].Files.Length == 1)
                    manifest = manifestDir.Directories[0].Files[0].GetData();

            return manifest;
        }

        private static bool IsManifestResource(int id)
        {
            switch ((uint)id)
            {
                case Constants.CREATEPROCESS_MANIFEST_RESOURCE_ID:
                case Constants.ISOLATIONAWARE_MANIFEST_RESOURCE_ID:
                case Constants.ISOLATIONAWARE_NOSTATICIMPORT_MANIFEST_RESOURCE_ID:
                    return true;
                default:
                    return false;
            }
        }

        private static IntPtr GetRemoteModuleHandle(string module, int processId)
        {
            var hModule = IntPtr.Zero;
            var target = Process.GetProcessById(processId);
            for (var i = 0; i < target.Modules.Count && hModule.IsNull(); i++)
                if (target.Modules[i].ModuleName.ToLower() == module.ToLower())
                    hModule = target.Modules[i].BaseAddress;
            return hModule;
        }
        #endregion

        #region Gigantic Assembly Bytecode, do not open for fear of losing eyesight.
        /* patch values */
        private static readonly IntPtr H_KERNEL32 = WinAPI.GetModuleHandleA("KERNEL32.dll");
        private static readonly IntPtr FN_CREATEACTCTXA = WinAPI.GetProcAddress(H_KERNEL32, "CreateActCtxA");
        private static readonly IntPtr FN_ACTIVATEACTCTX = WinAPI.GetProcAddress(H_KERNEL32, "ActivateActCtx");
        private static readonly IntPtr FN_LOADLIBRARYA = WinAPI.GetProcAddress(H_KERNEL32, "LoadLibraryA");
        private static readonly IntPtr FN_GETMODULEHANDLEA = WinAPI.GetProcAddress(H_KERNEL32, "GetModuleHandleA");
        private static readonly IntPtr FN_DEACTIVATEACTCTX = WinAPI.GetProcAddress(H_KERNEL32, "DeactivateActCtx");
        private static readonly IntPtr FN_RELEASEACTCTX = WinAPI.GetProcAddress(H_KERNEL32, "ReleaseActCtx");

        // DllMain function call stub.
        private static readonly byte[] DLLMAIN_STUB = 
        {
            0x68, 0x00, 0x00, 0x00, 0x00, //push lpReserved
            0x68, 0x01, 0x00, 0x00, 0x00, //push dwReason
            0x68, 0x00, 0x00, 0x00, 0x00, //push hModule
            0xFF, 0x54, 0x24, 0x10, //call [esp + 10h]
            0xC3//2, 0x0C, 0x00 
        };

        // Resolve a list of dlls.
        private static readonly byte[] RESOLVER_STUB = 
        {
            0x55,                              //push ebp
            0x8B, 0xEC,                        //mov ebp, esp
            0x83, 0xEC, 0x3C,                  //sub esp, 3Ch
            0x8B, 0xCC,                        //mov ecx, esp
            0x8B, 0xD1,                        //mov edx, ecx
            0x83, 0xC2, 0x3C,                  //add edx, 3Ch
            0xC7, 0x01, 0x00, 0x00, 0x00, 0x00,//mov [ecx], 0 <--- memset
            0x83, 0xC1, 0x04,                  //add ecx, 4h            ^
            0x3B, 0xCA,                        //cmp ecx, edx           |
            0x7E, 0xF3,                        //jle memset   -----------
            0xC6, 0x04, 0x24, 0x20,            //mov [esp], 20h
            0xB9, 0x00, 0x00, 0x00, 0x00,      //mov ecx, 0x00000000 //manifest char*
            0x89, 0x4C, 0x24, 0x08,            //mov [esp + 8], ecx
            0xB9, 0x00, 0x00, 0x00, 0x00,      //mov ecx, 0x00000000 //nFiles
            0x89, 0x4C, 0x24, 0x28,            //mov [esp + 28h], ecx
            0xB9, 0x00, 0x00, 0x00, 0x00,      //mov ecx, 0x00000000 //files char*
            0x89, 0x4C, 0x24, 0x2C,            //mov [esp + 2Ch], ecx
            0x54,                              //push esp
            0xE8, 0x00, 0x00, 0x00, 0x00,      //call CreateActCtxA
            0x83, 0x38, 0xFF,                  //cmp eax, -1
            0x0F, 0x84, 0x89, 0x00, 0x00, 0x00,//je finish -----------------------------------v
            0x89, 0x44, 0x24, 0x30,            //mov [esp + 30h], eax                         |
            0x8B, 0xCC,                        //mov ecx, esp                                 |
            0x83, 0xC1, 0x20,                  //add ecx, 20h                                 |
            0x51,                              //push ecx                                     |
            0x50,                              //push eax                                     |
            0xE8, 0x00, 0x00, 0x00, 0x00,      //call ActivateActCtxA                         |
            0x83, 0xF8, 0x00,                  //cmp eax, 0                                   |
            0x74, 0x6B,                        //je cleanupfinish -------------------------v  |
            0xC6, 0x44, 0x24, 0x24, 0x01,      //mov [esp + 24h], 1                        |  |
            0x8B, 0x4C, 0x24, 0x28,            //mov ecx, [esp + 28h] <--- loadloop <+     |  |
            0x83, 0xF9, 0x00,                  //cmp ecx, 0                          |     |  |
            0x7E, 0x3E,                        //jle endloop ------------------------|--v  |  |
            0x83, 0xE9, 0x01,                  //sub ecx, 1                          |  |  |  |
            0x89, 0x4C, 0x24, 0x28,            //mov [esp + 28h], ecx                |  |  |  |
            0x8B, 0x4C, 0x24, 0x24,            //mov ecx, [esp + 24h]                |  |  |  |
            0x83, 0xF9, 0x00,                  //cmp ecx, 0                          |  |  |  |
            0x74, 0x2E,                        //je endloop -------------------------|--v  |  |
            0xFF, 0x74, 0x24, 0x2C,            //push [esp + 2Ch]                    |  |  |  |
            0xE8, 0x00, 0x00, 0x00, 0x00,      //call GetModuleHandleA               |  |  |  |
            0x83, 0xF8, 0x00,                  //cmp eax, 0                          |  |  |  |
            0x75, 0x09,                        //jnz initnext -------------------+   |  |  |  |
            0xFF, 0x74, 0x24, 0x2C,            //push [esp + 2Ch]                |   |  |  |  |
            0xE8, 0x00, 0x00, 0x00, 0x00,      //call LoadLibraryA               v   |  |  |  |
            0x89, 0x44, 0x24, 0x24,            //mov [esp + 24h], eax <--- initnext  |  |  |  |
            0x8B, 0x4C, 0x24, 0x2C,            //mov ecx, [esp + 2Ch]                |  |  |  |
            0x8A, 0x01,                        //mov al, [ecx] <--- findnull         |  |  |  |
            0x83, 0xC1, 0x01,                  //add ecx, 1                 ^        |  |  |  |
            0x3C, 0x00,                        //cmp al, 0                  |        |  |  |  |
            0x75, 0xF7,                        //jnz findnull --------------+        |  |  |  |
            0x89, 0x4C, 0x24, 0x2C,            //mov [esp + 2Ch], ecx                |  |  |  |
            0xEB, 0xB9,                        //jmp loadloop -----------------------+  |  |  |
            0x8B, 0x44, 0x24, 0x24,            //mov eax, [esp + 24h]   <---endloop-----+  |  |
            0xB9, 0x01, 0x00, 0x00, 0x00,      //mov ecx, 1                                |  |
            0x23, 0xC1,                        //and eax, ecx                              |  |
            0x89, 0x4C, 0x24, 0x24,            //mov [esp + 24h], ecx                      |  |
            0x83, 0xF9, 0x00,                  //cmp ecx, 0                                |  |
            0x75, 0x14,                        //jne finish -------------------------------|--v
            0xFF, 0x74, 0x24, 0x20,            //push [esp + 20h]                          |  |
            0x6A, 0x00,                        //push 0                                    |  |
            0xE8, 0x00, 0x00, 0x00, 0x00,      //call DeactivateActCtx                     |  |
            0xFF, 0x74, 0x24, 0x30,            //push [esp + 30h] <--- cleanupfinish ------+  |
            0xE8, 0x00, 0x00, 0x00, 0x00,      //call ReleaseActCtx                           |
            0x8B, 0x44, 0x24, 0x24,            //mov eax, [esp + 24h] <--- finish <-----------+
            0x8B, 0xE5,                        //mov esp, ebp
            0x5D,                              //pop ebp
            0xC3				               //ret
        };
        #endregion
        private bool disposed;

        /// <summary>
        /// Destructor
        /// </summary>
        ~ManualMap()
        {
            Dispose(false);
        }

        /// <summary>
        /// The dispose method that implements IDisposable.
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The virtual dispose method that allows
        /// classes inherithed from this one to dispose their resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here.
                }

                // Dispose unmanaged resources here.
            }

            disposed = true;
        }
    }
    /**
     * This class is purely designed to reduce the amount of duplicate code needed for InjectionMethods
     * that use LoadLibrary at some level. Because LoadLibrary needs to be pointed to a file, the overloads
     * with PEImage parameters pretty much just write the data to a file, then call the string counterparts.
     * I also wrote a utility asm stub to avoid spamming CreateRemoteThread (MULTILOAD_STUB).
     */
    internal abstract class StandardInjectionMethod : InjectionMethod
    {
        public override IntPtr Inject(PortableExecutable dll, IntPtr hProcess)
        {
            //same as above, write the temp file and defer to the other Inject methods.
            ClearErrors();
            var path = Utils.WriteTempData(dll.ToArray());
            var hModule = IntPtr.Zero;
            if (!string.IsNullOrEmpty(path))
            {
                hModule = Inject(path, hProcess);
                try
                {
                    File.Delete(path);
                }
                catch { /* nom nom nom */ }

            }
            return hModule;
        }


        protected virtual IntPtr CreateMultiLoadStub(string[] paths, IntPtr hProcess, out IntPtr pModuleBuffer, uint nullmodule = 0)
        {
            // This function creates a multi-loading stub which essentially iterates a list of required modules and writes the results
            // of the LoadLibraryA / GetModuleHandleA call to a preallocated buffer (out IntPtr pModuleBuffer) in the remote process.
            pModuleBuffer = IntPtr.Zero;
            var pStub = IntPtr.Zero;

            try
            {
                // get function addresses.
                var hKernel32 = WinAPI.GetModuleHandleA("kernel32.dll");
                var fnLoadLibraryA = WinAPI.GetProcAddress(hKernel32, "LoadLibraryA");
                var fnGetModuleHandleA = WinAPI.GetProcAddress(hKernel32, "GetModuleHandleA");

                // sanity check
                if (fnLoadLibraryA.IsNull() || fnGetModuleHandleA.IsNull())
                    throw new Exception("Unable to find necessary function entry points in the remote process");

                // Create the necessary remote buffers and values for the assembler call
                pModuleBuffer = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)paths.Length << 2, 0x1000 | 0x2000, 0x04);
                var pLibs = WinAPI.CreateRemotePointer(hProcess, Encoding.ASCII.GetBytes(string.Join("\0", paths) + "\0"), 0x04);
                if (pModuleBuffer.IsNull() || pLibs.IsNull())
                    throw new InvalidOperationException("Unable to allocate memory in the remote process");

                try
                {
                    uint nbytes = 0;
                    var nullset = new byte[paths.Length << 2];
                    for (var i = 0; i < nullset.Length >> 2; i++)
                        BitConverter.GetBytes(nullmodule).CopyTo(nullset, i << 2);
                    WinAPI.WriteProcessMemory(hProcess, pModuleBuffer, nullset, nullset.Length, out nbytes);

                    var stub = (byte[])MULTILOAD_STUB.Clone();
                    pStub = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)stub.Length, 0x1000 | 0x2000, 0x40);

                    if (pStub.IsNull())
                        throw new InvalidOperationException("Unable to allocate memory in the remote process");

                    BitConverter.GetBytes(pLibs.ToInt32()).CopyTo(stub, 0x07);
                    BitConverter.GetBytes(paths.Length).CopyTo(stub, 0x0F);
                    BitConverter.GetBytes(pModuleBuffer.ToInt32()).CopyTo(stub, 0x18);

                    BitConverter.GetBytes(fnGetModuleHandleA.Subtract(pStub.Add(0x38)).ToInt32()).CopyTo(stub, 0x34);
                    BitConverter.GetBytes(fnLoadLibraryA.Subtract(pStub.Add(0x45)).ToInt32()).CopyTo(stub, 0x41);

                    if (!WinAPI.WriteProcessMemory(hProcess, pStub, stub, stub.Length, out nbytes) || nbytes != (uint)stub.Length)
                        throw new Exception("Error creating the remote function stub.");

                    return pStub;
                }
                finally // Don't actually handle the exception, just clean up some of the allocated memory.
                {
                    WinAPI.VirtualFreeEx(hProcess, pModuleBuffer, 0, 0x8000);
                    WinAPI.VirtualFreeEx(hProcess, pLibs, 0, 0x8000);
                    if (!pStub.IsNull())
                        WinAPI.VirtualFreeEx(hProcess, pStub, 0, 0x8000);
                    pModuleBuffer = IntPtr.Zero;

                }
            }
            catch (Exception e)
            {
                SetLastError(e);
                return IntPtr.Zero;
            }
        }

        protected static readonly byte[] MULTILOAD_STUB =
        {
            0x55, 							//push ebp
            0x8B, 0xEC, 					//mov ebp, esp
            0x83, 0xEC, 0x0C, 				//sub esp, 0x0C,
            0xB9, 0x00, 0x00, 0x00, 0x00,	//mov ecx, pLibs
            0x89, 0x0C, 0x24, 				//mov [esp], ecx
            0xB9, 0x00, 0x00, 0x00, 0x00,	//mov ecx, nLibs
            0x89, 0x4C, 0x24, 0x04, 		//mov [esp + 4], ecx
            0xB9, 0x00, 0x00, 0x00, 0x00,	//mov ecx, hBuffer
            0x89, 0x4C, 0x24, 0x08, 		//mov [esp + 8], ecx
            0x8B, 0x4C, 0x24, 0x04, 		//mov ecx, [esp + 4] <-- mainloop --------+
            0x83, 0xF9, 0x00, 				//cmp ecx, 0							  |
            0x74, 0x3A, 					//jz finish								  |
            0x83, 0xE9, 0x01, 				//sub ecx, 1							  |
            0x89, 0x4C, 0x24, 0x04, 		//mov [esp + 4], ecx					  |
            0xFF, 0x34, 0x24, 				//push [esp]							  |
            0xE8, 0x00, 0x00, 0x00, 0x00,	//call GetModuleHandleA 				  |
            0x83, 0xF8, 0x00, 				//cmp eax, 0							  |
            0x75, 0x08, 					//jz sethandle --------------------+	  |
            0xFF, 0x34, 0x24, 				//push [esp]					   |	  |
            0xE8, 0x00, 0x00, 0x00, 0x00,	//call LoadLibraryA 			   |	  |
            0x8B, 0x4C, 0x24, 0x08, 		//mov ecx, [esp + 8h] <--sethandle-+	  |
            0x89, 0x01, 					//mov [ecx], eax 						  |
            0x83, 0xC1, 0x04, 				//add ecx, 4							  |
            0x89, 0x4C, 0x24, 0x08, 		//mov [esp + 8], ecx					  |
            0x8B, 0x0C, 0x24, 				//mov ecx, [esp]                          |
            0x8A, 0x01, 					//mov al, BYTE PTR [ecx] <-- findnull--+  |
            0x83, 0xC1, 0x01, 				//add ecx, 1                           |  |
            0x3C, 0x00, 					//cmp al 0							   |  |
            0x75, 0xF7, 					//jnz findnull ------------------------+  |
            0x89, 0x0C, 0x24, 				//mov [esp], ecx						  |
            0xEB, 0xBD, 					//jmp mainloop ---------------------------+
            0x8B, 0xE5, 					//mov esp, ebp <-- finish
            0x5D, 							//pop ebp
            0xC3				            //ret
        };

        protected static readonly byte[] MULTIUNLOAD_STUB =
        {
            0x55,
            0x8B, 0xEC,
            0x83, 0xEC, 0x0C,
            0xB9, 0x00, 0x00, 0x00, 0x00,
            0x89, 0x0C, 0x24,
            0xB9, 0x00, 0x00, 0x00, 0x00,
            0x89, 0x4C, 0x24, 0x04,
            0x8B, 0x0C, 0x24,
            0x8B, 0x09,
            0x83, 0xF9, 0x00,
            0x74, 0x3A,
            0x89, 0x4C, 0x24, 0x08,
            0x8B, 0x4C, 0x24, 0x04,
            0xC7, 0x01, 0x00, 0x00, 0x00, 0x00,
            0xFF, 0x74, 0x24, 0x08,
            0xE8, 0x00, 0x00, 0x00, 0x00,
            0x83, 0xF8, 0x00,
            0x74, 0x08,
            0x8B, 0x4C, 0x24, 0x04,
            0x89, 0x01,
            0xEB, 0xEA,
            0x8B, 0x0C, 0x24,
            0x83, 0xC1, 0x04,
            0x89, 0x0C, 0x24,
            0x8B, 0x4C, 0x24, 0x04,
            0x83, 0xC1, 0x04,
            0x89, 0x4C, 0x24, 0x04,
            0xEB, 0xBC,
            0x8B, 0xE5,
            0x5D,
            0xC3
        };
    }
}
