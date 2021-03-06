﻿using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    [DebuggerDisplay("Tile [{X},{Y}]")]
    public class Tile : IXmlSerializable
    {
        /* #################################################################### */
        /* #                      CONSTANT FIELDS                             # */
        /* #################################################################### */
        private const float BaseTileMovementCost = 1f;

        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */
        private TileType type = TileType.Empty;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */
        public Tile()
        {
        }

        public Tile(World world, int x, int y)
        {
            this.World = world;
            this.X = x;
            this.Y = y;
        }

        /* #################################################################### */
        /* #                         DELEGATES                                # */
        /* #################################################################### */
        private Action<Tile> cbTileChanged;

        /* #################################################################### */
        /* #                         PROPERTIES                               # */
        /* #################################################################### */
        public int X { get; private set; }
        public int Y { get; private set; }
        public World World { get; private set; }
        public Furniture Furniture { get; private set; }
        public Job PendingFurnitureJob { get; set; }
        public Room Room { get; set; }

        public TileType Type
        {
            get
            {
                return type;
            }

            set
            {
                var oldType = type;
                type = value;

                if (cbTileChanged != null && oldType != type)
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
        /* #                           METHODS                                # */
        /* #################################################################### */

        public void UnRegisterTileTypeChangedCallback(Action<Tile> callback)
        {
            cbTileChanged -= callback;
        }

        public void RegisterTileTypeChangedCallback(Action<Tile> callback)
        {
            cbTileChanged += callback;
        }

        public bool PlaceFurniture(Furniture objectInstance)
        {
            // If a null objectInstance is provided, clear the current object.
            if (objectInstance == null)
            {
                Furniture = null;
                return true;
            }

            if (Furniture != null)
            {
                Debug.LogError("Trying to assign a Furniture to a Tile that already has one.");
                return false;
            }

            Furniture = objectInstance;
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

            ns[0] = World.GetTileAt(X, Y + 1); // N
            ns[1] = World.GetTileAt(X + 1, Y); // E
            ns[2] = World.GetTileAt(X, Y - 1); // S
            ns[3] = World.GetTileAt(X - 1, Y); // w

            if (allowDiagonal == true)
            {
                ns[4] = World.GetTileAt(X + 1, Y + 1); // NE
                ns[5] = World.GetTileAt(X + 1, Y - 1); // SE
                ns[6] = World.GetTileAt(X - 1, Y - 1); // SW
                ns[7] = World.GetTileAt(X - 1, Y + 1); // NW
            }

            return ns;
        }

        public Enterability IsEnterable()
        {
            if (MovementCost == 0)
            {
                return Enterability.Never;
            }

            // Check the furniture to see if it has any special rules on enterability.
            if (Furniture != null && Furniture.IsEntereable != null)
            {
                return Furniture.IsEntereable(Furniture);
            }

            return Enterability.Yes;
        }

        ///////////////////////////////////////////////////////
        /// 
        ///                    LOADING / SAVING
        /// 
        ///////////////////////////////////////////////////////

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //X = int.Parse(reader.GetAttribute("X"));
            //Y = int.Parse(reader.GetAttribute("Y"));
            Type = (TileType)int.Parse(reader.GetAttribute("Type"));
            //Debug.LogFormat("Read Tile [{0},{1}] with type {2}.", X, Y, Type.ToString());
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Tile");
            writer.WriteAttributeString("X", X.ToString());
            writer.WriteAttributeString("Y", Y.ToString());
            writer.WriteAttributeString("Type", ((int)Type).ToString());
            writer.WriteEndElement();
        }

        public Tile NorthNeighbour()
        {
            return World.GetTileAt(X, Y + 1);
        }

        public Tile EastNeighbour()
        {
            return World.GetTileAt(X + 1, Y);
        }

        public Tile SouthNeighbour()
        {
            return World.GetTileAt(X, Y - 1);
        }

        public Tile WestNeighbour()
        {
            return World.GetTileAt(X -1, Y);
        }
    }
}
