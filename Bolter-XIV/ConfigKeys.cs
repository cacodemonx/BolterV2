using System.Xml;
using Bolter_XIV;

namespace Config_Keys
{
    static public class ConfigKeys
    {
        static public XmlDocument Config;
        static public string[] POSKeys;
        static public string[] POSKeyMods;
        static public string[] POSNames;
        static public string[] POSZoneNames;
        static public string[] SpeedKeys;
        static public string[] SpeedKeyMods;
        static public string[] SpeedAmts;
        static public string[] SpeedDirs;
        static public string[] MoveKeys;
        static public string[] MoveKeyMods;
        static public string[] MoveAmts;
        static public string[] MoveDirs;
        static public int keycount;
        static public void Load()
        {
            Config = new XmlDocument();
            Config.Load(InterProcessCom.ConfigPath);
            keycount = Config.GetElementsByTagName("HotKey").Count;
        }
        static public  void GetKeys()
        {
            int POSCount = 0;
            int SpeedCount = 0;
            int MoveCount = 0;
            int N = 0;
            foreach (XmlNode thenode in Config.GetElementsByTagName("HotKey"))
            {
                if (thenode.Attributes.Item(4).InnerText == "POSKey")
                {
                    POSCount++;
                }
                else if (thenode.Attributes.Item(4).InnerText == "SpeedKey")
                {
                    SpeedCount++;
                }
                else if (thenode.Attributes.Item(4).InnerText == "MoveKey")
                {
                    MoveCount++;
                }
            }
            POSKeys = new string[POSCount];
            POSKeyMods = new string[POSCount];
            POSNames = new string[POSCount];
            POSZoneNames = new string[POSCount];
            SpeedKeys = new string[SpeedCount];
            SpeedKeyMods = new string[SpeedCount];
            SpeedAmts = new string[SpeedCount];
            SpeedDirs = new string[SpeedCount];
            MoveKeys = new string[MoveCount];
            MoveKeyMods = new string[MoveCount];
            MoveAmts = new string[MoveCount];
            MoveDirs = new string[MoveCount];
            foreach (XmlNode thenode in Config.GetElementsByTagName("HotKey"))
            {
                if (thenode.Attributes.Item(4).InnerText == "POSKey")
                {
                    POSKeys[N] = thenode.Attributes.Item(0).InnerText;
                    POSKeyMods[N] = thenode.Attributes.Item(1).InnerText;
                    POSNames[N] = thenode.Attributes.Item(2).InnerText;
                    POSZoneNames[N] = thenode.Attributes.Item(3).InnerText;
                    N++;
                }
            }
            N = 0;
            foreach (XmlNode thenode in Config.GetElementsByTagName("HotKey"))
            {
                if (thenode.Attributes.Item(4).InnerText == "SpeedKey")
                {
                    SpeedKeys[N] = thenode.Attributes.Item(0).InnerText;
                    SpeedKeyMods[N] = thenode.Attributes.Item(1).InnerText;
                    SpeedAmts[N] = thenode.Attributes.Item(2).InnerText;
                    SpeedDirs[N] = thenode.Attributes.Item(3).InnerText;
                    N++;
                }
                
            }
            N = 0;
            foreach (XmlNode thenode in Config.GetElementsByTagName("HotKey"))
            {
                if (thenode.Attributes.Item(4).InnerText == "MoveKey")
                {
                    MoveKeys[N] = thenode.Attributes.Item(0).InnerText;
                    MoveKeyMods[N] = thenode.Attributes.Item(1).InnerText;
                    MoveAmts[N] = thenode.Attributes.Item(2).InnerText;
                    MoveDirs[N] = thenode.Attributes.Item(3).InnerText;
                    N++;
                }         
            }
        }
    }
}
