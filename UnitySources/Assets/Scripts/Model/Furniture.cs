using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.Model
{
    /// <summary>
    /// Furniture represents an object that is 'permanently' installed on a <see cref="Tile"/>.
    /// </summary>
    public class Furniture : IXmlSerializable
    {
        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        public Dictionary<string, float> furnParameters;

        public Action<Furniture, float> updateActions;

        public Func<Furniture, Enterability> IsEntereable;

        /// <summary>
        /// Width of the Object in Tiles.
        /// </summary>
        private readonly int _width = 1;

        /// <summary>
        /// Height of the Object in Tiles.
        /// </summary>
        private readonly int _height = 1;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        /// <summary>
        /// Initializes a new instance of the <see cref="Furniture"/> class.
        /// </summary>
        public Furniture()
        {
            this.LinksToNeighbour = false;
            this.furnParameters = new Dictionary<string, float>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Furniture"/> class.
        /// </summary>
        /// <remarks>This will probably ONLY ever be used for prototypes.</remarks>
        /// <param name="objectType">The type of the new Furniture.</param>
        /// <param name="movementCost">The cost to move through this Furniture.</param>
        /// <param name="width">The width in Tiles of the new Furniture.</param>
        /// <param name="height">The height in Tiles of the new Furniture.</param>
        /// <param name="linksToNeighbour">Indicates whether this Furniture links to neighbouring Furniture or not.</param>
        public Furniture(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool isRoomEnclosure = false)
        {
            this.ObjectType = objectType;
            this.MovementCost = movementCost;
            this._width = width;
            this._height = height;
            this.LinksToNeighbour = linksToNeighbour;
            this.funcPositionValidation = this.__IsValidPosition;
            this.furnParameters = new Dictionary<string, float>();
            this.IsRoomEnclosure = isRoomEnclosure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Furniture"/> class.
        /// </summary>
        /// <param name="other">The Furniture instance to copy.</param>
        private Furniture(Furniture other)
        {
            this.ObjectType = other.ObjectType;
            this.MovementCost = other.MovementCost;
            this._width = other._width;
            this._height = other._height;
            this.LinksToNeighbour = other.LinksToNeighbour;

            this.furnParameters = new Dictionary<string, float>(other.furnParameters);
            if (other.updateActions != null)
            {
                this.updateActions = (Action<Furniture, float>)other.updateActions.Clone();
            }

            this.IsEntereable = other.IsEntereable;
            this.IsRoomEnclosure = other.IsRoomEnclosure;
        }

        /* #################################################################### */
        /* #                         DELEGATES                                # */
        /* #################################################################### */

        public Action<Furniture> cbOnChanged;

        private readonly Func<Tile, bool> funcPositionValidation;

        /* #################################################################### */
        /* #                         PROPERTIES                               # */
        /* #################################################################### */

        /// <summary>
        /// Gets the Base Tile for this object. Large objects may occupy more tiles.
        /// </summary>
        public Tile Tile { get; private set; }

        /// <summary>
        /// Gets the ObjectType for this object. Will lbe queried by the visual system to know what sprite to render for this object.
        /// </summary>
        public string ObjectType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this Furniture links to neighbouring furniture of the same type?
        /// </summary>
        public bool LinksToNeighbour { get; private set; }

        /// <summary>
        /// Gets the cost of moving through this object.
        /// </summary>
        /// <remarks>If this is zero, the Tile is impassable.</remarks>
        public float MovementCost { get; private set; }

        public bool IsRoomEnclosure { get; private set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        /// <summary>
        /// Install a copy of the specified Prototype to the specified Tile.
        /// </summary>
        /// <param name="proto">The Prototype Furniture to use to create an actual instance.</param>
        /// <param name="tile">The Tile to place the new Furniture onto.</param>
        /// <returns>The placed Furniture</returns>
        public static Furniture PlaceInstance(Furniture proto, Tile tile)
        {
            if (proto.funcPositionValidation(tile) == false)
            {
                Debug.LogError("PlaceInstance position validity function returned false.");
                return null;
            }

            var obj = proto.Clone();

            obj.Tile = tile;

            if (tile.PlaceFurniture(obj) == false)
            {
                // Something prevented the object being placed. Don't return it, so it gets GC'd out at some point.
                return null;
            }

            if (obj.LinksToNeighbour)
            {
                // Notify any linked neighbours of this new item.
                Tile t;
                int x = tile.X;
                int y = tile.Y;

                t = tile.World.GetTileAt(x, y + 1);
                if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null &&
                    t.Furniture.ObjectType == obj.ObjectType)
                {
                    // The North Tile needs to be updated.
                    t.Furniture.cbOnChanged(t.Furniture);
                }

                t = tile.World.GetTileAt(x + 1, y);
                if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null &&
                    t.Furniture.ObjectType == obj.ObjectType)
                {
                    // The East Tile needs to be updated.
                    t.Furniture.cbOnChanged(t.Furniture);
                }

                t = tile.World.GetTileAt(x, y - 1);
                if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null &&
                    t.Furniture.ObjectType == obj.ObjectType)
                {
                    // The South Tile needs to be updated.
                    t.Furniture.cbOnChanged(t.Furniture);
                }

                t = tile.World.GetTileAt(x - 1, y);
                if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null &&
                    t.Furniture.ObjectType == obj.ObjectType)
                {
                    // The West Tile needs to be updated.
                    t.Furniture.cbOnChanged(t.Furniture);
                }
            }

            return obj;
        }

        public virtual Furniture Clone()
        {
            return new Furniture(this);
        }

        public void RegisterOnChangedCallback(Action<Furniture> cb)
        {
            this.cbOnChanged += cb;
        }

        public void UnRegisterOnChangedCallback(Action<Furniture> cb)
        {
            this.cbOnChanged -= cb;
        }

        /// <summary>
        /// Called by the World each 'tick' to update this object.
        /// </summary>
        /// <param name="deltaTime">The amount of time that has passed since the last tick.</param>
        public void Update(float deltaTime)
        {
            if (this.updateActions != null)
            {
                this.updateActions(this, deltaTime);
            }
        }

        public bool IsValidPosition(Tile t)
        {
            return this.funcPositionValidation(t);
        }

        /*
        public bool IsValidPositionForDoor(Tile t)
        {
            if (__IsValidPosition(t) == false) return false;

            // TODO: Make sure we have either N/S walls or E/W walls.
            if (t.World.GetTileAt(t.X, t.Y + 1).Furniture.ObjectType == "Wall" &&
                t.World.GetTileAt(t.X, t.Y - 1).Furniture.ObjectType == "Wall")
            {
                return true;
            }
            if (t.World.GetTileAt(t.X + 1, t.Y).Furniture.ObjectType == "Wall" &&
                t.World.GetTileAt(t.X - 1, t.Y).Furniture.ObjectType == "Wall")
            {
                return true;
            }

            return true;
        }
        */

        public void Door_UpdateAction(float deltaTime)
        {
            //this.furnParameters["openness"] += deltaTime;
        }

        /// <summary>
        /// Part of the IXmlSerializable interface implementation.
        /// </summary>
        /// <returns>null</returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this.MovementCost = float.Parse(reader.GetAttribute("movementCost"));

            if (reader.ReadToDescendant("Param"))
            {
                do
                {
                    var k = reader.GetAttribute("name");
                    var v = float.Parse(reader.GetAttribute("value"));
                    furnParameters[k] = v;
                } while (reader.ReadToNextSibling("Param"));
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Furniture");
            writer.WriteAttributeString("X", this.Tile.X.ToString());
            writer.WriteAttributeString("Y", this.Tile.Y.ToString());
            writer.WriteAttributeString("objectType", this.ObjectType);
            writer.WriteAttributeString("movementCost", this.MovementCost.ToString());

            foreach (var k in furnParameters)
            {
                writer.WriteStartElement("Param");
                writer.WriteAttributeString("name", k.Key);
                writer.WriteAttributeString("value", k.Value.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private bool __IsValidPosition(Tile t)
        {
            // Make sure Tile is of type Floor.
            if (t.Type != TileType.Floor)
            {
                Debug.Log("Tile is not a floor.");
                return false;
            }

            // Make sure Tile doesn't already have any Furniture.
            if (t.Furniture != null)
            {
                Debug.Log("Tile already has furniture.");
                return false;
            }

            return true;
        }

    }
}