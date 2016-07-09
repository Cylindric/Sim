using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Assets.Scripts.Pathfinding;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    [DebuggerDisplay("Character at [{X}, {Y}]")]
    public class 
        Character : IXmlSerializable
    {
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


        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        public Character()
        {
            this._speed = 5f;
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

        public Tile CurrentTile { get; protected set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        public void AbandonJob()
        {
            _nextTile = DestTile = CurrentTile;
            World.Instance.JobQueue.Enqueue(_job);
            _job = null;
        }

        public void Update(float deltaTime)
        {
            Update_DoJob(deltaTime);
            Update_DoMovement(deltaTime);

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

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            // Read a single Inventory item in.
            if (reader.ReadToDescendant("Inventory"))
            {
                this.Inventory = new Inventory();
                this.Inventory.Character = this;
                this.Inventory.ReadXml(reader);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Character");
            writer.WriteAttributeString("X", CurrentTile.X.ToString());
            writer.WriteAttributeString("Y", CurrentTile.Y.ToString());
            if (this.Inventory != null)
            {
                this.Inventory.WriteXml(writer);
            }
            writer.WriteEndElement();
        }

        private void GetNewJob()
        {
            // Grab a new job.
            _job = World.Instance.JobQueue.Dequeue();

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
                return;
            }
        }

        private void Update_DoJob(float deltaTime)
        {
            // Do I have a job?
            if (_job == null)
            {
                GetNewJob();

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

                        // Look for the first item that matches
                        var supply = World.Instance.InventoryManager.GetClosestInventoryOfType(
                            objectType: unsatisfied.ObjectType,
                            t: CurrentTile,
                            desiredQty: unsatisfied.MaxStackSize - unsatisfied.StackSize,
                            searchInStockpiles: _job.CanTakeFromStockpile);

                        if (supply == null)
                        {
                            //Debug.LogFormat("No Tile found containing the desired type ({0}).", unsatisfied.ObjectType);
                            AbandonJob();
                            return;
                        }

                        DestTile = supply.Tile;
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

            j.UnregisterOnJobStoppedCallback(OnJobStopped);

            if (j != _job)
            {
                Debug.LogError("Character being told about job (" + j.Name + ") that isn't his. You forgot to unregister something.");
                return;
            }

            _job = null;
        }

    }
}