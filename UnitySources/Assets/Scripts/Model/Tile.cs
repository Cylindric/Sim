﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    [DebuggerDisplay("Tile [{X},{Y}]")]
    [MoonSharpUserData]
    public class Tile : IXmlSerializable
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        private const float BaseTileMovementCost = 1f;

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        public Inventory inventory;

        private TileType _type = TileType.Empty;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public Tile()
        {
        }

        public Tile(World world, int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        public Action<Tile> cbTileChanged;

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        public int X { get; private set; }
        public int Y { get; private set; }
        //public World World { get; private set; }
        public Furniture Furniture { get; private set; }
        public Job PendingFurnitureJob { get; set; }
        public Room Room { get; set; }

        public TileType Type
        {
            get
            {
                return _type;
            }

            set
            {
                var oldType = _type;
                _type = value;

                if (cbTileChanged != null && oldType != _type)
                {
                    cbTileChanged(this);
                }
            }
        }

        public float MovementCost
        {
            get
            {
                if (Type == TileType.Empty)
                {
                    return 0f;
                }

                if (Furniture == null)
                {
                    return BaseTileMovementCost;
                }

                return BaseTileMovementCost * Furniture.MovementCost;
            }
        }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void UnRegisterTileTypeChangedCallback(Action<Tile> callback)
        {
            cbTileChanged -= callback;
        }

        public void RegisterTileTypeChangedCallback(Action<Tile> callback)
        {
            cbTileChanged += callback;
        }

        public bool UnplaceFurniture()
        {
            if (Furniture == null) return false;

            var f = Furniture;
            for (var xOff = X; xOff < X + f._width; xOff++)
            {
                for (var yOff = Y; yOff < Y + f._height; yOff++)
                {
                    var t = World.Current.GetTileAt(xOff, yOff);
                    t.Furniture = null;
                }
            }

            Furniture = null;
            return true;
        }

        public bool PlaceFurniture(Furniture furn)
        {
            if (furn == null)
            {
                return UnplaceFurniture();
            }
                
            if(furn.IsValidPosition(this) == false)
            {
                Debug.LogError("Trying to assign a Furniture to a Tile that isn't valid.");
                return false;
            }

            for (var xOff = X; xOff < X + furn._width; xOff++)
            {
                for (var yOff = Y; yOff < Y + furn._height; yOff++)
                {
                    var t = World.Current.GetTileAt(xOff, yOff);
                    t.Furniture = furn;
                }
            }

            return true;
        }

        public bool PlaceInventory(Inventory inv)
        {
            // If a null inv is provided, clear the current object.
            if (inv == null)
            {
                inventory = null;
                return true;
            }

            if (inventory != null)
            {
                // Try to combine a stack.
                if (inventory.objectType != inv.objectType)
                {
                    Debug.LogError("Trying to assign an Inventory to a Tile that already has some.");
                    return false;
                }

                int numToMove = inv.stackSize;
                if (inventory.stackSize + numToMove > inventory.maxStackSize)
                {
                    numToMove = inventory.maxStackSize - inventory.stackSize;
                }

                inventory.stackSize += numToMove;
                inv.stackSize -= numToMove;
                return true;
            }

            // At this point, we know that the inventory is null.
            inventory = inv.Clone();
            inventory.tile = this;
            inventory.character = null;
            inv.stackSize = 0;

            return true;
        }

        public bool IsNeighbour(Tile tile, bool allowDiagonal = false)
        {
            // If we're on the same X Column, see if we differ by excactly one Y row.
            if (this.X == tile.X && Mathf.Abs(this.Y - tile.Y) == 1)
            {
                return true;
            }

            // If we're on the same Y Row, see if we differ by just one X column.
            if (this.Y == tile.Y && Mathf.Abs(this.X - tile.X) == 1)
            {
                return true;
            }

            if (allowDiagonal)
            {
                if (this.X == tile.X + 1 && Mathf.Abs(this.Y - tile.Y) == 1)
                {
                    return true;
                }
                if (this.X == tile.X - 1 && Mathf.Abs(this.Y - tile.Y) == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public Tile[] GetNeighbours(bool allowDiagonal = false)
        {
            Tile[] ns;

            if (allowDiagonal == false)
            {
                ns = new Tile[4]; // Tile order N E S W
            }
            else
            {
                ns = new Tile[8]; // Tile order N E S W NE SE SW NW
            }

            ns[0] = World.Current.GetTileAt(X, Y + 1); // N
            ns[1] = World.Current.GetTileAt(X + 1, Y); // E
            ns[2] = World.Current.GetTileAt(X, Y - 1); // S
            ns[3] = World.Current.GetTileAt(X - 1, Y); // w

            if (allowDiagonal == true)
            {
                ns[4] = World.Current.GetTileAt(X + 1, Y + 1); // NE
                ns[5] = World.Current.GetTileAt(X + 1, Y - 1); // SE
                ns[6] = World.Current.GetTileAt(X - 1, Y - 1); // SW
                ns[7] = World.Current.GetTileAt(X - 1, Y + 1); // NW
            }

            return ns;
        }

        public Enterability IsEnterable()
        {
            if (Mathf.Approximately(MovementCost, 0))
            {
                return Enterability.Never;
            }

            // Check the furniture to see if it has any special rules on enterability.
            if (Furniture != null)
            {
                return Furniture.IsEnterable();
            }

            return Enterability.Yes;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //X = int.Parse(reader.GetAttribute("X")); // Already read by the World
            //Y = int.Parse(reader.GetAttribute("Y")); // Already read by the World
            Type = (TileType)int.Parse(reader.GetAttribute("Type"));
            Room = World.Current.GetRoomFromId(int.Parse(reader.GetAttribute("RoomID")));
            if (Room == null) return;
            Room.AssignTile(this);
            //Debug.LogFormat("Read Tile [{0},{1}] with type {2}.", X, Y, Type.ToString());
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Tile");
            writer.WriteAttributeString("X", X.ToString());
            writer.WriteAttributeString("Y", Y.ToString());
            writer.WriteAttributeString("RoomID", Room == null ? "-1" : Room.Id.ToString());
            writer.WriteAttributeString("Type", ((int)Type).ToString());
            writer.WriteEndElement();
        }

        public Tile NorthNeighbour()
        {
            return World.Current.GetTileAt(X, Y + 1);
        }

        public Tile EastNeighbour()
        {
            return World.Current.GetTileAt(X + 1, Y);
        }

        public Tile SouthNeighbour()
        {
            return World.Current.GetTileAt(X, Y - 1);
        }

        public Tile WestNeighbour()
        {
            return World.Current.GetTileAt(X -1, Y);
        }
    }
}
