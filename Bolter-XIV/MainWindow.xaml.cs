/*  This is Bolter, a position and speed manipulation tool for FFXIV
    Copyright (C) 2013 devnull

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Config_Keys;
using Key_Strings;
using Player_Bits;
using UnManaged;

namespace Bolter_XIV
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public static class InterProcessCom
    {
        [MarshalAs(UnmanagedType.AnsiBStr, SizeConst = 200)]
        public static string ConfigPath;
        public static uint AssemblyBase;
        public static uint AssemblySize;
    }
    public class STAThread
    {
#pragma warning disable 0618
        public STAThread()
        {
            new Thread(LoadBolter) { ApartmentState = ApartmentState.STA }.Start();
        }
#pragma warning restore
        private Window w;
        private void LoadBolter()
        {
            w = new MainWindow();
            w.ShowDialog();
        }
        public int UnHideWindow(int i)
        {
            w.Dispatcher.BeginInvoke(new Action(delegate { w.Visibility = Visibility.Visible; }));
            return i;
        }

        public int GetPath(string path, uint AssAddress, uint asize)
        {
            InterProcessCom.ConfigPath = path;
            InterProcessCom.AssemblyBase = AssAddress;
            InterProcessCom.AssemblySize = asize;
            return 1;
        }
    }
    public partial class MainWindow : Window
    {
        #region Fields

        private static float _radarZJump;
        private static float _buttonJumpVal = 3;
        private static byte _lastSpeedByte;
        internal static byte[] SetSpeed = new byte[4];

        private static readonly IList<string> Compass =
            new List<string> {"N", "NE", "E", "SE", "S", "SW", "W", "NW", "Up", "Down"}.AsReadOnly();

        private static readonly IList<string> Speedstrings =
            new List<string> {"Up", "Down", "Sprint", "Normal", "Max"}.AsReadOnly();

        
        private HotKey[] _hotsKeys;

        #endregion

        #region Open/Close

        unsafe public MainWindow()
        {
            //allocate fixed pointer for GetPlayer ASM routine.
            Player.BasePlayerAddress = (IntPtr*)Marshal.AllocHGlobal(4).ToPointer();
            *Player.BasePlayerAddress = IntPtr.Zero;
            InitializeComponent();
            //Make a pseudo close button.
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,
                delegate { Close(); }));
        }

        //Drag function for self-made border
        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }
        //Process selection window

        //Wait for user to select the process.
        unsafe public void Loadme()
        {
            
            Dispatcher.BeginInvoke(new Action(delegate { label1.Content = "Loading..."; }));
            IntPtr BaseAddy = Process.GetCurrentProcess().MainModule.BaseAddress;
            using (var sigScam = new SigScan.SigScan(BaseAddy, 0xD40000))
            {
                byte[] lockBuff = {0x89, 0x55, 0xA8, 0x89, 0x4D, 0xB0, 0x83, 0xF8, 0x1E, 0x73, 0x3A, 0x0F};
                byte[] lockAxisC = {0x8B, 0x86, 0xEC, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x45, 0xE0, 0xF3};
                byte[] lockAxisS =
                {
                    0x7B, 0x12, 0xF3, 0x0F, 0x10, 0x45, 0x08, 0xF3, 0x0F, 0x11, 0x81, 0xE0, 0x00, 0x00, 0x00
                };
                byte[] hideBuffSig = {0x8B, 0x45, 0x08, 0x83, 0xF8, 0x1E, 0x72, 0x06, 0x33, 0xC0, 0x5D, 0xC2};
                byte[] playerStructSig =
                {
                    0x56, 0x8B, 0xDF, 0x89, 0x45, 0xF8, 0x90, 0x8B, 0x4D, 0x0C, 0x8B, 0x33, 0x8B,
                    0x01, 0x8B, 0x50, 0x04, 0x56
                };
                byte[] movementSig =
                {
                    0x84, 0xC0, 0x74, 0x09, 0x80, 0x3D, 0x04, 0x12, 0x23, 0x01, 0x00, 0x75, 0x2B, 0x80,
                    0xBE, 0xCC, 0x01, 0x00, 0x00, 0x00
                };
                Player.HideBuffAddress =
                    (int) sigScam.FindPattern(hideBuffSig, "xxxxxxxxxxxx", -(int) BaseAddy) + 3;
                Player.LockSprintAddress =
                    (int) sigScam.FindPattern(lockBuff, "xxxxxxxxxxxx", -(int) BaseAddy) + 6;
                Player.ServerSideLock =
                    (int) sigScam.FindPattern(lockAxisS, "xxxxxxxxxxxxxxx", -(int) BaseAddy) + 36;
                Player.ClientSideLock =
                    (int) sigScam.FindPattern(lockAxisC, "xxxxxxxxxxxx", -(int) BaseAddy) + 11;
                Player.BuffLoopAddress =
                    (int) sigScam.FindPattern(playerStructSig, "xxxxxxxxxxxxxxxxxx", -(int) BaseAddy) + 10;
                Player.MovementAddress =
                    Marshal.ReadInt32((sigScam.FindPattern(movementSig, "xxxxxx????xxxxx????x", -(int) BaseAddy) + 6) +
                                      (int) BaseAddy);
            }
            Player.RedirectBuffOp();

            while (*Player.BasePlayerAddress == IntPtr.Zero)
                Thread.Sleep(100);
            
            //Load HotKeys
            try
            {
                ConfigKeys.Load();
                //Are there any?
                if (ConfigKeys.Config.GetElementsByTagName("HotKey").Count == 0)
                {
                    //If not, skip this
                }
                else //if so
                {
                    //get the keys
                    ConfigKeys.GetKeys();
                    //Instance HotKeys
                    _hotsKeys = new HotKey[ConfigKeys.keycount];
                    //logic for Setting Hotkeys from XML
                    int pCount = 0;
                    int sCount = 0;
                    int mCount = 0;
                    int N = 0;
                    foreach (HotKey thekey in _hotsKeys)
                    {
                        if (pCount < ConfigKeys.POSKeys.Count())
                        {
                            _hotsKeys[N] = new HotKey(KeyString.Keys_M("Key." + ConfigKeys.POSKeys[pCount]),
                                KeyString.Key_Mods(ConfigKeys.POSKeyMods[pCount]), OnHotKeyHandler1);
                            pCount++;
                        }
                        else if (sCount < ConfigKeys.SpeedKeys.Count())
                        {
                            _hotsKeys[N] = new HotKey(KeyString.Keys_M("Key." + ConfigKeys.SpeedKeys[sCount]),
                                KeyString.Key_Mods(ConfigKeys.SpeedKeyMods[sCount]), OnHotKeyHandler2);
                            sCount++;
                        }
                        else if (mCount < ConfigKeys.MoveKeys.Count())
                        {
                            _hotsKeys[N] = new HotKey(KeyString.Keys_M("Key." + ConfigKeys.MoveKeys[mCount]),
                                KeyString.Key_Mods(ConfigKeys.MoveKeyMods[mCount]), OnHotKeyHandler3);
                            mCount++;
                        }
                        N++;
                    }
                }
                //fill combo boxes  
                Dispatcher.BeginInvoke(new Action(delegate { SpeedKey_Direct.ItemsSource = Speedstrings; }));
                Dispatcher.BeginInvoke(new Action(delegate { MoveKey_Direct.ItemsSource = Compass; }));
                Dispatcher.BeginInvoke(new Action(delegate { POSKbox.ItemsSource = KeyString.KeyList; }));
                Dispatcher.BeginInvoke(new Action(delegate { POSKmodbox.ItemsSource = KeyString.KeyModList; }));
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    POSKmodbox.SelectedIndex =
                        POSKbox.SelectedIndex =
                            SpeedKey_Direct.SelectedIndex =
                                MoveKey_Direct.SelectedIndex =
                                    0;
                }));
            }
            catch 
            {

            }
            
            Dispatcher.BeginInvoke(new Action(delegate { label1.Content = "Speed Normal"; }));
            Dispatcher.BeginInvoke(new Action(delegate { slider3.Value = 0.323886639676113; }));
            Dispatcher.BeginInvoke(new Action(delegate { ProgSpin.IsEnabled = false; }));
            Dispatcher.BeginInvoke(new Action(delegate { ProgSpin.Visibility = Visibility.Hidden; }));
            Dispatcher.BeginInvoke(new Action(delegate { Loading.Visibility = Visibility.Hidden; }));
            Dispatcher.BeginInvoke(new Action(delegate { LoadingText.Visibility = Visibility.Hidden; }));
        }

        //on Window Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Start waiting, on a new thread
            new Thread(Loadme).Start();
           
            ////set transparent slider parameters
            slider4.Minimum = 1;
            slider4.Maximum = 10;
            slider4.Value = 10;
        }

        //on window close
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //end loops
            Array.Clear(SetSpeed, 0, 4);
            _uchecked = false;
            boxcheck = false;
            Player.UndoRedirectBuffOp();
            Player.VirtualFreeEx(Process.GetCurrentProcess().Handle, Player.EntryPoint, 0, Player.FreeType.Release);
            GC.Collect();
            int p = 0;
            Player.CreateRemoteThread(Process.GetCurrentProcess().Handle, lpStartAddress: (IntPtr)InterProcessCom.AssemblyBase,
                lpThreadId: p);
        }

        #endregion

        #region POS Hotkey Handler

        //Hander for POS hotkeys
        private void OnHotKeyHandler1(HotKey hotKey)
        {
            int n = 0;
            //find index in the XML of the hotkey that was pressed.
            foreach (string thestring in ConfigKeys.POSKeys)
            {
                if (hotKey.Key == KeyString.Keys_M("Key." + thestring) &&
                    hotKey.KeyModifiers ==
                    KeyString.Key_Mods(ConfigKeys.POSKeyMods[Array.IndexOf(ConfigKeys.POSKeys, thestring)]))
                {
                    n = Array.IndexOf(ConfigKeys.POSKeys, thestring);
                }
            }
            //allocate some variables
            float X = 0f;
            float Y = 0f;
            float Z = 0f;
            //compare the pos name and area string with the saved_cords and return the XYZ
            foreach (XmlNode thenode in ConfigKeys.Config.GetElementsByTagName("Cord"))
            {
                if (thenode.Attributes != null)
                {
                    string cmp = thenode.Attributes.Item(0).InnerText + thenode.Attributes.Item(4).InnerText;
                    if (cmp == ConfigKeys.POSNames[n] + ConfigKeys.POSZoneNames[n])
                    {
                        X = StringToFloat(thenode.Attributes.Item(1).InnerText);
                        Y = StringToFloat(thenode.Attributes.Item(2).InnerText);
                        Z = StringToFloat(thenode.Attributes.Item(3).InnerText);
                    }
                }
            }
            //Set the new XYZ values.
            Player.WriteToPos("Z", Z);
            Player.WriteToPos("X", X);
            Player.WriteToPos("Y", Y);
        }

        #endregion

        #region Speed Hotkey Handler

        //handler for speed HotKey
        private void OnHotKeyHandler2(HotKey hotKey)
        {
            int n = 0;
            //find index in the XML of the hotkey that was pressed.
            foreach (string thestring in ConfigKeys.SpeedKeys)
            {
                if (hotKey.Key == KeyString.Keys_M("Key." + thestring) &&
                    hotKey.KeyModifiers ==
                    KeyString.Key_Mods(ConfigKeys.SpeedKeyMods[Array.IndexOf(ConfigKeys.SpeedKeys, thestring)]))
                {
                    n = Array.IndexOf(ConfigKeys.SpeedKeys, thestring);
                }
            }
            //logic for slider manipulation
            if (ConfigKeys.SpeedDirs[n] == "Up")
            {
                slider1.Value = slider1.Value + StringToFloat(ConfigKeys.SpeedAmts[n]);
            }
            if (ConfigKeys.SpeedDirs[n] == "Down")
            {
                slider1.Value = slider1.Value - StringToFloat(ConfigKeys.SpeedAmts[n]);
            }
            if (ConfigKeys.SpeedDirs[n] == "Normal")
            {
                slider1.Value = 0;
            }
            if (ConfigKeys.SpeedDirs[n] == "Sprint")
            {
                slider1.Value = 1.18852459016393;
            }
            if (ConfigKeys.SpeedDirs[n] == "Max")
            {
                slider1.Value = 10d;
            }
        }

        #endregion

        #region Movement Hotkey Handler

        //handler for Move HotKey

        private void OnHotKeyHandler3(HotKey hotKey)
        {
            int n = 0;
            //find index in the XML of the hotkey that was pressed.
            foreach (string thestring in ConfigKeys.MoveKeys)
            {
                if (hotKey.Key == KeyString.Keys_M("Key." + thestring) &&
                    hotKey.KeyModifiers ==
                    KeyString.Key_Mods(ConfigKeys.MoveKeyMods[Array.IndexOf(ConfigKeys.MoveKeys, thestring)]))
                {
                    n = Array.IndexOf(ConfigKeys.MoveKeys, thestring);
                }
            }
            //Move with the parameter for the hotkey.
            if (ConfigKeys.MoveDirs[n] == "N")
            {
                Player.AddToPos("Y", StringToFloat(ConfigKeys.MoveAmts[n]), false);
            }
            else if (ConfigKeys.MoveDirs[n] == "NE")
            {
                Player.AddToPos("X", StringToFloat(ConfigKeys.MoveAmts[n]), true);
                Player.AddToPos("Y", StringToFloat(ConfigKeys.MoveAmts[n]), false);
            }
            else if (ConfigKeys.MoveDirs[n] == "E")
            {
                Player.AddToPos("X", StringToFloat(ConfigKeys.MoveAmts[n]), true);
            }
            else if (ConfigKeys.MoveDirs[n] == "SE")
            {
                Player.AddToPos("X", StringToFloat(ConfigKeys.MoveAmts[n]), true);
                Player.AddToPos("Y", StringToFloat(ConfigKeys.MoveAmts[n]), true);
            }
            else if (ConfigKeys.MoveDirs[n] == "S")
            {
                Player.AddToPos("Y", StringToFloat(ConfigKeys.MoveAmts[n]), true);
            }
            else if (ConfigKeys.MoveDirs[n] == "SW")
            {
                Player.AddToPos("X", StringToFloat(ConfigKeys.MoveAmts[n]), false);
                Player.AddToPos("X", StringToFloat(ConfigKeys.MoveAmts[n]), true);
            }
            else if (ConfigKeys.MoveDirs[n] == "W")
            {
                Player.AddToPos("X", StringToFloat(ConfigKeys.MoveAmts[n]), false);
            }
            else if (ConfigKeys.MoveDirs[n] == "NW")
            {
                Player.AddToPos("X", StringToFloat(ConfigKeys.MoveAmts[n]), false);
                Player.AddToPos("Y", StringToFloat(ConfigKeys.MoveAmts[n]), false);
            }
            else if (ConfigKeys.MoveDirs[n] == "Up")
            {
                Player.AddToPos("Z", StringToFloat(ConfigKeys.MoveAmts[n]), true);
            }
            else if (ConfigKeys.MoveDirs[n] == "Down")
            {
                Player.AddToPos("Z", StringToFloat(ConfigKeys.MoveAmts[n]), false);
            }
        }

        #endregion

        #region POS Buttons

        /* The method that FFXIV uses to control player position is slightly different than FFXI 
         * It still uses XYZ floats, but has separated what the server sees and what the client sees 
         * This means there are two values in memory that will move your POS,  
         * but only changing the server side value will not do anything to the client,  
         * until he/she walks, and the client will then be updated. Vise versa, changing the client side 
         * value will move your character, but when you go to walk, the server will spit you back  
         * to what it reads from. The following button functions write to both Server and Client side,  
         * to make the full move 
         */

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            //Down
            Player.AddToPos("Z", _buttonJumpVal, false);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //North 
            Player.AddToPos("Y", _buttonJumpVal, false);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            //Up
            Player.AddToPos("Z", _buttonJumpVal, true);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //South
            Player.AddToPos("Y", _buttonJumpVal, true);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //East
            Player.AddToPos("X", _buttonJumpVal, true);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //West
            Player.AddToPos("X", _buttonJumpVal, false);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            //North East
            Player.AddToPos("X", _buttonJumpVal, true);
            Player.AddToPos("Y", _buttonJumpVal, false);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            //North West
            Player.AddToPos("X", _buttonJumpVal, false);
            Player.AddToPos("Y", _buttonJumpVal, false);
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            //South West
            Player.AddToPos("X", _buttonJumpVal, false);
            Player.AddToPos("Y", _buttonJumpVal, true);
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            //South East
            Player.AddToPos("X", _buttonJumpVal, true);
            Player.AddToPos("Y", _buttonJumpVal, true);
        }

        #endregion

        #region Speed Adjustment

        //Adjust Speed
        /* The method in witch FFXIV handles player speed is entirely different from FFXI
         * As of yet I have not found a value in memory that stores player speed
         * Instead player speed can only be increased by the buff "sprint", but that's ok
         * FFXIV Handles buffs like this; Read from 4 byte array, First 2 bytes = Buff ID, 
         * next 2 bytes = Parameters.
         * example: Sprint Buff ID = 0x32 Parameter1 = Speed, Parameter 2 = Gravity. & More Speed?.
         * 0x32 0x00 0x1E 0x00 is normal sprint speed. This slider changes parameter 1
         */

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0)
            {
                new Thread(Player.UpdateSpeed).Start();
            }
            if (Math.Truncate(25.5*e.NewValue) == 0)
            {
                Array.Clear(SetSpeed, 0, 4);
            }
            else
            {
                SpeedBytes(0x32, 0x00, (byte) Math.Truncate(25.5*e.NewValue), _lastSpeedByte);
            }
            if (SetSpeed[2] == 0)
            {
                label1.Content = "Normal";
            }
            else if (SetSpeed[2] == 0x1E)
            {
                label1.Content = "Sprint";
            }
            else if (SetSpeed[2] < 0x1E)
            {
                label1.Content = "Sprint " + Convert.ToString(SetSpeed[2] - (float) 0x1E);
            }
            else
            {
                label1.Content = "Sprint +" + Convert.ToString(SetSpeed[2] - (float) 0x1E);
            }
            label4.Content = "0x" + String.Format("{0:x2}", SetSpeed[0]).ToUpper();
            label6.Content = "0x" + String.Format("{0:x2}", SetSpeed[2]).ToUpper();
        }

        //Just a simple method to make setting the bytes easier
        private static int SpeedBytes(byte a, byte b, byte c, byte d)
        {
            SetSpeed[0] = a;
            SetSpeed[1] = b;
            SetSpeed[2] = c;
            SetSpeed[3] = d;
            return 0;
        }

        #endregion

        #region Radar & misc. tools

        //Set slider to Sprint speed
        private float _radarX;
        private float _radarY;
        private bool _uchecked;

        private void Sprint_Click(object sender, RoutedEventArgs e)
        {
            slider1.Value = 1.18852459016393;
        }

        //Set slider to Normal speed
        private void Normal_Click(object sender, RoutedEventArgs e)
        {
            slider1.Value = 0;
        }

        //Hander for the radar POS control
        private void eclipse1_MouseMove(object sender, MouseEventArgs e)
        {
            //Centers the axises
            if (e.GetPosition(eclipse1).Y > 125)
            {
                _radarY = ((float) e.GetPosition(eclipse1).Y - 125)/2;
            }
            else if (e.GetPosition(eclipse1).Y < 125)
            {
                _radarY = ((float) e.GetPosition(eclipse1).Y - 125)/2;
            }
            if (e.GetPosition(eclipse1).X > 125)
            {
                _radarX = ((float) e.GetPosition(eclipse1).X - 125)/2;
            }
            else if (e.GetPosition(eclipse1).X < 125)
            {
                _radarX = ((float) e.GetPosition(eclipse1).X - 125)/2;
            }
            textBox2.Text = Math.Round(_radarX, 1).ToString();
            textBox3.Text = Math.Round(_radarY, 1).ToString();
        }

        //Handler for clicking the radar
        private void eclipse1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Player.AddToPos("Z", _radarZJump, true);
            Player.AddToPos("X", _radarX, true);
            Player.AddToPos("Y", _radarY, true);
        }

        //Slider for Setting the Z value on Radar jump.
        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _radarZJump = (float) Math.Floor(10*e.NewValue);
            label2.Content = _radarZJump + " Yalms";
        }

        //Slider for setting the button jump value, default is 3
        private void Slider_ValueChanged_2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _buttonJumpVal = (float) Math.Floor(10*e.NewValue);
            label3.Content = _buttonJumpVal + " Yalms";
        }

        //Slider for setting the last byte in Sprint
        private void Slider_ValueChanged_3(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _lastSpeedByte = (byte) Math.Truncate(12.7*e.NewValue);
            label7.Content = "0x" + String.Format("{0:x2}", _lastSpeedByte).ToUpper();
            SpeedBytes(SetSpeed[0], SetSpeed[1], SetSpeed[2], _lastSpeedByte);
        }

        //Checkbox for Updating POS

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _uchecked = true;
            new Thread(UpdatedPosThread).Start();
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            _uchecked = false;
        }

        //Method for updating the POS
        private void UpdatedPosThread()
        {
            while (_uchecked)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    textBoxX.Text = Player.GetPos("X").ToString();
                    textBoxY.Text = Player.GetPos("Y").ToString();
                    textBoxZ.Text = Player.GetPos("Z").ToString();
                }));
                Thread.Sleep(300);
            }
        }

        #endregion

        #region Config

        //handler for updating the POS
        private void GetPos_Click(object sender, RoutedEventArgs e)
        {
            textBoxX.Text = Player.GetPos("X").ToString(CultureInfo.InvariantCulture);
            textBoxY.Text = Player.GetPos("Y").ToString(CultureInfo.InvariantCulture);
            textBoxZ.Text = Player.GetPos("Z").ToString(CultureInfo.InvariantCulture);
        }

        //Handler for loading the XML
        private void Load_XML(object sender, RoutedEventArgs e)
        {
            XMLWrapper.Load();
            XMLWrapper.RefreshAreaBox(AreaPOS_Box);
        }

        //Handler for changing the area
        private void AreaPOS_Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            XMLWrapper.RefreshNameBox(NamePOS_Box, AreaPOS_Box);
        }

        //Handler for changing the POS name
        private void NamePOS_Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            XMLWrapper.RefreshPOSBoxes(NamePOS_Box, AreaPOS_Box, NewPOS_X, NewPOS_Y, NewPOS_Z, POSKey_Zone, POSKey_Name);
        }

        //Handler for saving new cords
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            XMLWrapper.SaveCord(Area_Save, Name_Save, load_button);
        }

        //Handler for jumping
        private void Jump_Click(object sender, RoutedEventArgs e)
        {
            Player.WriteToPos("X", StringToFloat(NewPOS_X.Text));
            Player.WriteToPos("Y", StringToFloat(NewPOS_Y.Text));
            Player.WriteToPos("Z", StringToFloat(NewPOS_Z.Text));
        }

        //Handler for saving HotKeys
        private void SaveHotkey(object sender, RoutedEventArgs e)
        {
            if ((bool) POSRadio.IsChecked)
            {
                XMLWrapper.SaveHotKey(POSKbox, POSKmodbox, POSKey_Name, POSKey_Zone, XMLWrapper.KeyType.POSKey);
            }
            else if ((bool) SpeedRadio.IsChecked)
            {
                XMLWrapper.SaveHotKey(POSKbox, POSKmodbox, SpeedKey_Amount, SpeedKey_Direct, XMLWrapper.KeyType.SpeedKey);
            }
            else if ((bool) MoveRadio.IsChecked)
            {
                XMLWrapper.SaveHotKey(POSKbox, POSKmodbox, MoveKey_Dist, MoveKey_Direct, XMLWrapper.KeyType.MoveKey);
            }
        }

        #endregion

        #region mis

        //Minimize button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //Transparency slider
        private void Slider_ValueChanged_4(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity = e.NewValue/10;
            percnt_txt.Text = Math.Floor(e.NewValue*10) + "%";
        }

        //Always on top box
        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (aot.IsChecked == true)
            {
                Topmost = true;
            }
            else
            {
                Topmost = false;
            }
        }

        //Donation button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        private bool boxcheck;

        private void Updatebuffslots()
        {
            while (boxcheck)
            {
                Dispatcher.BeginInvoke(new Action(delegate { buff1.Content = Player.BuffNames[Player.DebugR(0)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff2.Content = Player.BuffNames[Player.DebugR(1)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff3.Content = Player.BuffNames[Player.DebugR(2)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff4.Content = Player.BuffNames[Player.DebugR(3)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff5.Content = Player.BuffNames[Player.DebugR(4)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff6.Content = Player.BuffNames[Player.DebugR(5)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff7.Content = Player.BuffNames[Player.DebugR(6)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff8.Content = Player.BuffNames[Player.DebugR(7)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff9.Content = Player.BuffNames[Player.DebugR(8)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff10.Content = Player.BuffNames[Player.DebugR(9)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff11.Content = Player.BuffNames[Player.DebugR(10)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff12.Content = Player.BuffNames[Player.DebugR(11)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff13.Content = Player.BuffNames[Player.DebugR(12)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff14.Content = Player.BuffNames[Player.DebugR(13)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff15.Content = Player.BuffNames[Player.DebugR(14)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff16.Content = Player.BuffNames[Player.DebugR(15)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff17.Content = Player.BuffNames[Player.DebugR(16)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff18.Content = Player.BuffNames[Player.DebugR(17)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff19.Content = Player.BuffNames[Player.DebugR(18)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff20.Content = Player.BuffNames[Player.DebugR(19)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff21.Content = Player.BuffNames[Player.DebugR(20)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff22.Content = Player.BuffNames[Player.DebugR(21)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff23.Content = Player.BuffNames[Player.DebugR(22)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff24.Content = Player.BuffNames[Player.DebugR(23)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff25.Content = Player.BuffNames[Player.DebugR(24)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff26.Content = Player.BuffNames[Player.DebugR(25)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff27.Content = Player.BuffNames[Player.DebugR(26)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff28.Content = Player.BuffNames[Player.DebugR(27)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff29.Content = Player.BuffNames[Player.DebugR(28)]; }));
                Dispatcher.BeginInvoke(new Action(delegate { buff30.Content = Player.BuffNames[Player.DebugR(29)]; }));
                Thread.Sleep(500);
            }
        }

        private void CheckBox_Checked_2(object sender, RoutedEventArgs e)
        {
            if ((bool) buffloopbox.IsChecked)
            {
                boxcheck = true;
                new Thread(Updatebuffslots).Start();
            }
            else
            {
                boxcheck = false;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Player.RedirectBuffOp();
        }

        private void Xbox(object sender, RoutedEventArgs e)
        {
            if ((bool) theXbox.IsChecked)
            {
                Player.LockAxis("X", true);
            }
            else
            {
                Player.LockAxis("X", false);
            }
        }

        private void Ybox(object sender, RoutedEventArgs e)
        {
            if ((bool) theYbox.IsChecked)
            {
                Player.LockAxis("Y", true);
            }
            else
            {
                Player.LockAxis("Y", false);
            }
        }

        private void Zbox(object sender, RoutedEventArgs e)
        {
            if ((bool)theZbox.IsChecked)
            {
                Player.LockAxis("Z", true);
            }
            else
            {
                Player.LockAxis("Z", false);
            }
        }

        private float StringToFloat(string theString)
        {
            if (theString.Contains('.'))
                return Convert.ToSingle(theString.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
            return Convert.ToSingle(theString.Replace(",", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
        }

        private void CheckBox_Checked_3(object sender, RoutedEventArgs e)
        {
            if ((bool) (hidesprint.IsChecked == true))
                Player.HideSprint(true);
            else
                Player.HideSprint(false);
        }

        private void CheckBox_Checked_4(object sender, RoutedEventArgs e)
        {
            if ((bool) RadarCheckBox.IsChecked)
            {
                HideRadarMain.Visibility = HideRadarSub.Visibility = Visibility.Visible;
            }
            else
            {
                HideRadarMain.Visibility = HideRadarSub.Visibility = Visibility.Hidden;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(InterProcessCom.ConfigPath + " " + InterProcessCom.AssemblyBase.ToString() + " " +InterProcessCom.AssemblySize.ToString());
        }
    }
}