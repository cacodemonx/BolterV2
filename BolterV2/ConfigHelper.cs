using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace ConfigHelper
{
    public class XmlSerializationHelper
    {
        public static void Serialize<T>(string filename, T obj)
        {
            var xs = new XmlSerializer(typeof(T));

            var dir = Path.GetDirectoryName(filename);

            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var sw = new StreamWriter(filename))
            {
                xs.Serialize(sw, obj);
            }
        }

        public static T Deserialize<T>(string filename)
        {
            try
            {
                var xs = new XmlSerializer(typeof(T));

                using (var rd = new StreamReader(filename))
                {
                    return (T)xs.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return default(T);
            }
        }
    }
    [Serializable]
    public class config
    {
        public List<Cord> saved_cords = new List<Cord>();
        public HotKeys HotKeys = new HotKeys();
        public List<PastProcess> MemInfo = new List<PastProcess>();
    }

    [Serializable]
    public class HotKeys
    {
        public List<POSKey> POSKeys = new List<POSKey>();
        public List<SpeedKey> SpeedKeys = new List<SpeedKey>();
        public List<MoveKey> MoveKeys = new List<MoveKey>();
    }

    [Serializable]
    public struct PastProcess
    {
        [XmlIgnore]
        public IntPtr hModule
        {
            get { return (IntPtr)_hModule; }
            set { _hModule = (int)value; }
        }
        [XmlIgnore]
        public IntPtr IsLoadedPtr
        {
            get { return (IntPtr)_IsLoadedPtr; }
            set { _IsLoadedPtr = (int)value; }
        }
        [XmlAttribute("ID")]
        public int ID;
        [XmlAttribute("hModule"), EditorBrowsable(EditorBrowsableState.Never)]
        public int _hModule;
        [XmlAttribute("ComPtr"), EditorBrowsable(EditorBrowsableState.Never)]
        public int _IsLoadedPtr;
    }

    [Serializable]
    public struct Cord
    {
        [XmlAttribute("Name")]
        public string Name;
        [XmlAttribute("X")]
        public float X;
        [XmlAttribute("Y")]
        public float Y;
        [XmlAttribute("Z")]
        public float Z;
        [XmlAttribute("ZoneID")]
        public string ZoneID;
    }

    [Serializable]
    public class HotKey
    {
        [XmlIgnore]
        public KeyModifier KeyMod
        {
            get { return (KeyModifier)Enum.Parse(typeof(KeyModifier), _KeyMod); }
            set { _KeyMod = value.ToString(); }
        }
        [XmlAttribute("Key")]
        public Key Key;
        [XmlAttribute("KeyMod"), EditorBrowsable(EditorBrowsableState.Never)]
        public string _KeyMod;
    }

    [Serializable]
    public class POSKey : HotKey
    {
        [XmlAttribute("POSName")]
        public string POSName;
        [XmlAttribute("ZoneName")]
        public string ZoneName;
    }

    [Serializable]
    public class SpeedKey : HotKey
    {
        [XmlAttribute("Amount")]
        public float Amount;
        [XmlAttribute("Direction")]
        public string Direction;
    }

    [Serializable]
    public class MoveKey : HotKey
    {
        [XmlAttribute("Direction")]
        public string Direction;
        [XmlAttribute("Distance")]
        public float Distance;
    }
    [Flags, Serializable]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}