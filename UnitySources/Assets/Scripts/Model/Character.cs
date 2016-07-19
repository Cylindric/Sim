using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    [DebuggerDisplay("Character {Name} at [{X}, {Y}]")]
    public class Character
    {
        private const float TimeBetweenJobSearches = 1f;
        private const float BaseMovementSpeed = 5f;

        private const float EnergyChargeRate = (1f/300); // The internal magic fusion core mcguffin charges the character at this rate.
        private const float ShieldEnergyUsageRate = (2f/300); // The shield uses power at this rate - should be larger than EnergyChargeRate, otherwise it'll never run out.

        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        public Inventory Inventory;

        private Tile _destTile;
        private Tile DestTile
        {
            get { return _destTile; }
            set
            {
                if (_destTile != value)
                {
                    _destTile = value;
                    _path = null;
                }
            }
        }

        private Tile _nextTile; // The next Tile in the pathfinding sequence
        private Path_AStar _path;
        private readonly float _speed;
        private Job _job;
        private float _movementPercentage;
        private float _timeSinceLastJobSearch;
        private readonly Dictionary<string, float> _conditions;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        public Character()
        {
            this._speed = BaseMovementSpeed;
            this.Name = MarkovNameGenerator.GetNextName("male") + ' ' + MarkovNameGenerator.GetNextName("last");
            this.IsWorking = false;
            _conditions = new Dictionary<string, float> {{"energy", 1f}, {"health", 1f}};
        }

        public Character(Tile tile) : this()
        {
            CurrentTile = DestTile = _nextTile = tile;
        }

        /* #################################################################### */
        /* #                         DELEGATES                                # */
        /* #################################################################### */

        private Action<Character> cbCharacterChanged;

        /* #################################################################### */
        /* #                         PROPERTIES                               # */
        /* #################################################################### */

        public float X
        {
            get
            {
                if (_nextTile == null) return CurrentTile.X;

                return Mathf.Lerp(CurrentTile.X, _nextTile.X, _movementPercentage);
            }
        }

        public float Y
        {
            get
            {
                if (_nextTile == null) return CurrentTile.Y;

                return Mathf.Lerp(CurrentTile.Y, _nextTile.Y, _movementPercentage);
            }
        }

        public Tile CurrentTile { get; private set; }

        public string Name { get; set; }

        public bool IsWorking { get; private set; }

        public Job CurrentJob
        {
            get { return _job; }
        }

        public bool ShieldStatus { get; private set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        public void AbandonJob()
        {
            _nextTile = DestTile = CurrentTile;
            World.Instance.JobQueue.Enqueue(_job);
            _job = null;
            IsWorking = false;
        }

        public void Update(float deltaTime)
        {
            Update_DoJob(deltaTime);
            Update_DoMovement(deltaTime);

            // Hack in some environmental effects.
            // Currently breathability is all that affects if the shield is up or not.
            if (this.CanBreathe())
            {
                this.Breathe(deltaTime);
                ShieldStatus = false;
            }
            else
            {
                ShieldStatus = true;
            }

            // Consume energy if the shield is enabled.
            if (ShieldStatus == true)
            {
                this.ChangeCondition("energy", -ShieldEnergyUsageRate * deltaTime);
            }

            // Charge up the energy store
            this.ChangeCondition("energy", EnergyChargeRate * deltaTime);
            

            if (cbCharacterChanged != null) cbCharacterChanged(this);
        }

        public void SetDestination(Tile tile)
        {
            if (CurrentTile.IsNeighbour(tile, true) == false)
            {
                Debug.Log("Character::SetDestination -- Our destination Tile isn't actually our neighbour.");
            }

            DestTile = tile;
        }

        public void RegisterOnChangedCallback(Action<Character> cb)
        {
            cbCharacterChanged += cb;
        }

        public void UnregisterOnChangedCallback(Action<Character> cb)
        {
            cbCharacterChanged -= cb;
        }

        public void ReadXml(XmlNode xml)
        {
            if (xml.Attributes != null && xml.Attributes["name"] != null && xml.Attributes["name"].ToString().Length > 0)
            {
                this.Name = xml.Attributes["name"].Value;
            }

            var condsNode = xml.SelectSingleNode("./Conditions");
            if (condsNode != null)
            {
                var condNodes = condsNode.SelectSingleNode("./Condition");
                if (condNodes != null)
                {
                    foreach (XmlElement condNode in condNodes)
                    {
                        var name = condNode.Attributes["name"].Value;
                        var value = float.Parse(condsNode.InnerText);
                        this.SetCondition(name, value);
                    }
                }
            }

            var inventoryNode = xml.SelectSingleNode("./Inventory");
            if (inventoryNode != null)
            {
                this.Inventory = new Inventory();
                this.Inventory.Character = this;
                this.Inventory.ReadXml(inventoryNode);
            }
        }

        public XmlElement WriteXml(XmlDocument xml)
        {
            var character = xml.CreateElement("Character");
            character.SetAttribute("x", CurrentTile.X.ToString());
            character.SetAttribute("y", CurrentTile.Y.ToString());
            character.SetAttribute("name", this.Name);

            if (_conditions.Count > 0)
            {
                var condsXml = xml.CreateElement("Conditions");
                foreach (var cond in _conditions)
                {
                    var condXml = xml.CreateElement("Condition");
                    condXml.SetAttribute("name", cond.Key);
                    condXml.InnerText = cond.Value.ToString(CultureInfo.InvariantCulture);
                    condsXml.AppendChild(condXml);
                }
                character.AppendChild(condsXml);
            }

            if (this.Inventory != null)
            {
                character.AppendChild(this.Inventory.WriteXml(xml));
            }

            return character;
        }

        private void GetNewJob()
        {
            // Grab a new job.
            // _job = World.Instance.JobQueue.TakeClosestJobToTile(CurrentTile);
            _job = World.Instance.JobQueue.TakeFirstJobFromQueue();
            _timeSinceLastJobSearch = 0;

            if (_job == null) return;
            
            if (_job.Furniture != null)
            {
                DestTile = _job.Furniture.GetJobSpotTile();
            }
            else
            {
                DestTile = _job.Tile;
            }

            _job.RegisterOnJobStoppedCallback(OnJobStopped);

            // Check to see if the job is reachable from the Character's current position.
            // We mmight have to go somewhere else first to get materials.

            // If we are already at the worksite, just return, otherwise need to calculate a route there.
            if (DestTile == CurrentTile)
            {
                return;
            }

            _path = new Path_AStar(World.Instance, CurrentTile, DestTile);
            if (_path.Length() == 0)
            {
                Debug.LogErrorFormat("Path_AStar returned no path from [{0},{1}] to target job Tile [{2},{3}]!", CurrentTile.X, CurrentTile.Y, DestTile.X, DestTile.Y);
                AbandonJob();
                _path = null;
                DestTile = CurrentTile;
            }
        }

        private void Update_DoJob(float deltaTime)
        {
            _timeSinceLastJobSearch += deltaTime;

            // Do I have a job?
            if (_job == null)
            {
                if (_timeSinceLastJobSearch >= TimeBetweenJobSearches)
                {
                    GetNewJob();
                }

                if (_job == null)
                {
                    // There is no job queued, so can just finish.
                    DestTile = CurrentTile;
                    return;
                }
            }

            // We have a job, and it is reachable.
            if (_job.HasAllMaterial() == false)
            {
                // We are missing resources required for the job
                
                // Are we carrying what we need?
                if (Inventory != null)
                {
                    if (_job.NeedsMaterial(Inventory) > 0)
                    {
                        DestTile = _job.Tile;

                        if (CurrentTile == DestTile || CurrentTile.IsNeighbour(DestTile, true))
                        {
                            // Set dest to current, just in case it was the neighbour-check that got us here
                            DestTile = CurrentTile;

                            // We are at the jobsite, so drop the Inventory.
                            World.Instance.InventoryManager.PlaceInventory(_job, Inventory);
                            _job.DoWork(0); // This will call all the cbJobWorked callbacks

                            if (Inventory.StackSize == 0)
                            {
                                Inventory = null;
                            }
                            else
                            {
                                Debug.LogError("Character is still carrying Inventory, which shouldn't be the case.");
                                Inventory = null;
                            }
                        }
                        else
                        {
                            // Still walking to the site.
                            DestTile = _job.Tile;
                            return;
                        }
                    }
                    else
                    {
                        // We are carrying something, but the job doesn't want it.
                        // Dump it where we are.
                        // TODO: actually dump it to an empty Tile, as we might be stood on a job Tile.
                        if (World.Instance.InventoryManager.PlaceInventory(CurrentTile, Inventory) == false)
                        {
                            Debug.LogError("Character tried to dump Inventory to an invalid Tile.");
                            // TODO: At this point we should try to dump this inv somewhere else, but for now we're just deleting it.
                            Inventory = null;
                        }
                    }
                }
                else
                {
                    // At this point, the job still requires Inventory, but we don't have it.
                    // That means we need to walk towards a Tile that does have the required items.

                    if (CurrentTile.Inventory != null && 
                        (_job.CanTakeFromStockpile || CurrentTile.Furniture == null || CurrentTile.Furniture.IsStockpile() == false) &&
                        _job.NeedsMaterial(CurrentTile.Inventory) != 0)
                    {
                        // The materials we need are right where we're stood!
                        World.Instance.InventoryManager.PlaceInventory(
                            character: this, 
                            source: CurrentTile.Inventory, 
                            qty: _job.NeedsMaterial(CurrentTile.Inventory));

                    }
                    else
                    {
                        // The Job needs some of this:
                        var unsatisfied = _job.GetFirstRequiredInventory();

                        // We might have a path to the item we need already
                        var endTile = _path == null ? null : _path.EndTile();
                        if (_path != null && endTile != null && endTile.Inventory != null &&
                            endTile.Inventory.ObjectType == unsatisfied.ObjectType)
                        {
                            // We are already moving towards a tile with the items we want, just keep going.
                        }
                        else
                        {
                            // Look for the first item that matches
                            this._path = World.Instance.InventoryManager.GetClosestPathToInventoryOfType(
                                objectType: unsatisfied.ObjectType,
                                t: CurrentTile,
                                desiredQty: unsatisfied.MaxStackSize - unsatisfied.StackSize,
                                searchInStockpiles: _job.CanTakeFromStockpile);

                            if (this._path == null || this._path.Length() == 0)
                            {
                                //Debug.LogFormat("No Tile found containing the desired type ({0}).", unsatisfied.ObjectType);
                                AbandonJob();
                                return;
                            }

                            _destTile = this._path.EndTile();
                            _nextTile = _path.Dequeue();
                        }

                        return;
                    }
                }

                // Cannot continue until we have everythign we need.
                return;
            }

            // We have all the material that we need
            // Make sure the destination Tile is the job Tile
            DestTile = _job.Tile;

            // Are we there yet?
            if (CurrentTile == _job.Tile || CurrentTile.IsNeighbour(_job.Tile, true))
            {
                IsWorking = true;

                // Set dest to current, just in case it was the neighbour-check that got us here
                DestTile = CurrentTile;

                _job.DoWork(deltaTime);
            }

            // Done.
        }

        private void Update_DoMovement(float deltaTime)
        {
            if (CurrentTile == DestTile)
            {
                _path = null;
                return; // We're already were we want to be.
            }
            
            if (_nextTile == null || _nextTile == CurrentTile)
            {
                // Get the next Tile from the pathfinder.
                if (_path == null || _path.Length() == 0)
                {
                    // Generate a path to our destination
                    _path = new Path_AStar(World.Instance, CurrentTile, DestTile);
                    // This will calculate a path from curr to dest.
                    if (_path.Length() == 0)
                    {
                        //Debug.LogError("Path_AStar returned no path to destination!");
                        AbandonJob();
                        _path = null;
                        return;
                    }

                    // Ignore the first Tile in the path, as that's the Tile we are currently in,
                    // and we can always move out of our current Tile.
                    _nextTile = _path.Dequeue();
                }

                // Grab the next waypoint from the pathing system!
                _nextTile = _path.Dequeue();


                if (_nextTile == CurrentTile)
                {
                    // Debug.LogError("Update_DoMovement - _nextTile is CurrentTile?");
                }
            }

            if (_nextTile == null) _nextTile = CurrentTile;

            // At this point we should have a valid _nextTile to move to.

            // What's the total distance from point A to point B?
            // We are going to use Euclidean distance FOR NOW...
            // But when we do the pathfinding system, we'll likely
            // switch to something like Manhattan or Chebyshev distance
            float distToTravel = 0;
            if (_nextTile != CurrentTile)
            {
                distToTravel = Mathf.Sqrt(
                    Mathf.Pow(CurrentTile.X - _nextTile.X, 2) +
                    Mathf.Pow(CurrentTile.Y - _nextTile.Y, 2)
                    );
            }

            // Before entering a Tile, make sure it is not impassable.
            // This might happen if the Tile is changed (e.g. wall built) after the pathfinder runs.
            if (_nextTile.IsEnterable() == Enterability.Never)
            {
                // Debug.LogError("Error - Character was strying to enter an impassable Tile!");
                _nextTile = null;
                _path = null;
            }
            else if(_nextTile.IsEnterable() == Enterability.Soon)
            {
                // The next Tile we're trying to enter is walkable, but maybe for some reason
                // cannot be entered right now. Perhaps it is occupied, or contains a closed door.
                return;
            }

            // How much distance can be travel this Update?
            if (_nextTile == null) _nextTile = CurrentTile;
            var distThisFrame = 0f;
            try
            {
                distThisFrame = (_speed/_nextTile.MovementCost)*deltaTime;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            // How much is that in terms of percentage to our destination?
            float percThisFrame;
            if (Mathf.Approximately(distToTravel, 0f))
            {
                percThisFrame = 1f;
            }
            else
            {
                percThisFrame = distThisFrame/distToTravel;
            }

            // Add that to overall percentage travelled.
            _movementPercentage += percThisFrame;

            if (_movementPercentage >= 1)
            {
                // We have reached our destination

                // TODO: Get the next Tile from the pathfinding system.
                //       If there are no more tiles, then we have TRULY
                //       reached our destination.

                CurrentTile = _nextTile;
                _movementPercentage = 0;
            }
        }

        private void OnJobStopped(Job j)
        {
            // Job completed or was cancelled.
            IsWorking = false;

            j.UnregisterOnJobStoppedCallback(OnJobStopped);

            if (j != _job)
            {
                // Debug.LogError("Character being told about job (" + j.Name + ") that isn't his. You forgot to unregister something.");
                return;
            }

            _job = null;
        }

        public bool CanBreathe()
        {
            if (CurrentTile == null) return false;
            if (CurrentTile.Room == null) return false;
            return CurrentTile.Room.Atmosphere.IsBreathable();
        }

        /// <summary>
        /// Perform some gas-exchange calculations and apply to the local environment.
        /// </summary>
        /// <remarks>
        /// It's not as simple as some people think...
        /// Some numbers at https://en.wikipedia.org/wiki/Breathing#Composition
        /// Alan Boyd has helpfully put some calculations up at http://biology.stackexchange.com/questions/5642/how-much-gas-is-exchanged-in-one-human-breath
        /// </remarks>
        /// <param name="deltaTime">Frame-time</param>
        public void Breathe(float deltaTime)
        {
            if (CurrentTile == null) return;
            if (CurrentTile.Room == null) return;

            // hack the deltaTime to speed up the simulation a bit
            deltaTime *= 10;

            // We can assume an at-rest breathing rate of about 15 breaths per minute (https://en.wikipedia.org/wiki/Lung_volumes)
            var breaths = (15f/60) * deltaTime; // Breaths-per-second (this frame) 

            // We can assume an average "tidal volume" of air moving in and out of a person is 0.5L (https://en.wikipedia.org/wiki/Lung_volumes)
            var consumedO2Volume = 0.5f * breaths * 0.001f; // Cubic Metres

            // Consume some oxygen.
            CurrentTile.Room.Atmosphere.ChangeGas("O2", -consumedO2Volume);

            // In each breath in, we take in about 18mg of O2, and release back out 36mg of CO2 and 20mg of H2O, which is 0.8 molecules of CO2 for every molecule of O2.
            // I'm not sure how to convert that into a sensible "CO2 produced" number, so this is MADE UP. TODO: Don't make this up.
            CurrentTile.Room.Atmosphere.ChangeGas("CO2", consumedO2Volume);
        }

        public float GetCondition(string name)
        {
            return _conditions.ContainsKey(name) ? _conditions[name] : 0f;
        }

        public float SetCondition(string name, float value, bool clamp = true)
        {
            if (clamp)
            {
                value = Mathf.Clamp01(value);
            }

            if (_conditions.ContainsKey(name) == false)
            {
                _conditions.Add(name, value);
            }
            else
            {
                _conditions[name] = value;
            }

            return _conditions[name];
        }

        public float ChangeCondition(string name, float delta, bool clamp = true)
        {
            return this.SetCondition(name, this.GetCondition(name) + delta, clamp);
        }
    }
}