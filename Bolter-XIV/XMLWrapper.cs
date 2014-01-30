using System;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using ConfigHelper;
using Player_Bits;
using UnManaged;

namespace Bolter_XIV
{
    public static class ConfigWrapper
    {

        //Load XML
        public static void Load()
        {
            MainWindow.Config = XmlSerializationHelper.Deserialize<config>(InterProcessCom.ConfigPath);
        }

        //Save XML
        public static void Save()
        {
            XmlSerializationHelper.Serialize<config>(InterProcessCom.ConfigPath,MainWindow.Config);
        }

        //Populates Area ComboBox
        public static void RefreshAreaBox(ComboBox areaBox)
        {
            try{areaBox.Items.Clear();}
            catch{}

            var box1List = MainWindow.Config.saved_cords.Select(p => p.ZoneID).Distinct().ToList();

            //Sort the list and add to the combo box items
            box1List.Sort();
            foreach (var myItem in box1List)
                areaBox.Items.Add(myItem);

            areaBox.SelectedIndex = 0;
        }

        //Populates Name ComboBox
        public static void RefreshNameBox(ComboBox TheBox, ComboBox AreaBox)
        {
            try{TheBox.Items.Clear();}
            catch{}

            var box2List = MainWindow.Config.saved_cords
                .Where(thenode => thenode.ZoneID == AreaBox.SelectedItem.ToString())
                .Select(thenode => thenode.Name).ToList();

            box2List.Sort();

            foreach (var myItem2 in box2List)
                TheBox.Items.Add(myItem2);

            TheBox.SelectedIndex = 0;
        }

        //Populates POS Boxes
        public static void RefreshPOSBoxes(ComboBox TheBox, ComboBox AreaBox, TextBox Xbox, TextBox Ybox, TextBox Zbox, TextBox KeyZone, TextBox KeyName)
        {
            // Find index of selected coordinates.
            var index =
                MainWindow.Config.saved_cords
                    .FindIndex(
                        p =>
                            p.ZoneID == AreaBox.SelectedItem.ToString() &&
                            p.Name == TheBox.SelectedItem.ToString());
            
            // Set cord info to the boxes.
            Xbox.Text = MainWindow.Config.saved_cords[index].X.ToString();
            Ybox.Text = MainWindow.Config.saved_cords[index].Y.ToString();
            Zbox.Text = MainWindow.Config.saved_cords[index].Z.ToString();
            KeyZone.Text = MainWindow.Config.saved_cords[index].ZoneID;
            KeyName.Text = MainWindow.Config.saved_cords[index].Name;

        }

        //Saves Cord
        public static void SaveCord(TextBox Area, TextBox Name, Button LoadButton)
        {
            Load();
            if (MainWindow.Config.saved_cords.Any(p => p.ZoneID == Area.Text && p.Name == Name.Text))
            {
                MessageBox.Show("Duplicate name for this zone already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MainWindow.Config.saved_cords.Add(new Cord()
            {
                Name = Name.Text,
                X = Player.GetPos("X"),
                Y = Player.GetPos("Y"),
                Z = Player.GetPos("Z"),
                ZoneID = Area.Text
            });
            Save();
            MessageBox.Show("Location saved into config");
            LoadButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        //Saves POS HotKey
        public static void SaveHotKey(ComboBox POSKbox, ComboBox POSKmodbox, TextBox POSKey_Name, TextBox POSKey_Zone, KeyType Type)
        {
            try
            {
                Load();
                MainWindow.Config.HotKeys.POSKeys.Add(new POSKey()
                {
                    Key = (Key)Enum.Parse(typeof(Key), POSKbox.SelectedItem.ToString()),
                    KeyMod = (KeyModifier)Enum.Parse(typeof(KeyModifier), POSKmodbox.SelectedItem.ToString()),
                    POSName = POSKey_Name.Text,
                    ZoneName = POSKey_Zone.Text
                });
                Save();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            MessageBox.Show("The HotKey has been saved, please restart Bolter to use it.");
        }

        //Saves Speed and Move HotKey
        public static void SaveHotKey(ComboBox POSKbox, ComboBox POSKmodbox, TextBox Amount, ComboBox Direction, KeyType Type)
        {
            Load();
            switch (Type)
            {
                case KeyType.SpeedKey:
                    MainWindow.Config.HotKeys.SpeedKeys.Add(new SpeedKey()
                    {
                        Key = (Key)Enum.Parse(typeof(Key), POSKbox.SelectedItem.ToString()),
                        KeyMod = (KeyModifier)Enum.Parse(typeof(KeyModifier), POSKmodbox.SelectedItem.ToString()),
                        Direction = Direction.SelectedItem.ToString(),
                        Amount = float.Parse(Amount.Text)
                    });
                    break;
                case KeyType.MoveKey:
                    MainWindow.Config.HotKeys.MoveKeys.Add(new MoveKey()
                    {
                        Key = (Key)Enum.Parse(typeof(Key), POSKbox.SelectedItem.ToString()),
                        KeyMod = (KeyModifier)Enum.Parse(typeof(KeyModifier), POSKmodbox.SelectedItem.ToString()),
                        Direction = Direction.SelectedItem.ToString(),
                        Distance = float.Parse(Amount.Text)
                    });
                    break;
            }
            Save();
            MessageBox.Show("The HotKey has been saved, please restart Bolter to use it.");
        }

        public enum Node
        {
            Cord,
            HotKey
        }
        public enum KeyType
        {
            POSKey,
            SpeedKey,
            MoveKey
        }
    }
    public static class WaypointsWrapper
    {

        //Load XML
        public static void Load()
        {
            Navigation._Waypoints = XmlSerializationHelper.Deserialize<Waypoints>(InterProcessCom.ConfigPath.Replace("config.xml","waypoints.xml"));
        }

        //Save XML
        public static void Save()
        {
            XmlSerializationHelper.Serialize<Waypoints>(InterProcessCom.ConfigPath.Replace("config.xml", "waypoints.xml"), Navigation._Waypoints);
        }


       
    }
}
