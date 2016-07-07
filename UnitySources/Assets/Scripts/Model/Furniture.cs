using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Assets.Scripts.Model
{
    /// <summary>
    /// Furniture represents an object that is 'permanently' installed on a <see cref="Tile"/>.
    /// </summary>
    [MoonSharpUserData]
    public class Furniture : IXmlSerializable
    {
        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

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

        /// <summary>
        /// If this furniture gets worked by a person, where is the correct place to stand?
        /// </summary>
        public Vector2 JobSpotOffset { get; set; }

        /// <summary>
        /// If this furniture spawns anything, where does it appear?
        /// </summary>
        public Vector2 JobSpawnOffset { get; set; }

        /// <summary>
        /// Backing-field for the property "Name".
        /// </summary>
        private string _name = string.Empty;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        /// <summary>
        /// Initializes a new instance of the <see cref="Furniture"/> class.
        /// </summary>
        public Furniture()
        {
            this.Name = string.Empty;
            this.LinksToNeighbour = false;
            this._parameters = new Dictionary<string, float>();
            this._jobs = new List<Job>();
            this.Tint = Color.white;
            this.JobSpotOffset = Vector2.zero;
            this.JobSpawnOffset = Vector2.zero;
            this._cbUpdateActions = new List<string>();
            this._cbIsEnterableAction = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Furniture"/> class.
        /// </summary>
        /// <remarks>Don't call this directly. Use Clone() instead.</remarks>
        /// <param name="other">The Furniture instance to copy.</param>
        private Furniture(Furniture other) : this()
        {
            this.ObjectType = other.ObjectType;
            this.Name = other.Name;
            this.MovementCost = other.MovementCost;
            this._width = other._width;
            this._height = other._height;
            this.LinksToNeighbour = other.LinksToNeighbour;
            this.IsRoomEnclosure = other.IsRoomEnclosure;
            this.Tint = other.Tint;
            this.JobSpotOffset = other.JobSpotOffset;
            this.JobSpawnOffset = other.JobSpawnOffset;
            this._parameters = new Dictionary<string, float>(other._parameters);
            this._cbUpdateActions = new List<string>(other._cbUpdateActions);
            this._cbIsEnterableAction = other._cbIsEnterableAction;

            if (other._funcPositionValidation != null)
            {
                this._funcPositionValidation = (Func<Tile, bool>)other._funcPositionValidation.Clone();
            }
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
        /// <param name="isRoomEnclosure">Indicates that this Furnitures defines rooms.</param>
        public Furniture(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool isRoomEnclosure = false) : this()
        {
            this.ObjectType = objectType;
            this.MovementCost = movementCost;
            this._width = width;
            this._height = height;
            this.LinksToNeighbour = linksToNeighbour;
            this._funcPositionValidation = this.__IsValidPosition;
            this.IsRoomEnclosure = isRoomEnclosure;
        }

        /* #################################################################### */
        /* #                         DELEGATES                                # */
        /* #################################################################### */

        public Action<Furniture> cbOnChanged;
        public Action<Furniture> cbOnRemoved;

        /// <summary>
        /// These actions are called on every update. They get called with a Furniture, and the deltaTime.
        /// </summary>
        private List<string> _cbUpdateActions;

        private string _cbIsEnterableAction;
         
        private readonly Func<Tile, bool> _funcPositionValidation;

        /* #################################################################### */
        /* #                         PROPERTIES                               # */
        /* #################################################################### */

        /// <summary>
        /// Gets the Base Tile for this object. Large objects may occupy more tiles.
        /// </summary>
        public Tile Tile { get; private set; }

        /// <summary>
        /// Gets the ObjectType for this object. Will be queried by the visual system to know what sprite to render for this object.
        /// </summary>
        public string ObjectType { get; private set; }

        /// <summary>
        /// Gets the Name of this object.
        /// </summary>
        public string Name {
            get { return string.IsNullOrEmpty(_name) ? ObjectType : _name; }
            set { _name = value; } }

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
        /// <returns>float</returns>
        public float GetParameter(string key)
        {
            return GetParameter(key, 0f);
        }

        /// <summary>
        /// Gets the custom Furniture parameter.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>float</returns>
        public float GetParameter(string key, float defaultValue)
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
        /// <param name="fname">Action to call.</param>
        public void RegisterUpdateAction(string fname)
        {
            _cbUpdateActions.Add(fname);
        }

        /// <summary>
        /// Unregisters a function that has been added with <see cref="RegisterIsEnterableAction"/>.
        /// </summary>
        /// <param name="fname">Action to remove.</param>
        public void UnregisterIsEnterableAction()
        {
            _cbIsEnterableAction = string.Empty;
        }

        /// <summary>
        /// Registers a function that will be called every time something needs to know if this Furniture is walkable.
        /// </summary>
        /// <param name="fname">Action to call.</param>
        public void RegisterIsEnterableAction(string fname)
        {
            _cbIsEnterableAction = fname;
        }

        /// <summary>
        /// Unregisters a function that has been set to be called on every Update.
        /// </summary>
        /// <param name="fname">Action to remove.</param>
        public void UnregisterUpdateAction(string fname)
        {
            _cbUpdateActions.Remove(fname);
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
                Debug.LogErrorFormat("PlaceInstance position [{0},{1}] validity function for {2} returned false.", tile.X, tile.Y, proto.ObjectType);
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

                var t = World.Current.GetTileAt(x, y + 1);
                if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null &&
                    t.Furniture.ObjectType == obj.ObjectType)
                {
                    // The North Tile needs to be updated.
                    t.Furniture.cbOnChanged(t.Furniture);
                }

                t = World.Current.GetTileAt(x + 1, y);
                if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null &&
                    t.Furniture.ObjectType == obj.ObjectType)
                {
                    // The East Tile needs to be updated.
                    t.Furniture.cbOnChanged(t.Furniture);
                }

                t = World.Current.GetTileAt(x, y - 1);
                if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null &&
                    t.Furniture.ObjectType == obj.ObjectType)
                {
                    // The South Tile needs to be updated.
                    t.Furniture.cbOnChanged(t.Furniture);
                }

                t = World.Current.GetTileAt(x - 1, y);
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

        public void RegisterOnRemovedCallback(Action<Furniture> cb)
        {
            this.cbOnRemoved += cb;
        }

        public void UnRegisterOnRemovedCallback(Action<Furniture> cb)
        {
            this.cbOnRemoved -= cb;
        }

        /// <summary>
        /// Called by the World each 'tick' to update this object.
        /// </summary>
        /// <param name="deltaTime">The amount of time that has passed since the last tick.</param>
        public void Update(float deltaTime)
        {
            if (this._cbUpdateActions != null)
            {
                FurnitureActions.CallFunctionsWithFurniture(_cbUpdateActions, this, deltaTime);
            }
        }

        public Enterability IsEnterable()
        {
            if (string.IsNullOrEmpty(_cbIsEnterableAction))
            {
                return Enterability.Yes;
            }

            var ret = FurnitureActions.CallFunction(_cbIsEnterableAction, this);
            return (Enterability)ret.Number;
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

        public void ReadXmllPrototype(XmlReader reader)
        {
            
        }
            
        public void ReadXml(XmlReader reader)
        {
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
            // writer.WriteAttributeString("movementCost", this.MovementCost.ToString());

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
                    var t2 = World.Current.GetTileAt(xOff, yOff);

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

        private void ClearJobs()
        {
            var jobs = _jobs.ToArray();
            foreach (var j in _jobs.ToArray())
            {
                RemoveJob(j);
            }
        }

        public void AddJob(Job job)
        {
            job.Furniture = this;
            _jobs.Add(job);
            job.RegisterOnJobStoppedCallback(OnJobStopped);
            World.Current.JobQueue.Enqueue(job);
        }

        public void CancelJobs()
        {
            var jobs = _jobs.ToArray();
            foreach (var j in _jobs.ToArray())
            {
                j.CancelJob();
            }
        }

        private void RemoveJob(Job job)
        {
            job.UnregisterOnJobStoppedCallback(OnJobStopped);
            _jobs.Remove(job);
            job.Furniture = null;
        }

        public void OnJobStopped(Job job)
        {
            RemoveJob(job);
        }

        public int GetJobCount()
        {
            return _jobs.Count;
        }

        public bool IsStockpile()
        {
            return ObjectType == "Stockpile";
        }

        public void Deconstruct()
        {
            Debug.Log("Deconstructing...");

            Tile.UnplaceFurniture();

            if (cbOnRemoved != null) cbOnRemoved(this);

            // If we removed something that defines a Room, we need to re-set the Rooms around it.
            if (IsRoomEnclosure)
            {
                Room.DoRoomFloodfill(this.Tile);
            }

            // If we've removed something, there's a fair chance routes to places have changed,
            // so recalculate the pathfinding graph.
            World.Current.InvalidateTileGraph();
        }

        public Tile GetJobSpotTile()
        {
            return World.Current.GetTileAt(Tile.X + (int)JobSpotOffset.x, Tile.Y + (int)JobSpotOffset.y);
        }

        public Tile GetSpawnSpotTile()
        {
            return World.Current.GetTileAt(Tile.X + (int)JobSpawnOffset.x, Tile.Y + (int)JobSpawnOffset.y);
        }
    }
}