/*
    ResourceWalker.cs
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

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BolterV2
{
    /**
     * Helper class to walk a Portable Executable's resource table,
     * Very, very basic, probably not overly efficient either but it gets the job done.
     * This is probably the messiest and hardest-to-follow file in the solution
     * but the amount of documentation needed to properly explain the concept
     * is way out of proportion to the use of this class in the project so I haven't really
     * bothered. If you really want a good explanation about the Resource Table structure,
     * see Matt Pietrek's work, as well as the Microsoft PE/COFF specification (v8)
     */
    public class ResourceWalker
    {
        public ResourceDirectory Root { get; private set; }

        public ResourceWalker(BolterV2.PortableExecutable image)
        {
            var rsrcDir = image.NTHeader.OptionalHeader.DataDirectory[(int)DATA_DIRECTORIES.ResourceTable];
            IMAGE_RESOURCE_DIRECTORY rootDir;
            uint rootAddr = 0;

            if (rsrcDir.VirtualAddress > 0 && rsrcDir.Size > 0)
            {
                if (image.Read((rootAddr = image.GetPtrFromRVA(rsrcDir.VirtualAddress)), SeekOrigin.Begin, out rootDir))
                    Root = new ResourceDirectory(image, new IMAGE_RESOURCE_DIRECTORY_ENTRY() { SubdirectoryRva = 0x80000000 }, false, rootAddr);
                else
                    throw image.GetLastError();
            }
        }

        public abstract class ResourceObject
        {
            private string _name;

            protected uint _root;
            protected BolterV2.PortableExecutable _owner;
            protected IMAGE_RESOURCE_DIRECTORY_ENTRY _entry;

            public string Name { get { return _name; } }
            public int Id { get {  return (IsNamedResource ? -1 : (int)_entry.IntegerId); }}
            public bool IsNamedResource { get; protected set; }

            public ResourceObject(BolterV2.PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root)
            {
                _owner = owner;
                _entry = entry;
                IsNamedResource = named;
                if (named)
                {
                    ushort len = 0;
                    if (owner.Read(root + (entry.NameRva & 0x7FFFFFFF), SeekOrigin.Begin, out len))
                    {
                        var unicodeBuffer = new byte[len << 1]; //each unicode character is 2 bytes wide
                        if (owner.Read(0, SeekOrigin.Current, unicodeBuffer))
                            _name = Encoding.Unicode.GetString(unicodeBuffer);
                    }

                    if (_name == null)
                        throw owner.GetLastError();
                }
                _root = root;
            }
        }

        public class ResourceFile : ResourceObject
        {
            private IMAGE_RESOURCE_DATA_ENTRY _base;

            public ResourceFile(BolterV2.PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root)
                : base(owner, entry, named, root)
            {
                if (!owner.Read(_root + entry.DataEntryRva, SeekOrigin.Begin, out _base))
                    throw owner.GetLastError();
            }

            public byte[] GetData()
            {
                var buffer = new byte[_base.Size];
                if (!_owner.Read(_owner.GetPtrFromRVA(_base.OffsetToData), SeekOrigin.Begin, buffer))
                    throw _owner.GetLastError();
                return buffer;
            }
        }

        public class ResourceDirectory : ResourceObject
        {
            private const uint SZ_ENTRY = 8;
            private const uint SZ_DIRECTORY = 16;
            private IMAGE_RESOURCE_DIRECTORY _base;

            private ResourceFile[] _files;
            private ResourceDirectory[] _dirs;

            public ResourceFile[] Files 
            {
                get
                {
                    if (_files == null)
                        Initialize();
                    return _files;
                }
            }

            public ResourceDirectory[] Directories
            {
                get
                {
                    if (_dirs == null)
                        Initialize();
                    return _dirs;
                }
            }

            private void Initialize()
            {
                IMAGE_RESOURCE_DIRECTORY_ENTRY curEnt;
                var directories = new List<ResourceDirectory>();
                var files = new List<ResourceFile>();

                int namedCount = _base.NumberOfNamedEntries;

                for (var i = 0; i < namedCount + _base.NumberOfIdEntries; i++)
                {
                    if (_owner.Read(_root + SZ_DIRECTORY + (_entry.SubdirectoryRva ^ 0x80000000) + (i * SZ_ENTRY), SeekOrigin.Begin, out curEnt))
                    {
                        if ((curEnt.SubdirectoryRva & 0x80000000) != 0)
                            directories.Add(new ResourceDirectory(_owner, curEnt, i < namedCount, _root));
                        else
                            files.Add(new ResourceFile(_owner, curEnt, i < namedCount, _root));
                    }
                }
                _files = files.ToArray();
                _dirs = directories.ToArray();
            }

            public ResourceDirectory(BolterV2.PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root)
                : base(owner, entry, named, root)
            {
                if (!owner.Read(root + (entry.SubdirectoryRva ^ 0x80000000), SeekOrigin.Begin, out _base))
                    throw owner.GetLastError();
            }
        }
    }
}
