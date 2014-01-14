using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ConfigHelper;
using InjectionLibrary;
using JLibrary.PortableExecutable;
using JLibrary.Win32;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace BolterV2
{
    /// <summary>
    /// Launcher program for BolterV2.
    /// 
    /// Manually maps an unmanaged CLR host into FFXIV, 
    /// loads the Bolter-XIV class library onto that host,
    /// and then instantiates the main Bolter-XIV class.
    /// </summary>
    public partial class MainWindow : Window
    {
        private string playerName;
        public MainWindow()
        {
            
            InitializeComponent();
        }
        
        //Start Button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hModule;
            StartButton.IsEnabled = false;
            //Serialize configuration XML.
            var eradstyle = XmlSerializationHelper.Deserialize<config>("confign.xml");
            //Make a List of ffxiv process IDs for later use.
            var pidList = Process.GetProcessesByName("ffxiv").Select(p => p.Id).ToList();
            //Check if we found any ffxiv processes running.
            if (!pidList.Any())
            {
                MessageBox.Show("No FFXIV process is active.");
                StartButton.IsEnabled = true;
                return;
            } 
            //Set our current pid.
            var pid = pidList[ProcessListBox.SelectedIndex];
            //Get handle for the selected ffxiv process.
            var hProc = Process.GetProcessById(pid).Handle;
            //Check if the CLR is already loaded into the selected process.
            if (Process.GetProcessById(pid).Modules.Cast<ProcessModule>().Any(mod => mod.ModuleName == "clr.dll"))
            {
                //Check if the Bolter Window is open.
                if (Process.GetProcesses().Any(process => process.MainWindowTitle == "Bolter-XIV"))
                {
                    MessageBox.Show("Bolter is already running.");
                    StartButton.IsEnabled = true;
                    return;
                }
                //Get base module address from the last saved instance.
                hModule = eradstyle.MemInfo.First(id => id.ID == pid).hModule;
            }
            //CLR not loaded. Map new instance of the CLR, into the ffxiv process.
            else
            {
                //Instantiate new injector class, set for manual mapping.
                using (var dllInjector = InjectionMethod.Create(InjectionMethodType.ManualMap))
                //Load unmanaged CLR host DLL, from resources, into memory.
                using (var img = new PortableExecutable(Properties.Resources.Link))
                //Map the DLL's raw data into the ffxiv process and all of it's dependencies.
                    hModule = dllInjector.Inject(img, pid);
            }
            uint bWritten;
            int hThread = 0;
            //Get byte array of the config.xml file path.
            byte[] configPathBytes = new ASCIIEncoding().GetBytes(Directory.GetCurrentDirectory() + "\\config.xml");
            //Allocate memory in ffxiv to hold the path.
            var pathPtr = WinAPI.VirtualAllocEx(hProc, (IntPtr)0, (uint)configPathBytes.Length, 0x1000 | 0x2000, 0x04);
            //Write path inside allocated space.
            WinAPI.WriteProcessMemory(hProc, pathPtr, configPathBytes, configPathBytes.Length, out bWritten);
            //Get pointer for the Load Assembly function, inside our unmanaged CLR host DLL.
            IntPtr routinePtr = WinAPI.GetProcAddressEx(hProc, hModule, "LoadIt");
            //Remove old pids
            eradstyle.MemInfo.RemoveAll(pe => !pidList.Contains(pe.ID) || pe.ID == pid);
            //Add current pid.
            eradstyle.MemInfo.Add(new PastProcess {ID = pid, hModule = hModule});
            //Save configuration.
            XmlSerializationHelper.Serialize("confign.xml",eradstyle);
            //Create remote thread in the selected ffxiv process starting at the Load Assembly routine.
            //Wait for completion or 2000ms.
            WinAPI.WaitForSingleObject(
                WinAPI.CreateRemoteThread(hProc, lpStartAddress: routinePtr, lpParameter: pathPtr, lpThreadId: hThread),
                2000);
            //Free Memory.
            WinAPI.VirtualFreeEx(hProc, pathPtr, 0, 0x8000);
            //Force Garbage Collection.
            GC.Collect();
            StartButton.IsEnabled = true;
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
           
            //Process[] ThePs = Process.GetProcessesByName("ffxiv");
            //foreach (var process in ThePs)
            //{
            //    IntPtr hProc = WinAPI.OpenProcess((uint)WinAPI.ProcessAccess.AllAccess, false, process.Id);
            //    var sigScam = new SigScan.SigScan(process, process.MainModule.BaseAddress + 0x119B000, 0x14B000);
            //    byte[] playerStructSig = { 0x46, 0x69, 0x72, 0x65, 0x20, 0x53, 0x68, 0x61, 0x72, 0x64, 0x02, 0x13, 0x02, 0xEC, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //    IntPtr NamePtr = sigScam.FindPattern(playerStructSig, "xxxxxxxxxxxxxxxxxxxx", -(int)process.MainModule.BaseAddress) - 0xB56;
            //    playerName = Encoding.ASCII.GetString(WinAPI.ReadRemoteMemory(hProc, (IntPtr)((int)process.MainModule.BaseAddress + (int)NamePtr), 21).TakeWhile(item => item != 0).ToArray());
            //    Box.Items.Add(string.Format("{0} - ID {1}",playerName,process.Id));
            //    WinAPI.CloseHandle(hProc);
            //    hProc = NamePtr = IntPtr.Zero;
            //    sigScam = null;
            //    GC.Collect();
            //}
            //ThePs = null;
            //GC.Collect();
            //Box.SelectedIndex = 0;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {
            }
        }

        private void MinimizeButton(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CloseBottonAnimation(object sender, MouseEventArgs e)
        {
            LinearGradientBrush b;
            if (e.RoutedEvent.Equals(MouseEnterEvent))
            {
                b = ((LinearGradientBrush)Resources["WindowFrameBrush2"]).CloneCurrentValue();
                b.GradientStops[0].Color = Colors.Aqua;
            }
            else
            {
                b = (LinearGradientBrush)Resources["WindowFrameBrush2"];

            }
            _CloseButton.Background = b;
            _CloseButton.BorderBrush = b;
            SpaceFix.Fill = b;
            SpaceFix.Stroke = b;
        }
        private void MinimizeBottonAnimation(object sender, MouseEventArgs e)
        {
            LinearGradientBrush b;
            if (e.RoutedEvent.Equals(MouseEnterEvent))
            {
                b = ((LinearGradientBrush)Resources["WindowFrameBrush2"]).CloneCurrentValue();
                b.GradientStops[0].Color = Colors.Aqua;
            }
            else
            {
                b = (LinearGradientBrush)Resources["WindowFrameBrush2"];
                
            }
            _MinimizeButton.Background = b;
            _MinimizeButton.BorderBrush = b;
            SpaceFix2.Fill = b;
            SpaceFix2.Stroke = b;
            SpaceFix3.Fill = b;
            SpaceFix3.Stroke = b;
        }

        public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");
            return Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private int clicknumber = 0;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            clicknumber++;
            var newimg = (ImageSource)CreateBitmapSourceFromBitmap(Properties.Resources.ffxiv);
            ProcessListBox.Items.Add(clicknumber == 1
                ? new ListImg("Usagi Tsukino\nPID - 3480", newimg)
                : new ListImg("Tomoe Hotaru\nPID - 8902", newimg));
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedIndex == 0)
            {
                ProcessesTab.Foreground = new SolidColorBrush(Colors.DarkCyan);
                AboutTab.Foreground = new SolidColorBrush(Colors.LightSlateGray);
            }
            else if (TabControl.SelectedIndex == 1)
            {
                ProcessesTab.Foreground = new SolidColorBrush(Colors.LightSlateGray);
                AboutTab.Foreground = new SolidColorBrush(Colors.DarkCyan);
            }
        }

        private void ProcessesTab_OnClick(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 0;
        }

        private void AboutTab_OnClick(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 1;
        }

        private void Tab_OnMouse(object sender, MouseEventArgs e)
        {
            if (e.RoutedEvent.Equals(MouseEnterEvent))
            {
                if (e.Source.Equals(ProcessesTab))
                {
                    ProcessesTab.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (e.Source.Equals(AboutTab))
                {
                    AboutTab.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
            else if (e.RoutedEvent.Equals(MouseLeaveEvent))
            {
                if (e.Source.Equals(ProcessesTab))
                {
                    ProcessesTab.Foreground = TabControl.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkCyan) : new SolidColorBrush(Colors.LightSlateGray);
                }
                else if (e.Source.Equals(AboutTab))
                {
                    AboutTab.Foreground = TabControl.SelectedIndex == 1 ? new SolidColorBrush(Colors.DarkCyan) : new SolidColorBrush(Colors.LightSlateGray);
                }
            }
        }
    }
    public class ListImg
    {
        public ListImg(string value, ImageSource img) { Str = value; Image = img; }
        public string Str { get; set; }
        public ImageSource Image { get; set; }
    }
    
}
