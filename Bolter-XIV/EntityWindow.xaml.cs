using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Bolter_XIV
{
    /// <summary>
    /// Interaction logic for EntityWindow.xaml
    /// </summary>
    unsafe public partial class EntityWindow : Window
    {
        //private static private static NativeMethods Game

        private static NativeMethods Game
        {
            get { return InterProcessCom.Game; }
        }

        public EntityWindow()
        {
            InitializeComponent();
        }

        private void ClickHandler(object sender, RoutedEventArgs e)
        {
            switch (((Button)e.Source).Content.ToString())
            {
                case "--":
                    WindowState = WindowState.Minimized;
                    break;
                case "X":
                    Visibility = Visibility.Hidden;
                    break;
                case "Get List":
                    try
                    { EListBox.Items.Clear(); }
                    catch { }
                    for (var i = 0; i < 64; i++)
                        if (Game.PCMobName(i) != "")
                            EListBox.Items.Add(new EntityListBoxItem(Game.PCMobName(i), Game.PCMobEntity[i].PCMob->ID));
                    break;
                case "Jump To":
                    Game.PCMobEntity[0].PCMob->X = float.Parse(XposBox.Text);
                    Game.PCMobEntity[0].PCMob->subStruct->X = float.Parse(XposBox.Text);

                    Game.PCMobEntity[0].PCMob->Y = float.Parse(YposBox.Text);
                    Game.PCMobEntity[0].PCMob->subStruct->Y = float.Parse(YposBox.Text);

                    Game.PCMobEntity[0].PCMob->Z = float.Parse(ZposBox.Text);
                    Game.PCMobEntity[0].PCMob->subStruct->Z = float.Parse(ZposBox.Text);
                    break;
                case "Update":
                    try
                    { NPCObjectListBox.Items.Clear(); }
                    catch { }
                    for (var i = 0; i < 22; i++)
                        if (new string(Game.ObjectEntity[i].Object->Name) != "")
                            NPCObjectListBox.Items.Add(new NPCObjectListBoxItem(new string(Game.ObjectEntity[i].Object->Name),
                            Game.ObjectEntity[i].Object->X, Game.ObjectEntity[i].Object->Y, Game.ObjectEntity[i].Object->Z,
                            Game.ObjectEntity[i].Object->IsActive == 0 ? "Up " : "Down ", Game.ObjectEntity[i].Object->ID));
                    break;
                case "Face":
                    break;
            }

        }

        private bool UpdateLoop = false;
        private void UpdatedGatherList()
        {
            while (UpdateLoop)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    try
                    {
                        NPCObjectListBox.Items.Clear();
                    }
                    catch
                    {
                    }
                    for (var i = 0; i < 22; i++)
                        if (new string(Game.ObjectEntity[i].Object->Name) != "")
                            NPCObjectListBox.Items.Add(new NPCObjectListBoxItem(new string(Game.ObjectEntity[i].Object->Name),
                            Game.ObjectEntity[i].Object->X, Game.ObjectEntity[i].Object->Y, Game.ObjectEntity[i].Object->Z,
                            Game.ObjectEntity[i].Object->IsActive == 0 ? "Up " : "Down ", Game.ObjectEntity[i].Object->ID));
                }));
                Thread.Sleep(500);
            }
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

        private void EntityWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        void RefreshBuffs()
        {
            ValidBlock.Visibility = Visibility.Hidden;
            try
            {
                Console.WriteLine(((EntityListBoxItem)EListBox.SelectedItem).ID);

                var selectedEnt = Game.PCMobEntity.ToList().FindIndex(p => p.PCMob->ID == ((EntityListBoxItem)EListBox.SelectedItem).ID);

                Console.WriteLine(Game.PCMobEntity[selectedEnt].PCMob->ID);

                var N = 0;
                foreach (var block in BuffsBox.Children.Cast<TextBlock>())
                {
                    
                    block.Text =
                        Game.GetBuff(((NativeStructs.BuffStruct)
                            Game.PCMobEntity[selectedEnt].PCMob->Buffs.GetType()
                                .GetField(string.Format("_{0}", N + 1))
                                .GetValue(Game.PCMobEntity[selectedEnt].PCMob->Buffs)).ID);

                    BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                        ((NativeStructs.BuffStruct)
                            Game.PCMobEntity[selectedEnt].PCMob->Buffs.GetType()
                                .GetField(string.Format("_{0}", N + 1))
                                .GetValue(Game.PCMobEntity[selectedEnt].PCMob->Buffs)).Timer
                            .ToString(); 
                    N++;
                    if (N == 30)
                        break;
                    
                }
                XposBox.Text = Game.PCMobEntity[selectedEnt].PCMob->X.ToString();
                YposBox.Text = Game.PCMobEntity[selectedEnt].PCMob->Y.ToString();
                ZposBox.Text = Game.PCMobEntity[selectedEnt].PCMob->Z.ToString();
                if (TargetCheckBox.IsChecked == true)
                    NativeStructs.MasterPtr->Target->TargetID = (uint)Game.PCMobEntity[selectedEnt].PCMob;
            }
            catch(Exception ex)
            {
                ValidBlock.Visibility = Visibility.Visible;
                Console.WriteLine(ex.Message);
            }
        }
       
        private void EListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshBuffs();
        }

        private void EntityWindow_OnClosing(object sender, CancelEventArgs e)
        {
            
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)e.Source).IsChecked == true)
            {
                UpdateLoop = true;
                new Thread(UpdatedGatherList).Start();
            }
            else
                UpdateLoop = false;

        }
    }
    public class NPCObjectListBoxItem
    {
        public NPCObjectListBoxItem(string oName, float oX, float oY, float oZ, string oActive, uint oID)
        {
            Name = oName;
            Coords = string.Format(" X: {0} Y: {1} Z: {2} ", oX, oY, oZ);
            IsActive = oActive;
            ID = oID.ToString("X");
            IsActiveColor = IsActive == "Up " ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
            Distance = string.Format(" {0}", Navigation.Distance(InterProcessCom.Game.GetPos(Axis.Y), InterProcessCom.Game.GetPos(Axis.X), oY, oX));
        }
        public string Name { get; set; }
        public string Coords { get; set; }
        public string IsActive { get; set; }
        public string ID { get; set; }
        public SolidColorBrush IsActiveColor { get; set; }
        public string Distance { get; set; }
    }
    public class EntityListBoxItem
    {
        public EntityListBoxItem(string oName, uint ID)
        {
            Name = oName;
            this.ID = ID;
        }
        public string Name { get; set; }
        public uint ID { get; set; }
    }
}
