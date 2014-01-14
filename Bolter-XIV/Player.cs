using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using Bolter_XIV;

namespace Player_Bits
{
    public static unsafe class Player
    {
        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }

        [Flags]
        public enum ProcessAccess
        {
            /// <summary>Specifies all possible access flags for the process object.</summary>
            AllAccess =
                CreateThread | DuplicateHandle | QueryInformation | SetInformation | Terminate | VMOperation | VMRead |
                VMWrite | Synchronize,

            /// <summary>Enables usage of the process handle in the CreateRemoteThread function to create a thread in the process.</summary>
            CreateThread = 0x2,

            /// <summary>
            ///     Enables usage of the process handle as either the source or target process in the DuplicateHandle function to
            ///     duplicate a handle.
            /// </summary>
            DuplicateHandle = 0x40,

            /// <summary>
            ///     Enables usage of the process handle in the GetExitCodeProcess and GetPriorityClass functions to read
            ///     information from the process object.
            /// </summary>
            QueryInformation = 0x400,

            /// <summary>Enables usage of the process handle in the SetPriorityClass function to set the priority class of the process.</summary>
            SetInformation = 0x200,

            /// <summary>Enables usage of the process handle in the TerminateProcess function to terminate the process.</summary>
            Terminate = 0x1,

            /// <summary>
            ///     Enables usage of the process handle in the VirtualProtectEx and WriteProcessMemory functions to modify the
            ///     virtual memory of the process.
            /// </summary>
            VMOperation = 0x8,

            /// <summary>
            ///     Enables usage of the process handle in the ReadProcessMemory function to' read from the virtual memory of the
            ///     process.
            /// </summary>
            VMRead = 0x10,

            /// <summary>
            ///     Enables usage of the process handle in the WriteProcessMemory function to write to the virtual memory of the
            ///     process.
            /// </summary>
            VMWrite = 0x20,

            /// <summary>Enables usage of the process handle in any of the wait functions to wait for the process to terminate.</summary>
            Synchronize = 0x100000
        }
        public static int HideBuffAddress;
        public static int LockSprintAddress;
        public static int ClientSideLock;
        public static int ServerSideLock;
        public static int BuffLoopAddress;
        private static readonly byte[] Xopcode = {0xF3, 0x0F, 0x11, 0x40, 0x30}; // movss [eax+34],xmm0
        private static readonly byte[] Zopcode = {0xF3, 0x0F, 0x11, 0x40, 0x34}; // movss [eax+34],xmm0
        private static readonly byte[] Yopcode = {0xF3, 0x0F, 0x11, 0x40, 0x38}; // movss [eax+34],xmm0
        private static readonly byte[] SXopcode = {0xF3, 0x0F, 0x11, 0x81, 0xA0, 0x00, 0x00, 0x00};
        private static readonly byte[] SZopcode = {0xF3, 0x0F, 0x11, 0x81, 0xA4, 0x00, 0x00, 0x00};
        private static readonly byte[] SYopcode = {0xF3, 0x0F, 0x11, 0x81, 0xA8, 0x00, 0x00, 0x00};

        //BuffRedirect
/*
        private static byte[] _redirectop = {0xEB, 0x27, 0x90, 0x90};
*/
        //
        //const int PLAYER_OFFSET = 0x10727B0;
        internal static IntPtr* BasePlayerAddress;

        private static readonly IList<int> PlayerBuffArray = new List<int>
        {
            0x2FF8,
            0x3004,
            0x3010,
            0x301C,
            0x3028,
            0x3034,
            0x3040,
            0x304C,
            0x3058,
            0x3064,
            0x3070,
            0x307C,
            0x3088,
            0x3094,
            0x30A0,
            0x30AC,
            0x30B8,
            0x30C4,
            0x30D0,
            0x30DC,
            0x30E8,
            0x30F4,
            0x3100,
            0x310C,
            0x3118,
            0x3124,
            0x3130,
            0x313C,
            0x3148,
            0x3154
        }.AsReadOnly();

        public static IntPtr EntryPoint;

        #region BuffIDs

        public static readonly byte[][] BuffIds =
        {
            new byte[] {0x01, 0x00}, new byte[] {0x02, 0x00}, new byte[] {0x03, 0x00}, new byte[] {0x04, 0x00},
            new byte[] {0x05, 0x00}, new byte[] {0x06, 0x00}, new byte[] {0x07, 0x00}, new byte[] {0x08, 0x00},
            new byte[] {0x09, 0x00}, new byte[] {0x0A, 0x00}, new byte[] {0x0B, 0x00}, new byte[] {0x0C, 0x00},
            new byte[] {0x0D, 0x00}, new byte[] {0x0E, 0x00}, new byte[] {0x0F, 0x00}, new byte[] {0x10, 0x00},
            new byte[] {0x11, 0x00}, new byte[] {0x12, 0x00}, new byte[] {0x13, 0x00}, new byte[] {0x14, 0x00},
            new byte[] {0x15, 0x00}, new byte[] {0x16, 0x00}, new byte[] {0x17, 0x00}, new byte[] {0x18, 0x00},
            new byte[] {0x19, 0x00}, new byte[] {0x1A, 0x00}, new byte[] {0x1B, 0x00}, new byte[] {0x1C, 0x00},
            new byte[] {0x1D, 0x00}, new byte[] {0x1E, 0x00}, new byte[] {0x1F, 0x00}, new byte[] {0x20, 0x00},
            new byte[] {0x21, 0x00}, new byte[] {0x22, 0x00}, new byte[] {0x23, 0x00}, new byte[] {0x24, 0x00},
            new byte[] {0x25, 0x00}, new byte[] {0x26, 0x00}, new byte[] {0x27, 0x00}, new byte[] {0x28, 0x00},
            new byte[] {0x29, 0x00}, new byte[] {0x2A, 0x00}, new byte[] {0x2B, 0x00}, new byte[] {0x2C, 0x00},
            new byte[] {0x2D, 0x00}, new byte[] {0x2E, 0x00}, new byte[] {0x2F, 0x00}, new byte[] {0x30, 0x00},
            new byte[] {0x31, 0x00}, new byte[] {0x32, 0x00}, new byte[] {0x33, 0x00}, new byte[] {0x34, 0x00},
            new byte[] {0x35, 0x00}, new byte[] {0x36, 0x00}, new byte[] {0x37, 0x00}, new byte[] {0x38, 0x00},
            new byte[] {0x39, 0x00}, new byte[] {0x3A, 0x00}, new byte[] {0x3B, 0x00}, new byte[] {0x3C, 0x00},
            new byte[] {0x3D, 0x00}, new byte[] {0x3E, 0x00}, new byte[] {0x3F, 0x00}, new byte[] {0x40, 0x00},
            new byte[] {0x41, 0x00}, new byte[] {0x42, 0x00}, new byte[] {0x43, 0x00}, new byte[] {0x44, 0x00},
            new byte[] {0x45, 0x00}, new byte[] {0x46, 0x00}, new byte[] {0x47, 0x00}, new byte[] {0x48, 0x00},
            new byte[] {0x49, 0x00}, new byte[] {0x4A, 0x00}, new byte[] {0x4B, 0x00}, new byte[] {0x4C, 0x00},
            new byte[] {0x4D, 0x00}, new byte[] {0x4E, 0x00}, new byte[] {0x4F, 0x00}, new byte[] {0x50, 0x00},
            new byte[] {0x51, 0x00}, new byte[] {0x52, 0x00}, new byte[] {0x53, 0x00}, new byte[] {0x54, 0x00},
            new byte[] {0x55, 0x00}, new byte[] {0x56, 0x00}, new byte[] {0x57, 0x00}, new byte[] {0x58, 0x00},
            new byte[] {0x59, 0x00}, new byte[] {0x5A, 0x00}, new byte[] {0x5B, 0x00}, new byte[] {0x5C, 0x00},
            new byte[] {0x5D, 0x00}, new byte[] {0x5E, 0x00}, new byte[] {0x5F, 0x00}, new byte[] {0x60, 0x00},
            new byte[] {0x61, 0x00}, new byte[] {0x62, 0x00}, new byte[] {0x63, 0x00}, new byte[] {0x64, 0x00},
            new byte[] {0x65, 0x00}, new byte[] {0x66, 0x00}, new byte[] {0x67, 0x00}, new byte[] {0x68, 0x00},
            new byte[] {0x69, 0x00}, new byte[] {0x6A, 0x00}, new byte[] {0x6B, 0x00}, new byte[] {0x6C, 0x00},
            new byte[] {0x6D, 0x00}, new byte[] {0x6E, 0x00}, new byte[] {0x6F, 0x00}, new byte[] {0x70, 0x00},
            new byte[] {0x71, 0x00}, new byte[] {0x72, 0x00}, new byte[] {0x73, 0x00}, new byte[] {0x74, 0x00},
            new byte[] {0x75, 0x00}, new byte[] {0x76, 0x00}, new byte[] {0x77, 0x00}, new byte[] {0x78, 0x00},
            new byte[] {0x79, 0x00}, new byte[] {0x7A, 0x00}, new byte[] {0x7B, 0x00}, new byte[] {0x7C, 0x00},
            new byte[] {0x7D, 0x00}, new byte[] {0x7E, 0x00}, new byte[] {0x7F, 0x00}, new byte[] {0x80, 0x00},
            new byte[] {0x81, 0x00}, new byte[] {0x82, 0x00}, new byte[] {0x83, 0x00}, new byte[] {0x84, 0x00},
            new byte[] {0x85, 0x00}, new byte[] {0x86, 0x00}, new byte[] {0x87, 0x00}, new byte[] {0x88, 0x00},
            new byte[] {0x89, 0x00}, new byte[] {0x8A, 0x00}, new byte[] {0x8B, 0x00}, new byte[] {0x8C, 0x00},
            new byte[] {0x8D, 0x00}, new byte[] {0x8E, 0x00}, new byte[] {0x8F, 0x00}, new byte[] {0x90, 0x00},
            new byte[] {0x91, 0x00}, new byte[] {0x92, 0x00}, new byte[] {0x93, 0x00}, new byte[] {0x94, 0x00},
            new byte[] {0x95, 0x00}, new byte[] {0x96, 0x00}, new byte[] {0x97, 0x00}, new byte[] {0x98, 0x00},
            new byte[] {0x99, 0x00}, new byte[] {0x9A, 0x00}, new byte[] {0x9B, 0x00}, new byte[] {0x9C, 0x00},
            new byte[] {0x9D, 0x00}, new byte[] {0x9E, 0x00}, new byte[] {0x9F, 0x00}, new byte[] {0xA0, 0x00},
            new byte[] {0xA1, 0x00}, new byte[] {0xA2, 0x00}, new byte[] {0xA3, 0x00}, new byte[] {0xA4, 0x00},
            new byte[] {0xA5, 0x00}, new byte[] {0xA6, 0x00}, new byte[] {0xA7, 0x00}, new byte[] {0xA8, 0x00},
            new byte[] {0xA9, 0x00}, new byte[] {0xAA, 0x00}, new byte[] {0xAB, 0x00}, new byte[] {0xAC, 0x00},
            new byte[] {0xAD, 0x00}, new byte[] {0xAE, 0x00}, new byte[] {0xAF, 0x00}, new byte[] {0xB0, 0x00},
            new byte[] {0xB1, 0x00}, new byte[] {0xB2, 0x00}, new byte[] {0xB3, 0x00}, new byte[] {0xB4, 0x00},
            new byte[] {0xB5, 0x00}, new byte[] {0xB6, 0x00}, new byte[] {0xB7, 0x00}, new byte[] {0xB8, 0x00},
            new byte[] {0xB9, 0x00}, new byte[] {0xBA, 0x00}, new byte[] {0xBB, 0x00}, new byte[] {0xBC, 0x00},
            new byte[] {0xBD, 0x00}, new byte[] {0xBE, 0x00}, new byte[] {0xBF, 0x00}, new byte[] {0xC0, 0x00},
            new byte[] {0xC1, 0x00}, new byte[] {0xC2, 0x00}, new byte[] {0xC3, 0x00}, new byte[] {0xC4, 0x00},
            new byte[] {0xC5, 0x00}, new byte[] {0xC6, 0x00}, new byte[] {0xC7, 0x00}, new byte[] {0xC8, 0x00},
            new byte[] {0xC9, 0x00}, new byte[] {0xCA, 0x00}, new byte[] {0xCB, 0x00}, new byte[] {0xCC, 0x00},
            new byte[] {0xCD, 0x00}, new byte[] {0xCE, 0x00}, new byte[] {0xCF, 0x00}, new byte[] {0xD0, 0x00},
            new byte[] {0xD1, 0x00}, new byte[] {0xD2, 0x00}, new byte[] {0xD3, 0x00}, new byte[] {0xD4, 0x00},
            new byte[] {0xD5, 0x00}, new byte[] {0xD6, 0x00}, new byte[] {0xD7, 0x00}, new byte[] {0xD8, 0x00},
            new byte[] {0xD9, 0x00}, new byte[] {0xDA, 0x00}, new byte[] {0xDB, 0x00}, new byte[] {0xDC, 0x00},
            new byte[] {0xDD, 0x00}, new byte[] {0xDE, 0x00}, new byte[] {0xDF, 0x00}, new byte[] {0xE0, 0x00},
            new byte[] {0xE1, 0x00}, new byte[] {0xE2, 0x00}, new byte[] {0xE3, 0x00}, new byte[] {0xE4, 0x00},
            new byte[] {0xE5, 0x00}, new byte[] {0xE6, 0x00}, new byte[] {0xE7, 0x00}, new byte[] {0xE8, 0x00},
            new byte[] {0xE9, 0x00}, new byte[] {0xEA, 0x00}, new byte[] {0xEB, 0x00}, new byte[] {0xEC, 0x00},
            new byte[] {0xED, 0x00}, new byte[] {0xEE, 0x00}, new byte[] {0xEF, 0x00}, new byte[] {0xF0, 0x00},
            new byte[] {0xF1, 0x00}, new byte[] {0xF2, 0x00}, new byte[] {0xF3, 0x00}, new byte[] {0xF4, 0x00},
            new byte[] {0xF5, 0x00}, new byte[] {0xF6, 0x00}, new byte[] {0xF7, 0x00}, new byte[] {0xF8, 0x00},
            new byte[] {0xF9, 0x00}, new byte[] {0xFA, 0x00}, new byte[] {0xFB, 0x00}, new byte[] {0xFC, 0x00},
            new byte[] {0xFD, 0x00}, new byte[] {0xFE, 0x00}, new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x01},
            new byte[] {0x01, 0x01}, new byte[] {0x02, 0x01}, new byte[] {0x03, 0x01}, new byte[] {0x04, 0x01},
            new byte[] {0x05, 0x01}, new byte[] {0x06, 0x01}, new byte[] {0x07, 0x01}, new byte[] {0x08, 0x01},
            new byte[] {0x09, 0x01}, new byte[] {0x0A, 0x01}, new byte[] {0x0B, 0x01}, new byte[] {0x0C, 0x01},
            new byte[] {0x0D, 0x01}, new byte[] {0x0E, 0x01}, new byte[] {0x0F, 0x01}, new byte[] {0x10, 0x01},
            new byte[] {0x11, 0x01}, new byte[] {0x12, 0x01}, new byte[] {0x13, 0x01}, new byte[] {0x14, 0x01},
            new byte[] {0x15, 0x01}, new byte[] {0x16, 0x01}, new byte[] {0x17, 0x01}, new byte[] {0x18, 0x01},
            new byte[] {0x19, 0x01}, new byte[] {0x1A, 0x01}, new byte[] {0x1B, 0x01}, new byte[] {0x1C, 0x01},
            new byte[] {0x1D, 0x01}, new byte[] {0x1E, 0x01}, new byte[] {0x1F, 0x01}, new byte[] {0x20, 0x01},
            new byte[] {0x21, 0x01}, new byte[] {0x22, 0x01}, new byte[] {0x23, 0x01}, new byte[] {0x24, 0x01},
            new byte[] {0x25, 0x01}, new byte[] {0x26, 0x01}, new byte[] {0x27, 0x01}, new byte[] {0x28, 0x01},
            new byte[] {0x29, 0x01}, new byte[] {0x2A, 0x01}, new byte[] {0x2B, 0x01}, new byte[] {0x2C, 0x01},
            new byte[] {0x2D, 0x01}, new byte[] {0x2E, 0x01}, new byte[] {0x2F, 0x01}, new byte[] {0x30, 0x01},
            new byte[] {0x31, 0x01}, new byte[] {0x32, 0x01}, new byte[] {0x33, 0x01}, new byte[] {0x34, 0x01},
            new byte[] {0x35, 0x01}, new byte[] {0x36, 0x01}, new byte[] {0x37, 0x01}, new byte[] {0x38, 0x01},
            new byte[] {0x39, 0x01}, new byte[] {0x3A, 0x01}, new byte[] {0x3B, 0x01}, new byte[] {0x3C, 0x01},
            new byte[] {0x3D, 0x01}, new byte[] {0x3E, 0x01}, new byte[] {0x3F, 0x01}, new byte[] {0x40, 0x01},
            new byte[] {0x41, 0x01}, new byte[] {0x42, 0x01}, new byte[] {0x43, 0x01}, new byte[] {0x44, 0x01},
            new byte[] {0x45, 0x01}, new byte[] {0x46, 0x01}, new byte[] {0x47, 0x01}, new byte[] {0x48, 0x01},
            new byte[] {0x49, 0x01}, new byte[] {0x4A, 0x01}, new byte[] {0x4B, 0x01}, new byte[] {0x4C, 0x01},
            new byte[] {0x4D, 0x01}, new byte[] {0x4E, 0x01}, new byte[] {0x4F, 0x01}, new byte[] {0x50, 0x01},
            new byte[] {0x51, 0x01}, new byte[] {0x52, 0x01}, new byte[] {0x53, 0x01}, new byte[] {0x54, 0x01},
            new byte[] {0x55, 0x01}, new byte[] {0x56, 0x01}, new byte[] {0x57, 0x01}, new byte[] {0x58, 0x01},
            new byte[] {0x59, 0x01}, new byte[] {0x5A, 0x01}, new byte[] {0x5B, 0x01}, new byte[] {0x5C, 0x01},
            new byte[] {0x5D, 0x01}, new byte[] {0x5E, 0x01}, new byte[] {0x5F, 0x01}, new byte[] {0x60, 0x01},
            new byte[] {0x61, 0x01}, new byte[] {0x62, 0x01}, new byte[] {0x63, 0x01}, new byte[] {0x64, 0x01},
            new byte[] {0x65, 0x01}, new byte[] {0x66, 0x01}, new byte[] {0x67, 0x01}, new byte[] {0x68, 0x01},
            new byte[] {0x69, 0x01}, new byte[] {0x6A, 0x01}, new byte[] {0x6B, 0x01}, new byte[] {0x6C, 0x01},
            new byte[] {0x6D, 0x01}, new byte[] {0x6E, 0x01}, new byte[] {0x6F, 0x01}, new byte[] {0x70, 0x01}
        };

        #endregion

        #region BuffNames

        public static readonly string[] BuffNames =
        {
            "",
            "Petrification",
            "Stun",
            "Sleep",
            "Daze",
            "Amnesia",
            "Pacification",
            "Silence",
            "Haste",
            "Slow",
            "Slow",
            "0",
            "0",
            "Bind",
            "Heavy",
            "Blind",
            "0",
            "Paralysis",
            "Poison",
            "Pollen",
            "TP Bleed",
            "HP Boost",
            "HP Penalty",
            "MP Boost",
            "MP Penalty",
            "Attack Up",
            "Attack Down",
            "Accuracy Up",
            "Accuracy Down",
            "Defense Up",
            "Defense Down",
            "Evasion Up",
            "Evasion Down",
            "Magic Potency Up",
            "Magic Potency Down",
            "Healing Potency Up",
            "Healing Potency Down",
            "Magic Defense Up",
            "Magic Defense Down",
            "0",
            "0",
            "0",
            "0",
            "Weakness",
            "Brink of Death",
            "Crafter's Grace",
            "Gatherer's Grace",
            "Stealth",
            "Food Benefits",
            "0",
            "Sprint",
            "Strength Down",
            "Vitality Down",
            "Physical Damage Up",
            "Physical Damage Down",
            "Physical Vulnerability Down",
            "Physical Vulnerability Up",
            "Magic Damage Up",
            "Magic Damage Down",
            "Magic Vulnerability Down",
            "Magic Vulnerability Up",
            "Determination Up",
            "Determination Down",
            "Vulnerability Down",
            "Vulnerability Up",
            "Critical Skill",
            "Terror",
            "Leaden",
            "Drainstrikes",
            "Aspirstrikes",
            "Stunstrikes",
            "Rampart",
            "Convalescence",
            "Awareness",
            "Sentinel",
            "Tempered Will",
            "Fight or Flight",
            "Bulwark ",
            "Sword Oath",
            "Shield Oath",
            "Cover",
            "Covered",
            "Hallowed Ground",
            "Foresight",
            "Bloodbath",
            "Maim",
            "Berserk",
            "Thrill of Battle",
            "Holmgang",
            "Vengeance",
            "Storm's Eye",
            "Defiance",
            "Unchained",
            "Wrath",
            "Wrath II",
            "Wrath III",
            "Wrath IV",
            "Infuriated",
            "Dragon Kick",
            "Featherfoot",
            "Internal Release",
            "Twin Snakes",
            "Mantra",
            "Fists of Fire",
            "Fists of Earth",
            "Fists of Wind",
            "Touch of Death",
            "Opo-opo Form",
            "Raptor Form",
            "Coeurl Form",
            "Perfect Balance",
            "Greased Lightning",
            "Greased Lightniing II",
            "Greased Lightning III",
            "Keen Flurry",
            "Heavy Thrust",
            "Life Surge",
            "Blood for Blood",
            "Chaos Thrust",
            "Phlebotomize",
            "Power Surge",
            "Disembowel",
            "Straighter Shot",
            "Hawk's Eye",
            "Venomous Bite",
            "Raging Strikes",
            "Freeshot",
            "Quelling Strike",
            "Barrage",
            "Windbite",
            "Straight Shot",
            "Downpour of Death",
            "Quicker Nock",
            "Swiftsong",
            "Swiftsong",
            "Mage's Ballad",
            "Mage's Ballad",
            "Army's Paeon",
            "Army's Paeon",
            "Foe Requiem",
            "Foe Requiem",
            "Battle Voice",
            "Chameleon",
            "Aero",
            "Aero II",
            "Cleric Stance",
            "Protect",
            "Protect",
            "Raise",
            "Rebirth",
            "Medica II",
            "Stoneskin",
            "Some Japanese buff",
            "Some Japanese buff",
            "Shroud of Saint",
            "Freecure",
            "Overcure",
            "Presence of Min",
            "Regen",
            "Divine Seal",
            "Surecast",
            "Thunder",
            "Thunder II",
            "Thunder III",
            "Thundercloud",
            "Firestarter",
            "Succor",
            "Swiftcast",
            "Manaward",
            "Manawall",
            "Apocatastasis",
            "Ekpyrosis",
            "Infirmity",
            "Astral Fire",
            "Astral Fire II",
            "Astral Fire III",
            "Umbral Ice",
            "Umbral Ice II",
            "Umbral Ice III",
            "Bio",
            "Miasma",
            "Disease",
            "Virus",
            "Fever",
            "Sustain",
            "Eye for an Eye",
            "Eye for an Eye II",
            "Rouse",
            "Miasma II",
            "Bio II",
            "Shadow Flare",
            "Tri-disaster",
            "Spur",
            "Slow",
            "Shield Wall",
            "Mighty Guard",
            "Last Bastion",
            "Blaze Spikes",
            "Ice Spikes",
            "Shock Spikes",
            "Physical Vulner",
            "Stun",
            "Vulnerability Up",
            "Boost",
            "Enfire",
            "Enblizzard",
            "Enaero",
            "Enstone",
            "Enthunder",
            "Enwater",
            "Doom",
            "Sharpened Knife",
            "True Sight",
            "Pacification",
            "Agitation",
            "Determination Down",
            "Paralysis",
            "Triangulate",
            "Gathering Rate Up",
            "Gathering Yield Up",
            "Gathering Fortune Up",
            "Truth of Forests",
            "Truth of Mountains",
            "Byregot's Ward",
            "Nophica's Ward",
            "Prospect",
            "Haste",
            "0",
            "Menphina's Ward",
            "Nald'thal's Ward",
            "Llymlaen's Ward",
            "Thaliak's Ward",
            "Preparation",
            "Arbor Call",
            "Lay of the Land",
            "0",
            "Choco Beak",
            "Choco Regen",
            "Choco Surge",
            "The Echo",
            "0",
            "Blessing of Light",
            "Arbor Call II",
            "Lay of the Land II",
            "Fracture",
            "Sanction",
            "Demolish",
            "Rain of Death",
            "Circle of Scorn",
            "Flaming Arrow",
            "Burns",
            "Inner Quiet",
            "Waste Not",
            "Steady Hand",
            "Great Strides",
            "Ingenuity",
            "Ingenuity II",
            "Waste Not II",
            "Manipulation",
            "Innovation",
            "Reclaim",
            "Comfort Zone",
            "Steady Hand II",
            "Vulnerability Down",
            "Flesh Wound",
            "Stab Wound",
            "Concussion",
            "Burns",
            "Frostbite",
            "Windburn",
            "Sludge",
            "Electrocution",
            "Dropsy",
            "Bleeding",
            "Recuperation",
            "Poison +",
            "Voice of Valor",
            "Some Japanese buff",
            "0",
            "Rehabilitation",
            "Bind",
            "Physical Damage Down",
            "Mana Modulation",
            "Dropsy",
            "Burns",
            "Frostbite",
            "Windburn",
            "Sludge",
            "Electrocution",
            "Dropsy",
            "Determination Up",
            "Hundred Fists",
            "Fetters",
            "Skill Speed Up",
            "Spell Speed Up",
            "Goldbile",
            "Hysteria",
            "Adloquium",
            "Sacred Soil",
            "Sacred Soil",
            "Determination Up",
            "Critical Strike",
            "Gold Lung",
            "Burrs",
            "Aetherflow",
            "The Dragon's Curse",
            "Inner Dragon",
            "Voice of Valor",
            "Some Japanese buff",
            "0",
            "Curl",
            "Earthen Ward",
            "Earthen Fury",
            "Radiant Shield",
            "Inferno",
            "Whispering Dawn",
            "Fey Covenant",
            "Fey Illumination",
            "Fey Glow",
            "Fey Light",
            "Bleeding",
            "Gungnir",
            "Crystal Veil",
            "Reduced Immunity",
            "Greenwrath",
            "Invincibility",
            "Lightning Charg",
            "Ice Charge",
            "Heart of the Moon",
            "Modification",
            "Haste",
            "Magic Vulnerabi",
            "Physical Damage",
            "Allagan Rot",
            "Allagan Immunit",
            "Firestream",
            "Sequence ",
            "Sequence AP",
            "Sequence AS",
            "Bleeding",
            "Physical Field",
            "Aetherial Field",
            "Repelling Spray",
            "Bleeding",
            "Neurolink",
            "Recharge",
            "Waxen Flesh",
            "Pox",
            "Disseminate",
            "Steel Scales",
            "Vulnerability Down",
            "Rancor",
            "Spjot",
            "Brave New World",
            "Live off the Land",
            "What You See",
            "Eat from the Hand",
            "In Control",
            "0",
            "0",
            "Meat and Mead",
            "That Which Binds Us",
            "Proper Care",
            "Back on Your Feet",
            "Reduced Rates",
            "The Heat of Battle",
            "A Man's Best Friend",
            "Earth and Water",
            "Helping Hand"
        };

        #endregion

        public enum Protection
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        public static int MovementAddress;

        [DllImport("kernel32.dll")]
        private static extern int VirtualQuery(
            IntPtr lpAddress,
            ref MEMORY_BASIC_INFORMATION lpBuffer,
            IntPtr dwLength
            );

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize,
            AllocationType flAllocationType, MemoryProtection flProtect);

        [Flags()]
        public enum AllocationType : uint
        {
            COMMIT = 0x1000,
            RESERVE = 0x2000,
            RESET = 0x80000,
            LARGE_PAGES = 0x20000000,
            PHYSICAL = 0x400000,
            TOP_DOWN = 0x100000,
            WRITE_WATCH = 0x200000
        }

        [Flags()]
        public enum MemoryProtection : uint
        {
            EXECUTE = 0x10,
            EXECUTE_READ = 0x20,
            EXECUTE_READWRITE = 0x40,
            EXECUTE_WRITECOPY = 0x80,
            NOACCESS = 0x01,
            READONLY = 0x02,
            READWRITE = 0x04,
            WRITECOPY = 0x08,
            GUARD_Modifierflag = 0x100,
            NOCACHE_Modifierflag = 0x200,
            WRITECOMBINE_Modifierflag = 0x400
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateRemoteThread(IntPtr hProcess, int lpThreadAttributes = 0, int dwStackSize = 0, IntPtr lpStartAddress = default(IntPtr), uint lpParameter = 0, int dwCreationFlags = 0, int lpThreadId = 0);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
            Protection flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
            int dwSize, FreeType dwFreeType);

        //Function for returning Players address in memory (Server Side)
        private static PlayerStructure* GetPlayer()
        {
            return (PlayerStructure*) *BasePlayerAddress;
        }

        private static SubPlayerStruct* GetPlayerSub()
        {
            return GetPlayer()->subStruct;
        }

        public static void LockAxis(string axis, bool lockit)
        {
            uint oldProtect;
            byte[] asm = { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
            IntPtr mainEntryPoint = Process.GetCurrentProcess().MainModule.BaseAddress + ClientSideLock;
            IntPtr subEntryPoint = Process.GetCurrentProcess().MainModule.BaseAddress + ServerSideLock;
            VirtualProtect(mainEntryPoint, (uint)asm.Length - 0x14 - 3, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            VirtualProtect(subEntryPoint, (uint)asm.Length + 0x1A, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            if (lockit)
            {
                if (axis == "X")
                {
                    Marshal.Copy(asm, 3, mainEntryPoint, asm.Length - 3);
                    Marshal.Copy(asm, 0, subEntryPoint, asm.Length);
                }
                else if (axis == "Y")
                {
                    Marshal.Copy(asm, 3, mainEntryPoint + 0x14, asm.Length - 3);
                    Marshal.Copy(asm, 0, subEntryPoint + 0x1A, asm.Length);
                }
                else if (axis == "Z")
                {
                    Marshal.Copy(asm, 3, mainEntryPoint + 0xA, asm.Length - 3);
                    Marshal.Copy(asm, 0, subEntryPoint + 0xD, asm.Length);
                }
            }
            else
            {
                if (axis == "X")
                {
                    Marshal.Copy(Xopcode, 0, mainEntryPoint, Xopcode.Length);
                    Marshal.Copy(SXopcode, 0, subEntryPoint, SXopcode.Length);
                }
                else if (axis == "Y")
                {
                    Marshal.Copy(Yopcode, 0, mainEntryPoint + 0x14, Yopcode.Length);
                    Marshal.Copy(SYopcode, 0, subEntryPoint + 0x1A, SYopcode.Length);
                }
                else if (axis == "Z")
                {
                    Marshal.Copy(Zopcode, 0, mainEntryPoint + 0xA, Zopcode.Length);
                    Marshal.Copy(SZopcode, 0, subEntryPoint + 0xD, SZopcode.Length);
                }
            }
        }

        public static string PointerToString(byte* unmanagedBytes, int arraySize)
        {
            var theBytes = new byte[arraySize];
            Marshal.Copy(new IntPtr(unmanagedBytes), theBytes, 0, arraySize);
            return Encoding.ASCII.GetString(theBytes);
        }

        public static float GetPos(string axis)
        {
            if (axis == "X")
            {
                return GetPlayer()->ServerX;
            }
            if (axis == "Y")
            {
                return GetPlayer()->ServerY;
            }
            if (axis == "Z")
            {
                return GetPlayer()->ServerZ;
            }
            return 0;
        }

        public static void WriteToPos(string axis, float value)
        {
            if (axis == "X")
            {
                GetPlayer()->ServerX = value;
                GetPlayerSub()->CliX = value;
            }
            if (axis == "Y")
            {
                GetPlayer()->ServerY = value;
                GetPlayerSub()->CliY = value;
            }
            if (axis == "Z")
            {
                GetPlayer()->ServerZ = value;
                GetPlayerSub()->CliZ = value;
            }
        }

        public static void AddToPos(string axis, float value, bool add)
        {
            if (add)
            {
                if (axis == "X")
                {
                    GetPlayer()->ServerX = GetPlayer()->ServerX + value;
                    GetPlayerSub()->CliX = GetPlayerSub()->CliX + value;
                }
                else if (axis == "Y")
                {
                    GetPlayer()->ServerY = GetPlayer()->ServerY + value;
                    GetPlayerSub()->CliY = GetPlayerSub()->CliY + value;
                }
                else if (axis == "Z")
                {
                    GetPlayer()->ServerZ = GetPlayer()->ServerZ + value;
                    GetPlayerSub()->CliZ = GetPlayerSub()->CliZ + value;
                }
            }
            else
            {
                if (axis == "X")
                {
                    GetPlayer()->ServerX = GetPlayer()->ServerX - value;
                    GetPlayerSub()->CliX = GetPlayerSub()->CliX - value;
                }
                else if (axis == "Y")
                {
                    GetPlayer()->ServerY = GetPlayer()->ServerY - value;
                    GetPlayerSub()->CliY = GetPlayerSub()->CliY - value;
                }
                else if (axis == "Z")
                {
                    GetPlayer()->ServerZ = GetPlayer()->ServerZ - value;
                    GetPlayerSub()->CliZ = GetPlayerSub()->CliZ - value;
                }
            }
        }

        public static void UpdateSpeed()
        {
            LockLastBuff(true);
            //Loop while Slider is > 0
            while (MainWindow.SetSpeed[0] == 0x32)
            {
                try
                {
                    if (GetPlayer()->Buff_30_ID == BitConverter.ToUInt16(MainWindow.SetSpeed, 0) &&
                        GetPlayer()->Buff_30_Paras == BitConverter.ToUInt16(MainWindow.SetSpeed, 2))
                    {
                        //Do Nothing
                    }
                    else
                    {
                        GetPlayer()->Buff_30_ID = BitConverter.ToUInt16(MainWindow.SetSpeed, 0);
                        GetPlayer()->Buff_30_Paras = BitConverter.ToUInt16(MainWindow.SetSpeed, 2);
                    }
                }
                catch
                {
                }
                Thread.Sleep(1000);
            }
            //if slider value = 0 end thread and remove sprint.
            GetPlayer()->Buff_30_ID = BitConverter.ToUInt16(MainWindow.SetSpeed, 0);
            GetPlayer()->Buff_30_Paras = BitConverter.ToUInt16(MainWindow.SetSpeed, 2);
            LockLastBuff(false);
        }

        private static void LockLastBuff(bool lockit)
        {
            uint oldProtect;
            byte[] asm = { 0x83, 0xF8, 0x1D };
            IntPtr entryPoint = Process.GetCurrentProcess().MainModule.BaseAddress + LockSprintAddress;
            VirtualProtect(entryPoint, (uint) asm.Length, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            if (lockit)
                asm[2] = 0x1D;
            else
                asm[2] = 0x1E;
            Marshal.Copy(asm, 0, entryPoint, asm.Length);
        }

        public static void HideSprint(bool hide)
        {
            uint oldProtect;
            byte[] asm = {0x83, 0xF8, 0x1D};
            IntPtr entryPoint = Process.GetCurrentProcess().MainModule.BaseAddress + HideBuffAddress;
            VirtualProtect(entryPoint, (uint) asm.Length*2 + 0x20, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            if (hide)
                asm[2] = 0x1D;
            else
                asm[2] = 0x1E;
            Marshal.Copy(asm, 0, entryPoint, asm.Length);
            Marshal.Copy(asm, 0, entryPoint + 0x20, asm.Length);
        }

        //Curently in devolopment "stuff" below


        public static void RedirectBuffOp()
        {
            try
            {
                EntryPoint = VirtualAlloc((IntPtr) null, (UIntPtr)64, AllocationType.COMMIT | AllocationType.RESERVE, MemoryProtection.EXECUTE_READWRITE);
                byte[] PA = BitConverter.GetBytes((int) BasePlayerAddress); //Player address bytes
                byte[] FP = BitConverter.GetBytes((int) EntryPoint + 0x1E); //Pointer to intercepted function
                byte[] EP = BitConverter.GetBytes((int) EntryPoint); //Pointer to EntryPoint
                byte[] FA =
                    BitConverter.GetBytes(
                        (int)Process.GetCurrentProcess().MainModule.BaseAddress +
                        (BuffLoopAddress + 0x07)); //Address to Function that exposes PlayerAddress
                byte[] ASM =
                {
                    0x8B, 0x33, // mov esi,[ebx] //EntryPoint
                    0x8B, 0x01, // mov eax,[ecx] //Entrypoint +0x2
                    0x8B, 0x50, 0x04, // mov edx,[eax,+04] //EntryPoint +0x4
                    0x66, 0x81, 0xFF, 0xB0, 0x27, // cmp di,27B0 //EntryPoint +0x7
                    0x75, 0x06, // jne 6 bytes //EntryPoint +0xC
                    0x89, 0x35, PA[0], PA[1], PA[2], PA[3], // mov [playeraddress],esi //EntryPoint +0xE
                    0xFF, 0x25, FP[0], FP[1], FP[2], FP[3], // jmp dword ptr [PointerToFunction] //EntryPoint +0x14
                    EP[0], EP[1], EP[2], EP[3], //Store Root Address here //EntryPoint+0x1A
                    FA[0], FA[1], FA[2], FA[3] //Store Buff function address //EntryPoint+0x1E
                };
                Marshal.Copy(ASM,0,EntryPoint,ASM.Length);
                //Inject redirect routine
                ASM = new byte[] {0xFF, 0x25}.Concat(BitConverter.GetBytes((int) EntryPoint + 0x1A))
                        .ToArray()
                        .Concat(new byte[] {0x90})
                        .ToArray();
                VirtualProtect(Process.GetCurrentProcess().MainModule.BaseAddress + BuffLoopAddress, (uint)ASM.Length,
                    Protection.PAGE_EXECUTE_READWRITE, out new uint[] {0}[0]);
                Marshal.Copy(ASM,0,Process.GetCurrentProcess().MainModule.BaseAddress + BuffLoopAddress,ASM.Length);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void UndoRedirectBuffOp()
        {
            try
            {
                byte[] ASM =
                {
                    0x8B, 0x33, // mov esi,[ebx] 
                    0x8B, 0x01, // mov eax,[ecx] 
                    0x8B, 0x50, 0x04, // mov edx,[eax,+04]
                };
                Marshal.Copy(ASM,0,Process.GetCurrentProcess().MainModule.BaseAddress + BuffLoopAddress,ASM.Length);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }




        public static short DebugR(int buffindex)
        {
            return (short) GetPlayer()->Buff_30_ID;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PlayerStructure
        {
            private fixed byte Unknown [0x30]; //Unknown 0x30 bytes
            public fixed byte Name [0x18];
            private fixed byte Unknown2 [0x58]; //Unknown 0x58 bytes
            public float ServerX; //Server Side X cord
            public float ServerZ; //Server Side Z cord
            public float ServerY; //Server Side Y cord
            private float Unknown3; //Unknown float
            public float Heading; //Server Side Heading
            public float ServerHight; //Cam hight?
            private float Unknown4; //Unknown float
            private float Unknown5; //Unknown float
            private float Unknown6; //Unknown float
            private fixed byte Unknown7 [0x28]; //Unknown 0x28 bytes
            public SubPlayerStruct* subStruct;
            private fixed byte Unknown8 [0x44]; //Unknown 0x44 bytes
            public float CamGlide;
            private fixed byte Unknown9 [0x3C]; //Unknown 0x3C bytes
            public float StaticCamGlide;
            private fixed byte Unknown10 [0x10]; //Unknown 0x10 bytes
            public uint StatusAdjust; //Has something to do with Player status. setting to 2 gives "Return" prompt
            private fixed byte Unknown11 [0x44]; //Unknown 0x60 bytes
            public float dynamicXCord; //Only updates when you are moving
            public float dynamicZCord; //Only updates when you are moving
            public float dynamicYCord; //Only updates when you are moving
            private uint Unknown12; // Unkown value
            public bool IsMoving;
            public float dynamicHeading; //Only updates when you change heading
            public float dynamicHeading2; //Only updates when you change heading
            private uint Unknown13; // Unkown value
            public float SetMoveLock; //Always -1, setting to < -1 incresses speed but moves player backwards.
            public float SetMoveLock2; //Always -1, setting to 0 locks player in place.
            public float SetMoveLock3; //Always -1, setting to 0 locks player in place.
            public float SetMoveLock4; //Always -1, setting to 0 locks player in place.
            public float SetMoveLock5; //Always -1, setting to 0 locks player in place.
            private fixed byte Unknown14 [0x10]; //Unknown 0x10 bytes
            public bool IsMoving2;
            private fixed byte Unknown15 [0x18]; //Unknown 0x18 bytes
            public float TimeTraveled; //Stores ammount of secounds travled since you last started moving.
            private fixed byte Unknown16 [0x2D1C]; //Unknown 0x2D1C bytes
            public uint someCounter; // just keeps counting
            private fixed byte Unknown17 [0xA4]; //Unknown 0x2D1C bytes
            public UInt16 Buff_1_ID;
            public UInt16 Buff_1_Paras;
            public float Buff_1_Timmer;
            public uint Buff_1_ExtraInfo;
            public UInt16 Buff_2_ID;
            public UInt16 Buff_2_Paras;
            public float Buff_2_Timmer;
            public uint Buff_2_ExtraInfo;
            public UInt16 Buff_3_ID;
            public UInt16 Buff_3_Paras;
            public float Buff_3_Timmer;
            public uint Buff_3_ExtraInfo;
            public UInt16 Buff_4_ID;
            public UInt16 Buff_4_Paras;
            public float Buff_4_Timmer;
            public uint Buff_4_ExtraInfo;
            public UInt16 Buff_5_ID;
            public UInt16 Buff_5_Paras;
            public float Buff_5_Timmer;
            public uint Buff_5_ExtraInfo;
            public UInt16 Buff_6_ID;
            public UInt16 Buff_6_Paras;
            public float Buff_6_Timmer;
            public uint Buff_6_ExtraInfo;
            public UInt16 Buff_7_ID;
            public UInt16 Buff_7_Paras;
            public float Buff_7_Timmer;
            public uint Buff_7_ExtraInfo;
            public UInt16 Buff_8_ID;
            public UInt16 Buff_8_Paras;
            public float Buff_8_Timmer;
            public uint Buff_8_ExtraInfo;
            public UInt16 Buff_9_ID;
            public UInt16 Buff_9_Paras;
            public float Buff_9_Timmer;
            public uint Buff_9_ExtraInfo;
            public UInt16 Buff_10_ID;
            public UInt16 Buff_10_Paras;
            public float Buff_10_Timmer;
            public uint Buff_10_ExtraInfo;
            public UInt16 Buff_11_ID;
            public UInt16 Buff_11_Paras;
            public float Buff_11_Timmer;
            public uint Buff_11_ExtraInfo;
            public UInt16 Buff_12_ID;
            public UInt16 Buff_12_Paras;
            public float Buff_12_Timmer;
            public uint Buff_12_ExtraInfo;
            public UInt16 Buff_13_ID;
            public UInt16 Buff_13_Paras;
            public float Buff_13_Timmer;
            public uint Buff_13_ExtraInfo;
            public UInt16 Buff_14_ID;
            public UInt16 Buff_14_Paras;
            public float Buff_14_Timmer;
            public uint Buff_14_ExtraInfo;
            public UInt16 Buff_15_ID;
            public UInt16 Buff_15_Paras;
            public float Buff_15_Timmer;
            public uint Buff_15_ExtraInfo;
            public UInt16 Buff_16_ID;
            public UInt16 Buff_16_Paras;
            public float Buff_16_Timmer;
            public uint Buff_16_ExtraInfo;
            public UInt16 Buff_17_ID;
            public UInt16 Buff_17_Paras;
            public float Buff_17_Timmer;
            public uint Buff_17_ExtraInfo;
            public UInt16 Buff_18_ID;
            public UInt16 Buff_18_Paras;
            public float Buff_18_Timmer;
            public uint Buff_18_ExtraInfo;
            public UInt16 Buff_19_ID;
            public UInt16 Buff_19_Paras;
            public float Buff_19_Timmer;
            public uint Buff_19_ExtraInfo;
            public UInt16 Buff_20_ID;
            public UInt16 Buff_20_Paras;
            public float Buff_20_Timmer;
            public uint Buff_20_ExtraInfo;
            public UInt16 Buff_21_ID;
            public UInt16 Buff_21_Paras;
            public float Buff_21_Timmer;
            public uint Buff_21_ExtraInfo;
            public UInt16 Buff_22_ID;
            public UInt16 Buff_22_Paras;
            public float Buff_22_Timmer;
            public uint Buff_22_ExtraInfo;
            public UInt16 Buff_23_ID;
            public UInt16 Buff_23_Paras;
            public float Buff_23_Timmer;
            public uint Buff_23_ExtraInfo;
            public UInt16 Buff_24_ID;
            public UInt16 Buff_24_Paras;
            public float Buff_24_Timmer;
            public uint Buff_24_ExtraInfo;
            public UInt16 Buff_25_ID;
            public UInt16 Buff_25_Paras;
            public float Buff_25_Timmer;
            public uint Buff_25_ExtraInfo;
            public UInt16 Buff_26_ID;
            public UInt16 Buff_26_Paras;
            public float Buff_26_Timmer;
            public uint Buff_26_ExtraInfo;
            public UInt16 Buff_27_ID;
            public UInt16 Buff_27_Paras;
            public float Buff_27_Timmer;
            public uint Buff_27_ExtraInfo;
            public UInt16 Buff_28_ID;
            public UInt16 Buff_28_Paras;
            public float Buff_28_Timmer;
            public uint Buff_28_ExtraInfo;
            public UInt16 Buff_29_ID;
            public UInt16 Buff_29_Paras;
            public float Buff_29_Timmer;
            public uint Buff_29_ExtraInfo;
            public UInt16 Buff_30_ID;
            public UInt16 Buff_30_Paras;
            public float Buff_30_Timmer;
            public uint Buff_30_ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SubPlayerStruct
        {
            private fixed byte Unknown [0x30]; //Unknown 0x2C bytes
            public float CliX;
            public float CliZ;
            public float CliY;
            private fixed byte Unknown2 [8];
            public float CliHeading;
            private uint Unknown3;
            public float RadiusFromN;
            public float PlayerWidth;
            public float PlayerHieght;
            public float PlayerGirth;
            private fixed byte Unknown4 [18]; //Unknown 0x2C bytes

            /// <summary>
            /// Values 1-16 Control each body part.
            /// </summary>
            public uint DisplayedBody;

            private fixed byte Unknown5 [0xA8]; //Unknown 0xA8 bytes
            public float CliX2;
            public float CliZ2;
            public float CliY2;
            private fixed byte Unknown6 [0x188]; //Unknown 0xA8 bytes
            public float PlayerSizeNoCam;
            public float PlayerSize;
        }

    }
}