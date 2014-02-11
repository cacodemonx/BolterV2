using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Bolter_XIV
{
    /// <summary>
    /// Interaction logic for DevWindow.xaml
    /// </summary>
    public partial class DevWindow : Window
    {
        public DevWindow()
        {
            InitializeComponent();
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LinkSlider.Maximum = 100;
            BackSlider.Maximum = 100;
            ForwardSlider.Maximum = 100;
            SideSlider.Maximum = 100;
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
            Visibility = Visibility.Hidden;
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

        private static NativeMethods Game
        {
            get { return InterProcessCom.Game; }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            switch (((Button)e.Source).Content.ToString())
            {
                case "Get Current":
                    unsafe
                    {
                        ForwardSlider.Value = Game.MovementAdj->ForwardSpeed;
                        SideSlider.Value = Game.MovementAdj->LeftRightSpeed;
                        BackSlider.Value = Game.MovementAdj->BackwardSpeed;
                    }
                    break;
                case "Sprint":
                    ForwardSlider.Value = 8d;
                    SideSlider.Value = 8d;
                    BackSlider.Value = 3.12d;
                    break;
                case "Normal":
                    ForwardSlider.Value = 6d;
                    SideSlider.Value = 6d;
                    BackSlider.Value = 2.4d;
                    break;
            }
        }
        unsafe private void SetClick(object sender, RoutedEventArgs e)
        {
            switch (((Button)e.Source).Name)
            {
                case "AreialGlide":
                    Game.PCMobEntity[0].PCMob->CamGlide = float.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(0).Text);
                    break;
                case "AreialStatic":
                    Game.PCMobEntity[0].PCMob->StaticCamGlide = float.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(1).Text);
                    break;
                case "StatusMod":
                    Game.PCMobEntity[0].PCMob->StatusAdjust = uint.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(2).Text);
                    break;
                case "BodyParts":
                    Game.PCMobEntity[0].PCMob->subStruct->DisplayedBody = uint.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(3).Text);
                    Console.WriteLine(SetGrid.Children.OfType<TextBox>().ElementAt(3).Text);
                    break;
                case "CharHeight":
                    Game.PCMobEntity[0].PCMob->subStruct->PlayerHieght = float.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(4).Text);
                    break;
                case "CharWidth":
                    Game.PCMobEntity[0].PCMob->subStruct->PlayerWidth = float.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(5).Text);
                    break;
                case "CharGirth":
                    Game.PCMobEntity[0].PCMob->subStruct->PlayerGirth = float.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(6).Text);
                    break;
                case "CharSize":
                    Game.PCMobEntity[0].PCMob->subStruct->PlayerSize = float.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(7).Text);
                    break;
                case "CharSizeNoCam":
                    Game.PCMobEntity[0].PCMob->subStruct->PlayerSizeNoCam = float.Parse(SetGrid.Children.OfType<TextBox>().ElementAt(8).Text);
                    break;
            }
        }
        private void LockSpeed_Changed(object sender, RoutedEventArgs e)
        {
            
        }

        private void Slider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            unsafe
            {
                switch (((Slider)e.Source).Name)
                {
                    case "LinkSlider":
                        LinkedText.Text = e.NewValue.ToString("F");
                        ForwardSlider.Value = LinkSlider.Value;
                        SideSlider.Value = LinkSlider.Value;
                        BackSlider.Value = LinkSlider.Value >= 2.4d ? LinkSlider.Value - 2.4d : LinkSlider.Value;
                        break;
                    case "ForwardSlider":
                        ForwardText.Text = e.NewValue.ToString("F");
                        Game.MovementAdj->ForwardSpeed = (float)ForwardSlider.Value;
                        break;
                    case "BackSlider":
                        BackText.Text = e.NewValue.ToString("F");
                        Game.MovementAdj->BackwardSpeed = (float)BackSlider.Value;
                        break;
                    case "SideSlider":
                        SideText.Text = e.NewValue.ToString("F");
                        Game.MovementAdj->LeftRightSpeed = (float)SideSlider.Value;
                        break;
                }
            }
        }

        private void CheckBoxHandler(object sender, RoutedEventArgs e)
        {
            var cbox = (CheckBox) e.Source;
            var isChecked = cbox.IsChecked == true;
            switch (cbox.Content.ToString())
            {
                case "Collision Detection Off":
                    Game.CollsionToggle(isChecked);
                    break;
            }
        }
    }
}
