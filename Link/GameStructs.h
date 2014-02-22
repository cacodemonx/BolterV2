#include "stdafx.h"
#define PCMobSize 0x4100
#define NPCSize 0x3190

struct D3DVECTOR3
{
	float x;
	float z;
	float y;
};

enum MovementEnum
{
	CurrentSpeed,
	ForwardSpeed,
	LeftRightSpeed,
	BackwardSpeed
};

enum TargetStatus : INT32
{
	NoTarget = 0x00010001,
	HasTarget = 0x00010000,
	Locked = 0x01010000
};

enum WalkingStatus
{
	Standing = 0x00000000,
	Running = 0x00000001,
	Heading = 0x00000100,
	Walking = 0x00010000,
	Autorun = 0x01000000
};
enum Axis
{
	X,
	Y,
	Z
};

enum PosType
{
	Server,
	Client
};

enum EntityType
{
	PCMob,
	Object,
	NPC
};

#pragma pack(push, 1)
struct BuffStruct
{
	USHORT ID;
	USHORT Paras;
	float Timer;
	INT32 ExtraInfo;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct BuffsAndDebuffs
{
	BuffStruct
		_1, _2, _3, _4, _5,
		_6, _7, _8, _9, _10,
		_11, _12, _13, _14, _15,
		_16, _17, _18, _19, _20,
		_21, _22, _23, _24, _25,
		_26, _27, _28, _29, _30;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct SubPlayerStruct
{
	char Unknown[0x30]; //Unknown 0x2C chars
	D3DVECTOR3 POS;
	char Unknown2[8];
	D3DVECTOR3 Vectors; 
	float PlayerWidth, PlayerHieght, PlayerGirth;
	char Unknown4[18]; //Unknown 0x2C chars

	/// <summary>
	/// Values 1-16 Control each body part.
	/// </summary>
	INT32 DisplayedBody;

	char Unknown5[0xA8]; //Unknown 0xA8 chars
	float X2, Z2, Y2;
	char Unknown6[0x188]; //Unknown 0xA8 chars
	float PlayerSizeNoCam;
	float PlayerSize;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct ObjectStruct
{
	char Unknown[0x30]; //Unknown 0x30 chars
	char Name[0x18];
	char Unknown1[0x2C];
	INT32 ID;
	INT32 NPCID;
	char Unknown2[0xE]; //Unknown 0x28 bytes
	char MobType;
	char Unknown0;
	char CurrentTarget;
	char Distance;
	char GatheringStatus;
	char _Unknown2[0x11];
	D3DVECTOR3 POS; 
	float Unknown3; //Unknown float
	float Heading; //Server Side Heading
	float ServerHight; //Cam height?
	float Unknown4, Unknown5, Unknown6; //Unknown float
	char Unknown7[0x28]; //Unknown 0x28 chars
	SubPlayerStruct* subStruct;
	char Unknown8[0x2C];
	INT32 IsActive;
	char Unknown9[0x84];
	INT32 EntityID;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct NPCStruct
{
	//Contains names at every *NPCSize offset starting at 0x30.
	char Unknown[0x30]; //Unknown 0x30 chars
	char Name[0x18];
	char Unknown1[0x2C];
	INT32 ID;
	INT32 NPCID;
	char Unknown2[0xE]; //Unknown 0x28 bytes
	char MobType;
	char Unknown0;
	char CurrentTarget;
	char Distance;
	char GatheringStatus;
	char _Unknown2[0x11];
	D3DVECTOR3 POS; 
	float Unknown3; //Unknown float
	float Heading; //Server Side Heading
	float ServerHight; //Cam height?
	float Unknown4, Unknown5, Unknown6; //Unknown float
	char Unknown7[0x28]; //Unknown 0x28 chars
	SubPlayerStruct* subStruct;
	char Unknown8[0x2C];
	INT32 IsActive;
	char Unknown9[0xA0];
	INT32 EntityID;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct PCMobStruct
{
	char Unknown[0x30]; //Unknown 0x30 bytes
	char Name[0x18];
	char Unknown1[0x2C];
	INT32 ID;
	INT32 NPCID;
	char Unknown2[0xE]; //Unknown 0x28 bytes
	char MobType;
	char Unknown0;
	char CurrentTarget;
	char Distance;
	char GatheringStatus;
	char _Unknown2[0x11];
	D3DVECTOR3 POS; 
	float Unknown3; //Unknown float
	float Heading; //Server Side Heading
	float ServerHight; //Cam height?
	float Unknown4; //Unknown float
	float Unknown5; //Unknown float
	float Unknown6; //Unknown float
	char Unknown7[0x24]; //Unknown 0x28 bytes
	USHORT FateId;
	USHORT _Unknown0;
	SubPlayerStruct* subStruct;
	char _Unknown8[0x2C];
	char GatheringInvisible;
	char Unknown8[0x17]; //Unknown 0x44 bytes
	float CamGlide;
	char Unknown9[0x3C]; //Unknown 0x3C bytes
	float StaticCamGlide;
	char Unknown10[0x10]; //Unknown 0x10 bytes
	char StatusAdjust; //Has something to do with Player status. setting to 2 gives "Return" prompt
	char IsGM;
	char Unknown11[0xA]; //Unknown 0x60 bytes
	char Icon;
	char IsEngaged;
	char _Unknown11[0x3A];
	INT32 EntityID;
	char _Unknown12[0xC];
	float dynamicXCord, dynamicZCord, dynamicYCord; //Only updates when you are moving
	INT32 Unknown12; // Unknown value
	INT32 IsMoving;
	float dynamicHeading; //Only updates when you change heading
	float dynamicHeading2; //Only updates when you change heading
	INT32 Unknown13; // Unkown value
	float SetMoveLock; //Always -1, setting to < -1 increases speed but moves player backwards.
	float SetMoveLock2; //Always -1, setting to 0 locks player in place.
	float SetMoveLock3; //Always -1, setting to 0 locks player in place.
	float SetMoveLock4; //Always -1, setting to 0 locks player in place.
	float SetMoveLock5; //Always -1, setting to 0 locks player in place.
	char Unknown14[0x10]; //Unknown 0x10 bytes
	INT32 IsMoving2;
	char Unknown15[0x18]; //Unknown 0x18 bytes
	float TimeTraveled; //Stores amount of seconds traveled since you last started moving.
	char _Unknown17[0x864];
	float TargetID;
	char Unknown16[0x270C]; //Unknown 0x2D1C bytes
	BuffsAndDebuffs Buffs;

};
#pragma pack(pop)
struct Camera
{
	char Unknown0[0x40];
	float UnknownCam0; //offset 0x40
	float UnknownCam1; //offset 0x44
	float UnknownCam2; //offset 0x48
	char Unknown3[0x24];
	float UnknownCam3; //offset 0x70
	float UnknownCam4; //offset 0x74
	float UnknownCam5; //offset 0x78
	char Unknown6[0x4];
	float UnknownCam6; //offset 0x80
	float UnknownCam7; //offset 0x84
	float UnknownCam8; //offset 0x88
	char Unknown9[0x4];
	float UnknownCam9; //offset 0x90
	float UnknownCam10; //offset 0x94
	float UnknownCam11; //offset 0x98
	char Unknown12[0x8];
	float UnknownCam12; //offset 0xA4
	float UnknownCam13; //offset 0xA8
	char Unknown14[0x4];
	float UnknownCam14; //offset 0xB0
	float UnknownCam15; //offset 0xB4
	float UnknownCam16; //offset 0xB8
	char Unknown17[0x4];
	float UnknownCam17; //offset 0xC0
	float UnknownCam18; //offset 0xC4
	float UnknownCam19; //offset 0xC8
	char Unknown20[0x1C];
	float Zoom; //offset 0xE8
	char Unknown21[0x14];
	float UnknownCam21; //offset 0x100
	float UnknownCam22; //offset 0x104
	char Unknown23[0x38];
	float Zoom2; //offset 0x140
	char Unknown24[0x1C];
	float UnknownCam24; //offset 0x160
	float UnknownCam25; //offset 0x164
	float UnknownCam26; //offset 0x168
	char Unknown27[0x4];
	float UnknownCam27; //offset 0x170
	float UnknownCam28; //offset 0x174
	float UnknownCam29; //offset 0x178
	char Unknown30[0x4];
	float UnknownCam30; //offset 0x180
	float UnknownCam31; //offset 0x184
	float UnknownCam32; //offset 0x188
	char Unknown33[0x44];
	float UnknownCam33; //offset 0x1D0
	float UnknownCam34; //offset 0x1D4
	float UnknownCam35; //offset 0x1D8
	char Unknown36[0xC];
	float UnknownCam36; //offset 0x1E8
	char Unknown37[0x44];
	float UnknownCam37; //offset 0x230
	float UnknownCam38; //offset 0x234
	float UnknownCam39; //offset 0x238

};
#pragma pack(push, 1)
struct TargetStruct
{
	Camera* CamPtr;
	char Unknown[0x84];
	INT32 EntityID;
	char Unknown2[0x54];
	INT32 LastTargetID;
	TargetStatus TargetStatus;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct MasterPointer
{
	LPVOID unknownPtr;
	char spacer[6];

	/* Pointer to Target Structure */
	TargetStruct* Target;

	char spacer2[6];
	LPVOID unknownPtr2;
	char spacer3[16];
	LPVOID unknownPtr3;
	char spacer4[6];
	LPVOID unknownPtr4;
	char spacer5[6];
	LPVOID unknownPtr5;
	char spacer6[6];

	/* Contains many floats. including PI. Maybe something to do with entity geometry/navigation */
	LPVOID unknownPtrtoPtr; //Contains many floats.

	char spacer7[6];
	LPVOID unknownPtr6;
	char spacer8[6];
	LPVOID unknownPtr7; //may be PtrtoPtr too.
	char spacer9[6];
	LPVOID unknownPtr8;
	char spacer10[6];
	LPVOID unknownPtr9;
	char spacer11[6];
	LPVOID unknownPtr10;
	char spacer12[6];
	LPVOID unknownPtr11; //may be PtrtoPtr too.
	char spacer13[16];
	char _spacer13[0xA];
	/* Pointer to Pointer of Player Structure */
	INT32* Player;

	char spacer14[6];
	LPVOID unknownPtr12;
	char spacer15[7];
	LPVOID unknownPtr13;
	char spacer16[2];
	LPVOID unknownPtr14;
	char spacer17[6];
	LPVOID unknownPtrtoPtr2;
	char spacer18[5];

	/* Pointer to Pointer of Object type NPC entities Structure */
	INT32* NPCObject;

	char spacer19[6];

	/* Pointer to Pointer of NPC entity Structure */
	INT32* NPC;

	char spacer20[6];
	LPVOID unknownPtr15;
};
#pragma pack(pop)


#pragma pack(push, 1)
struct Movment
{
	WalkingStatus Status;
	char IsFollowing;
	char Unknown2[19];
	float CurrentSpeed;
	char Unknown3[4];
	float ForwardSpeed;
	char Unknown4[4];
	float LeftRightSpeed;
	char Unknown5[4];
	float BackwardSpeed;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct MenuItem
{
	char Unknown[0x90];
	INT32 ID;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct MenuStruct
{
	char Unknown[0xD48];
	MenuItem* SelectedItem;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct CollisionAsm
{
	char cmpArg;
	char unused[7];
	char cmpArg2;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct RemoveBuffAsm
{
	char unused[2];
	char cmpArg;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct ShowBuffAsm
{
	char unused[2];
	char cmpArg;
	char unused2[0x1F];
	char cmpArg2;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct ClientPosAsm
{
	char XFunc[5];
	char unused[5];
	char ZFunc[5];
	char unused2[5];
	char YFunc[5];
};
#pragma pack(pop)

#pragma pack(push, 1)
struct ServerPosAsm
{
	char XFunc[8];
	char unused2[5];
	char ZFunc[8];
	char unused3[5];
	char YFunc[8];
};
#pragma pack(pop)


MasterPointer* MasterPtr = NULL;

Movment** MovPtr = NULL;

float GetPOS(EntityType type, Axis axis, INT32 index)
{
	if ((type == PCMob && index > 64) || (type == Object && index > 22) || (type == NPC && index > 40))
	{
		MessageBoxA(0,"Index out of range","",0);
		throw EXCEPTION_ARRAY_BOUNDS_EXCEEDED;
	}

	switch (axis)
	{
		case X:
			switch (type)
			{
				case PCMob:
					return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->POS.x;
				case Object:
					return ((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->POS.x;
				default:
					return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->POS.x;
			}
		case Y:
			switch (type)
			{
				case PCMob:
					return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->POS.y;
				case Object:
					return ((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->POS.y;
				default:
					return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->POS.y;
			}
		case Z:
			switch (type)
			{
				case PCMob:
					return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->POS.z;
				case Object:
					return ((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->POS.z;
				default:
					return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->POS.z;
			}
		default:
			break;
	}
	return 0;
}

const char * GetName(EntityType type, INT32 index)
{
	switch (type)
	{
	case PCMob:
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Name;
	case Object:
		return ((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->Name;
	case NPC:
		return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->Name;
	default:
		break;
	}

	return NULL;

}

void SetPOS (EntityType type, Axis axis, INT32 index, float value)
{
	if ((type == PCMob && index > 64) || (type == Object && index > 22) || (type == NPC && index > 40))
	{
		MessageBoxA(0,"Index out of range","",0);
		throw EXCEPTION_ARRAY_BOUNDS_EXCEEDED;
	}

	switch (axis)
	{
		case X:
			switch (type)
			{
				case PCMob:
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->POS.x = value;
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->POS.x = value;
					break;
				case Object:
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->POS.x = value;
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->POS.x = value;
					break;
				default:
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->POS.x = value;
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->POS.x = value;
					break;	
			}
		case Y:
			switch (type)
			{
				case PCMob:
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->POS.y = value;
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->POS.y = value;
					break;
				case Object:
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->POS.y = value;
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->POS.y = value;
					break;
				default:
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->POS.y = value;
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->POS.y = value;
					break;
			}
		case Z:
			switch (type)
			{
				case PCMob:
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->POS.z = value;
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->POS.z = value;
					break;
				case Object:
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->POS.z = value;
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->POS.z = value;
					break;
				default:
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->POS.z = value;
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->POS.z = value;
					break;
			}
		default:
			break;
	}
}

void SetHeading(EntityType type, INT32 index, float value)
{
	switch (type)
	{
	case PCMob:
		((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Heading = value;
		break;
	case Object:
		((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->Heading = value;
		break;
	case NPC:
		((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->Heading = value;
		break;
	default:
		break;
	}
}

float GetHeading(EntityType type, INT32 index)
{
	switch (type)
	{
	case PCMob:
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Heading;
	case Object:
		return ((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->Heading;
	case NPC:
		return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->Heading;
	default:
		break;
	}
	return NULL;
}

void Set3DVector(EntityType type, Axis axis, INT32 index, float value)
{
	switch (axis)
	{
		case X:
			switch (type)
			{
				case PCMob:
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->Vectors.x = value;
					break;
				case Object:
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->Vectors.x = value;
					break;
				default:
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->Vectors.x = value;
					break;	
			}
		case Y:
			switch (type)
			{
				case PCMob:
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->Vectors.y = value;
					break;
				case Object:
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->Vectors.y = value;
					break;
				default:
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->Vectors.y = value;
					break;
			}
		case Z:
			switch (type)
			{
				case PCMob:
					((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->Vectors.z = value;
					break;
				case Object:
					((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->Vectors.z = value;
					break;
				default:
					((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->Vectors.z = value;
					break;
			}
		default:
			break;
	}
}

float Get3DVector(EntityType type, Axis axis, INT32 index)
{
	switch (axis)
	{
		case X:
			switch (type)
			{
				case PCMob:
					return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->Vectors.x;
				case Object:
					return ((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->Vectors.x;
				default:
					return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->Vectors.x;
			}
		case Y:
			switch (type)
			{
				case PCMob:
					return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->Vectors.y;
				case Object:
					return ((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->Vectors.y;
				default:
					return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->Vectors.y;
			}
		case Z:
			switch (type)
			{
				case PCMob:
					return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->subStruct->Vectors.z;
				case Object:
					return ((ObjectStruct*)(*MasterPtr->NPCObject + (index*0x200)))->subStruct->Vectors.z;
				default:
					return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->subStruct->Vectors.z;
			}
		default:
			return 0;
	}
}

BuffStruct GetBuff(INT32 buffIndex, INT32 index)
{
	if (buffIndex == 1)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._1;
	else if (buffIndex == 2)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._2;
	else if (buffIndex == 3)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._3;
	else if (buffIndex == 4)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._4;
	else if (buffIndex == 5)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._5;
	else if (buffIndex == 6)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._6;
	else if (buffIndex == 7)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._7;
	else if (buffIndex == 8)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._8;
	else if (buffIndex == 9)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._9;
	else if (buffIndex == 10)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._10;
	else if (buffIndex == 11)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._11;
	else if (buffIndex == 12)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._12;
	else if (buffIndex == 13)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._13;
	else if (buffIndex == 14)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._14;
	else if (buffIndex == 15)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._15;
	else if (buffIndex == 16)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._16;
	else if (buffIndex == 17)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._17;
	else if (buffIndex == 18)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._18;
	else if (buffIndex == 19)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._19;
	else if (buffIndex == 20)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._20;
	else if (buffIndex == 21)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._21;
	else if (buffIndex == 22)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._22;
	else if (buffIndex == 23)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._23;
	else if (buffIndex == 24)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._24;
	else if (buffIndex == 25)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._25;
	else if (buffIndex == 26)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._26;
	else if (buffIndex == 27)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._27;
	else if (buffIndex == 28)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._28;
	else if (buffIndex == 29)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._29;
	else if (buffIndex == 30)
		return ((PCMobStruct*)(*MasterPtr->Player + (index*PCMobSize)))->Buffs._30;
	throw EXCEPTION_ARRAY_BOUNDS_EXCEEDED;
}

float GetMovement(MovementEnum mEnum)
{
	switch (mEnum)
	{
	case CurrentSpeed:
		return (*MovPtr)->CurrentSpeed;
	case ForwardSpeed:
		return (*MovPtr)->ForwardSpeed;
	case LeftRightSpeed:
		return (*MovPtr)->LeftRightSpeed;
	case BackwardSpeed:
		return (*MovPtr)->BackwardSpeed;
	default:
		return NULL;
	}
}

void SetMovement(MovementEnum mEnum, float value)
{
	switch (mEnum)
	{
	case CurrentSpeed:
		(*MovPtr)->CurrentSpeed = value;
		break;
	case ForwardSpeed:
		(*MovPtr)->ForwardSpeed = value;
		break;
	case LeftRightSpeed:
		(*MovPtr)->LeftRightSpeed = value;
		break;
	case BackwardSpeed:
		(*MovPtr)->BackwardSpeed = value;
		break;
	default:
		break;
	}
}

WalkingStatus GetMoveStatus()
{
	return (*MovPtr)->Status;
}

void SetMoveStatus(WalkingStatus status)
{
	(*MovPtr)->Status = status;
}

INT32 GetEntityID(EntityType type, INT32 index)
{

	switch (type)
	{
	case PCMob:
		return ((PCMobStruct*)(*MasterPtr->Player + (index * PCMobSize)))->EntityID;
	case Object:
		return ((ObjectStruct*)(*MasterPtr->NPCObject + (index * 0x200)))->EntityID;
	case NPC:
		return ((NPCStruct*)(*MasterPtr->NPC + (index*NPCSize)))->EntityID;
	default:
		break;
	}
	return NULL;
}

INT32 GetTargetEntityID()
{
	return MasterPtr->Target->EntityID;
}