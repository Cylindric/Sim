﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Assets.Scripts.Model
{
    [MoonSharpUserData]
    public class Room
    {
        public AtmosphereManager Atmosphere;

        private List<Tile> _tiles;

        public int Size
        {
            get { return _tiles.Count; }
        }

        public int Id
        {
            get { return World.Instance.GetRoomId(this); }
        }

        public Room()
        {
            _tiles = new List<Tile>();
            Atmosphere = new AtmosphereManager(this);
        }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public Room Clone()
        {
            var room = new Room();
            room._tiles = new List<Tile>(this._tiles);
            room.Atmosphere = this.Atmosphere.Clone();
            return room;
        }

        public bool IsOutsideRoom()
        {
            return this == World.Instance.GetOutsideRoom();
        }

        /* #################################################################### */
        /* #                            ATMOSPHERICS                          # */
        /* #################################################################### */

        //public bool HasBreathableAtmosphere()
        //{
        //    if (GetGasPercentage("O2") < 0.2f)
        //    {
        //        return false;
        //    }
        //    if (GetTotalAtmosphericPressure() < 0.3f)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        //public void ChangeGas(string name, float amount)
        //{
        //    if (IsOutsideRoom())
        //    {
        //        return;
        //    }

        //    Atmosphere.ChangeGas(name, amount);
        //}

        //public float GetGasAmount(string name)
        //{
        //    return Atmosphere.GetGasAmount(name);
        //}

        //public float GetGasPercentage(string name)
        //{
        //    return Atmosphere.GetGasPercentage(name);
        //}

        //public IEnumerable<string> GetGasNames()
        //{
        //    return Atmosphere.GetGasNames();
        //}

        //public float GetTotalAtmosphericPressure()
        //{
        //    return Atmosphere.GetTotalAtmosphericPressure();
        //}

        public void AssignTile(Tile t)
        {
            if (_tiles.Contains(t))
            {
                return;
            }

            if (t.Room != null)
            {
                t.Room._tiles.Remove(t);
            }

            t.Room = this;
            _tiles.Add(t);
        }

        public void ReturnTilesToOutsideRoom()
        {
            foreach (var t in _tiles)
            {
                t.Room = World.Instance.GetOutsideRoom();
            }
            _tiles = new List<Tile>();
        }

        public static void DoRoomFloodfill(Tile sourceTile, bool onlyIfOutside = false)
        {
            // Get a reference to the World, for convenience.
            var world = World.Instance;
            var oldRoom = sourceTile.Room;

            if (oldRoom != null)
            {
                // The source Tile had a room, so this must be a new piece of Furniture
                // That is potentially dividing this old Room into some new Rooms.

                // Create a new Room on each side of the selected Furniture.
                foreach (var t in sourceTile.GetNeighbours())
                {
                    if (t.Room != null && (onlyIfOutside == false || t.Room.IsOutsideRoom()))
                    {
                        FloodFill(t, oldRoom);
                    }
                }

                // Tiles with unpassable furniture are not in a room.
                sourceTile.Room = null;
                oldRoom._tiles.Remove(sourceTile);

                // Unassign all Tiles from the Room that the Furniture is in.
                // Never delete the outside Room.
                if (oldRoom.IsOutsideRoom() == false)
                {
                    if (oldRoom._tiles.Count > 0)
                    {
                        Debug.LogError("oldRoom still has tiles assigned to it!");
                    }
                    world.DeleteRoom(oldRoom);
                }
            }
            else
            {
                // The old Room is null, which means the source Tile was probably a Room,
                // but might not be now - so the wall was probably deconstructed.
                // Turn any rooms connected to this Tile into one Room.

                FloodFill(sourceTile, null);

                // Purge any leftover rooms that don't have any tiles in them any more.
                world.DeleteEmptyRooms();
            }
        }

        private static void FloodFill(Tile tile, Room oldRoom)
        {
            if (tile == null)
            {
                return;
            }

            if (tile.Room != oldRoom)
            {
                // This Tile was already assigned to another new Room, 
                // so the direction picked isn'Tile isolated.
                return;
            }

            if (tile.Furniture != null && tile.Furniture.IsRoomEnclosure)
            {
                // This Tile has a wall or door or something, so doesn'Tile have a Room.
                return;
            }

            if (tile.Type == TileType.Empty)
            {
                // Empty Tile, so must stay "outside"
                return;
            }

            // If we get this far, we know that we need to create a new Room.

            var newRoom = new Room();
            var tilesToCheck = new Queue<Tile>();
            tilesToCheck.Enqueue(tile);

            var mergedRooms = new Dictionary<int, Room>();

            bool isConnectedToSpace = false;

            while (tilesToCheck.Count > 0)
            {
                var t = tilesToCheck.Dequeue();

                if (t.Room != newRoom)
                {
                    newRoom.AssignTile(t);

                    foreach (var t2 in t.GetNeighbours())
                    {
                        if (t2 == null || t2.Type == TileType.Empty)
                        {
                            // We've hit an open-space Tile - either edge of the World, or an empty Tile.
                            // That means this new Room is actually now connected to the outside, so it's not valid.
                            isConnectedToSpace = true;
                        }
                        else
                        {
                            if (
                                t2.Room != newRoom && 
                                (t2.Furniture == null || t2.Furniture.IsRoomEnclosure == false))
                            {
                                tilesToCheck.Enqueue(t2);

                                if (t2.Room != null && mergedRooms.ContainsKey(t2.Room.Id) == false)
                                {
                                    mergedRooms.Add(t2.Room.Id, t2.Room.Clone());
                                }
                            }
                        }
                    }
                }
            }

            if (isConnectedToSpace)
            {
                newRoom.ReturnTilesToOutsideRoom();
                return;
            }

            // Copy data from the old room to the new room.
            if (oldRoom != null)
            {
                // In this case we are splitting one Room into two or more, so we
                // can just keep the old gas values.
                newRoom.CopyGas(oldRoom);
            }
            else
            {
                // In this case we are merging one or more rooms into one big Room.
                // This means we have to make a decision on how to merge the gas levels.
                newRoom.MergeGas(mergedRooms.Values.ToList());
            }

            // Tell the World that a new Room has been created.
            World.Instance.AddRoom(newRoom);
        }

        /* #################################################################### */
        /* #                            ATMOSPHERICS                          # */
        /* #################################################################### */

        private void MergeGas(List<Room> other)
        {
            this.Atmosphere.MergeAtmosphere(other);
        }

        private void CopyGas(Room other)
        {
            this.Atmosphere = other.Atmosphere.Clone();
        }

        /* #################################################################### */
        /* #                          SAVE AND RESTORE                        # */
        /* #################################################################### */

        public static Room ReadXml(XmlElement element)
        {
            var room = new Room();
            room.Atmosphere.ReadXml(element);
            return room;
        }

        public XmlElement WriteXml(XmlDocument xml)
        {
            var room = xml.CreateElement("Room");
            room.SetAttribute("id", this.Id.ToString());
            if (this.IsOutsideRoom())
            {
                room.SetAttribute("outside", "true");
            }

            // Write out all the atmospheric data.
            Atmosphere.WriteXml(xml, room);

            return room;
        }
    }
}
