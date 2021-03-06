﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace Bolter_XIV
{
    unsafe public class NativeStructs : GameResources
    {

        #region Feilds

        public static ShowBuffAsm* ShowHideBuff;

        public static RemoveBuffAsm* RemoveBuff;

        public static ClientPosAsm* CliPosFunc;

        public static ServerPosAsm* SerPosFunc;

        private const int PCMobEntitySize = 64;

        private const int ObjectEntitySize = 22;

        const int NPCEntitySize = 40;

        public static IntPtr ZoneAddress;

        public static CollisionAsm* Collision;

        public static MenuStruct* _Menu;

        public static Movment* MovementPtr;

        public static MasterPointer* MasterPtr;

        public readonly PCMobHelper[] PCMobEntity = new PCMobHelper[PCMobEntitySize];

        public readonly ObjectHelper[] ObjectEntity = new ObjectHelper[ObjectEntitySize];

        public readonly NPCHelper[] NPCEntity = new NPCHelper[NPCEntitySize];

        public string CurrentZone
        {
            get
            {
                var ZoneID = Marshal.ReadInt32(ZoneAddress);
                string val;
                return Zones.TryGetValue(ZoneID, out val) ? val : ZoneID.ToString();
            }
        }

        #endregion

        public NativeStructs()
        {
            // Occupy our proxy entity structures.
            for (var i = 0; i < PCMobEntitySize; i++)
            {
                PCMobEntity[i] = new PCMobHelper(i*0x4100);
            }

            for (var i = 0; i < ObjectEntitySize; i++)
            {
                ObjectEntity[i] = new ObjectHelper(i*0x200);
            }
            
            for (var i = 0; i < NPCEntitySize; i++)
            {
                NPCEntity[i] = new NPCHelper(i * 0x3190);
            }
        }

        #region Helpers

        public class PCMobHelper
        {
            private readonly int _offset;

            public PCMobStruct* PCMob
            {
                get { return (PCMobStruct*)(((uint)*MasterPtr->Player) + _offset); }
            }
            public PCMobHelper(int pOffest)
            {
                _offset = pOffest;
            }
        }

        public class ObjectHelper
        {
            private readonly int _offset;

            public ObjectStruct* Object
            {
                get { return (ObjectStruct*)(((uint)*MasterPtr->NPCObject) + _offset); }
            }

            public ObjectHelper(int pOffest)
            {
                _offset = pOffest;
            }
        }

        public class NPCHelper
        {
            private readonly int _offset;

            public NPCStruct* NPC
            {
                get { return (NPCStruct*)(((uint)*MasterPtr->NPC) + _offset); }
            }

            public NPCHelper(int pOffest)
            {
                _offset = pOffest;
            }
        }

        public Movment* MovementAdj
        {
            get { return MovementPtr; }
        }

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MasterPointer
        {
            public IntPtr unknownPtr;
            public fixed byte spacer[6];

            /* Pointer to Target Structure */
            public TargetStruct* Target;

            public fixed byte spacer2[6];
            public IntPtr unknownPtr2;
            public fixed byte spacer3[16];
            public IntPtr unknownPtr3;
            public fixed byte spacer4[6];
            public IntPtr unknownPtr4;
            public fixed byte spacer5[6];
            public IntPtr unknownPtr5;
            public fixed byte spacer6[6];

            /* Contains many floats. including PI. Maybe something to do with entity geometry/navigation */
            public IntPtr unknownPtrtoPtr; //Contains many floats.

            public fixed byte spacer7[6];
            public IntPtr unknownPtr6;
            public fixed byte spacer8[6];
            public IntPtr unknownPtr7; //may be PtrtoPtr too.
            public fixed byte spacer9[6];
            public IntPtr unknownPtr8;
            public fixed byte spacer10[6];
            public IntPtr unknownPtr9;
            public fixed byte spacer11[6];
            public IntPtr unknownPtr10;
            public fixed byte spacer12[6];
            public IntPtr unknownPtr11; //may be PtrtoPtr too.
            public fixed byte spacer13[16];
            public fixed byte _spacer13[0xA];
            /* Pointer to Pointer of Player Structure */
            public PCMobStruct** Player;

            private fixed byte spacer14[6];
            private IntPtr unknownPtr12;
            private fixed byte spacer15[7];
            private IntPtr unknownPtr13;
            private fixed byte spacer16[2];
            private IntPtr unknownPtr14;
            private fixed byte spacer17[6];
            private IntPtr unknownPtrtoPtr2;
            private fixed byte spacer18[5];

            /* Pointer to Pointer of Object type NPC entities Structure */
            public ObjectStruct** NPCObject;

            private fixed byte spacer19[6];

            /* Pointer to Pointer of NPC entity Structure */
            public NPCStruct** NPC;

            private fixed byte spacer20[6];
            private IntPtr unknownPtr15;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PCMobStruct
        {
            private fixed byte Unknown[0x30]; //Unknown 0x30 bytes
            public fixed sbyte Name[0x18];
            private fixed byte Unknown1[0x2C];
            public uint ID;
            public uint NPCID;
            private fixed byte Unknown2[0xE]; //Unknown 0x28 bytes
            public byte MobType;
            private byte Unknown0;
            public byte CurrentTarget;
            public byte Distance;
            public byte GatheringStatus;
            private fixed byte _Unknown2[0x11];
            public float X, Z, Y; 
            private float Unknown3; //Unknown float
            public float Heading; //Server Side Heading
            public float ServerHight; //Cam height?
            private float Unknown4; //Unknown float
            private float Unknown5; //Unknown float
            private float Unknown6; //Unknown float
            private fixed byte Unknown7[0x24]; //Unknown 0x28 bytes
            public UInt16 FateId;
            private UInt16 _Unknown0;
            public SubPlayerStruct* subStruct;
            private fixed byte _Unknown8[0x2C];
            public byte GatheringInvisible;
            private fixed byte Unknown8[0x17]; //Unknown 0x44 bytes
            public float CamGlide;
            private fixed byte Unknown9[0x3C]; //Unknown 0x3C bytes
            public float StaticCamGlide;
            private fixed byte Unknown10[0x10]; //Unknown 0x10 bytes
            public byte StatusAdjust; //Has something to do with Player status. setting to 2 gives "Return" prompt
            public byte IsGM;
            private fixed byte Unknown11[0xA]; //Unknown 0x60 bytes
            public byte Icon;
            public byte IsEngaged;
            private fixed byte _Unknown11[0x3A];
            public UInt32 EntityID;
            private fixed byte _Unknown12[0xC];
            public float dynamicXCord, dynamicZCord, dynamicYCord; //Only updates when you are moving
            private uint Unknown12; // Unknown value
            public uint IsMoving;
            public float dynamicHeading; //Only updates when you change heading
            public float dynamicHeading2; //Only updates when you change heading
            private uint Unknown13; // Unkown value
            public float SetMoveLock; //Always -1, setting to < -1 increases speed but moves player backwards.
            public float SetMoveLock2; //Always -1, setting to 0 locks player in place.
            public float SetMoveLock3; //Always -1, setting to 0 locks player in place.
            public float SetMoveLock4; //Always -1, setting to 0 locks player in place.
            public float SetMoveLock5; //Always -1, setting to 0 locks player in place.
            private fixed byte Unknown14[0x10]; //Unknown 0x10 bytes
            public uint IsMoving2;
            private fixed byte Unknown15[0x18]; //Unknown 0x18 bytes
            public float TimeTraveled; //Stores amount of seconds traveled since you last started moving.
            private fixed byte _Unknown17[0x864];
            public float TargetID;
            private fixed byte Unknown16[0x270C]; //Unknown 0x2D1C bytes
            public BuffsAndDebuffs Buffs;

        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BuffStruct
        {
            public UInt16 ID;
            public UInt16 Paras;
            public float Timer;
            public uint ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BuffsAndDebuffs
        {
            public BuffStruct
                _1, _2, _3, _4, _5,
                _6, _7, _8, _9, _10,
                _11, _12, _13, _14, _15,
                _16, _17, _18, _19, _20,
                _21, _22, _23, _24, _25,
                _26, _27, _28, _29, _30;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SubPlayerStruct
        {
            private fixed byte Unknown[0x30]; //Unknown 0x2C bytes
            public float X, Z, Y;
            private fixed byte Unknown2[8];
            public float VectorX, VectorZ, VectorY;
            public float PlayerWidth, PlayerHieght, PlayerGirth;
            private fixed byte Unknown4[18]; //Unknown 0x2C bytes

            /// <summary>
            /// Values 1-16 Control each body part.
            /// </summary>
            public uint DisplayedBody;

            private fixed byte Unknown5[0xA8]; //Unknown 0xA8 bytes
            public float X2, Z2, Y2;
            private fixed byte Unknown6[0x188]; //Unknown 0xA8 bytes
            public float PlayerSizeNoCam;
            public float PlayerSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Movment
        {
            public WalkingStatus Status;
            public byte IsFollowing;
            private fixed byte Unknown2[19];
            public float CurrentSpeed;
            private fixed byte Unknown3[4];
            public float ForwardSpeed;
            private fixed byte Unknown4[4];
            public float LeftRightSpeed;
            private fixed byte Unknown5[4];
            public float BackwardSpeed;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TargetStruct
        {
            private fixed byte Unknown[0x88];
            public uint TargetID;
            private fixed byte Unknown2[0x50];
            public uint LastTargetID;
            public TargetStatus TargetStatus;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjectStruct
        {
            private fixed byte Unknown[0x30]; //Unknown 0x30 bytes
            public fixed sbyte Name[0x18];
            private fixed byte Unknown1[0x2C];
            public uint ID;
            private fixed byte Unknown2[0x28]; //Unknown 0x28 bytes
            public float X, Z, Y;
            private float Unknown3; //Unknown float
            public float Heading; //Server Side Heading
            public float ServerHight; //Cam height?
            private float Unknown4, Unknown5, Unknown6; //Unknown float
            private fixed byte Unknown7[0x28]; //Unknown 0x28 bytes
            public SubPlayerStruct* subStruct;
            private fixed byte Unknown8[0x2C];
            public uint IsActive;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MenuStruct
        {
            private fixed byte Unknown[0xD48];
            public MenuItem* SelectedItem;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MenuItem
        {
            private fixed byte Unknown[0x90];
            public uint ID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NPCStruct
        {
            //Contains names at every 0x2FD0 offset starting at 0x30.
            private fixed byte Unknown[0x30]; //Unknown 0x30 bytes
            public fixed sbyte Name[0x18];
            private fixed byte Unknown1[0x2C];
            public uint ID;
            private fixed byte Unknown2[0x28]; //Unknown 0x28 bytes
            public float X, Z, Y;
            private float Unknown3; //Unknown float
            public float Heading; //Server Side Heading
            public float ServerHight; //Cam height?
            private float Unknown4, Unknown5, Unknown6; //Unknown float
            private fixed byte Unknown7[0x28]; //Unknown 0x28 bytes
            public SubPlayerStruct* subStruct;
            private fixed byte Unknown8[0x2C];
            public uint IsActive;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CollisionAsm
        {
            public byte cmpArg;
            private fixed byte unused[7];
            public byte cmpArg2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RemoveBuffAsm
        {
            private fixed byte unused[2];
            public byte cmpArg;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ShowBuffAsm
        {
            private fixed byte unused[2];
            public byte cmpArg;
            private fixed byte unused2[0x1F];
            public byte cmpArg2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ClientPosAsm
        {
            public fixed byte XFunc[5];
            private fixed byte unused[5];
            public fixed byte ZFunc[5];
            private fixed byte unused2[5];
            public fixed byte YFunc[5];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ServerPosAsm
        {
            public fixed byte XFunc[8];
            private fixed byte unused2[5];
            public fixed byte ZFunc[8];
            private fixed byte unused3[5];
            public fixed byte YFunc[8];
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct Camera
        {
            private fixed byte Unknown0[0x40];
            public float UnknownCam0; //offset 0x40
            public float UnknownCam1; //offset 0x44
            public float UnknownCam2; //offset 0x48
            private fixed byte Unknown3[0x24];
            public float UnknownCam3; //offset 0x70
            public float UnknownCam4; //offset 0x74
            public float UnknownCam5; //offset 0x78
            private fixed byte Unknown6[0x4];
            public float UnknownCam6; //offset 0x80
            public float UnknownCam7; //offset 0x84
            public float UnknownCam8; //offset 0x88
            private fixed byte Unknown9[0x4];
            public float UnknownCam9; //offset 0x90
            public float UnknownCam10; //offset 0x94
            public float UnknownCam11; //offset 0x98
            private fixed byte Unknown12[0x8];
            public float UnknownCam12; //offset 0xA4
            public float UnknownCam13; //offset 0xA8
            private fixed byte Unknown14[0x4];
            public float UnknownCam14; //offset 0xB0
            public float UnknownCam15; //offset 0xB4
            public float UnknownCam16; //offset 0xB8
            private fixed byte Unknown17[0x4];
            public float UnknownCam17; //offset 0xC0
            public float UnknownCam18; //offset 0xC4
            public float UnknownCam19; //offset 0xC8
            private fixed byte Unknown20[0x1C];
            public float Zoom; //offset 0xE8
            private fixed byte Unknown21[0x14];
            public float UnknownCam21; //offset 0x100
            public float UnknownCam22; //offset 0x104
            private fixed byte Unknown23[0x38];
            public float Zoom2; //offset 0x140
            private fixed byte Unknown24[0x1C];
            public float UnknownCam24; //offset 0x160
            public float UnknownCam25; //offset 0x164
            public float UnknownCam26; //offset 0x168
            private fixed byte Unknown27[0x4];
            public float UnknownCam27; //offset 0x170
            public float UnknownCam28; //offset 0x174
            public float UnknownCam29; //offset 0x178
            private fixed byte Unknown30[0x4];
            public float UnknownCam30; //offset 0x180
            public float UnknownCam31; //offset 0x184
            public float UnknownCam32; //offset 0x188
            private fixed byte Unknown33[0x44];
            public float UnknownCam33; //offset 0x1D0
            public float UnknownCam34; //offset 0x1D4
            public float UnknownCam35; //offset 0x1D8
            private fixed byte Unknown36[0xC];
            public float UnknownCam36; //offset 0x1E8
            private fixed byte Unknown37[0x44];
            public float UnknownCam37; //offset 0x230
            public float UnknownCam38; //offset 0x234
            public float UnknownCam39; //offset 0x238

        }
    
    }
        #endregion

    #region Enums
    [Flags]
    public enum TargetStatus : uint
    {
        NoTarget = 0x00010001,
        HasTarget = 0x00010000,
        Locked = 0x01010000
    }

    [Flags]
    public enum WalkingStatus
    {
        Standing = 0x00000000,
        Running = 0x00000001,
        Heading = 0x00000100,
        Walking = 0x00010000,
        Autorun = 0x01000000
    }
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public enum PosType
    {
        Server,
        Client
    }
    public enum EntityType
    {
        PCMob,
        Object,
        NPC
    };
    #endregion
}
