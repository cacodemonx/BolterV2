using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bolter_XIV
{
    public class UnmanagedDelegates
    {
        public struct Funcs
        {
            public static fUnloadIt UnloadAppDomain;

            public static fGetPOS GetPOS;

            public static fGetName GetName;

            public static fSetPOS SetPOS;

            public static fSetHeading SetHeading;

            public static fGetHeading GetHeading;

            public static fSet3DVector Set3DVector;

            public static fGet3DVector Get3DVector;

            public static fGetBuff GetBuff;

            public static fGetMovement GetMovement;

            public static fSetMovement SetMovement;

            public static fGetMoveStatus GetMoveStatus;

            public static fSetMoveStatus SetMoveStatus;

            public static fGetEntityID GetEntityID;

            public static fGetTargetEntityID GetTargetEntityID;
        }
        

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void fUnloadIt([MarshalAs(UnmanagedType.LPStr)]string domainName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate float fGetPOS(EntityType eType, Axis axis, Int32 index);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr fGetName(EntityType eType, Int32 index);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void fSetPOS(EntityType type, Axis axis, Int32 index, float value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void fSetHeading(EntityType type, Int32 index, float value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate float fGetHeading(EntityType type, Int32 index);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void fSet3DVector(EntityType type, Axis axis, Int32 index, float value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate float fGet3DVector(EntityType type, Axis axis, Int32 index);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate NativeStructs.BuffStruct fGetBuff(Int32 buffIndex, Int32 eIndex);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate float fGetMovement(MovementEnum mEnum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void fSetMovement(MovementEnum mEnum, float value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate WalkingStatus fGetMoveStatus();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void fSetMoveStatus(WalkingStatus status);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate Int32 fGetEntityID(EntityType eType, Int32 index);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate Int32 fGetTargetEntityID();
    }
}
