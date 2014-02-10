using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bolter_XIV
{
    public static class LinkAPI
    {

        public static T PtrToFunc<T>(IntPtr pointer)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(pointer, typeof(T));
        }

        public static T PtrToFunc<T>(int pointer)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer((IntPtr)pointer, typeof(T));
        }

        public static UnloadItFunc UnloadAppDomain;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void UnloadItFunc();
    }
}
