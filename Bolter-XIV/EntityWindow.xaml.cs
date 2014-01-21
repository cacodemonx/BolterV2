using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
                        EListBox.Items.Add(new ListBoxItem {Content = entities[i].Name, FontSize = 10});
                    break;
                case "Jump To":
                    Player.GetPlayer()->ServerX = float.Parse(XposBox.Text);
                    Player.GetPlayerSub()->CliX = float.Parse(XposBox.Text);

                    Player.GetPlayer()->ServerY = float.Parse(YposBox.Text);
                    Player.GetPlayerSub()->CliY = float.Parse(YposBox.Text);

                    Player.GetPlayer()->ServerZ = float.Parse(ZposBox.Text);
                    Player.GetPlayerSub()->CliZ = float.Parse(ZposBox.Text);
                    break;
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
            try
            {
                var selectedEnt =
                    entities.FindIndex(p => p.Name == ((ListBoxItem) EListBox.SelectedItem).Content.ToString());
                var N = 0;
                foreach (var bloop in BuffsBox.Children.Cast<TextBlock>())
                {
                    switch (N)
                    {
                        case 0:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_1_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_1_Timmer.ToString("N1");
                            N++;
                            break;
                        case 1:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_2_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_2_Timmer.ToString("N1");
                            N++;
                            break;
                        case 2:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_3_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_3_Timmer.ToString("N1");
                            N++;
                            break;
                        case 3:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_4_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_4_Timmer.ToString("N1");
                            N++;
                            break;
                        case 4:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_5_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_5_Timmer.ToString("N1");
                            N++;
                            break;
                        case 5:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_6_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_6_Timmer.ToString("N1");
                            N++;
                            break;
                        case 6:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_7_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_7_Timmer.ToString("N1");
                            N++;
                            break;
                        case 7:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_8_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_8_Timmer.ToString("N1");
                            N++;
                            break;
                        case 8:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_9_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_9_Timmer.ToString("N1");
                            N++;
                            break;
                        case 9:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_10_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_10_Timmer.ToString("N1");
                            N++;
                            break;
                        case 10:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_11_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_11_Timmer.ToString("N1");
                            N++;
                            break;
                        case 11:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_12_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_12_Timmer.ToString("N1");
                            N++;
                            break;
                        case 12:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_13_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_13_Timmer.ToString("N1");
                            N++;
                            break;
                        case 13:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_14_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_14_Timmer.ToString("N1");
                            N++;
                            break;
                        case 14:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_15_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_15_Timmer.ToString("N1");
                            N++;
                            break;
                        case 15:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_16_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_16_Timmer.ToString("N1");
                            N++;
                            break;
                        case 16:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_17_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_17_Timmer.ToString("N1");
                            N++;
                            break;
                        case 17:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_18_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_18_Timmer.ToString("N1");
                            N++;
                            break;
                        case 18:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_19_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_19_Timmer.ToString("N1");
                            N++;
                            break;
                        case 19:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_20_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_20_Timmer.ToString("N1");
                            N++;
                            break;
                        case 20:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_21_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_21_Timmer.ToString("N1");
                            N++;
                            break;
                        case 21:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_22_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_22_Timmer.ToString("N1");
                            N++;
                            break;
                        case 22:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_23_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_23_Timmer.ToString("N1");
                            N++;
                            break;
                        case 23:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_24_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_24_Timmer.ToString("N1");
                            N++;
                            break;
                        case 24:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_25_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_25_Timmer.ToString("N1");
                            N++;
                            break;
                        case 25:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_26_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_26_Timmer.ToString("N1");
                            N++;
                            break;
                        case 26:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_27_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_27_Timmer.ToString("N1");
                            N++;
                            break;
                        case 27:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_28_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_28_Timmer.ToString("N1");
                            N++;
                            break;
                        case 28:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_29_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_29_Timmer.ToString("N1");
                            N++;
                            break;
                        case 29:
                            bloop.Text = Player.GetBuff(entities[selectedEnt].Buff_30_ID);
                            BuffsBox.Children.Cast<TextBlock>().ElementAt(N + 30).Text =
                                entities[selectedEnt].Buff_30_Timmer.ToString("N1");
                            N++;
                            break;
                    }
                    
                }
                XposBox.Text = entities[selectedEnt].X.ToString();
                YposBox.Text = entities[selectedEnt].Y.ToString();
                ZposBox.Text = entities[selectedEnt].Z.ToString();
            }
            catch
            {
                return;
            }
        }
        List<Entity> entities = new List<Entity>();
        class Entity
        {
            private Player.PlayerStructure* baseStructure {
                get { return (Player.PlayerStructure*) (((int) *Player.BasePlayerAddress) + offset); }
            }
            private int offset;
            public Entity(int Offset)
            {
                offset = Offset;
                //baseStructure = baseStructure;
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
            public UInt16 Buff_1_ID {get { return baseStructure->Buff_1_ID; }}
            public float Buff_1_Timmer { get { return baseStructure->Buff_1_Timmer; } }
            public UInt16 Buff_2_ID { get { return baseStructure->Buff_2_ID; } }
            public float Buff_2_Timmer { get { return baseStructure->Buff_2_Timmer; } }
            public UInt16 Buff_3_ID { get { return baseStructure->Buff_3_ID; } }
            public float Buff_3_Timmer { get { return baseStructure->Buff_3_Timmer; } }
            public UInt16 Buff_4_ID { get { return baseStructure->Buff_4_ID; } }
            public float Buff_4_Timmer { get { return baseStructure->Buff_4_Timmer; } }
            public UInt16 Buff_5_ID { get { return baseStructure->Buff_5_ID; } }
            public float Buff_5_Timmer { get { return baseStructure->Buff_5_Timmer; } }
            public UInt16 Buff_6_ID { get { return baseStructure->Buff_6_ID; } }
            public float Buff_6_Timmer { get { return baseStructure->Buff_6_Timmer; } }
            public UInt16 Buff_7_ID { get { return baseStructure->Buff_7_ID; } }
            public float Buff_7_Timmer { get { return baseStructure->Buff_7_Timmer; } }
            public UInt16 Buff_8_ID { get { return baseStructure->Buff_8_ID; } }
            public float Buff_8_Timmer { get { return baseStructure->Buff_8_Timmer; } }
            public UInt16 Buff_9_ID { get { return baseStructure->Buff_9_ID; } }
            public float Buff_9_Timmer { get { return baseStructure->Buff_9_Timmer; } }
            public UInt16 Buff_10_ID { get { return baseStructure->Buff_10_ID; } }
            public float Buff_10_Timmer { get { return baseStructure->Buff_10_Timmer; } }
            public UInt16 Buff_11_ID { get { return baseStructure->Buff_11_ID; } }
            public float Buff_11_Timmer { get { return baseStructure->Buff_11_Timmer; } }
            public UInt16 Buff_12_ID { get { return baseStructure->Buff_12_ID; } }
            public float Buff_12_Timmer { get { return baseStructure->Buff_12_Timmer; } }
            public UInt16 Buff_13_ID { get { return baseStructure->Buff_13_ID; } }
            public float Buff_13_Timmer { get { return baseStructure->Buff_13_Timmer; } }
            public UInt16 Buff_14_ID { get { return baseStructure->Buff_14_ID; } }
            public float Buff_14_Timmer { get { return baseStructure->Buff_14_Timmer; } }
            public UInt16 Buff_15_ID { get { return baseStructure->Buff_15_ID; } }
            public float Buff_15_Timmer { get { return baseStructure->Buff_15_Timmer; } }
            public UInt16 Buff_16_ID { get { return baseStructure->Buff_16_ID; } }
            public float Buff_16_Timmer { get { return baseStructure->Buff_16_Timmer; } }
            public UInt16 Buff_17_ID { get { return baseStructure->Buff_17_ID; } }
            public float Buff_17_Timmer { get { return baseStructure->Buff_17_Timmer; } }
            public UInt16 Buff_18_ID { get { return baseStructure->Buff_18_ID; } }
            public float Buff_18_Timmer { get { return baseStructure->Buff_18_Timmer; } }
            public UInt16 Buff_19_ID { get { return baseStructure->Buff_19_ID; } }
            public float Buff_19_Timmer { get { return baseStructure->Buff_19_Timmer; } }
            public UInt16 Buff_20_ID { get { return baseStructure->Buff_20_ID; } }
            public float Buff_20_Timmer { get { return baseStructure->Buff_20_Timmer; } }
            public UInt16 Buff_21_ID { get { return baseStructure->Buff_21_ID; } }
            public float Buff_21_Timmer { get { return baseStructure->Buff_21_Timmer; } }
            public UInt16 Buff_22_ID { get { return baseStructure->Buff_22_ID; } }
            public float Buff_22_Timmer { get { return baseStructure->Buff_22_Timmer; } }
            public UInt16 Buff_23_ID { get { return baseStructure->Buff_23_ID; } }
            public float Buff_23_Timmer { get { return baseStructure->Buff_23_Timmer; } }
            public UInt16 Buff_24_ID { get { return baseStructure->Buff_24_ID; } }
            public float Buff_24_Timmer { get { return baseStructure->Buff_24_Timmer; } }
            public UInt16 Buff_25_ID { get { return baseStructure->Buff_25_ID; } }
            public float Buff_25_Timmer { get { return baseStructure->Buff_25_Timmer; } }
            public UInt16 Buff_26_ID { get { return baseStructure->Buff_26_ID; } }
            public float Buff_26_Timmer { get { return baseStructure->Buff_26_Timmer; } }
            public UInt16 Buff_27_ID { get { return baseStructure->Buff_27_ID; } }
            public float Buff_27_Timmer { get { return baseStructure->Buff_27_Timmer; } }
            public UInt16 Buff_28_ID { get { return baseStructure->Buff_28_ID; } }
            public float Buff_28_Timmer { get { return baseStructure->Buff_28_Timmer; } }
            public UInt16 Buff_29_ID { get { return baseStructure->Buff_29_ID; } }
            public float Buff_29_Timmer { get { return baseStructure->Buff_29_Timmer; } }
            public UInt16 Buff_30_ID { get { return baseStructure->Buff_30_ID; } }
            public float Buff_30_Timmer { get { return baseStructure->Buff_30_Timmer; } }
        }

        private void EListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshBuffs();
        }
    }
}
