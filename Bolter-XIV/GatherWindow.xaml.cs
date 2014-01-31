using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Player_Bits;

namespace Bolter_XIV
{
    /// <summary>
    /// Interaction logic for GatherWindow.xaml
    /// </summary>
    public partial class GatherWindow : Window
    {
        public GatherWindow()
        {
            InitializeComponent();
        }

        GatherHelper _helper = new GatherHelper();
        
        private void ClickHandler(object sender, RoutedEventArgs e)
        {
            switch (((Button) e.Source).Content.ToString())
            {
                case "Record":
                    _helper.Record(int.Parse(InterText.Text), PathTextBox.Text, LoggingRecTog.IsChecked == true ? this : null);
                    StatusTextBlock.Text = "Recording";
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case "Stop":
                    _helper.StopRecord();
                    StatusTextBlock.Text = "Stopped";
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Blue);
                    break;
                case "Add Single":
                    _helper.Record(0, PathTextBox.Text, LoggingRecTog.IsChecked == true ? this : null);
                    break;
                case "Normal":
                    _helper.Play(SaveedPathsBox.SelectedItem.ToString(), NavLogTog.IsChecked == true ? this : null,
                        Pathing.Normal, ForwardTog.IsChecked == true);
                    break;
                case "At Index":
                    _helper.Play(SaveedPathsBox.SelectedItem.ToString(), this, Pathing.At_Index, ForwardTog.IsChecked == true,
                        int.Parse(IndexBox.Text));
                    break;
                case "Closest":
                    _helper.Play(SaveedPathsBox.SelectedItem.ToString(), this, Pathing.Closest, ForwardTog.IsChecked == true);
                    break;
                case "Halt":
                    Navigation.HaltFlag = true;
                    break;
                case "Remove":
                    Navigation._Waypoints.Zone.First(p => p.Name == Player.GetZoneByID())
                        .Path.RemoveAll(i => i.Name == SaveedPathsBox.SelectedItem.ToString());
                    _helper.Reload();
                    break;
                case "Refresh":
                    if (!SaveedPathsBox.Items.IsEmpty)
                        SaveedPathsBox.Items.Clear();
                    Navigation._Waypoints.Zone.First(p => p.Name == Player.GetZoneByID())
                        .Path.ForEach(i => SaveedPathsBox.Items.Add(i.Name));
                    SaveedPathsBox.SelectedIndex = 0;
                    break;

            }
        }

        public void AddText(int N, string zonename, string pathname, float x, float y)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                RecLog.AppendText(string.Format("{0} Zone: {1} Path: {2} X: {3} Y: {4}\u2028",
                    N, zonename, pathname, x, y));
                RecLog.ScrollToEnd();
            }));
        }

        private void TopDrag(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void RotDelayHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Navigation.HeadToll = (int)e.NewValue;
            headtolText.Text = string.Format("{0}ms",Navigation.HeadToll);
        }

        private void CorrectionDelayHandler(object sender, RoutedEventArgs e)
        {
            Navigation.CorDelay = ((CheckBox) e.Source).IsChecked == true;
        }

        private void GatherWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _helper = null;
            GC.Collect();
        }
    }
}
