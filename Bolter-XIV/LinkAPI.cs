using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Bolter_XIV
{
    public class LinkAPI : UnmanagedDelegates
    {
        public IList<IPCMobEntity> PcMobEntities = new IPCMobEntity[64];

        public IList<IObjectEntity> ObjectEntities = new IObjectEntity[22];

        public IList<INPCEntity> NpcEntities = new INPCEntity[40];

        public LinkAPI(IList<IntPtr> funcPtrs)
        {
            var count = 0;
            foreach (var f in typeof (Funcs).GetFields())
            {
                f.SetValue(null, Marshal.GetDelegateForFunctionPointer(funcPtrs[count], f.FieldType));
                count++;
            }

            for (var i = 0; i < 64; i++)
            {
                PcMobEntities[i] = new PCMobEntity(i, EntityType.PCMob);
            }
            for (var i = 0; i < 22; i++)
            {
                ObjectEntities[i] = new ObjectEntity(i, EntityType.Object);
            }

            for (var i = 0; i < 40; i++)
            {
                NpcEntities[i] = new NPCEntity(i, EntityType.NPC);
            }
            
        }

        public interface IEntity
        {
            int Index { get; set; }
            EntityType EType { get; set; }
            float X { get; set; }
            float Y { get; set; }
            float Z { get; set; }
            float VectorX { get; set; }
            float VectorY { get; set; }
            float VectorZ { get; set; }
            string Name { get; }
            Int32 EntityID { get; }
        }

        public interface IPCMobEntity : IEntity
        {
            NativeStructs.BuffStruct GetBuffDebuff(int index);
        }

        public interface IObjectEntity : IEntity
        {
            int IsActive { get; set; }
        }

        public interface INPCEntity : IEntity
        {
            int IsActive { get; set; }
        }

        public class PCMobEntity : Entity, IPCMobEntity
        {
            public PCMobEntity(int index, EntityType eType)
            {
                Index = index;
                EType = eType;
            }
            public new NativeStructs.BuffStruct GetBuffDebuff(int index)
            {
                return Funcs.GetBuff(index, Index);
            }
        }

        public class ObjectEntity : Entity, IObjectEntity
        {
            public ObjectEntity(int index, EntityType eType)
            {
                Index = index;
                EType = eType;
            }
            public new int IsActive { get; set; }
        }

        public class NPCEntity : Entity, INPCEntity
        {
            public NPCEntity(int index, EntityType eType)
            {
                Index = index;
                EType = eType;
            }
            public new int IsActive { get; set; }
        }

        public class Entity
        {
            public int Index { get; set; }
            public EntityType EType { get; set; }
            
            public float X 
            { 
                get { return Funcs.GetPOS(EType, Axis.X, Index); }
                set { Funcs.SetPOS(EType, Axis.X, Index, value); }
            }

            public float Y
            {
                get { return Funcs.GetPOS(EType, Axis.Y, Index); }
                set { Funcs.SetPOS(EType, Axis.Y, Index, value); }
            }

            public float Z
            {
                get { return Funcs.GetPOS(EType, Axis.Z, Index); }
                set { Funcs.SetPOS(EType, Axis.Z, Index, value); }
            }

            public float VectorX
            {
                get { return Funcs.Get3DVector(EType, Axis.X, Index); }
                set { Funcs.Set3DVector(EType, Axis.X, Index, value); }
            }

            public float VectorY
            {
                get { return Funcs.Get3DVector(EType, Axis.Y, Index); }
                set { Funcs.Set3DVector(EType, Axis.Y, Index, value); }
            }

            public float VectorZ
            {
                get { return Funcs.Get3DVector(EType, Axis.Z, Index); }
                set { Funcs.Set3DVector(EType, Axis.Z, Index, value); }
            }

            public string Name
            {
                get { return Marshal.PtrToStringAnsi(Funcs.GetName(EType, Index)); }
            }

            public Int32 EntityID
            {
                get { return Funcs.GetEntityID(EType, Index); }
            }

            public virtual NativeStructs.BuffStruct GetBuffDebuff(int index)
            {
                return EType == EntityType.PCMob ? Funcs.GetBuff(index, Index) : NullStruct;
            }

            public virtual int IsActive { get; set; }
        }

        public static NativeStructs.BuffStruct NullStruct = new NativeStructs.BuffStruct();

        public Entity TargetEntity
        {
            get
            {
                foreach (var ent in PcMobEntities.Where(ent => ent.EntityID == Funcs.GetTargetEntityID()))
                {
                    return (Entity)ent;
                }
                foreach (var ent in ObjectEntities.Where(ent => ent.EntityID == Funcs.GetTargetEntityID()))
                {
                    return (Entity)ent;
                }
                return NpcEntities.Where(ent => ent.EntityID == Funcs.GetTargetEntityID()).Cast<Entity>().FirstOrDefault();
            }
        }

    }

    public abstract class UnmanagedFunctions
    {
        public static T PtrToFunc<T>(IntPtr pointer)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(pointer, typeof(T));
        } 
    }
    public enum MovementEnum
    {
        CurrentSpeed,
        ForwardSpeed,
        LeftRightSpeed,
        BackwardSpeed
    };
}
