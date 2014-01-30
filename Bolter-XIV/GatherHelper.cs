using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ConfigHelper;
using Player_Bits;

namespace Bolter_XIV
{

    public enum Pathing
    {
        Normal,
        At_Index,
        Closest
    }
    unsafe public class Navigation : NativeDx
    {
        /// <summary>
        /// Container for our waypoints.xml
        /// </summary>
        public static Waypoints _Waypoints;

        /// <summary>
        /// Flag that stops and starts recording
        /// </summary>
        public static bool RecordFlag;

        public static bool HaltFlag;

        public static int HeadToll = 50;

        public static bool Moving;

        public static bool CorDelay = true;

        /// <summary>
        /// A recipe for PI
        /// </summary>
        private readonly float _pi = (float)(4 * Math.Atan(1));


        protected float Distance(D3DXVECTOR2 p1, D3DXVECTOR2 p2) { return Distance(p1.y, p1.x, p2.y, p2.x); }
        protected float Distance(float y1, float x1, float y2, float x2) { return (float)Math.Sqrt(((y1 - y2) * (y1 - y2)) + ((x1 - x2) * (x1 - x2))); }
        protected int MathMod(int a, int b) { return ((a % b) + b) % b; }
        protected double PosHToDegrees(float PosH) { return MathMod((int)(((PosH * 180.0) / _pi) + 90.0), 360); }
        protected float DegreesToPosH(double Degrees) { return (MathMod((int)(((Degrees + 90) * _pi) / 180.0), (int)(2 * _pi)) - _pi); }
        protected float HeadingToRad(D3DXVECTOR2 from, D3DXVECTOR2 to) { return (float)Math.Atan2((to.x - from.x), (to.y - from.y)); }

        /// <summary>
        /// Rotates the character's model, to face the given destination point.
        /// </summary>
        /// <param name="pointB">The destination point</param>
        protected void ModelRotation(D3DXVECTOR2 pointB)
        {
            // Get new heading angle, for the given point.
            var newHeading =
                HeadingToRad(
                    Player.Get2DPos(),
                    pointB);
            // Get new 3D matrix Y axis vector, rotated to the given angle.
            var newVector = GetNewVector(newHeading);
            // Set new heading
            (*Player.MasterPtr->Player)->Heading = newHeading;
            // Set new rotation vector.
            (*Player.MasterPtr->Player)->subStruct->VectorX = newVector.x;
            (*Player.MasterPtr->Player)->subStruct->VectorY = newVector.y;
        }

        private void RecordWaypoint(int interval, string pathName, GatherWindow Gwindow)
        {
            if (RecordFlag)
                return;
            // Set recording as true.
            RecordFlag = true;

            // Get ID of current zone.
            var zoneId = Marshal.ReadInt32((IntPtr)Player.ZoneAddress);

            // Check if we need to make a new Zone entry.
            var nozones = _Waypoints.Zone.All(p => p.ID != zoneId);

            // We need to make a new entry.
            if (nozones)
            {
                // Add new Zone entry for our current zone.
                _Waypoints.Zone.Add(new Zones(zoneId));
                // Add an empty Path entry.
                _Waypoints.AddPathToZone(zoneId, pathName);
                // This is a new entry, so grab the last Zone we added, and the first Path in that Zone,
                // Then start adding new Point entries (at the rate of the given interval), and wait for the user to click stop.
                _Waypoints.Zone.Last().Path.First().AddPoints(interval, Gwindow);
            }
            else
                // We already have the Zone in our xml, so lets go strait to adding points.
                _Waypoints.Zone.First(p => p.ID == zoneId).AddPoints(pathName, interval, Gwindow);

            // If this is a single add, just reset the record flag and save the xml.
            if (interval == 0)
                StopRecord();
        }

        private void PlayPath(string pathName, GatherWindow Gwindow, Pathing pType)
        {
            if (Moving)
                return;
            Moving = true;
            var zoneId = Marshal.ReadInt32((IntPtr) Player.ZoneAddress);
            HaltFlag = false;
            switch (pType)
            {
                    case Pathing.Normal:
                    foreach (var waypoint in _Waypoints.Zone.First(p => p.ID == zoneId).Path.First(i => i.Name == pathName).Point)
                    {
                        if (CorDelay)
                            Player.GetMovment()->Status = Player.WalkingStatus.Standing;

                        ModelRotation(new D3DXVECTOR2(waypoint.x, waypoint.y));
                        
                        SendKeyPress(KeyStates.Toggled, Key.End);

                        if (CorDelay)
                            Thread.Sleep(HeadToll);

                        var decX = (Player.GetPos(Player.Axis.X) > waypoint.x);

                        Player.GetMovment()->Status = Player.WalkingStatus.Autorun | Player.WalkingStatus.Running;

                        if (decX)
                        {
                            while (Player.GetPos(Player.Axis.X) > waypoint.x)
                            {
                                if (HaltFlag)
                                {
                                    Player.GetMovment()->Status = Player.WalkingStatus.Standing;
                                    Moving = false;
                                    return;
                                }
                                Thread.Sleep(1);
                            }
                        }
                        else
                        {
                            while (Player.GetPos(Player.Axis.X) < waypoint.x)
                            {
                                if (HaltFlag)
                                {
                                    Player.GetMovment()->Status = Player.WalkingStatus.Standing;
                                    Moving = false;
                                    return;
                                }
                                Thread.Sleep(1);
                            }
                        }
                        if (Gwindow != null)
                            Gwindow.AddText(0, "", pathName, waypoint.x, waypoint.y);
                    }
                    Player.GetMovment()->Status = Player.WalkingStatus.Standing;
                    Moving = false;
                    break;
                    case Pathing.At_Index:

                    break;
                    case Pathing.Closest:

                    break;
            }
        }

        /// <summary>
        /// Starts recording waypoints on a new thread.
        /// </summary>
        /// <param name="interval">Time in ms, between waypoints</param>
        /// <param name="pathName">Name of the path to save the waypoints to. A new on will be created, if it does not exist</param>
        /// <param name="logBox"></param>
        public void Record(int interval, string pathName, GatherWindow Gwindow)
        {
            new Thread(() => RecordWaypoint(interval, pathName, Gwindow)).Start();
        }

        public void Play(string pathName, GatherWindow Gwindow, Pathing pType)
        {
            new Thread(() => PlayPath(pathName, Gwindow, pType)).Start();
        }

        /// <summary>
        /// Stops an active recording session, and saves the results to xml.
        /// </summary>
        public void StopRecord()
        {
            RecordFlag = false;
            XmlSerializationHelper.Serialize(InterProcessCom.ConfigPath.Replace("config.xml", "waypoints.xml"), _Waypoints);
        }

        public void Reload()
        {
            XmlSerializationHelper.Serialize(InterProcessCom.ConfigPath.Replace("config.xml", "waypoints.xml"), _Waypoints);
            _Waypoints = XmlSerializationHelper.Deserialize<Waypoints>(InterProcessCom.ConfigPath.Replace("config.xml", "waypoints.xml"));
        }
    }

    public class GatherHelper : Navigation
    {
        /// <summary>
        /// Instantiates new GatherHelper class, and loads our Waypoints
        /// </summary>
        public GatherHelper()
        {
            var filepath = InterProcessCom.ConfigPath.Replace("config.xml", "waypoints.xml");
            _Waypoints = !File.Exists(filepath) ? new Waypoints() : XmlSerializationHelper.Deserialize<Waypoints>(filepath);
        }
        public void test(float x, float y)
        {
            Record(500,"DebugPath",null);
        }
    }
}
