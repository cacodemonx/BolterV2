using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ConfigHelper;

namespace BolterV2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 594, CharSet = CharSet.Ansi)]
    public struct PassInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string FilePath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string DomainName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 70)]
        public string InterProcClass;
        public int Raw;
    }
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
        
        private readonly ReflectMeBro _mapper = new ReflectMeBro();
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hModule;

            StartButton.IsEnabled = false;

            //Serialize configuration XML.
            var eradstyle = XmlSerializationHelper.Deserialize<config>("config.xml");

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
            var hProc = _mapper.OpenHan(0x001F0FFF, pid);

            //Check if the CLR is already loaded into the selected process.
            if (Process.GetProcessById(pid).Modules.Cast<ProcessModule>().Any(mod => mod.ModuleName == "clr.dll"))
            {
                hModule = eradstyle.MemInfo.First(id => id.ID == pid).hModule;
            }
            //CLR not loaded. Map new instance of the CLR, into the ffxiv process.
            else
            {
                hModule = _mapper.Inject(Properties.Resources.Link, hProc);
                if (hModule == IntPtr.Zero)
                {
                    MessageBox.Show("Something blocked Bolter from loading, Check any Virus Scanners, or Windows Restrictions");
                    StartButton.IsEnabled = true;
                    return;
                }
            }

            var mainNamespace = MainNamespaceOfPlugin(PluginsBox.SelectedItem.ToString());

            var pInfo = new PassInfo
            {
                DomainName = mainNamespace,
                FilePath = string.Format("{0}\\{1}.dll", Directory.GetCurrentDirectory(), PluginsBox.SelectedItem),
                Raw = 0,
                InterProcClass = string.Format("{0}.InterProcessCom", mainNamespace)
            };

            var ppInfo = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PassInfo)));

            Marshal.StructureToPtr(pInfo, ppInfo, true);

            // Allocate memory in ffxiv to hold the parameters struct.
            var pathPtr = _mapper.AllocMem(hProc, (uint)Marshal.SizeOf(typeof(PassInfo)), 0x1000 | 0x2000, 0x04);

            SigScan.WriteProcessMemory(hProc, pathPtr, ppInfo, (uint) Marshal.SizeOf(typeof(PassInfo)), new UIntPtr());

            Marshal.FreeHGlobal(ppInfo);

            // Get pointer for the Load Assembly function, inside our unmanaged CLR host DLL.
            var routinePtr = _mapper.GetFuncPointer(hProc, hModule, "LoadIt");

            // Remove old pids
            eradstyle.MemInfo.RemoveAll(pe => !pidList.Contains(pe.ID) || pe.ID == pid);

            // Add current pid.
            eradstyle.MemInfo.Add(new PastProcess {ID = pid, hModule = hModule});

            // Save configuration.
            XmlSerializationHelper.Serialize("config.xml",eradstyle);

            // Create remote thread in the selected ffxiv process starting at the Load Assembly routine.
            var ntThread = _mapper.CreateThread(hProc, routinePtr, pathPtr);
            
            // Wait for completion or 2000ms.
           _mapper.WaitForEvent(ntThread, 2000);

            // Close handles.
            _mapper.CloseHan(ntThread);
            _mapper.CloseHan(hProc);
            StartButton.IsEnabled = true;

        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            var ThePs = Process.GetProcessesByName("ffxiv");
            foreach (var process in ThePs)
            {
                var hProc = process.Handle;
                var sigScam = new SigScan(process, process.MainModule.BaseAddress + 0xFB9000, 0x14B000);
                byte[] playerStructSig = { 0x46, 0x69, 0x72, 0x65, 0x20, 0x53, 0x68, 0x61, 0x72, 0x64, 0x02, 0x13, 0x02, 0xEC, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 };
                var NamePtr = sigScam.FindPattern(playerStructSig, "xxxxxxxxxxxxxxxxxxxx", -(int)process.MainModule.BaseAddress) - 0xC26;
                playerName =
                    Encoding.ASCII.GetString(_mapper.ReadMemory(
                        hProc, process.MainModule.BaseAddress + (int)NamePtr, 21).TakeWhile(p => p != 0).ToArray());

                var newimg = (ImageSource)CreateBitmapSourceFromBitmap(Properties.Resources.ffxiv);
                ProcessListBox.Items.Add(new ListImg(string.Format("{0}\nPID - {1}", playerName, process.Id), newimg));
            }

            foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory()).Where(file => file.EndsWith(".dll")))
                PluginsBox.Items.Add(new string(file.Skip(Directory.GetCurrentDirectory().Length + 1).ToArray()).Replace(".dll", ""));
            PluginsBox.SelectedIndex = 0;
            ProcessListBox.SelectedIndex = 0;
        }

        private static string MainNamespaceOfPlugin(string pluginName)
        {
            var decoder = AppDomain.CreateDomain("Decode_Assembly", AppDomain.CurrentDomain.Evidence,
                new AppDomainSetup {PrivateBinPath = Directory.GetCurrentDirectory()});

            decoder.Load(pluginName);

            var ns = decoder.GetAssemblies()
                .Where(assem => assem.GetName().Name == pluginName)
                .SelectMany(assem => assem.GetTypes().Where(type => type.Name == "InterProcessCom"))
                .Select(type => type.Namespace)
                .FirstOrDefault();

            AppDomain.Unload(decoder);

            return ns;
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
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }


        #region Pseduo-Metro. This isn't the code your looking for.

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

        private void Donate(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("http://goo.gl/VbcSxW");
            }
            catch
            {
            }
        }
    }
    public class ListImg
    {
        public ListImg(string value, ImageSource img) { Str = value; Image = img; }
        public string Str { get; set; }
        public ImageSource Image { get; set; }
    }
#endregion

}
