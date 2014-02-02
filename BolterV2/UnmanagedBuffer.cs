/*
    UnmanagedBuffer.cs
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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace BolterV2
{
    /*
     * Essentially this class is designed to provide a flexible and 
     * easily maintained buffer of unmanaged memory.
     * It will resize as needed for various writing operations, and
     * when explicitly requested by the user. The class itself consists
     * of a combination of Read/Write/Delete operations with Generics
     * support to make it as easy as possible to read/write between managed
     * and unmanaged memory.
     */
    [Serializable]
    public class UnmanagedBuffer : ErrorBase
    {
        public IntPtr Pointer { get; private set; }
        public int Size { get; private set; }

        public UnmanagedBuffer(int cbneeded)
        {
            if (cbneeded > 0)
            {
                Pointer = Marshal.AllocHGlobal(cbneeded);
                Size = cbneeded;
            }
            else
            {
                Pointer = IntPtr.Zero;
                Size = 0;
            }
        }

        public bool Commit(byte[] data, int index, int count)
        {
            if (data != null && Alloc(count))
            {
                Marshal.Copy(data, index, Pointer, count);
                return true;
            }
            if (data == null)
                SetLastError(new ArgumentException("Attempting to commit a null reference", "data"));
            return false;
        }

        public bool Commit<T>(T data) where T : struct
        {
            try
            {
                if (Alloc(Marshal.SizeOf(typeof(T))))
                {
                    // You may note the fact that fDeleteOld is set to false here, despite the fact that Microsoft recommends 
                    // this to be set to true, due to the generic nature of this class, absently setting it to true every time 
                    // could break more stuff than it will help (as DestroyStructure is called with the current type of <T>, if 
                    // the last commit wasn't the same struct type, there's no point calling DestroyStructure. If you're allocating 
                    // referential structs, call SafeDecommit<T> to deallocate them safely.
                    Marshal.StructureToPtr(data, Pointer, false);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return SetLastError(e);
            }
        }

        public bool SafeDecommit<T>() where T : struct
        {
            try
            {
                if (Size >= Marshal.SizeOf(typeof(T)))
                {
                    Marshal.DestroyStructure(Pointer, typeof(T));
                    return true;
                }
                throw new InvalidCastException("Not enough unmanaged memory is allocated to contain this structure type.");
            }
            catch (Exception e)
            {
                return SetLastError(e);
            }
        }

        public bool Read<TResult>(out TResult data) where TResult : struct
        {
            data = default(TResult);
            try
            {
                if (Size >= Marshal.SizeOf(typeof(TResult)))
                {
                    data = (TResult)Marshal.PtrToStructure(Pointer, typeof(TResult));
                    return true;
                }
                throw new InvalidCastException("Not enough unmanaged memory is allocated to contain this structure type.");
            }
            catch (Exception e)
            {
                return SetLastError(e);
            }
        }

        public byte[] Read(int count)
        {
            try
            {
                if (count <= Size && count > 0)
                {
                    var buffer = new byte[count];
                    Marshal.Copy(Pointer, buffer, 0, count);
                    return buffer;
                }
                throw new ArgumentException("There is either not enough memory allocated to read 'count' bytes, or 'count' is negative (" + count.ToString() + ")", "count");
            }
            catch (Exception e)
            {
                SetLastError(e);
                return null;
            }
        }

        public bool Translate<TSource>(TSource data, out byte[] buffer) 
            where TSource : struct
        {
            buffer = null;
            if (Commit(data))
            {
                buffer = Read(Marshal.SizeOf(typeof(TSource)));
                SafeDecommit<TSource>();
            }
            return buffer != null;
        }

        public bool Translate<TResult>(byte[] buffer, out TResult result) 
            where TResult : struct
        {
            result = default(TResult);
            if (buffer == null)
            {
                return SetLastError(new ArgumentException("Attempted to translate a null reference to a structure.", "buffer"));
            }

            return (Commit(buffer, 0, buffer.Length) && Read(out result));
        }

        public bool Translate<TSource, TResult>(TSource data, out TResult result)
            where TSource : struct
            where TResult : struct
        {
            result = default(TResult);
            return (Commit(data) && Read(out result) && SafeDecommit<TSource>());
        }

        public bool Resize(int size)
        {
            if (size < 0)
            {
                return SetLastError( new ArgumentException("Attempting to resize to less than zero bytes of memory", "size") );
            }
            if (size == Size)
            {
                return true; //already have this much memory allocated, no point wasting resources.
            }
            if (size > Size)
            {
                //increasing memory, already have internal method to handle this.
                return Alloc(size);
            }
            try
            {
                if (size == 0) //Resizing to zero, essentially a free.
                {
                    Marshal.FreeHGlobal(Pointer);
                    Pointer = IntPtr.Zero;
                }
                else if (size > 0) //just downsizing the memory. May come after a particularly large "commit" call.
                {
                    Pointer = Marshal.ReAllocHGlobal(Pointer, new IntPtr(size));
                }
                Size = size; //reflect size change.
                return true;
            }
            catch (Exception e)
            {
                return SetLastError(e);
            }
        }

        private bool Alloc(int cb)
        {
            try
            {
                if (cb > Size)
                {
                    Pointer = Pointer == IntPtr.Zero ? Marshal.AllocHGlobal(cb) : Marshal.ReAllocHGlobal(Pointer, new IntPtr(cb));
                    Size = cb;
                }
                return true;
            }
            catch (Exception e)
            {
                return SetLastError(e);
            }
        }

        #region IDisposable Implementation
        private bool _disposed;

        private new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Resize(0);
                }
                _disposed = true;
            }
        }
        #endregion
    }

    public static class Utils
    {
        /// <summary>
        /// Attempts to write some data to a temp file on disk.
        /// </summary>
        /// <param name="data">Data to write to disk</param>
        /// <returns>The path to the temporary disk location if successful, null otherwise.</returns>
        /// <exception cref="ArgumentNullException(string)">The 'data' parameter is null</exception>
        /// <remarks>
        /// First, the function attempts to obtain a temp file name
        /// from the system, but if that fails a randomly-named file
        /// will be created in the same folder as the application
        /// </remarks>
        public static string WriteTempData(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            string path = null;
            try
            {
                path = Path.GetTempFileName();
            }
            catch (IOException)
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), Path.GetRandomFileName());
            }

            try { File.WriteAllBytes(path, data); }
            catch { path = null; }

            return path;
        }

        /// <summary>
        /// Deep clone a managed object to create an exact replica.
        /// </summary>
        /// <typeparam name="T">A formattable type (must be compatible with a BinaryFormatter)</typeparam>
        /// <param name="obj">The object to clone</param>
        /// <returns>A clone of the Object 'obj'.</returns>
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        public static uint SizeOf(this Type t)
        {
            //I'm super lazy, and I hate looking at ugly casts everywhere.
            return (uint)Marshal.SizeOf(t);
        }
    }
}
