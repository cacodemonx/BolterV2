/*  This is Bolter, a position and speed manipulation tool for FFXIV
    Copyright (C) 2014 devnull

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

using System.IO;
using ConfigHelper;
#pragma warning disable 0618
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
using Player_Bits;
using HotKey = UnManaged.HotKey;

namespace Bolter_XIV
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public static class InterProcessCom
    {
        [MarshalAs(UnmanagedType.AnsiBStr, SizeConst = 200)]
        public static string ConfigPath;

        public static IntPtr UnloadBolter;
    }
    unsafe public class STAThread
    {

        public STAThread()
        {
            new Thread(LoadBolter) { ApartmentState = ApartmentState.STA }.Start();
        }
        private Window w;

        private void LoadBolter()
        {
            w = new MainWindow();
            w.ShowDialog();
            w = null;
            GC.Collect();
        }
        public int PassInfo(int unloadaddress, int menuSig, int masterSig, int collisionSig, int movementSig, int playerStructSig, int hideBuffSig, int lockAxisS, int lockAxisC, int lockBuff, int zoneAddress, string path)
        {
            var mainAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
            InterProcessCom.UnloadBolter = new IntPtr(unloadaddress);
            InterProcessCom.ConfigPath = path;

            Player.ZoneAddress = Marshal.ReadInt32(
                (IntPtr)zoneAddress +
                mainAddress.ToInt32() +
                12);
            Player.HideBuffAddress = hideBuffSig + 3;
            Player.LockSprintAddress = lockBuff + 6;
            Player.ServerSideLock = lockAxisS + 36;
            Player.ClientSideLock = lockAxisC + 11;
            Player.BuffLoopAddress = playerStructSig + 10;
            Player.CollisionAddress = collisionSig + 8;
            Player.MovementAddress = Marshal.ReadInt32(
                (IntPtr)movementSig +
                mainAddress.ToInt32() +
                6);
            Player.MasterPtr = (Player.MasterPointer*)(mainAddress + masterSig + 0x30);
            Player.Menu = (Player.MenuStruct*)Marshal.ReadInt32(mainAddress + menuSig + 0xC);
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
            new List<string> { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "Up", "Down" }.AsReadOnly();

        private static readonly IList<string> Speedstrings =
            new List<string> { "Up", "Down", "Sprint", "Normal", "Max" }.AsReadOnly();


        private static List<HotKey> _hotKeys = new List<HotKey>();

        #endregion

        #region Open/Close

        public MainWindow()
        {
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
        public static config Config;
        public static Window w2;
        public static Window w3;
        private void RegHotKeys()
        {
            //logic for Setting Hotkeys from XML
            if (Config.HotKeys.POSKeys.Count > 0)
                foreach (var posKey in Config.HotKeys.POSKeys)
                {
                    _hotKeys.Add(new HotKey(posKey.Key, posKey.KeyMod, PosOnKey));
                }
            if (Config.HotKeys.SpeedKeys.Count > 0)
                foreach (var speedKey in Config.HotKeys.SpeedKeys)
                {
                    _hotKeys.Add(new HotKey(speedKey.Key, speedKey.KeyMod, SpeedOnKey));
                }
            if (Config.HotKeys.MoveKeys.Count > 0)
                foreach (var moveKey in Config.HotKeys.MoveKeys)
                {
                    _hotKeys.Add(new HotKey(moveKey.Key, moveKey.KeyMod, MoveOnKey));
                }
        }
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool AllocConsole();
        //on Window Load
        unsafe private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Player.BasePlayerAddress = (Player.PlayerStructure**)Marshal.AllocHGlobal(4);
            Player.RedirectBuffOp();
            Thread.Sleep(2000);
            //Some load finishing stuff, to be cleaned.
            SpeedKey_Direct.ItemsSource = Speedstrings;
            MoveKey_Direct.ItemsSource = Compass;
            POSKbox.ItemsSource = KeyString.KeyList;
            POSKmodbox.ItemsSource = KeyString.KeyModList;
            POSKmodbox.SelectedIndex =
                POSKbox.SelectedIndex =
                    SpeedKey_Direct.SelectedIndex =
                        MoveKey_Direct.SelectedIndex =
                            0;
            label1.Content = "Speed Normal";
            slider3.Value = 0.323886639676113;
            ProgSpin.IsEnabled = false;
            ProgSpin.Visibility = Visibility.Hidden;
            Loading.Visibility = Visibility.Hidden;
            LoadingText.Visibility = Visibility.Hidden;
            slider4.Minimum = 1;
            slider4.Maximum = 10;
            slider4.Value = 10;
            ConfigWrapper.Load();
            RegHotKeys();
            Player.FillObjectList();
            //FreeConsole();
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateThread(IntPtr SecurityAttributes = default(IntPtr), uint StackSize = 0,
            IntPtr StartFunction = default(IntPtr), IntPtr ThreadParameter = default(IntPtr), uint CreationFlags = 0,
            [Out] uint ThreadId = 0);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        //on window close
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Clean up
            if (GetConsoleWindow() != IntPtr.Zero)
                FreeConsole();

            Array.Clear(SetSpeed, 0, 4);
            _uchecked = false;
            Player.UndoRedirectBuffOp();
            Player.VirtualFreeEx(Process.GetCurrentProcess().Handle, Player.EntryPoint, 0, Player.FreeType.Release);
            for (var i = 0; i < _hotKeys.Count; i++)
            {
                _hotKeys[i].Unregister();
                _hotKeys[i].Dispose();
            }
            _hotKeys.Clear();
            unsafe
            {
                Marshal.FreeHGlobal((IntPtr)Player.BasePlayerAddress);
            }
            GC.Collect();
            CreateThread(StartFunction: InterProcessCom.UnloadBolter, ThreadId: new uint[] {0}[0]);

        }

        #endregion

        #region POS Hotkey Handler

        //Hander for POS hotkeys
        private void PosOnKey(HotKey hotKey)
        {
            // Find index of pressed key.
            var keyIndex = Config.HotKeys.POSKeys.FindIndex(k => k.Key == hotKey.Key && k.KeyMod == hotKey.KeyModifiers);

            // Find index of saved coordinates that corresponds to the key.
            var coordIndex =
                Config.saved_cords.FindIndex(
                    c => c.Name == Config.HotKeys.POSKeys[keyIndex].POSName && c.ZoneID == Config.HotKeys.POSKeys[keyIndex].ZoneName);

            //Set the new XYZ values.
            Player.WriteToPos("Z", Config.saved_cords[coordIndex].Z);
            Player.WriteToPos("X", Config.saved_cords[coordIndex].X);
            Player.WriteToPos("Y", Config.saved_cords[coordIndex].Y);
        }

        #endregion

        #region Speed Hotkey Handler

        //handler for speed HotKey
        private void SpeedOnKey(HotKey hotKey)
        {
            // Find index of pressed key.
            var keyIndex = Config.HotKeys.SpeedKeys.FindIndex(k => k.Key == hotKey.Key && k.KeyMod == hotKey.KeyModifiers);

            // Set slider to corresponding hotkey values.
            switch (Config.HotKeys.SpeedKeys[keyIndex].Direction)
            {
                case "Up":
                    slider1.Value = slider1.Value + Config.HotKeys.SpeedKeys[keyIndex].Amount;
                    break;
                case "Down":
                    slider1.Value = slider1.Value - Config.HotKeys.SpeedKeys[keyIndex].Amount;
                    break;
                case "Normal":
                    slider1.Value = 0;
                    break;
                case "Sprint":
                    slider1.Value = 1.18852459016393;
                    break;
                case "Max":
                    slider1.Value = 10d;
                    break;
            }
        }

        #endregion

        #region Movement Hotkey Handler

        //handler for Move HotKey

        private void MoveOnKey(HotKey hotKey)
        {
            // Find index of pressed hotkey.
            var keyIndex = Config.HotKeys.MoveKeys.FindIndex(k => k.Key == hotKey.Key && k.KeyMod == hotKey.KeyModifiers);

            // Get distance to be moved.
            var distance = Config.HotKeys.MoveKeys[keyIndex].Distance;

            // Move in the direction dictated by the hotkey.
            switch (Config.HotKeys.MoveKeys[keyIndex].Direction)
            {
                case "N":
                    Player.AddToPos("Y", distance, false);
                    break;
                case "NE":
                    Player.AddToPos("X", distance, true);
                    Player.AddToPos("Y", distance, false);
                    break;
                case "E":
                    Player.AddToPos("X", distance, true);
                    break;
                case "SE":
                    Player.AddToPos("X", distance, true);
                    Player.AddToPos("Y", distance, true);
                    break;
                case "S":
                    Player.AddToPos("Y", distance, true);
                    break;
                case "SW":
                    Player.AddToPos("X", distance, false);
                    Player.AddToPos("X", distance, true);
                    break;
                case "W":
                    Player.AddToPos("X", distance, false);
                    break;
                case "NW":
                    Player.AddToPos("X", distance, false);
                    Player.AddToPos("Y", distance, false);
                    break;
                case "Up":
                    Player.AddToPos("Z", distance, true);
                    break;
                case "Down":
                    Player.AddToPos("Z", distance, false);
                    break;
            }
        }

        #endregion

        #region POS Buttons

        private void PoSButton(object sender, RoutedEventArgs e)
        {
            switch (((Button)e.Source).Content.ToString())
            {
                case "Down":
                    Player.AddToPos("Z", _buttonJumpVal, false);
                    break;
                case "North":
                    Player.AddToPos("Y", _buttonJumpVal, false);
                    break;
                case "Up":
                    Player.AddToPos("Z", _buttonJumpVal, true);
                    break;
                case "South":
                    Player.AddToPos("Y", _buttonJumpVal, true);
                    break;
                case "East":
                    Player.AddToPos("X", _buttonJumpVal, true);
                    break;
                case "West":
                    Player.AddToPos("X", _buttonJumpVal, false);
                    break;
                case "NE":
                    Player.AddToPos("X", _buttonJumpVal, true);
                    Player.AddToPos("Y", _buttonJumpVal, false);
                    break;
                case "NW":
                    Player.AddToPos("X", _buttonJumpVal, false);
                    Player.AddToPos("Y", _buttonJumpVal, false);
                    break;
                case "SW":
                    Player.AddToPos("X", _buttonJumpVal, false);
                    Player.AddToPos("Y", _buttonJumpVal, true);
                    break;
                case "SE":
                    Player.AddToPos("X", _buttonJumpVal, true);
                    Player.AddToPos("Y", _buttonJumpVal, true);
                    break;
            }
        }

        #endregion

        #region Speed Adjustment


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0)
            {
                new Thread(Player.UpdateSpeed).Start();
            }
            if (Math.Truncate(25.5 * e.NewValue) == 0)
            {
                Array.Clear(SetSpeed, 0, 4);
            }
            else
            {
                SpeedBytes(0x32, 0x00, (byte)Math.Truncate(25.5 * e.NewValue), _lastSpeedByte);
            }
            switch (SetSpeed[2])
            {
                case 0:
                    label1.Content = "Normal";
                    break;
                case 0x1E:
                    label1.Content = "Sprint";
                    break;
                default:
                    label1.Content = String.Format(SetSpeed[2] < 0x1E ? "Sprint {0}" : "Sprint +{0}", (SetSpeed[2] - (float)0x1E));
                    break;
            }
            label4.Content = String.Format("0x{0:X2}", SetSpeed[0]);
            label6.Content = String.Format("0x{0:X2}", SetSpeed[2]);
        }

        //Just a simple method to make setting the bytes easier
        private static void SpeedBytes(byte a, byte b, byte c, byte d)
        {
            SetSpeed[0] = a;
            SetSpeed[1] = b;
            SetSpeed[2] = c;
            SetSpeed[3] = d;
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
            //Centers the axes
            if (e.GetPosition(eclipse1).Y > 125)
            {
                _radarY = ((float)e.GetPosition(eclipse1).Y - 125) / 2;
            }
            else if (e.GetPosition(eclipse1).Y < 125)
            {
                _radarY = ((float)e.GetPosition(eclipse1).Y - 125) / 2;
            }
            if (e.GetPosition(eclipse1).X > 125)
            {
                _radarX = ((float)e.GetPosition(eclipse1).X - 125) / 2;
            }
            else if (e.GetPosition(eclipse1).X < 125)
            {
                _radarX = ((float)e.GetPosition(eclipse1).X - 125) / 2;
            }
            textBox2.Text = Math.Round(_radarX, 1).ToString("N");
            textBox3.Text = Math.Round(_radarY, 1).ToString("N");
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
            _radarZJump = (float)Math.Floor(10 * e.NewValue);
            label2.Content = String.Format("{0} Yalms", _radarZJump);
        }

        //Slider for setting the button jump value, default is 3
        private void Slider_ValueChanged_2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _buttonJumpVal = (float)Math.Floor(10 * e.NewValue);
            label3.Content = String.Format("{0} Yalms", _buttonJumpVal);
        }

        //Slider for setting the last byte in Sprint
        private void Slider_ValueChanged_3(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _lastSpeedByte = (byte)Math.Truncate(12.7 * e.NewValue);
            label7.Content = String.Format("0x{0:X2}", _lastSpeedByte);
            SpeedBytes(SetSpeed[0], SetSpeed[1], SetSpeed[2], _lastSpeedByte);
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
            Config = XmlSerializationHelper.Deserialize<config>(InterProcessCom.ConfigPath);
            ConfigWrapper.RefreshAreaBox(AreaPOS_Box);
        }

        //Handler for changing the area
        private void AreaPOS_Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigWrapper.RefreshNameBox(NamePOS_Box, AreaPOS_Box);
        }

        //Handler for changing the POS name
        private void NamePOS_Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigWrapper.RefreshPOSBoxes(NamePOS_Box, AreaPOS_Box, NewPOS_X, NewPOS_Y, NewPOS_Z, POSKey_Zone, POSKey_Name);
        }

        //Handler for saving new cords
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ConfigWrapper.SaveCord(Area_Save, Name_Save, load_button);
        }

        //Handler for jumping
        private void Jump_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Player.WriteToPos("X", StringToFloat(NewPOS_X.Text));
                Player.WriteToPos("Y", StringToFloat(NewPOS_Y.Text));
                Player.WriteToPos("Z", StringToFloat(NewPOS_Z.Text));
            }
            catch
            {
                
            }
        }

        //Handler for saving HotKeys
        private void SaveHotkey(object sender, RoutedEventArgs e)
        {
            if ((bool)POSRadio.IsChecked)
            {
                ConfigWrapper.SaveHotKey(POSKbox, POSKmodbox, POSKey_Name, POSKey_Zone, ConfigWrapper.KeyType.POSKey);
            }
            else if ((bool)SpeedRadio.IsChecked)
            {
                ConfigWrapper.SaveHotKey(POSKbox, POSKmodbox, SpeedKey_Amount, SpeedKey_Direct, ConfigWrapper.KeyType.SpeedKey);
            }
            else if ((bool)MoveRadio.IsChecked)
            {
                ConfigWrapper.SaveHotKey(POSKbox, POSKmodbox, MoveKey_Dist, MoveKey_Direct, ConfigWrapper.KeyType.MoveKey);
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
            Opacity = e.NewValue / 10;
            percnt_txt.Text = Math.Floor(e.NewValue * 10) + "%";
        }

        //Always on top box
        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            Topmost = aot.IsChecked == true;
        }

        //Donation button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("http://goo.gl/VbcSxW");
            }
            catch
            {
            }
        }

        #endregion

        public static EntityWindow EWindow;
        public static GatherWindow GWindow;
        public static DevWindow DWindow;
      
        private void CheckBoxHandler(object sender, RoutedEventArgs e)
        {
            var cbox = (CheckBox) e.Source;
            var isChecked = cbox.IsChecked == true;
            switch (cbox.Content.ToString())
            {
                case "Dev Speed Window":
                    if (isChecked)
                        new Thread(new ThreadStart(delegate
                        {
                            DWindow = new DevWindow();
                            DWindow.ShowDialog();
                        })) {ApartmentState = ApartmentState.STA}.Start();
                    else
                        DWindow.Dispatcher.BeginInvoke(new Action(() => DWindow.Close()));
                    break;
                case "Entity Window":
                    if (isChecked)
                        new Thread(new ThreadStart(delegate
                        {
                            EWindow = new EntityWindow();
                            EWindow.ShowDialog();
                        })) {ApartmentState = ApartmentState.STA}.Start();
                    else
                        EWindow.Dispatcher.BeginInvoke(new Action(() => EWindow.Close()));
                    break;
                case "Gather/Nav Window":
                    if (isChecked)
                        new Thread(new ThreadStart(delegate
                        {
                            GWindow = new GatherWindow();
                            GWindow.ShowDialog();
                        })) {ApartmentState = ApartmentState.STA}.Start();
                    else
                        GWindow.Dispatcher.BeginInvoke(new Action(() => GWindow.Close()));
                    break;
                case "Debug Console":
                    if (isChecked)
                        AllocConsole();
                    else
                        FreeConsole();
                    break;
                case "XLock":
                    Player.LockAxis("X", isChecked);
                    break;
                case "YLock":
                    Player.LockAxis("Y", isChecked);
                    break;
                case "ZLock":
                    Player.LockAxis("Z", isChecked);
                    break;
                case "Auto Update":
                    if (isChecked)
                    {
                        _uchecked = true;
                        new Thread(UpdatedPosThread).Start();
                    }
                    else
                        _uchecked = false;
                    break;
                case "Hide Sprint":
                    Player.HideSprint(isChecked);
                    break;
                case "Disable Radar":
                    HideRadarMain.Visibility = isChecked
                        ? (HideRadarSub.Visibility = Visibility.Visible)
                        : (HideRadarSub.Visibility = Visibility.Hidden);
                    break;

            }
        }

        private static float StringToFloat(string theString)
        {
            return
                Convert.ToSingle(theString.Contains('.')
                    ? theString.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    : theString.Replace(",", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
        }

        private void ZoneGetOnclick(object sender, RoutedEventArgs e)
        {
            Area_Save.Text = Player.GetZoneByID();
        }

        private void ZoneBoxGetOnClick(object sender, RoutedEventArgs e)
        {
            AreaPOS_Box.SelectedIndex = AreaPOS_Box.Items.Cast<string>().ToList().FindIndex(p => p == Player.GetZoneByID());
        }
    }
}