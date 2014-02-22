using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Bolter_XIV
{
    public unsafe class NativeMethods : NativeStructs
    {

        public NativeMethods(IntPtr BaseAddress, int masterOffset, int zoneOffset, int collisionOffset, int menuOffset, int serverSideLock, int clientSideLock, int lockSprintAddress, int hideBuffAddress, int movementOffset)
        {
            
            ZoneAddress = Marshal.ReadIntPtr(BaseAddress + zoneOffset + 12);

            ShowHideBuff = (ShowBuffAsm*)(BaseAddress + hideBuffAddress + 3);

            RemoveBuff = (RemoveBuffAsm*)(BaseAddress + lockSprintAddress + 0x136);

            SerPosFunc = (ServerPosAsm*)(BaseAddress + serverSideLock + 36);

            CliPosFunc = (ClientPosAsm*)(BaseAddress + clientSideLock + 11);

            Collision = (CollisionAsm*)(BaseAddress + collisionOffset + 8);

            MovementPtr = (Movment*)Marshal.ReadIntPtr(BaseAddress + movementOffset + 6);
      
            MasterPtr = (MasterPointer*)(BaseAddress + masterOffset + 0x3C);
            
            _Menu = (MenuStruct*)Marshal.ReadInt32(BaseAddress + menuOffset + 0xC);
        }

        private void UpdateProtect<T>(void* basePtr)
        {
            uint o;
            WinAPI.VirtualProtect((IntPtr)basePtr, _SizeOf<T>(), Protection.PAGE_EXECUTE_READWRITE, out o);
        }

        public void CollsionToggle(bool on)
        {
            UpdateProtect<CollisionAsm>(Collision);

            switch (on)
            {
                case true:
                    Collision->cmpArg = 0x96;
                    Collision->cmpArg2 = 0x50;
                    break;
                case false:
                    Collision->cmpArg = 0x8E;
                    Collision->cmpArg2 = 0x48;
                    break;
            }

        }

        public uint _SizeOf<T>()
        {
            return (uint)Marshal.SizeOf(typeof(T));
        }

        protected byte[] PosBytes(Axis axis, PosType type)
        {
            var asm = type == PosType.Client ? CliPosBytes : SerPosBytes;

            switch (axis)
            {
                case Axis.X:
                    asm[4] = type == PosType.Client ? (byte)0x30 : (byte)0xA0;
                    break;
                case Axis.Y:
                    asm[4] = type == PosType.Client ? (byte)0x38 : (byte)0xA8;
                    break;
                case Axis.Z:
                    asm[4] = type == PosType.Client ? (byte)0x34 : (byte)0xA4;
                    break;
            }

            return asm;
        }

        public void LockAxis(Axis axis, bool lockit)
        {
            UpdateProtect<ClientPosAsm>(CliPosFunc);

            UpdateProtect<ServerPosAsm>(SerPosFunc);

            if (lockit)
            {
                switch (axis)
                {
                    case Axis.X:
                        WinAPI.FillMemory(CliPosFunc->XFunc, 5, 0x90);
                        WinAPI.FillMemory(SerPosFunc->XFunc, 8, 0x90);
                        break;
                    case Axis.Y:
                        WinAPI.FillMemory(CliPosFunc->YFunc, 5, 0x90);
                        WinAPI.FillMemory(SerPosFunc->YFunc, 8, 0x90);
                        break;
                    case Axis.Z:
                        WinAPI.FillMemory(CliPosFunc->ZFunc, 5, 0x90);
                        WinAPI.FillMemory(SerPosFunc->ZFunc, 8, 0x90);
                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case Axis.X:
                        WinAPI.memcpy(CliPosFunc->XFunc, PosBytes(Axis.X, PosType.Client), 5);
                        WinAPI.memcpy(SerPosFunc->XFunc, PosBytes(Axis.X, PosType.Server), 8);
                        break;
                    case Axis.Y:
                        WinAPI.memcpy(CliPosFunc->YFunc, PosBytes(Axis.Y, PosType.Client), 5);
                        WinAPI.memcpy(SerPosFunc->YFunc, PosBytes(Axis.Y, PosType.Server), 8);
                        break;
                    case Axis.Z:
                        WinAPI.memcpy(CliPosFunc->ZFunc, PosBytes(Axis.Z, PosType.Client), 5);
                        WinAPI.memcpy(SerPosFunc->ZFunc, PosBytes(Axis.Z, PosType.Server), 8);
                        break;
                }
            }

        }

        public float GetPos(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return PCMobEntity[0].PCMob->X;
                case Axis.Y:
                    return PCMobEntity[0].PCMob->Y;
                case Axis.Z:
                    return PCMobEntity[0].PCMob->Z;
            }
            return 0;
        }

        
        public D3DXVECTOR2 Get2DPos()
        {
            return new D3DXVECTOR2(PCMobEntity[0].PCMob->X, PCMobEntity[0].PCMob->Y);
        }

        public void WriteToPos(Axis axis, float value)
        {
            switch (axis)
            {
                case Axis.X:
                    PCMobEntity[0].PCMob->X = value;
                    PCMobEntity[0].PCMob->subStruct->X = value;
                    break;
                case Axis.Y:
                    PCMobEntity[0].PCMob->Y = value;
                    PCMobEntity[0].PCMob->subStruct->Y = value;
                    break;
                case Axis.Z:
                    PCMobEntity[0].PCMob->Z = value;
                    PCMobEntity[0].PCMob->subStruct->Z = value;
                    break;
            }
        }

        public void AddToPos(Axis axis, float value, bool add)
        {
            if (add)
            {
                switch (axis)
                {
                    case Axis.X:
                        PCMobEntity[0].PCMob->X += value;
                        PCMobEntity[0].PCMob->subStruct->X += value;
                        break;
                    case Axis.Y:
                        PCMobEntity[0].PCMob->Y += value;
                        PCMobEntity[0].PCMob->subStruct->Y += value;
                        break;
                    case Axis.Z:
                        PCMobEntity[0].PCMob->Z += value;
                        PCMobEntity[0].PCMob->subStruct->Z += value;
                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case Axis.X:
                        PCMobEntity[0].PCMob->X -= value;
                        PCMobEntity[0].PCMob->subStruct->X -= value;
                        break;
                    case Axis.Y:
                        PCMobEntity[0].PCMob->Y -= value;
                        PCMobEntity[0].PCMob->subStruct->Y -= value;
                        break;
                    case Axis.Z:
                        PCMobEntity[0].PCMob->Z -= value;
                        PCMobEntity[0].PCMob->subStruct->Z -= value;
                        break;
                }
            }
        }

        public string PCMobName(int index)
        {
            return new string(PCMobEntity[index].PCMob->Name);
        }

        public string ObjectName(int index)
        {
            return new string(ObjectEntity[index].Object->Name);
        }

        public string NPCName(int index)
        {
            return new string(NPCEntity[index].NPC->Name);
        }
        
        public void UpdateBuffDebuff()
        {
            LockLastBuff(true);
            //Loop while Slider is > 0
            while (MainWindow.SetSpeed[0] == 0x32)
            {
                try
                {
                    if (PCMobEntity[0].PCMob->Buffs._30.ID == BitConverter.ToUInt16(MainWindow.SetSpeed, 0) &&
                        PCMobEntity[0].PCMob->Buffs._30.Paras == BitConverter.ToUInt16(MainWindow.SetSpeed, 2))
                    {
                        //Do Nothing
                    }
                    else
                    {
                        PCMobEntity[0].PCMob->Buffs._30.ID = BitConverter.ToUInt16(MainWindow.SetSpeed, 0);
                        PCMobEntity[0].PCMob->Buffs._30.Paras = BitConverter.ToUInt16(MainWindow.SetSpeed, 2);
                    }
                }
                catch
                {
                }
                Thread.Sleep(1000);
            }
            //if slider value = 0 end thread and remove the buff/debuff.
            PCMobEntity[0].PCMob->Buffs._30.ID = BitConverter.ToUInt16(MainWindow.SetSpeed, 0);
            PCMobEntity[0].PCMob->Buffs._30.Paras = BitConverter.ToUInt16(MainWindow.SetSpeed, 2);
            LockLastBuff(false);
        }

        private void LockLastBuff(bool lockit)
        {
            UpdateProtect<RemoveBuffAsm>(RemoveBuff);

            if (lockit)
                RemoveBuff->cmpArg = 0x1D;
            else
                RemoveBuff->cmpArg = 0x1E;
        }

        public void HideSprint(bool hide)
        {
            UpdateProtect<ShowBuffAsm>(ShowHideBuff);

            if (hide)
            {
                ShowHideBuff->cmpArg = 0x1D;
                ShowHideBuff->cmpArg2 = 0x1D;
            }
            else
            {
                ShowHideBuff->cmpArg = 0x1E;
                ShowHideBuff->cmpArg2 = 0x1E;
            }
        }

        public string GetBuff(ushort id)
        {
            string value;
            return BuffNames.TryGetValue(id, out value) ? value : id.ToString();
        }
    }
   

}
