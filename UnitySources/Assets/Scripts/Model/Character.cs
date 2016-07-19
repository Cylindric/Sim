using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    [DebuggerDisplay("Character {Name} at [{X}, {Y}]")]
    public partial class Character
    {
        public enum State
        {
            Idle,
            PreparingForJob,
            FetchingStock,
            WorkingJob,
            MovingToJobsite,
            WaitingForAccess
        }

        private const float TimeBetweenJobSearches = 1f;
        private const float BaseMovementSpeed = 5f;

        private const float EnergyChargeRate = (1f/300);
            // The internal magic fusion core mcguffin charges the character at this rate.

        private const float ShieldEnergyUsageRate = (2f/300);
            // The shield uses power at this rate - should be larger than EnergyChargeRate, otherwise it'll never run out.

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

        private Tile _nextTile;
        private Path_AStar _path;
        private readonly float _speed;
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
            this.CurrentState = State.Idle;
            _conditions = new Dictionary<string, float> {{"energy", 1f}, {"health", 1f}};
        }

        public Character(Tile tile) : this()
        {
            CurrentTile = DestTile = _nextTile = tile;
        }

        /* #################################################################### */
        /* #                         DELEGATES                                # */
        /* #################################################################### */

        private Action<Character> _cbCharacterChanged;

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

        public Job CurrentJob { get; private set; }

        public bool ShieldStatus { get; private set; }

        public State CurrentState { get; private set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        /// <summary>
        /// Abandon the current job, and put it back on the queue.
        /// </summary>
        public void AbandonJob()
        {
            _nextTile = DestTile = CurrentTile;
            World.Instance.JobQueue.Enqueue(CurrentJob);
            CurrentJob = null;
            CurrentState = State.Idle;
        }

        public void Update(float deltaTime)
        {
            Update_DoJob(deltaTime);
            Update_DoMovement(deltaTime);
            Update_Idle(deltaTime);

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

            if (_cbCharacterChanged != null) _cbCharacterChanged(this);
        }

        private void GetNewJob()
        {
            // Make sure we're starting off in an Idle state
            CurrentState = State.Idle;

            // Grab a new job.
            // _job = World.Instance.JobQueue.TakeClosestJobToTile(CurrentTile);
            CurrentJob = World.Instance.JobQueue.TakeFirstJobFromQueue();
            _timeSinceLastJobSearch = 0;

            if (CurrentJob == null) return;
            CurrentState = State.PreparingForJob;

            if (CurrentJob.Furniture != null)
            {
                DestTile = CurrentJob.Furniture.GetJobSpotTile();
            }
            else
            {
                DestTile = CurrentJob.Tile;
            }

            CurrentJob.RegisterOnJobStoppedCallback(OnJobStopped);

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

        private void Update_Idle(float deltaTime)
        {
            if (CurrentJob != null)
            {
                return;
            }

            if (CurrentTile != _destTile)
            {
                return;
            }

            // If we're standing in a dangerous place and not actually working in it, try and move somewhere safer.

            if (CurrentTile.Room == null || CurrentTile.Room.IsOutsideRoom() || CurrentTile.Room.Atmosphere.IsBreathable() == false)
            {
                // No room at all probably means we're stood in a door or some other furniture.
                // Outside room means we're out in space, so try and find somewhere safe.
                var targetRoom = FindNearestSafeRoom();

                // Create a new job to flee to safety.
                CurrentJob = new Job(targetRoom, null, OnJobStopped, 0f, null, false);
                CurrentJob.Description = "Seeking safety";
            }
        }

        private void Update_DoJob(float deltaTime)
        {
            _timeSinceLastJobSearch += deltaTime;

            // If I'm not doing anything, look for work to do.
            if (CurrentState == State.Idle)
            {
                // Get a new job, but wait for just a bit so we don't immediately pick up a new job.
                if (_timeSinceLastJobSearch >= TimeBetweenJobSearches)
                {
                    GetNewJob();
                }

                if (CurrentState == State.Idle)
                {
                    // There are no jobs queued, so can just finish.
                    DestTile = CurrentTile;
                    return;
                }
            }

            if (CurrentState == State.Idle)
            {
                // If we're still idle, there's nothing more to work out for jobs.
                // Basically means there either are no more jobs, or we're waiting a bit before getting one.
                return;
            }

            // We have a job.

            if (CurrentJob.HasAllMaterial() == false)
            {
                // We are missing resources required for the job
                
                // Are we carrying what we need?
                if (Inventory != null)
                {
                    if (CurrentJob.NeedsMaterial(Inventory) > 0)
                    {
                        // We are carrying at least some of what the current job needs, so take it to the job site.
                        CurrentState = State.MovingToJobsite;
                        DestTile = CurrentJob.Tile;

                        if (CurrentTile == DestTile || (_path != null && _path.Length() <= CurrentJob.MinRange))
                        {
                            // Set dest to current, just in case it was the neighbour-check that got us here
                            DestTile = CurrentTile;

                            // We are at the jobsite, so drop the Inventory.
                            World.Instance.InventoryManager.PlaceInventory(CurrentJob, Inventory);
                            CurrentJob.DoWork(0); // This will call all the cbJobWorked callbacks

                            if (Inventory.StackSize == 0)
                            {
                                Inventory = null;
                            }
                            else
                            {
                                Debug.LogError("Character is still carrying Inventory, which shouldn't be the case.");
                                Inventory = null;
                            }

                            // Once the stock is dropped, we're available for work
                            CurrentState = State.Idle;

                        }
                        else
                        {
                            // Still walking to the site.
                            CurrentState = State.MovingToJobsite;
                            DestTile = CurrentJob.Tile;
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

                        // Once the stock is dropped, we're available for work
                        CurrentState = State.Idle;
                    }
                }
                else
                {
                    // At this point, the job still requires Inventory, but we don't have it.
                    // That means we need to walk towards a Tile that does have the required items.
                    CurrentState = State.FetchingStock;

                    if (CurrentTile.Inventory != null && 
                        (CurrentJob.CanTakeFromStockpile || CurrentTile.Furniture == null || CurrentTile.Furniture.IsStockpile() == false) &&
                        CurrentJob.NeedsMaterial(CurrentTile.Inventory) != 0)
                    {
                        // The materials we need are right where we're stood!
                        World.Instance.InventoryManager.PlaceInventory(
                            character: this, 
                            source: CurrentTile.Inventory, 
                            qty: CurrentJob.NeedsMaterial(CurrentTile.Inventory));

                        // We've picked up what we need, so wait for further orders.
                        this.CurrentState = State.Idle;
                    }
                    else
                    {
                        // The Job needs some of this:
                        var unsatisfied = CurrentJob.GetFirstRequiredInventory();

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
                                searchInStockpiles: CurrentJob.CanTakeFromStockpile);

                            if (this._path == null || this._path.Length() == 0)
                            {
                                //Debug.LogFormat("No Tile found containing the desired type ({0}).", unsatisfied.ObjectType);
                                AbandonJob();
                                return;
                            }

                            this.CurrentState = State.FetchingStock;
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
            DestTile = CurrentJob.Tile;

            // Are we there yet?
            if ((CurrentTile == CurrentJob.Tile)
                || (_path != null && _path.Length() <= CurrentJob.MinRange))
            {
                Debug.LogFormat("Standing at [{0},{1}] Working job \"{2}\" at [{3},{4}]", CurrentTile.X, CurrentTile.Y, CurrentJob.Description, CurrentJob.Tile.X, CurrentJob.Tile.Y);
                CurrentState = State.WorkingJob;

                // Set dest to current, just in case it was the neighbour-check that got us here
                DestTile = CurrentTile;
                _path = null;

                CurrentJob.DoWork(deltaTime);
            }

            // Done.
        }

        private void Update_DoMovement(float deltaTime)
        {
            if (this.CurrentState != State.FetchingStock && _path != null)
            {
                // Debug.LogFormat("Path length {0}.", _path.Length());
                if (_path.Length() <= CurrentJob.MinRange)
                {
                    Debug.Log("Close enough");
                    DestTile = CurrentTile;
                }
            }

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
                CurrentState = State.Idle;
            }
            else if(_nextTile.IsEnterable() == Enterability.Soon)
            {
                // The next Tile we're trying to enter is walkable, but maybe for some reason
                // cannot be entered right now. Perhaps it is occupied, or contains a closed door.
                CurrentState = State.WaitingForAccess;
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
                // We have reached our (current) destination
                CurrentTile = _nextTile;
                _movementPercentage = 0;
            }
        }

        private void OnJobStopped(Job j)
        {
            Debug.LogFormat("Standing at [{0},{1}] Finished job \"{2}\" at [{3},{4}]", CurrentTile.X, CurrentTile.Y, CurrentJob.Description, CurrentJob.Tile.X, CurrentJob.Tile.Y);

            // Job completed or was cancelled.
            CurrentState = State.Idle;

            j.UnregisterOnJobStoppedCallback(OnJobStopped);

            if (j != CurrentJob)
            {
                // Debug.LogError("Character being told about job (" + j.Name + ") that isn't his. You forgot to unregister something.");
                return;
            }

            CurrentJob = null;
        }

    }
}