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

        public Func<Furniture, Enterability> IsEntereable;

        private Dictionary<string, float> _parameters;

        /// <summary>
        /// Width of the Object in Tiles.
        /// </summary>
        public readonly int _width = 1; // TODO: change to property

        /// <summary>
        /// Height of the Object in Tiles.
        /// </summary>
        public readonly int _height = 1; // TODO: change to property

        private List<Job> _jobs;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        /// <summary>
        /// Initializes a new instance of the <see cref="Furniture"/> class.
        /// </summary>
        public Furniture()
        {
            this.LinksToNeighbour = false;
            this._parameters = new Dictionary<string, float>();
            this._jobs = new List<Job>();
            this.Tint = Color.white;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Furniture"/> class.
        /// </summary>
        /// <remarks>Don't call this directly. Use Clone() instead.</remarks>
        /// <param name="other">The Furniture instance to copy.</param>
        private Furniture(Furniture other)
        {
            this.ObjectType = other.ObjectType;
            this.MovementCost = other.MovementCost;
            this._width = other._width;
            this._height = other._height;
            this.LinksToNeighbour = other.LinksToNeighbour;
            this.IsEntereable = other.IsEntereable;
            this.IsRoomEnclosure = other.IsRoomEnclosure;
            this.Tint = other.Tint;
            this._parameters = new Dictionary<string, float>(other._parameters);

            if (other._cbUpdateActions != null)
            {
                this._cbUpdateActions = (Action<Furniture, float>)other._cbUpdateActions.Clone();
            }
            if (other._funcPositionValidation != null)
            {
                this._funcPositionValidation = (Func<Tile, bool>)other._funcPositionValidation.Clone();
            }

            this._jobs = new List<Job>();
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
            this._funcPositionValidation = this.__IsValidPosition;
            this._parameters = new Dictionary<string, float>();
            this.IsRoomEnclosure = isRoomEnclosure;
            this._jobs = new List<Job>();
            this.Tint = Color.white;
        }

        /* #################################################################### */
        /* #                         DELEGATES                                # */
        /* #################################################################### */

        public Action<Furniture> cbOnChanged;

        /// <summary>
        /// These actions are called on every update. They get called with a Furniture, and the deltaTime.
        /// </summary>
        private Action<Furniture, float> _cbUpdateActions;

        private readonly Func<Tile, bool> _funcPositionValidation;

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

        /// <summary>
        /// Gets a value if this Furniture defines a separate room.
        /// </summary>
        public bool IsRoomEnclosure { get; private set; }

        public Color Tint { get; set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        /// <summary>
        /// Gets the custom Furniture parameter.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>float</returns>
        public float GetParameter(string key, float defaultValue = 0)
        {
            if (_parameters.ContainsKey(key) == false)
            {
                return defaultValue;
            }
            return _parameters[key];
        }

        /// <summary>
        /// Sets the custom Furniture parameter.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">New Value</param>
        public void SetParameter(string key, float value)
        {
            _parameters[key] = value;
        }

        /// <summary>
        /// Changes the custom Furniture parameter by the specified amount.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Delta value</param>
        public void OffsetParameter(string key, float value)
        {
            if (_parameters.ContainsKey(key) == false)
            {
                _parameters[key] = value;
            }
            _parameters[key] += value;
        }

        /// <summary>
        /// Registers a function that will be called on every Update.
        /// </summary>
        /// <param name="a">Action to call.</param>
        public void RegisterUpdateAction(Action<Furniture, float> a)
        {
            _cbUpdateActions += a;
        }

        /// <summary>
        /// Unregisters a function that has been set to be called on every Update.
        /// </summary>
        /// <param name="a">Action to remove.</param>
        public void UnregisterUpdateAction(Action<Furniture, float> a)
        {
            _cbUpdateActions -= a;
        }

        /// <summary>
        /// Install a copy of the specified Prototype to the specified Tile.
        /// </summary>
        /// <param name="proto">The Prototype Furniture to use to create an actual instance.</param>
        /// <param name="tile">The Tile to place the new Furniture onto.</param>
        /// <returns>The placed Furniture</returns>
        public static Furniture PlaceInstance(Furniture proto, Tile tile)
        {
            if (proto._funcPositionValidation(tile) == false)
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
                int x = tile.X;
                int y = tile.Y;

                var t = tile.World.GetTileAt(x, y + 1);
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

        /// <summary>
        /// Make a copy of the current Furniture object.
        /// </summary>
        /// <remarks>Sub-classes should override this Clone() if a different copy constructor should be run.</remarks>
        /// <returns></returns>
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
            if (this._cbUpdateActions != null)
            {
                this._cbUpdateActions(this, deltaTime);
            }
        }

        public bool IsValidPosition(Tile t)
        {
            return this._funcPositionValidation(t);
        }

        public void Door_UpdateAction(float deltaTime)
        {
            //this.OffsetParameter("openness", deltaTime);
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
                    _parameters[k] = v;
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

            foreach (var k in _parameters)
            {
                writer.WriteStartElement("Param");
                writer.WriteAttributeString("name", k.Key);
                writer.WriteAttributeString("value", k.Value.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// This will be replaced by validation checks fed to us from customisable LUA files.
        /// </summary>
        /// <param name="t"></param>
        /// <returns>True if the Tile is valid; else false.</returns>
        private bool __IsValidPosition(Tile t)
        {
            for (var xOff = t.X; xOff < t.X + _width; xOff++)
            {
                for (var yOff = t.Y; yOff < t.Y + _height; yOff++)
                {
                    var t2 = t.World.GetTileAt(xOff, yOff);

                    // Make sure Tile is of type Floor.
                    if (t2.Type != TileType.Floor)
                    {
                        // Debug.Log("Tile is not a floor.");
                        return false;
                    }

                    // Make sure Tile doesn't already have any Furniture.
                    if (t2.Furniture != null)
                    {
                        // Debug.Log("Tile already has furniture.");
                        return false;
                    }
                }
            }

            return true;
        }

        public void ClearJobs()
        {
            foreach (var j in _jobs)
            {
                j.CancelJob();
                Tile.World.JobQueue.Remove(j);
            }

            _jobs = new List<Job>();
        }

        public void AddJob(Job job)
        {
            _jobs.Add(job);
            Tile.World.JobQueue.Enqueue(job);
        }

        public void RemoveJob(Job job)
        {
            _jobs.Remove(job);
            job.CancelJob();
            Tile.World.JobQueue.Remove(job);
        }

        public int GetJobCount()
        {
            return _jobs.Count;
        }

        public bool IsStockpile()
        {
            return ObjectType == "Stockpile";
        }
    }
}