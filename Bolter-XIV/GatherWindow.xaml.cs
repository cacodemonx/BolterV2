using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Bolter_XIV
{
    /// <summary>
    /// Interaction logic for GatherWindow.xaml
    /// </summary>
    public partial class GatherWindow : Window
    {
        private readonly static NativeMethods Game = InterProcessCom.Game;

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
                    Navigation._Waypoints.Zone.First(p => p.Name == Game.CurrentZone)
                        .Path.RemoveAll(i => i.Name == SaveedPathsBox.SelectedItem.ToString());
                    _helper.Reload();
                    break;
                case "Refresh":
                    if (!SaveedPathsBox.Items.IsEmpty)
                        SaveedPathsBox.Items.Clear();
                    Navigation._Waypoints.Zone.First(p => p.Name == Game.CurrentZone)
                        .Path.ForEach(i => SaveedPathsBox.Items.Add(i.Name));
                    SaveedPathsBox.SelectedIndex = 0;
                    break;
                case "X":
                    Close();
                    break;
                case "─":
                    WindowState = WindowState.Minimized;
                    break;

            }
        }

        public void AddTextRec(int N, string zonename, string pathname, float x, float y)
        {
            Dispatcher.BeginInvoke(new Action(
                () => RecLog.AppendText(string.Format("{0} Zone: {1} Path: {2} X: {3} Y: {4}\u2028",
                    N, zonename, pathname, x, y))));
        }
        public void AddTextNav(int N, string zonename, string pathname, float x, float y)
        {
            Dispatcher.BeginInvoke(new Action(
                () => NavLog.AppendText(string.Format("Path: {0} X: {1} Y: {2}\u2028",
                    pathname, x, y))));
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
            switch (((CheckBox)e.Source).Content.ToString())
            {
                case "AI Navigation":
                    Navigation.AICorrection = ((CheckBox)e.Source).IsChecked == true;
                    break;
                case "Correction delay":
                    Navigation.CorDelay = ((CheckBox)e.Source).IsChecked == true;
                    break;
                case "Turn Filter":
                    Navigation.TurnFilter = ((CheckBox)e.Source).IsChecked == true;
                    break;
            }
        }

        private void GatherWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _helper = null;
            GC.Collect();
        }

        private void Log_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ((RichTextBox)e.Source).ScrollToEnd();
        }
    }
}
