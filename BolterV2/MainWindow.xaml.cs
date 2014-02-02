using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        private string log;
        //Start Button
        unsafe private void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = false;
            try
            {
                File.Delete("BolterLog.txt");
            }
            catch
            {
            }
            var derpStream = new FileStream("BolterLog.txt",FileMode.Append);

            IntPtr hModule;
            log = string.Format("hModule defined {0}\n",IsUserAdministrator());
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            result = StartButton.IsEnabled = false;
            log = string.Format("Button held {0}\n",result);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);
            //Serialize configuration XML.
            var eradstyle = XmlSerializationHelper.Deserialize<config>("config.xml");
            log = string.Format("Load XML {0}\n",eradstyle.saved_cords.Count);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Make a List of ffxiv process IDs for later use.
            var pidList = Process.GetProcessesByName("ffxiv").Select(p => p.Id).ToList();
            log = string.Format("Load XIV processes {0}\n",pidList.Count);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Check if we found any ffxiv processes running.
            if (!pidList.Any())
            {
                MessageBox.Show("No FFXIV process is active.");
                StartButton.IsEnabled = true;
                return;
            }
            log = string.Format("Check if running \n");
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Set our current pid.
            var pid = pidList[ProcessListBox.SelectedIndex];
            log = string.Format("Get Current process {0}\n",pid);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Get handle for the selected ffxiv process.
            var hProc = Process.GetProcessById(pid).Handle;
            log = string.Format("Get handle {0}\n",hProc);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Check if the CLR is already loaded into the selected process.
            if (Process.GetProcessById(pid).Modules.Cast<ProcessModule>().Any(mod => mod.ModuleName == "clr.dll"))
            {
                //Check if the Bolter Window is open.
                if (Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero).Any(process => process.MainWindowTitle == "Bolter-XIV"))
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
                log = string.Format("Load Bolter into FFXIV \n");
                derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);
                //Instantiate new injector class, set for manual mapping.
                using (var dllInjector = InjectionMethod.Create(InjectionMethodType.ManualMap))
                //Load unmanaged CLR host DLL, from resources, into memory.
                using (var img = new PortableExecutable(Properties.Resources.Link))
                //Map the DLL's raw data into the ffxiv process and all of it's dependencies.
                    hModule = dllInjector.Inject(img, pid);
                if (hModule == IntPtr.Zero)
                {
                    MessageBox.Show("Something blocked Bolter from loading, Check any Virus Scanners, or Windows Restrictions");
                    StartButton.IsEnabled = true;
                    return;
                }
                log = string.Format("Loaded in {0}\n",hModule);
                derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);
            }
            uint bWritten;
            int hThread = 0;
            //Get byte array of the config.xml file path.
            byte[] configPathBytes = new ASCIIEncoding().GetBytes(Directory.GetCurrentDirectory() + "\\config.xml");
            log = string.Format("Get path to config {0}\n",configPathBytes.Length);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Allocate memory in ffxiv to hold the path.
            var pathPtr = WinAPI.VirtualAllocEx(hProc, (IntPtr)0, (uint)configPathBytes.Length, 0x1000 | 0x2000, 0x04);
            log = string.Format("Allocate memory to hold path {0}\n",pathPtr);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Write path inside allocated space.
            WinAPI.WriteProcessMemory(hProc, pathPtr, configPathBytes, configPathBytes.Length, out bWritten);
            log = string.Format("Write path {0}\n",bWritten);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Get pointer for the Load Assembly function, inside our unmanaged CLR host DLL.
            IntPtr routinePtr = WinAPI.GetProcAddressEx(hProc, hModule, "LoadIt");
            log = string.Format("Get load pointer {0}\n",routinePtr);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Remove old pids
            eradstyle.MemInfo.RemoveAll(pe => !pidList.Contains(pe.ID) || pe.ID == pid);
            log = string.Format("Remove any unused pointers \n");
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Add current pid.
            eradstyle.MemInfo.Add(new PastProcess {ID = pid, hModule = hModule});
            log = string.Format("Add current process to XML \n");
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Save configuration.
            XmlSerializationHelper.Serialize("config.xml",eradstyle);
            log = string.Format("Save XML \n");
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Create remote thread in the selected ffxiv process starting at the Load Assembly routine.
            //Wait for completion or 2000ms.
            log = string.Format("Call load pointer \n");
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            var ntThread = new IntPtr();

            var ntstatus = WinAPI.NtCreateThreadEx(&ntThread, 0x1FFFFF, null, hProc, routinePtr,
                (void *)pathPtr, false, 0, null,
                null, null);

            WinAPI.WaitForSingleObject(ntThread, 2000);
            log = string.Format("Free unused memory {0:X}\n",ntstatus);
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);

            //Free Memory.
            WinAPI.VirtualFreeEx(hProc, pathPtr, 0, 0x8000);
            //Force Garbage Collection.
            GC.Collect();
            StartButton.IsEnabled = true;
            log = string.Format("Re-enable button \n");
            derpStream.Write(Encoding.Default.GetBytes(log), 0, log.Length);
            derpStream.Dispose();
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

            var ThePs = Process.GetProcessesByName("ffxiv");
            foreach (var process in ThePs)
            {
                var hProc = WinAPI.OpenProcess((uint)WinAPI.ProcessAccess.AllAccess, false, process.Id);
                var sigScam = new SigScan.SigScan(process, process.MainModule.BaseAddress + 0x119B000, 0x14B000);
                byte[] playerStructSig = { 0x46, 0x69, 0x72, 0x65, 0x20, 0x53, 0x68, 0x61, 0x72, 0x64, 0x02, 0x13, 0x02, 0xEC, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 };
                var NamePtr = sigScam.FindPattern(playerStructSig, "xxxxxxxxxxxxxxxxxxxx", -(int)process.MainModule.BaseAddress) - 0xB56;
                playerName = Encoding.ASCII.GetString(WinAPI.ReadRemoteMemory(hProc, (IntPtr)((int)process.MainModule.BaseAddress + (int)NamePtr), 21).TakeWhile(item => item != 0).ToArray());
                var newimg = (ImageSource)CreateBitmapSourceFromBitmap(Properties.Resources.ffxiv);
                ProcessListBox.Items.Add(new ListImg(string.Format("{0}\nPID - {1}", playerName, process.Id), newimg));
                WinAPI.CloseHandle(hProc);
                hProc = NamePtr = IntPtr.Zero;
                sigScam = null;
                GC.Collect();
            }
            ThePs = null;
            GC.Collect();
        }
        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                //get the currently logged in user
                var user = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
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

        private void refreshbutt(object sender, RoutedEventArgs e)
        {
            if (ProcessListBox.Items.Cast<ListImg>().Any())
                ProcessListBox.Items.Clear();

            var ThePs = Process.GetProcessesByName("ffxiv");
            foreach (var process in ThePs)
            {
                var hProc = WinAPI.OpenProcess((uint)WinAPI.ProcessAccess.AllAccess, false, process.Id);
                var sigScam = new SigScan.SigScan(process, process.MainModule.BaseAddress + 0x119B000, 0x14B000);
                byte[] playerStructSig = { 0x46, 0x69, 0x72, 0x65, 0x20, 0x53, 0x68, 0x61, 0x72, 0x64, 0x02, 0x13, 0x02, 0xEC, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 };
                var NamePtr = sigScam.FindPattern(playerStructSig, "xxxxxxxxxxxxxxxxxxxx", -(int)process.MainModule.BaseAddress) - 0xB56;
                playerName = Encoding.ASCII.GetString(WinAPI.ReadRemoteMemory(hProc, (IntPtr)((int)process.MainModule.BaseAddress + (int)NamePtr), 21).TakeWhile(item => item != 0).ToArray());
                var newimg = (ImageSource)CreateBitmapSourceFromBitmap(Properties.Resources.ffxiv);
                ProcessListBox.Items.Add(new ListImg(string.Format("{0}\nPID - {1}", playerName, process.Id), newimg));
                WinAPI.CloseHandle(hProc);
                hProc = NamePtr = IntPtr.Zero;
                sigScam = null;
                GC.Collect();
            }
            ThePs = null;
            GC.Collect();
        }
    }
    public class ListImg
    {
        public ListImg(string value, ImageSource img) { Str = value; Image = img; }
        public string Str { get; set; }
        public ImageSource Image { get; set; }
    }
    
}
