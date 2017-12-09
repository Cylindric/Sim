using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Xsl;
using MoonSharp.Interpreter;
// using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Engine.Model
{
    /// <summary>
    /// Furniture represents an object that is 'permanently' installed on a <see cref="Tile"/>.
    /// </summary>
    [DebuggerDisplay("Furniture ({ObjectType} at [{Tile.X},{Tile.Y}])")]
    [MoonSharpUserData]
    public class Furniture 
    {
        private const float _repairStartThreshold = 0.1f;

        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        /// <summary>
        /// Backing-field for the property "Name".
        /// </summary>
        private string _name = string.Empty;

        private readonly Dictionary<string, float> _parameters;

        private readonly List<string> _services; 

        private readonly List<Job> _jobs;

        private float _lastFrameChange = 0f;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        /// <summary>
        /// Initializes a new instance of the <see cref="Furniture"/> class.
        /// </summary>
        public Furniture()
        {
            Name = string.Empty;
            LinksToNeighbour = false;
            Tint = Color.white;
            JobSpotOffset = Vector2.zero;
            JobSpawnOffset = Vector2.zero;
            MovementCost = 1f;
            IsRoomEnclosure = false;
            Width = 1;
            Height = 1;
            _parameters = new Dictionary<string, float> {{"condition", 1f}, {"decayTime", 1200f}};
            _services = new List<string>();
            _jobs = new List<Job>();
            _cbUpdateActions = new List<string>();
            _cbIsEnterableAction = string.Empty;
            GasParticlesEnabled = false;
            WorkingCharacter = null;
            IdleSprites = 0;
            _lastFrameChange = Random.Range(0f, 1f);
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
            this.Width = other.Width;
            this.Height = other.Height;
            this.LinksToNeighbour = other.LinksToNeighbour;
            this.IsRoomEnclosure = other.IsRoomEnclosure;
            this.Tint = other.Tint;
            this.JobSpotOffset = other.JobSpotOffset;
            this.JobSpawnOffset = other.JobSpawnOffset;
            this._parameters = new Dictionary<string, float>(other._parameters);
            this._services = new List<string>(other._services);
            this._cbUpdateActions = new List<string>(other._cbUpdateActions);
            this._cbIsEnterableAction = other._cbIsEnterableAction;
            this.Width = other.Width;
            this.Height = other.Height;
            this.GasParticlesEnabled = other.GasParticlesEnabled;
            this.WorkingCharacter = other.WorkingCharacter;
            this.IdleSprites = other.IdleSprites;

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
            this.Width = width;
            this.Height = height;
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

        /// <summary>
        /// Width of the Object in Tiles.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of the Object in Tiles.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// If this furniture gets worked by a person, where is the correct place to stand?
        /// </summary>
        public Vector2 JobSpotOffset { get; set; }

        /// <summary>
        /// If this furniture spawns anything, where does it appear?
        /// </summary>
        public Vector2 JobSpawnOffset { get; set; }

        public bool GasParticlesEnabled { get; set; }

        public Character WorkingCharacter { get; set; }

        public int IdleSprites { get; set; }

        public int CurrentIdleFrame { get; set; }

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
        public float OffsetParameter(string key, float value)
        {
            if (_parameters.ContainsKey(key) == false)
            {
                _parameters[key] = value;
            }
            _parameters[key] += value;

            return _parameters[key];
        }

        /// <summary>
        /// Returns a list of all tiles under this piece of furniture.
        /// </summary>
        /// <returns>The Tiles under this Furniture.</returns>
        public List<Tile> GetTilesUnderFurniture()
        {
            var list = new List<Tile>();
            for (var x = this.Tile.X; x <= this.Tile.X + this.Width; x++)
            {
                for (var y = this.Tile.Y; y <= this.Tile.Y + this.Height; x++)
                {
                    list.Add(World.Instance.GetTileAt(x, y));
                }
            }

            return list;
        } 

        public float OffsetParameter(string key, float value, float clampMin, float clampMax)
        {
            OffsetParameter(key, value);
            _parameters[key] = Mathf.Max(_parameters[key], clampMin);
            _parameters[key] = Mathf.Min(_parameters[key], clampMax);
            return _parameters[key];
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
                tile.UpdateNeighbours();
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
            if (_lastFrameChange > 1f)
            {
                CurrentIdleFrame++;
                CurrentIdleFrame = IdleSprites == 0 ? 0 : CurrentIdleFrame % IdleSprites;
                _lastFrameChange = 0;
            }
            _lastFrameChange += deltaTime;

            ApplyDecay(deltaTime);

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

        private void ApplyDecay(float deltaTime)
        {
            var decayTime = GetParameter("decayTime");
            float newCondition;

            if (decayTime <= 0)
            {
                newCondition = GetParameter("condition");
            }
            else { 
                newCondition = OffsetParameter("condition", -(1/decayTime)*deltaTime, 0, 1);
            }

            // Need repair?
            if (newCondition < _repairStartThreshold)
            {
                StartNewRepairJob();
            }

            //if (newCondition < 1f)
            //{
            //    Debug.LogFormat("{0} decayed to {1}", Name, newCondition);
            //}
        }

        private void StartNewRepairJob()
        {
            var j = new Job(
                tile: this.Tile,
                jobObjectType: null,
                cbJobComplete: null,
                jobTime: 5,
                inventoryRequirements: null,
                jobRepeats: false
                );
            j.Name = "Repairing";
            j.MinRange = 1;
            j.RegisterOnJobCompletedCallback(OnRepairComplete);
            AddJob(j);
        }

        private void OnRepairComplete(Job j)
        {
            var newCondition = OffsetParameter("condition", 0.1f);
            if (newCondition < 0.95f)
            {
                StartNewRepairJob();
            }
        }

        public void ReadXml(XmlElement element)
        {
            var parameters = (XmlElement)element.SelectSingleNode("./Parameters");
            if (parameters != null)
            {
                var param = parameters.SelectNodes("./Param");
                if (param != null)
                {
                    foreach (XmlNode p in param)
                    {
                        var name = p.Attributes["name"].Value;
                        var value = float.Parse(p.InnerText);
                        this.SetParameter(name, value);
                    }
                }
            }
        }

        public XmlElement WriteXml(XmlDocument xml)
        {
            var furniture = xml.CreateElement("Furniture");
            furniture.SetAttribute("x", this.Tile.X.ToString());
            furniture.SetAttribute("y", this.Tile.Y.ToString());
            furniture.SetAttribute("objectType", this.ObjectType);

            // Write out all the atmospheric data.
            if (_parameters.Count > 0)
            {
                var paramElement = xml.CreateElement("Parameters");
                foreach (var k in _parameters)
                {
                    if (k.Key == "condition" && Mathf.Approximately(k.Value, 1f))
                    {
                        continue;
                    }
                    var p = xml.CreateElement("Param");
                    p.SetAttribute("name", k.Key);
                    p.InnerText = k.Value.ToString(CultureInfo.InvariantCulture);
                    paramElement.AppendChild(p);
                }
                if (paramElement.ChildNodes.Count > 0)
                {
                    furniture.AppendChild(paramElement);
                }
            }

            return furniture;
        }

        /// <summary>
        /// This will be replaced by validation checks fed to us from customisable LUA files.
        /// </summary>
        /// <param name="t"></param>
        /// <returns>True if the Tile is valid; else false.</returns>
        private bool __IsValidPosition(Tile t)
        {
            for (var xOff = t.X; xOff < t.X + Width; xOff++)
            {
                for (var yOff = t.Y; yOff < t.Y + Height; yOff++)
                {
                    var t2 = World.Instance.GetTileAt(xOff, yOff);

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

        public void AddJob(Job job)
        {
            job.Furniture = this;
            _jobs.Add(job);
            job.RegisterOnJobStoppedCallback(OnJobStopped);
            World.Instance.JobQueue.Enqueue(job);
        }

        public void CancelJobs()
        {
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

        public int JobCount()
        {
            return _jobs.Count;
        }

        public bool IsStockpile()
        {
            return ObjectType == "furn_stockpile";
        }

        public void Deconstruct()
        {
            // Debug.Log("Deconstructing...");

            Tile.UnplaceFurniture();

            if (cbOnRemoved != null) cbOnRemoved(this);

            // If we removed something that defines a Room, we need to re-set the Rooms around it.
            if (IsRoomEnclosure)
            {
                Room.DoRoomFloodfill(this.Tile);
            }

            // If we've removed something, there's a fair chance routes to places have changed,
            // so recalculate the pathfinding graph.
            World.Instance.InvalidateTileGraph();
        }

        public Tile GetJobSpotTile()
        {
            var t = World.Instance.GetTileAt(Tile.X + (int)JobSpotOffset.x, Tile.Y + (int)JobSpotOffset.y);
            return t;
        }

        public Tile GetSpawnSpotTile()
        {
            return World.Instance.GetTileAt(Tile.X + (int)JobSpawnOffset.x, Tile.Y + (int)JobSpawnOffset.y);
        }
    }
}