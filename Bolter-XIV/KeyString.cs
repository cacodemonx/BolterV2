using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using UnManaged;

namespace Key_Strings
{
    static class KeyString
    {
        static public KeyModifier Key_Mods(string ID)
        {
            if (ID == "Alt")
            {
                return KeyModifier.Alt;
            }
            else if (ID == "Ctrl")
            {
                return KeyModifier.Ctrl;
            }
            else if (ID == "None")
            {
                return KeyModifier.None;
            }
            else if (ID == "Shift")
            {
                return KeyModifier.Shift;
            }
            else if (ID == "Win")
            {
                return KeyModifier.Win;
            }
            return KeyModifier.None;
        }
        static public Key Keys_M(string ID)
        {
            if (ID == "Key.A")
            {
                return Key.A;
            }
            else if (ID == "Key.Add")
            {
                return Key.Add;
            }
            else if (ID == "Key.Apps")
            {
                return Key.Apps;
            }
            else if (ID == "Key.B")
            {
                return Key.B;
            }
            else if (ID == "Key.Back")
            {
                return Key.Back;
            }
            else if (ID == "Key.C")
            {
                return Key.C;
            }
            else if (ID == "Key.CapsLock")
            {
                return Key.CapsLock;
            }
            else if (ID == "Key.D")
            {
                return Key.D;
            }
            else if (ID == "Key.D0")
            {
                return Key.D0;
            }
            else if (ID == "Key.D1")
            {
                return Key.D1;
            }
            else if (ID == "Key.D2")
            {
                return Key.D2;
            }
            else if (ID == "Key.D3")
            {
                return Key.D3;
            }
            else if (ID == "Key.D4")
            {
                return Key.D4;
            }
            else if (ID == "Key.D5")
            {
                return Key.D5;
            }
            else if (ID == "Key.D6")
            {
                return Key.D6;
            }
            else if (ID == "Key.D7")
            {
                return Key.D7;
            }
            else if (ID == "Key.D8")
            {
                return Key.D8;
            }
            else if (ID == "Key.D9")
            {
                return Key.D9;
            }
            else if (ID == "Key.Decimal")
            {
                return Key.Decimal;
            }
            else if (ID == "Key.Delete")
            {
                return Key.Delete;
            }
            else if (ID == "Key.Divide")
            {
                return Key.Divide;
            }
            else if (ID == "Key.Down")
            {
                return Key.Down;
            }
            else if (ID == "Key.E")
            {
                return Key.E;
            }
            else if (ID == "Key.End")
            {
                return Key.End;
            }
            else if (ID == "Key.Enter")
            {
                return Key.Enter;
            }
            else if (ID == "Key.Escape")
            {
                return Key.Escape;
            }
            else if (ID == "Key.F")
            {
                return Key.F;
            }
            else if (ID == "Key.F1")
            {
                return Key.F1;
            }
            else if (ID == "Key.F10")
            {
                return Key.F10;
            }
            else if (ID == "Key.F11")
            {
                return Key.F11;
            }
            else if (ID == "Key.F12")
            {
                return Key.F12;
            }
            else if (ID == "Key.F13")
            {
                return Key.F13;
            }
            else if (ID == "Key.F14")
            {
                return Key.F14;
            }
            else if (ID == "Key.F15")
            {
                return Key.F15;
            }
            else if (ID == "Key.F16")
            {
                return Key.F16;
            }
            else if (ID == "Key.F19")
            {
                return Key.F19;
            }
            else if (ID == "Key.F2")
            {
                return Key.F2;
            }
            else if (ID == "Key.F20")
            {
                return Key.F20;
            }
            else if (ID == "Key.F21")
            {
                return Key.F21;
            }
            else if (ID == "Key.F22")
            {
                return Key.F22;
            }
            else if (ID == "Key.F23")
            {
                return Key.F23;
            }
            else if (ID == "Key.F24")
            {
                return Key.F24;
            }
            else if (ID == "Key.F3")
            {
                return Key.F3;
            }
            else if (ID == "Key.F4")
            {
                return Key.F4;
            }
            else if (ID == "Key.F5")
            {
                return Key.F5;
            }
            else if (ID == "Key.F6")
            {
                return Key.F6;
            }
            else if (ID == "Key.F7")
            {
                return Key.F7;
            }
            else if (ID == "Key.F8")
            {
                return Key.F8;
            }
            else if (ID == "Key.F9")
            {
                return Key.F9;
            }
            else if (ID == "Key.G")
            {
                return Key.G;
            }
            else if (ID == "Key.H")
            {
                return Key.H;
            }
            else if (ID == "Key.Home")
            {
                return Key.Home;
            }
            else if (ID == "Key.I")
            {
                return Key.I;
            }
            else if (ID == "Key.Insert")
            {
                return Key.Insert;
            }
            else if (ID == "Key.J")
            {
                return Key.J;
            }
            else if (ID == "Key.K")
            {
                return Key.K;
            }
            else if (ID == "Key.L")
            {
                return Key.L;
            }
            else if (ID == "Key.Left")
            {
                return Key.Left;
            }
            else if (ID == "Key.LeftAlt")
            {
                return Key.LeftAlt;
            }
            else if (ID == "Key.LeftCtrl")
            {
                return Key.LeftCtrl;
            }
            else if (ID == "Key.LeftShift")
            {
                return Key.LeftShift;
            }
            else if (ID == "Key.LWin")
            {
                return Key.LWin;
            }
            else if (ID == "Key.M")
            {
                return Key.M;
            }
            else if (ID == "Key.Multiply")
            {
                return Key.Multiply;
            }
            else if (ID == "Key.N")
            {
                return Key.N;
            }
            else if (ID == "Key.Next")
            {
                return Key.Next;
            }
            else if (ID == "Key.NumLock")
            {
                return Key.NumLock;
            }
            else if (ID == "Key.NumPad0")
            {
                return Key.NumPad0;
            }
            else if (ID == "Key.NumPad1")
            {
                return Key.NumPad1;
            }
            else if (ID == "Key.NumPad2")
            {
                return Key.NumPad2;
            }
            else if (ID == "Key.NumPad3")
            {
                return Key.NumPad3;
            }
            else if (ID == "Key.NumPad4")
            {
                return Key.NumPad4;
            }
            else if (ID == "Key.NumPad5")
            {
                return Key.NumPad5;
            }
            else if (ID == "Key.NumPad6")
            {
                return Key.NumPad6;
            }
            else if (ID == "Key.NumPad7")
            {
                return Key.NumPad7;
            }
            else if (ID == "Key.NumPad8")
            {
                return Key.NumPad8;
            }
            else if (ID == "Key.NumPad9")
            {
                return Key.NumPad9;
            }
            else if (ID == "Key.O")
            {
                return Key.O;
            }
            else if (ID == "Key.P")
            {
                return Key.P;
            }
            else if (ID == "Key.PageDown")
            {
                return Key.PageDown;
            }
            else if (ID == "Key.PageUp")
            {
                return Key.PageUp;
            }
            else if (ID == "Key.Pause")
            {
                return Key.Pause;
            }
            else if (ID == "Key.PrintScreen")
            {
                return Key.PrintScreen;
            }
            else if (ID == "Key.Prior")
            {
                return Key.Prior;
            }
            else if (ID == "Key.Q")
            {
                return Key.Q;
            }
            else if (ID == "Key.R")
            {
                return Key.R;
            }
            else if (ID == "Key.Return")
            {
                return Key.Return;
            }
            else if (ID == "Key.Right")
            {
                return Key.Right;
            }
            else if (ID == "Key.RightAlt")
            {
                return Key.RightAlt;
            }
            else if (ID == "Key.RightCtrl")
            {
                return Key.RightCtrl;
            }
            else if (ID == "Key.RightShift")
            {
                return Key.RightShift;
            }
            else if (ID == "Key.RWin")
            {
                return Key.RWin;
            }
            else if (ID == "Key.S")
            {
                return Key.S;
            }
            else if (ID == "Key.Scroll")
            {
                return Key.Scroll;
            }
            else if (ID == "Key.Select")
            {
                return Key.Select;
            }
            else if (ID == "Key.Separator")
            {
                return Key.Separator;
            }
            else if (ID == "Key.Space")
            {
                return Key.Space;
            }
            else if (ID == "Key.Subtract")
            {
                return Key.Subtract;
            }
            else if (ID == "Key.T")
            {
                return Key.T;
            }
            else if (ID == "Key.Tab")
            {
                return Key.Tab;
            }
            else if (ID == "Key.U")
            {
                return Key.U;
            }
            else if (ID == "Key.Up")
            {
                return Key.Up;
            }
            else if (ID == "Key.V")
            {
                return Key.V;
            }
            else if (ID == "Key.W")
            {
                return Key.W;
            }
            else if (ID == "Key.X")
            {
                return Key.X;
            }
            else if (ID == "Key.Y")
            {
                return Key.Y;
            }
            else if (ID == "Key.Z")
            {
                return Key.Z;
            }
            else if (ID == "Key.Oem1")
            {
                return Key.Oem1;
            }
            else if (ID == "Key.Oem2")
            {
                return Key.Oem2;
            }
            else if (ID == "Key.Oem3")
            {
                return Key.Oem3;
            }
            else if (ID == "Key.Oem4")
            {
                return Key.Oem4;
            }
            else if (ID == "Key.Oem5")
            {
                return Key.Oem5;
            }
            else if (ID == "Key.Oem6")
            {
                return Key.Oem6;
            }
            else if (ID == "Key.Oem7")
            {
                return Key.Oem7;
            }
            else if (ID == "Key.Oem8")
            {
                return Key.Oem8;
            }
            else if (ID == "Key.OemBackslash")
            {
                return Key.OemBackslash;
            }
            else if (ID == "Key.OemBackTab")
            {
                return Key.OemBackTab;
            }
            else if (ID == "Key.OemCloseBrackets")
            {
                return Key.OemCloseBrackets;
            }
            else if (ID == "Key.OemComma")
            {
                return Key.OemComma;
            }
            else if (ID == "Key.OemMinus")
            {
                return Key.OemMinus;
            }
            else if (ID == "Key.OemOpenBrackets")
            {
                return Key.OemOpenBrackets;
            }
            else if (ID == "Key.OemPeriod")
            {
                return Key.OemPeriod;
            }
            else if (ID == "Key.OemPipe")
            {
                return Key.OemPipe;
            }
            else if (ID == "Key.OemPlus")
            {
                return Key.OemPlus;
            }
            else if (ID == "Key.OemQuestion")
            {
                return Key.OemQuestion;
            }
            else if (ID == "Key.OemQuotes")
            {
                return Key.OemQuotes;
            }
            else if (ID == "Key.OemSemicolon")
            {
                return Key.OemSemicolon;
            }
            else if (ID == "Key.OemTilde")
            {
                return Key.OemTilde;
            }
            return Key.None;
        }
        static public string[] KeyList =
        {
            "A",
            "Add",
            "Apps",
            "B",
            "Back",
            "C",
            "CapsLock",
            "D",
            "D0",
            "D1",
            "D2",
            "D3",
            "D4",
            "D5",
            "D6",
            "D7",
            "D8",
            "D9",
            "Decimal",
            "Delete",
            "Divide",
            "Down",
            "E",
            "End",
            "Enter",
            "Escape",
            "F",
            "F1",
            "F10",
            "F11",
            "F12",
            "F13",
            "F14",
            "F15",
            "F16",
            "F19",
            "F2",
            "F20",
            "F21",
            "F22",
            "F23",
            "F24",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "G",
            "H",
            "Home",
            "I",
            "Insert",
            "J",
            "K",
            "L",
            "Left",
            "LeftAlt",
            "LeftCtrl",
            "LeftShift",
            "LWin",
            "M",
            "Multiply",
            "N",
            "Next",
            "NumLock",
            "NumPad0",
            "NumPad1",
            "NumPad2",
            "NumPad3",
            "NumPad4",
            "NumPad5",
            "NumPad6",
            "NumPad7",
            "NumPad8",
            "NumPad9",
            "O",
            "P",
            "PageDown",
            "PageUp",
            "Pause",
            "PrintScreen",
            "Prior",
            "Q",
            "R",
            "Right",
            "Return",
            "RightAlt",
            "RightCtrl",
            "RightShift",
            "RWin",
            "S",
            "Scroll",
            "Select",
            "Separator",
            "Space",
            "Subtract",
            "T",
            "Tab",
            "U",
            "Up",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "Oem1",
            "Oem2",
            "Oem3",
            "Oem4",
            "Oem5",
            "Oem6",
            "Oem7",
            "Oem8",
            "OemBackslash",
            "OemBackTab",
            "OemCloseBrackets",
            "OemComma",
            "OemMinus",
            "OemOpenBrackets",
            "OemPeriod",
            "OemPipe",
            "OemPlus",
            "OemQuestion",
            "OemQuotes",
            "OemSemicolon",
            "OemTilde"
        };
        static public string[] KeyModList =
        {
            "Alt",
            "Ctrl",
            "None",
            "Shift",
            "Win"
        };
    }
}
