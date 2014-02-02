using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Player_Bits;

namespace Bolter_XIV
{
    /// <summary>
    /// Interaction logic for EntityWindow.xaml
    /// </summary>
    unsafe public partial class EntityWindow : Window
    {
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
                    {EListBox.Items.Clear();}
                    catch{}
                    for (var i = 0; i < 64; i++)
                        if (entities[i].Name != "")
                            EListBox.Items.Add(new EntityListBoxItem(entities[i].Name, entities[i].ID));
                    break;
                case "Jump To":
                    Player.GetPlayer()->ServerX = float.Parse(XposBox.Text);
                    Player.GetPlayerSub()->CliX = float.Parse(XposBox.Text);

                    Player.GetPlayer()->ServerY = float.Parse(YposBox.Text);
                    Player.GetPlayerSub()->CliY = float.Parse(YposBox.Text);

                    Player.GetPlayer()->ServerZ = float.Parse(ZposBox.Text);
                    Player.GetPlayerSub()->CliZ = float.Parse(ZposBox.Text);
                    break;
                case "Update":
                    try
                    { NPCObjectListBox.Items.Clear(); }
                    catch { }
                    for (var i = 0; i < 22; i++)
                        if (new string(Player.bloop[i].NPC->Name) != "")
                        NPCObjectListBox.Items.Add(new NPCObjectListBoxItem(new string(Player.bloop[i].NPC->Name),
                            Player.bloop[i].NPC->X, Player.bloop[i].NPC->Y, Player.bloop[i].NPC->Z,
                            Player.bloop[i].NPC->IsActive == 0 ? "Up " : "Down ", Player.bloop[i].NPC->ID));
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
                        if (new string(Player.bloop[i].NPC->Name) != "")
                            NPCObjectListBox.Items.Add(new NPCObjectListBoxItem(new string(Player.bloop[i].NPC->Name),
                                Player.bloop[i].NPC->X, Player.bloop[i].NPC->Y, Player.bloop[i].NPC->Z,
                                Player.bloop[i].NPC->IsActive == 0 ? "Up " : "Down ", Player.bloop[i].NPC->ID));
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
            for (var i = 0; i < 0x18B500; i = i + 0x3F40)
                entities.Add(new Entity(i));
        }

        void RefreshBuffs()
        {
            ValidBlock.Visibility = Visibility.Hidden;
            try
            {
                var selectedEnt =
                    entities.FindIndex(p => p.ID == ((EntityListBoxItem)EListBox.SelectedItem).ID);
                var N = 0;
                foreach (var bloop in BuffsBox.Children.Cast<TextBlock>())
                {
                    switch (N)
                    {
                        case 0:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_1_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_1_Timer.ToString("N1");
                            N++;
                            break;
                        case 1:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_2_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_2_Timer.ToString("N1");
                            N++;
                            break;
                        case 2:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_3_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_3_Timer.ToString("N1");
                            N++;
                            break;
                        case 3:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_4_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_4_Timer.ToString("N1");
                            N++;
                            break;
                        case 4:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_5_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_5_Timer.ToString("N1");
                            N++;
                            break;
                        case 5:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_6_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_6_Timer.ToString("N1");
                            N++;
                            break;
                        case 6:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_7_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_7_Timer.ToString("N1");
                            N++;
                            break;
                        case 7:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_8_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_8_Timer.ToString("N1");
                            N++;
                            break;
                        case 8:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_9_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_9_Timer.ToString("N1");
                            N++;
                            break;
                        case 9:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_10_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_10_Timer.ToString("N1");
                            N++;
                            break;
                        case 10:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_11_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_11_Timer.ToString("N1");
                            N++;
                            break;
                        case 11:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_12_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_12_Timer.ToString("N1");
                            N++;
                            break;
                        case 12:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_13_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_13_Timer.ToString("N1");
                            N++;
                            break;
                        case 13:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_14_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_14_Timer.ToString("N1");
                            N++;
                            break;
                        case 14:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_15_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_15_Timer.ToString("N1");
                            N++;
                            break;
                        case 15:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_16_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_16_Timer.ToString("N1");
                            N++;
                            break;
                        case 16:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_17_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_17_Timer.ToString("N1");
                            N++;
                            break;
                        case 17:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_18_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_18_Timer.ToString("N1");
                            N++;
                            break;
                        case 18:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_19_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_19_Timer.ToString("N1");
                            N++;
                            break;
                        case 19:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_20_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_20_Timer.ToString("N1");
                            N++;
                            break;
                        case 20:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_21_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_21_Timer.ToString("N1");
                            N++;
                            break;
                        case 21:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_22_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_22_Timer.ToString("N1");
                            N++;
                            break;
                        case 22:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_23_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_23_Timer.ToString("N1");
                            N++;
                            break;
                        case 23:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_24_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_24_Timer.ToString("N1");
                            N++;
                            break;
                        case 24:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_25_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_25_Timer.ToString("N1");
                            N++;
                            break;
                        case 25:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_26_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_26_Timer.ToString("N1");
                            N++;
                            break;
                        case 26:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_27_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_27_Timer.ToString("N1");
                            N++;
                            break;
                        case 27:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_28_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_28_Timer.ToString("N1");
                            N++;
                            break;
                        case 28:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_29_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_29_Timer.ToString("N1");
                            N++;
                            break;
                        case 29:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_30_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_30_Timer.ToString("N1");
                            N++;
                            break;
                    }
                    
                }
                XposBox.Text = entities[selectedEnt].X.ToString();
                YposBox.Text = entities[selectedEnt].Y.ToString();
                ZposBox.Text = entities[selectedEnt].Z.ToString();
                if (TargetCheckBox.IsChecked == true)
                    Player.MasterPtr->Target->TargetID = entities[selectedEnt].EntityID;
            }
            catch
            {
                ValidBlock.Visibility = Visibility.Visible;
            }
        }
        List<Entity> entities = new List<Entity>();
        class Entity
        {
            public uint EntityID
            {
                get { return (uint) baseStructure; }
            }
            private Player.PlayerStructure* baseStructure {
                get { return (Player.PlayerStructure*) (((int) *Player.BasePlayerAddress) + offset); }
            }
            private readonly int offset;
            public Entity(int Offset)
            {
                offset = Offset;
            }

            public string Name
            {
                get { return new string(baseStructure->Name); }
            }
            public float X
            {
                get { return baseStructure->ServerX; }
            }
            public float Y
            {
                get { return baseStructure->ServerY; }
            }
            public float Z
            {
                get { return baseStructure->ServerZ; }
            }
            public uint ID
            {
                get { return baseStructure->ID; }
            }
            public UInt16 Buff_1_ID {get { return baseStructure->Buffs.Buff1.ID; }}
            public float Buff_1_Timer { get { return baseStructure->Buffs.Buff1.Timer; } }
            public UInt16 Buff_2_ID { get { return baseStructure->Buffs.Buff2.ID; } }
            public float Buff_2_Timer { get { return baseStructure->Buffs.Buff2.Timer; } }
            public UInt16 Buff_3_ID { get { return baseStructure->Buffs.Buff3.ID; } }
            public float Buff_3_Timer { get { return baseStructure->Buffs.Buff3.Timer; } }
            public UInt16 Buff_4_ID { get { return baseStructure->Buffs.Buff4.ID; } }
            public float Buff_4_Timer { get { return baseStructure->Buffs.Buff4.Timer; } }
            public UInt16 Buff_5_ID { get { return baseStructure->Buffs.Buff5.ID; } }
            public float Buff_5_Timer { get { return baseStructure->Buffs.Buff5.Timer; } }
            public UInt16 Buff_6_ID { get { return baseStructure->Buffs.Buff6.ID; } }
            public float Buff_6_Timer { get { return baseStructure->Buffs.Buff6.Timer; } }
            public UInt16 Buff_7_ID { get { return baseStructure->Buffs.Buff7.ID; } }
            public float Buff_7_Timer { get { return baseStructure->Buffs.Buff7.Timer; } }
            public UInt16 Buff_8_ID { get { return baseStructure->Buffs.Buff8.ID; } }
            public float Buff_8_Timer { get { return baseStructure->Buffs.Buff8.Timer; } }
            public UInt16 Buff_9_ID { get { return baseStructure->Buffs.Buff9.ID; } }
            public float Buff_9_Timer { get { return baseStructure->Buffs.Buff9.Timer; } }
            public UInt16 Buff_10_ID { get { return baseStructure->Buffs.Buff10.ID; } }
            public float Buff_10_Timer { get { return baseStructure->Buffs.Buff10.Timer; } }
            public UInt16 Buff_11_ID { get { return baseStructure->Buffs.Buff11.ID; } }
            public float Buff_11_Timer { get { return baseStructure->Buffs.Buff11.Timer; } }
            public UInt16 Buff_12_ID { get { return baseStructure->Buffs.Buff12.ID; } }
            public float Buff_12_Timer { get { return baseStructure->Buffs.Buff12.Timer; } }
            public UInt16 Buff_13_ID { get { return baseStructure->Buffs.Buff13.ID; } }
            public float Buff_13_Timer { get { return baseStructure->Buffs.Buff13.Timer; } }
            public UInt16 Buff_14_ID { get { return baseStructure->Buffs.Buff14.ID; } }
            public float Buff_14_Timer { get { return baseStructure->Buffs.Buff14.Timer; } }
            public UInt16 Buff_15_ID { get { return baseStructure->Buffs.Buff15.ID; } }
            public float Buff_15_Timer { get { return baseStructure->Buffs.Buff15.Timer; } }
            public UInt16 Buff_16_ID { get { return baseStructure->Buffs.Buff16.ID; } }
            public float Buff_16_Timer { get { return baseStructure->Buffs.Buff16.Timer; } }
            public UInt16 Buff_17_ID { get { return baseStructure->Buffs.Buff17.ID; } }
            public float Buff_17_Timer { get { return baseStructure->Buffs.Buff17.Timer; } }
            public UInt16 Buff_18_ID { get { return baseStructure->Buffs.Buff18.ID; } }
            public float Buff_18_Timer { get { return baseStructure->Buffs.Buff18.Timer; } }
            public UInt16 Buff_19_ID { get { return baseStructure->Buffs.Buff19.ID; } }
            public float Buff_19_Timer { get { return baseStructure->Buffs.Buff19.Timer; } }
            public UInt16 Buff_20_ID { get { return baseStructure->Buffs.Buff20.ID; } }
            public float Buff_20_Timer { get { return baseStructure->Buffs.Buff20.Timer; } }
            public UInt16 Buff_21_ID { get { return baseStructure->Buffs.Buff21.ID; } }
            public float Buff_21_Timer { get { return baseStructure->Buffs.Buff21.Timer; } }
            public UInt16 Buff_22_ID { get { return baseStructure->Buffs.Buff22.ID; } }
            public float Buff_22_Timer { get { return baseStructure->Buffs.Buff22.Timer; } }
            public UInt16 Buff_23_ID { get { return baseStructure->Buffs.Buff23.ID; } }
            public float Buff_23_Timer { get { return baseStructure->Buffs.Buff23.Timer; } }
            public UInt16 Buff_24_ID { get { return baseStructure->Buffs.Buff24.ID; } }
            public float Buff_24_Timer { get { return baseStructure->Buffs.Buff24.Timer; } }
            public UInt16 Buff_25_ID { get { return baseStructure->Buffs.Buff25.ID; } }
            public float Buff_25_Timer { get { return baseStructure->Buffs.Buff25.Timer; } }
            public UInt16 Buff_26_ID { get { return baseStructure->Buffs.Buff26.ID; } }
            public float Buff_26_Timer { get { return baseStructure->Buffs.Buff26.Timer; } }
            public UInt16 Buff_27_ID { get { return baseStructure->Buffs.Buff27.ID; } }
            public float Buff_27_Timer { get { return baseStructure->Buffs.Buff27.Timer; } }
            public UInt16 Buff_28_ID { get { return baseStructure->Buffs.Buff28.ID; } }
            public float Buff_28_Timer { get { return baseStructure->Buffs.Buff28.Timer; } }
            public UInt16 Buff_29_ID { get { return baseStructure->Buffs.Buff29.ID; } }
            public float Buff_29_Timer { get { return baseStructure->Buffs.Buff29.Timer; } }
            public UInt16 Buff_30_ID { get { return baseStructure->Buffs.Buff30.ID; } }
            public float Buff_30_Timer { get { return baseStructure->Buffs.Buff30.Timer; } }
        }

        private void EListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshBuffs();
        }

        private void EntityWindow_OnClosing(object sender, CancelEventArgs e)
        {
            entities.Clear();
            entities = null;
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox) e.Source).IsChecked == true)
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
            Coords = string.Format(" X: {0} Y: {1} Z: {2} ",oX,oY,oZ);
            IsActive = oActive;
            ID = oID.ToString("X");
            IsActiveColor = IsActive == "Up " ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
            Distance = string.Format(" {0}",Navigation.Distance(Player.GetPos(Player.Axis.Y), Player.GetPos(Player.Axis.X), oY, oX));
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
