using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using Player_Bits;

namespace Bolter_XIV
{
    public static class XMLWrapper
    {
        static XmlDocument config = new XmlDocument();

        //Load XML
        public static void Load()
        {
            config.Load(InterProcessCom.ConfigPath);
        }

        //Save XML
        public static void Save()
        {
            config.Save(InterProcessCom.ConfigPath);
        }

        //Method for creating new line in the config.xml
        public static void CreateConfigLine(string parrentelement, string elementname, string[] attributeNames, string[] innertext)
        {
            XmlNodeList Parrent = config.GetElementsByTagName(parrentelement);
            XmlElement theele = config.CreateElement(elementname);
            XmlAttribute[] TheList = new XmlAttribute[attributeNames.Count<string>()];
            int N = 0;
            foreach (XmlAttribute TheAtt in TheList)
            {
                TheList[N] = config.CreateAttribute(attributeNames[N]);
                TheList[N].Value = innertext[N];
                theele.Attributes.Append(TheList[N]);
                N++;
            }
            Parrent.Item(0).AppendChild(theele);
        }

        public static XmlNodeList GetNodeList(Node thename)
        {
            return config.GetElementsByTagName(thename.ToString());
        }

        //Populates Area ComboBox
        public static void RefreshAreaBox(ComboBox AreaBox)
        {
            try { AreaBox.Items.Clear(); }
            catch { }
            List<string> box1List = new List<string>();
            XmlNodeList saved_cords = GetNodeList(Node.Cord);
            string[] Areas = new string[saved_cords.Count];
            int N = 0;
            foreach (XmlNode thenode in saved_cords)
            {
                if (!Areas.Contains<string>(thenode.Attributes.Item(4).InnerText))
                {
                    box1List.Add(thenode.Attributes.Item(4).InnerText);
                }
                Areas[N] = thenode.Attributes.Item(4).InnerText;
                N++;
            }
            //Sort the list and add to the combo box items
            box1List.Sort();
            foreach (var myItem in box1List)
            {
                AreaBox.Items.Add(myItem);
            }
            AreaBox.SelectedIndex = 0;
        }

        //Populates Name ComboBox
        public static void RefreshNameBox(ComboBox TheBox, ComboBox AreaBox)
        {
            try { TheBox.Items.Clear(); }
            catch { }
            List<string> box2List = new List<string>();
            foreach (XmlNode thenode in GetNodeList(Node.Cord))
            {
                if (thenode.Attributes.Item(4).InnerText == AreaBox.SelectedItem.ToString())
                {
                    box2List.Add(thenode.Attributes.Item(0).InnerText);
                }
            }
            box2List.Sort();
            foreach (var myItem2 in box2List)
            {
                TheBox.Items.Add(myItem2);
            }
            TheBox.SelectedIndex = 0;
        }

        //Populates POS Boxes
        public static void RefreshPOSBoxes(ComboBox TheBox, ComboBox AreaBox, TextBox Xbox, TextBox Ybox, TextBox Zbox, TextBox KeyZone, TextBox KeyName)
        {
            foreach (XmlNode thenode in GetNodeList(Node.Cord))
            {
                if (thenode.Attributes.Item(4).InnerText == AreaBox.SelectedItem.ToString() && thenode.Attributes.Item(0).InnerText == TheBox.SelectedItem.ToString())
                {
                    Xbox.Text = thenode.Attributes.Item(1).InnerText;
                    Ybox.Text = thenode.Attributes.Item(2).InnerText;
                    Zbox.Text = thenode.Attributes.Item(3).InnerText;
                    KeyZone.Text = thenode.Attributes.Item(4).InnerText;
                    KeyName.Text = thenode.Attributes.Item(0).InnerText;
                }
            }
        }

        //Saves Cord
        public static void SaveCord(TextBox Area, TextBox Name, Button LoadButton)
        {
            Load();
            int N = 0;
            foreach (XmlNode the_node in GetNodeList(Node.Cord))
            {
                if (the_node.Attributes.Item(4).InnerText == Area.Text && the_node.Attributes.Item(0).InnerText == Name.Text)
                {
                    N++;
                }
            }
            if (N > 0)
            {
                MessageBox.Show("Duplicate name for this zone already exists", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string[] TheAtts = { "Name", "X", "Y", "Z", "ZoneID" };
            string[] AttText = 
            { 
                Name.Text,
                Player.GetPos("X").ToString(), 
                Player.GetPos("Y").ToString(), 
                Player.GetPos("Z").ToString(), 
                Area.Text 
            };
            CreateConfigLine("saved_cords", "Cord", TheAtts, AttText);
            Save();
            MessageBox.Show("Location saved into config");
            LoadButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        //Saves POS HotKey
        public static void SaveHotKey(ComboBox POSKbox, ComboBox POSKmodbox, TextBox POSKey_Name, TextBox POSKey_Zone, KeyType Type)
        {
            Load();
            string[] TheAtts = { "Key", "KeyMod", "POSName", "ZoneName", "Type" };
            string[] AttText = { (string)POSKbox.SelectedItem, (string)POSKmodbox.SelectedItem, POSKey_Name.Text, POSKey_Zone.Text, Type.ToString() };
            CreateConfigLine("hotkeys", "HotKey", TheAtts, AttText);
            Save();
            MessageBox.Show("The HotKey has been saved, please restart Bolter to use it.");
        }

        //Saves Speed and Move HotKey
        public static void SaveHotKey(ComboBox POSKbox, ComboBox POSKmodbox, TextBox Amount, ComboBox Direction, KeyType Type)
        {
            Load();
            string[] TheAtts = null;
            if (Type == KeyType.SpeedKey)
            {
                TheAtts = new string[]{ "Key", "KeyMod", "Amount", "Direction", "Type" };
            }
            else if (Type == KeyType.MoveKey)
            {
                TheAtts = new string[]{ "Key", "KeyMod", "Distance", "Direction", "Type" };
            }
            string[] AttText = { (string)POSKbox.SelectedItem, (string)POSKmodbox.SelectedItem, Amount.Text, (string)Direction.SelectedItem, Type.ToString() };
            CreateConfigLine("hotkeys", "HotKey", TheAtts, AttText);
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
}
