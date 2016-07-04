using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Assets.Scripts.Controllers;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class Room : IXmlSerializable
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        private Dictionary<string, float> atmosphericGasses;

        private List<Tile> _tiles = new List<Tile>();

        private World _world;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public Room()
        {
            atmosphericGasses = new Dictionary<string, float>();
        }

        public Room(World world)
        {
            this._world = world;
            atmosphericGasses = new Dictionary<string, float>();
        }

        public float Size
        {
            get { return _tiles.Count; }
        }

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public bool IsOutsideRoom()
        {
            if (_tiles.Count == 0)
            {
                return true;
            }

            return this == _tiles[0].World.GetOutsideRoom();
        }

        public void ChangeGas(string name, float amount)
        {
            if (IsOutsideRoom())
            {
                return;
            }

            if (atmosphericGasses.ContainsKey(name))
            {
                atmosphericGasses[name] += amount;
            }
            else
            {
                atmosphericGasses[name] = amount;
            }

            atmosphericGasses[name] = Mathf.Clamp01(atmosphericGasses[name]);
        }

        public float GetGasAmount(string name)
        {
            if (atmosphericGasses.ContainsKey(name))
            {
                return atmosphericGasses[name];
            }
            return 0f;

        }

        public float GetGasPercentage(string name)
        {
            if (atmosphericGasses.ContainsKey(name) == false)
            {
                return 0f;
            }

            var total = atmosphericGasses.Values.Sum(g => g);

            if (Mathf.Approximately(total, 0))
            {
                return 0f;
            }

            return atmosphericGasses[name]/total;
        }

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

        public void UnassignAllTiles()
        {
            foreach (var t in _tiles)
            {
                t.Room = t.World.GetOutsideRoom();
            }
            _tiles = new List<Tile>();
        }

        public static void DoRoomFloodfill(Furniture furniture)
        {
            // Get a reference to the World, for convenience.
            var world = furniture.Tile.World;

            var oldRoom = furniture.Tile.Room;

            // Create a new Room on each side of the selected Furniture.
            foreach (var t in furniture.Tile.GetNeighbours())
            {
                FloodFill(t, oldRoom);
            }

            // Tiles with unpassable furniture are not in a room.
            furniture.Tile.Room = null;
            oldRoom._tiles.Remove(furniture.Tile);

            // Unassign all Tiles from the Room that the Furniture is in.
            // Never delete the outside Room.
            if (oldRoom != world.GetOutsideRoom())
            {
                if (oldRoom._tiles.Count > 0)
                {
                    Debug.LogError("oldRoom still has tiles assigned to it!");
                }
                world.DeleteRoom(oldRoom);
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
                // This Tile was already assigned to another new Room, so the direction picked isn'tile isolated.
                return;
            }

            if (tile.Furniture != null && tile.Furniture.IsRoomEnclosure)
            {
                // This Tile has a wall or door or something, so doesn'tile have a Room.
                return;
            }

            if (tile.Type == TileType.Empty)
            {
                // Empty Tile, so must stay "outside"
                return;
            }

            // If we get this far, we know that we need to create a new Room.

            var newRoom = new Room(tile.World);

            var tilesToCheck = new Queue<Tile>();
            tilesToCheck.Enqueue(tile);

            while (tilesToCheck.Count > 0)
            {
                var t = tilesToCheck.Dequeue();

                if (t.Room == oldRoom)
                {
                    newRoom.AssignTile(t);

                    foreach (var t2 in t.GetNeighbours())
                    {
                        if (t2 == null || t2.Type == TileType.Empty)
                        {
                            // We've hit an open-space Tile - either edge of the World, or an empty Tile.
                            // That means this new Room is actually now connected to the outside, so it's not valid.
                            newRoom.UnassignAllTiles();
                            return;
                        }

                        if (t2.Room == oldRoom && (t2.Furniture == null || t2.Furniture.IsRoomEnclosure == false))
                        {
                            tilesToCheck.Enqueue(t2);
                        }
                    }
                }
            }

            // Copy data from the old room to the new room.
            newRoom.CopyGas(oldRoom);

            // Tell the World that a new Room has been created.
            tile.World.AddRoom(newRoom);
        }

        private void CopyGas(Room other)
        {
            foreach (var gas in other.atmosphericGasses)
            {
                this.atmosphericGasses[gas.Key] = gas.Value;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            // Rooms are tricky, because they are regenerated automatically, making it hard to link back to.
            if (reader.ReadToDescendant("Room"))
            {
                if (reader.ReadToDescendant("Gasses"))
                {
                    if (reader.ReadToDescendant("Gas"))
                    {
                        do
                        {
                            var name = reader.GetAttribute("name");
                            var value = float.Parse(reader.GetAttribute("amount"));
                            this.atmosphericGasses[name] = value;
                        } while (reader.ReadToNextSibling("Gas"));
                    }
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Room");
            if (_tiles.Count == 0)
            {
                writer.WriteAttributeString("x", "-1");
                writer.WriteAttributeString("y", "-1");
            }
            else
            {
                writer.WriteAttributeString("x", _tiles[0].X.ToString());
                writer.WriteAttributeString("y", _tiles[0].X.ToString());
            }

            writer.WriteStartElement("Gasses");
            foreach (var gas in atmosphericGasses)
            {
                writer.WriteStartElement("Gas");
                writer.WriteAttributeString("name", gas.Key);
                writer.WriteAttributeString("amount", gas.Value.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

    }
}
