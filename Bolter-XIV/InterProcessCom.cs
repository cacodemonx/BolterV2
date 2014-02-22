using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
#pragma warning disable 0618

namespace Bolter_XIV
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PassInfo
    {
        public IntPtr pathptr;
        public IntPtr sigs;
        public IntPtr strptr;
        public IntPtr Funcs;
    }

    public class InterProcessCom
    {
        private const int FuncCount = 15;
        /// <summary>
        /// Path to the configuration document
        /// </summary>
        [MarshalAs(UnmanagedType.AnsiBStr, SizeConst = 200)]
        public static string ConfigPath;

        /// <summary>
        /// Public reference to the class that manages all native structure manipulation.
        /// </summary>
        public static NativeMethods Game;

        public static LinkAPI LinkApi;

        /// <summary>
        /// Function that starts Bolter. Takes various information 
        /// that the unmanaged side needs to pass to the managed side.
        /// </summary>
        public int PassInfo(int ppInfo)
        {

            var pInfo = (PassInfo)Marshal.PtrToStructure(new IntPtr(ppInfo), typeof (PassInfo));

            // Grab our configuration path
            var configstr = Marshal.PtrToStringAnsi(pInfo.pathptr);

            ConfigPath = configstr.Contains("Bolter-XIV.dll")
                ? configstr.Replace("Bolter-XIV.dll", "config.xml")
                : configstr + "\\config.xml";

            var sigs = new int[10];

            Marshal.Copy(pInfo.sigs, sigs, 0, 10);

            Game = new NativeMethods(Process.GetCurrentProcess().MainModule.BaseAddress, sigs[8], sigs[0], sigs[7], sigs[9],
            sigs[3], sigs[2], sigs[1], sigs[4], sigs[6]);

            var funcs = new IntPtr[FuncCount];

            Marshal.Copy(pInfo.Funcs, funcs, 0, FuncCount);
            LinkApi = new LinkAPI(funcs);

            WinAPI.VirtualFreeEx(Process.GetCurrentProcess().Handle, pInfo.strptr, 0, FreeType.Release);

            new Thread(new ThreadStart(delegate
            {
                var w = new MainWindow();
                w.ShowDialog();
                UnmanagedDelegates.Funcs.UnloadAppDomain(AppDomain.CurrentDomain.FriendlyName);

            })) { ApartmentState = ApartmentState.STA }.Start();

            return 1;
        }
    }
}