using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Bolter_XIV;
using UnManaged;

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

    #region Config XML
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
    #endregion

    [Serializable]
    public class Waypoints
    {
        [XmlElement("Zone")]
        public List<Zones> Zone = new List<Zones>();

        public void AddPathToZone(int zoneId, string pathName)
        {
            Zone.First(p => p.ID == zoneId).Path.Add(new Paths(pathName));
        }

    }

    [Serializable]
    public class Zones
    {
        [XmlIgnore]
        private static NativeMethods Game
        {
            get { return InterProcessCom.Game; }
        }

        public Zones()
        {

        }
        public Zones(int id)
        {
            ID = id;
            Name = Game.CurrentZone;
        }

        public void AddPoints(string pathName, int interval, GatherWindow Gwindow)
        {
            if (Path.All(p => p.Name != pathName))
            {
                Path.Add(new Paths(pathName));
                Path.Last().AddPoints(interval, Gwindow);
            }
            else
                Path.First(p => p.Name == pathName).AddPoints(interval, Gwindow);
        }

        [XmlAttribute("ID")]
        public int ID;
        [XmlAttribute("Name")]
        public string Name;
        [XmlElement("Path")]
        public List<Paths> Path = new List<Paths>();
    }

    [Serializable]
    public class Paths
    {
        [XmlIgnore]
        private static NativeMethods Game
        {
            get { return InterProcessCom.Game; }
        }

        public Paths()
        {

        }

        public Paths(string pathName)
        {
            Name = pathName;
        }

        unsafe public void AddPoints(int interval, GatherWindow Gwindow)
        {
            var lastHeading = 0f;
            // Save our current heading.
            if (Navigation.TurnFilter)
                lastHeading = Game.PCMobEntity[0].PCMob->Heading;

            // Loop while record flag is true.
            while (Navigation.RecordFlag)
            {
                // Get current position.
                var currentPos = Game.Get2DPos();

                // Get last saved position, or null if there are none.
                var lastPos = Point.LastOrDefault();
            
                // Check if this is a new entry, or if we have moved from our last position.
                if (lastPos == null || lastPos.x != currentPos.x || lastPos.y != currentPos.y)
                {
                    if (Navigation.TurnFilter && (lastHeading == Game.PCMobEntity[0].PCMob->Heading))
                    {
                        Thread.Sleep(interval);
                        continue;
                    }
                    // Add the saved waypoint information to the log.
                    if (Gwindow != null)
                        Gwindow.AddTextRec(Point.Count, Game.CurrentZone, Name, currentPos.x, currentPos.y);

                    // Add the new waypoint.
                    Point.Add(new D3DXVECTOR2(currentPos.x, currentPos.y));

                    // Save out last heading
                    if (Navigation.TurnFilter)
                        lastHeading = Game.PCMobEntity[0].PCMob->Heading;
                }
                // End if this is a single add.
                if (interval == 0)
                    return;
                // Hold for the given interval.
                Thread.Sleep(interval);
            }


        }



        [XmlAttribute("Name")]
        public string Name;
        [XmlElement("Point")]
        public List<D3DXVECTOR2> Point = new List<D3DXVECTOR2>();
    }

}