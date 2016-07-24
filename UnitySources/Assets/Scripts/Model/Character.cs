using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Utilities;
using FluentBehaviourTree;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private const float TimeBetweenJobSearches = 3f;
        private const float BaseMovementSpeed = 5f;

        private const float EnergyChargeRate = (1f/300);
            // The internal magic fusion core mcguffin charges the character at this rate.

        private const float ShieldEnergyUsageRate = (2f/300);
            // The shield uses power at this rate - should be larger than EnergyChargeRate, otherwise it'll never run out.

        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        public Inventory Inventory;

        private Tile _destinationTile;

        private Tile DestinationTile
        {
            get { return _destinationTile; }
            set
            {
                if (_destinationTile != value)
                {
                    _destinationTile = value;
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

        private IBehaviourTreeNode _tree;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        public Character()
        {
            this._speed = BaseMovementSpeed;
            this.Name = MarkovNameGenerator.GetNextName("male") + ' ' + MarkovNameGenerator.GetNextName("last");
            this.CurrentState = State.Idle;
            _conditions = new Dictionary<string, float> {{"energy", 1f}, {"health", 1f}};
            _timeSinceLastJobSearch = TimeBetweenJobSearches;

            // This Behaviour Tree specifies the process for getting a job, fetching any materials it needs, and then executing that job.
            var jobBehaviour = new BehaviourTreeBuilder()
                .Sequence("work")
                
                    .Selector("get-job")
                        .Condition("have-a-job", t => DoesCharacterHaveAJob_Condition())
                        .Do("get-next-job", t => GetNextJob_Action(t.deltaTime))
                    .End()

                    .Selector("get-materials")
                        .Condition("job-needs-materials", t => JobHasAllNeedMaterials_Condition())
                        .Condition("am-carrying-materials", t => IsCarryingMaterials_Condition())
                        .Inverter().Do("find-material", t => FindRequiredStock_Action()).End()
                        .Inverter().Do("move-to-material", t => MoveTowardsDestination_Action(t.deltaTime)).End()
                        .Do("pickup-material", t => PickUpStock_Action())
                    .End()

                    .Sequence("work-job")
                        .Do("movesetup-move-to-jobsite", t => SetupMoveToJobSite_Action())
                        .Do("move-to-jobsite", t => MoveTowardsDestination_Action(t.deltaTime))
                        .Do("drop_stock", t => TransferStockToJob_Action())
                        .Do("do-work", t => DoWork_Action(t.deltaTime))
                    .End()

                .End()
                .Build();

            // Combine all the BTs.
            _tree = new BehaviourTreeBuilder()
                .Sequence("worker")
                    .Splice(jobBehaviour)
                .End()
                .Build();
        }

        public Character(Tile tile) : this()
        {
            CurrentTile = DestinationTile = _nextTile = tile;
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
            _nextTile = DestinationTile = CurrentTile;
            World.Instance.JobQueue.Enqueue(CurrentJob);
            CurrentJob = null;
            CurrentState = State.Idle;
        }

        private void AbandonMove()
        {
            DestinationTile = CurrentTile;
        }

        private bool DoesCharacterHaveAJob_Condition()
        {
            return CurrentJob != null;
        }

        private bool JobHasAllNeedMaterials_Condition()
        {
            return CurrentJob.HasAllMaterial();
        }

        private bool IsCarryingMaterials_Condition()
        {
            if (Inventory != null)
            {
                if (CurrentJob.NeedsMaterial(Inventory) > 0)
                {
                    // We are carrying at least some of what the current job needs, so take it to the job site.
                    return true;
                }
            }
            return false;
        }

        private BehaviourTreeStatus GetNextJob_Action(float deltaTime)
        {
            //Debug.Log("GetNextJob_Action");

            _timeSinceLastJobSearch += deltaTime;
            if (_timeSinceLastJobSearch < TimeBetweenJobSearches)
            {
                return BehaviourTreeStatus.Failure;
            }

            if (World.Instance.JobQueue.Count() == 0)
            {
                return BehaviourTreeStatus.Failure;
            }

            CurrentJob = World.Instance.JobQueue.TakeFirstJobFromQueue();

            if (CurrentJob == null)
            {
                return BehaviourTreeStatus.Failure;
            }

            // Debug.LogFormat("GetNextJob_Action got new job {0} at [{1},{2}]", CurrentJob.Name, CurrentJob.Tile.X, CurrentJob.Tile.Y);
            CurrentJob.RegisterOnJobStoppedCallback(OnJobStopped);
            return BehaviourTreeStatus.Success;
        }

        private BehaviourTreeStatus FindRequiredStock_Action()
        {
            //Debug.Log("FindRequiredStock_Action");

            // If the current job has all the materials it needs already, we can just skip this step.
            if (CurrentJob.HasAllMaterial())
            {
                AbandonMove();
                return BehaviourTreeStatus.Success;
            }

            // The job needs some more materials, perhaps we're holding them already so don't need to go anywhere.
            if (Inventory != null && CurrentJob.NeedsMaterial(Inventory) > 0)
            {
                // We are carrying at least some of what the current job needs, so continue on our merry way.
                AbandonMove();
                return BehaviourTreeStatus.Success;
            }

            // The job needs some more materials, and we're not carrying it already, so we need to move to where there are some.

            // Perhaps the stock is right where we're stood?
            if (CurrentTile.Inventory != null &&
               (CurrentJob.CanTakeFromStockpile || CurrentTile.Furniture == null || CurrentTile.Furniture.IsStockpile() == false) &&
               (CurrentJob.NeedsMaterial(CurrentTile.Inventory) != 0))
            {
                // The materials we need are right where we're stood!
                AbandonMove();
                return BehaviourTreeStatus.Success;
            }

            // The Job needs some of this:
            var unsatisfied = CurrentJob.GetFirstRequiredInventory();

            // We might have a path to the item we need already
            var endTile = _path == null ? null : _path.EndTile();
            if (_path != null && endTile != null && endTile.Inventory != null && endTile.Inventory.ObjectType == unsatisfied.ObjectType)
            {
                // We are already moving towards a tile with the items we want, just keep going.
                return BehaviourTreeStatus.Success;
            }

            // Look for the first item that matches.
            _path = World.Instance.InventoryManager.GetClosestPathToInventoryOfType(
                objectType: unsatisfied.ObjectType,
                t: CurrentTile,
                desiredQty: unsatisfied.MaxStackSize - unsatisfied.StackSize,
                searchInStockpiles: CurrentJob.CanTakeFromStockpile);


            // If there are no items anywhere that satisfy the requirements, we have to give up on this job.
            if (_path == null || _path.Length() == 0)
            {
                //// Debug.LogFormat("No Tile found containing the desired type ({0}).", unsatisfied.ObjectType);
                AbandonJob();
                return BehaviourTreeStatus.Failure;
            }

            // We've identified where the missing items can be found, so head there.
            _destinationTile = _path.EndTile();
            _nextTile = _path.Dequeue();
            return BehaviourTreeStatus.Success;
        }

        public BehaviourTreeStatus SetupMoveToStockSite_Action()
        {
            Debug.Log("SetupMoveToStockSite_Action");

            // If we've already arrived at our destination, just continue.
            if (CurrentTile == _destinationTile)
            {
                return BehaviourTreeStatus.Success;
            }

            // If the Job doesn't need anything, we're done too.
            if (CurrentJob.HasAllMaterial())
            {
                AbandonMove();
                return BehaviourTreeStatus.Success;
            }

            // Keep walking towards the destination.
            if (_nextTile == null || _nextTile == CurrentTile)
            {
                // Get the next Tile from the pathfinder.
                if (_path == null || _path.Length() == 0)
                {
                    // Generate a path to our destination
                    _path = new Path_AStar(World.Instance, CurrentTile, DestinationTile);

                    if (_path.Length() == 0)
                    {
                        //Debug.LogError("Path_AStar returned no path to destination!");
                        AbandonJob();
                        _path = null;
                        return BehaviourTreeStatus.Failure;
                    }
                }

                // Grab the next waypoint from the pathing system!
                _nextTile = _path.Dequeue();
            }

            if (_nextTile == null) _nextTile = CurrentTile;

            // At this point we should have a valid _nextTile to move to.
            return BehaviourTreeStatus.Success;
        }

        public BehaviourTreeStatus MoveTowardsDestination_Action(float deltaTime)
        {
            // Debug.Log("MoveTowardsDestination_Action");

            // If we've already arrived at our destination, just continue.
            if (CurrentTile == _destinationTile)
            {
                // Debug.Log("MoveTowardsDestination_Action: Destination Reached");
                return BehaviourTreeStatus.Success;
            }

            // If we don't have a next tile yet, get one.
            if (_nextTile == null || _nextTile == CurrentTile)
            {
                // If we don't have a route to the current destination, plan one.
                if (_path == null || _path.Length() == 0 || _path.EndTile() != DestinationTile)
                {
                    // Debug.LogFormat("MoveTowardsDestination_Action: calculating new path from [{0},{1}] to [{2},{3}]", CurrentTile.X, CurrentTile.Y, DestinationTile.X, DestinationTile.Y);
                    _path = new Path_AStar(World.Instance, CurrentTile, DestinationTile);
                }

                // If Path is still null, we were not able to find a route to our goal
                if (_path == null)
                {
                    AbandonJob();
                    // Debug.LogFormat("MoveTowardsDestination_Action: Could not find a route to the next tile!");
                    return BehaviourTreeStatus.Failure;
                }

                _nextTile = _path.Dequeue();
                // Debug.LogFormat("MoveTowardsDestination_Action: moving to next adjacent tile from [{0},{1}] to [{2},{3}]", CurrentTile.X, CurrentTile.Y, _nextTile.X, _nextTile.Y);
            }

            if (_nextTile == null)
            {
                _nextTile = CurrentTile; // TODO: Not sure when this might apply
            }

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
                _nextTile = null;
                _path = null;
                // Debug.LogFormat("MoveTowardsDestination_Action: failed trying to move into a blocked tile.");
                return BehaviourTreeStatus.Failure;
            }

            if (_nextTile.IsEnterable() == Enterability.Soon)
            {
                // The next Tile we're trying to enter is walkable, but maybe for some reason
                // cannot be entered right now. Perhaps it is occupied, or contains a closed door.
                CurrentState = State.WaitingForAccess;
                return BehaviourTreeStatus.Running;
            }

            // How much distance can be travel this Update?
            if (_nextTile == null) _nextTile = CurrentTile;
            var distThisFrame = 0f;
            try
            {
                distThisFrame = (_speed / _nextTile.MovementCost) * deltaTime;
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
                percThisFrame = distThisFrame / distToTravel;
            }

            // Add that to overall percentage travelled.
            _movementPercentage += percThisFrame;

            if (_movementPercentage >= 1)
            {
                // We have reached our (current) destination
                CurrentTile = _nextTile;
                _movementPercentage = 0;
            }

            // Debug.LogFormat("MoveTowardsDestination_Action: Character at [{0:F2},{1:F2}] {2:P2}", this.X, this.Y, _movementPercentage);
            
            return BehaviourTreeStatus.Running;
        }

        public BehaviourTreeStatus PickUpStock_Action()
        {
            //Debug.Log("PickUpStock_Action");

            // Quickly check to see if the job still needs the stuff we're stood on, in case someone else has already taken it there.
            if (CurrentJob.HasAllMaterial())
            {
                return BehaviourTreeStatus.Success;
            }

            // We should be standing on the stuff we were looking for, but check again just to be sure.
            // TODO: someone else migh have nicked this stuff already. Should we "reserve" distant stock as soon as we decide we're going to go and get it?
            if (CurrentTile.Inventory != null &&
                (CurrentJob.CanTakeFromStockpile || CurrentTile.Furniture == null || CurrentTile.Furniture.IsStockpile() == false) &&
                (CurrentJob.NeedsMaterial(CurrentTile.Inventory) != 0))
            {
                // The materials we need are right where we're stood!
                World.Instance.InventoryManager.TransferInventory(
                    character: this,
                    source: CurrentTile.Inventory,
                    qty: CurrentJob.NeedsMaterial(CurrentTile.Inventory));

                // We've picked up what we need, so wait for further orders.
                return BehaviourTreeStatus.Success;
            }

            return BehaviourTreeStatus.Failure;
        }

        public BehaviourTreeStatus SetupMoveToJobSite_Action()
        {
            //Debug.Log("SetupMoveToJobSite_Action");

            if (DestinationTile == CurrentJob.Tile)
            {
                // Already moving towards job, so just carry on.
                return BehaviourTreeStatus.Success;
            }

            // Make sure we're heading for the site.
            DestinationTile = CurrentJob.Tile;
            // Debug.LogFormat("SetupMoveToJobSite_Action moving to [{0},{1}] ", DestinationTile.X, DestinationTile.Y);

            // See if we're already stood on the tile.
            if (CurrentTile == DestinationTile)
            {
                return BehaviourTreeStatus.Success;
            }

            // Make sure we have a route to the job.
            if (_path == null || _path.EndTile() != DestinationTile)
            {
                _path = new Path_AStar(World.Instance, CurrentTile, DestinationTile);
            }

            // If we still don't have a path, there is no path.
            if (_path.IsUnReachable)
            {
                AbandonJob();
                return BehaviourTreeStatus.Failure;
            }

            // See if we're "close enough" to the job.
            if (_path.Length() <= CurrentJob.MinRange)
            {
                // Set dest to current, just in case it was the proximity-check that got us here
                DestinationTile = CurrentTile;
                return BehaviourTreeStatus.Success;
            }

            // At this point we should have a valid _nextTile to move to.
            return BehaviourTreeStatus.Success;
        }

        public BehaviourTreeStatus TransferStockToJob_Action()
        {
            // Debug.Log("TransferStockToJob_Action");

            // If we're not carrying anything, nothing to do.
            if (this.Inventory == null || this.Inventory.StackSize == 0)
            {
                return BehaviourTreeStatus.Success;
            }

            // Only drop what we're carrying if it is what the job wants.
            if (CurrentJob.NeedsMaterial(Inventory) == 0)
            {
                return BehaviourTreeStatus.Success;
            }

            Debug.Log("TransferStockToJob_Action is transfering stock to job.");

            // We are at the jobsite, so drop the Inventory.
            World.Instance.InventoryManager.TransferInventory(CurrentJob, Inventory);
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
            return BehaviourTreeStatus.Success;
        }

        public BehaviourTreeStatus DoWork_Action(float deltaTime)
        {
            // Debug.Log("DoWork_Action");

            // If there isn't a current job, that probably means while this step was running, the job was completed.
            if (CurrentJob == null)
            {
                return BehaviourTreeStatus.Success;
            }

            // TODO: Not sure if we need to check this - that should've been handled by the preceeding actions. Can jobs move by themselves?
            var rangeToJob = 0;

            if (CurrentTile == CurrentJob.Tile)
            {
                // We're stood on the Job site.
                rangeToJob = 0;
            }

            if (_path != null)
            {
                // We're stood close enough to the job site.
                rangeToJob = _path.Length();
            }

            // If we're too far from the job site, something went wrong.
            if (rangeToJob > CurrentJob.MinRange)
            {
                return BehaviourTreeStatus.Failure;
            }

            // Debug.LogFormat("DoWork_Action working job \"{3}\" at [{0},{1}] ({2:F2})", CurrentTile.X, CurrentTile.Y, CurrentJob.JobTime, CurrentJob.Name);

            CurrentState = State.WorkingJob;

            // Set dest to current, just in case it was the proximity-check that got us here.
            DestinationTile = CurrentTile;
            _path = null;

            // Do some work.
            CurrentJob.DoWork(deltaTime);
            return BehaviourTreeStatus.Running;
        }

        public void Update(float deltaTime)
        {
            _tree.Tick(new TimeData(deltaTime));

            //Update_DoJob(deltaTime);
            //Update_DoMovement(deltaTime);
            //Update_Idle(deltaTime);

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
                DestinationTile = CurrentJob.Furniture.GetJobSpotTile();
            }
            else
            {
                DestinationTile = CurrentJob.Tile;
            }

            CurrentJob.RegisterOnJobStoppedCallback(OnJobStopped);

            // Check to see if the job is reachable from the Character's current position.
            // We mmight have to go somewhere else first to get materials.

            // If we are already at the worksite, just return, otherwise need to calculate a route there.
            if (DestinationTile == CurrentTile)
            {
                return;
            }

            _path = new Path_AStar(World.Instance, CurrentTile, DestinationTile);
            if (_path.Length() == 0)
            {
                Debug.LogErrorFormat("Path_AStar returned no path from [{0},{1}] to target job Tile [{2},{3}]!", CurrentTile.X, CurrentTile.Y, DestinationTile.X, DestinationTile.Y);
                AbandonJob();
                _path = null;
                DestinationTile = CurrentTile;
            }
        }

        private void Update_Idle(float deltaTime)
        {
            if (CurrentJob != null)
            {
                return;
            }

            if (CurrentTile != _destinationTile)
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
                    DestinationTile = CurrentTile;
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
                        DestinationTile = CurrentJob.Tile;

                        if (CurrentTile == DestinationTile || (_path != null && _path.Length() <= CurrentJob.MinRange))
                        {
                            // Set dest to current, just in case it was the neighbour-check that got us here
                            DestinationTile = CurrentTile;

                            // We are at the jobsite, so drop the Inventory.
                            World.Instance.InventoryManager.TransferInventory(CurrentJob, Inventory);
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
                            DestinationTile = CurrentJob.Tile;
                            return;
                        }
                    }
                    else
                    {
                        // We are carrying something, but the job doesn't want it.
                        // Dump it where we are.
                        // TODO: actually dump it to an empty Tile, as we might be stood on a job Tile.
                        if (World.Instance.InventoryManager.TransferInventory(CurrentTile, Inventory) == false)
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
                        World.Instance.InventoryManager.TransferInventory(
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
                                //// Debug.LogFormat("No Tile found containing the desired type ({0}).", unsatisfied.ObjectType);
                                AbandonJob();
                                return;
                            }

                            this.CurrentState = State.FetchingStock;
                            _destinationTile = this._path.EndTile();
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
            DestinationTile = CurrentJob.Tile;

            // Are we there yet?
            if ((CurrentTile == CurrentJob.Tile)
                || (_path != null && _path.Length() <= CurrentJob.MinRange))
            {
                // Debug.LogFormat("Standing at [{0},{1}] Working job \"{2}\" at [{3},{4}]", CurrentTile.X, CurrentTile.Y, CurrentJob.Description, CurrentJob.Tile.X, CurrentJob.Tile.Y);
                CurrentState = State.WorkingJob;

                // Set dest to current, just in case it was the neighbour-check that got us here
                DestinationTile = CurrentTile;
                _path = null;

                CurrentJob.DoWork(deltaTime);
            }

            // Done.
        }

        private void OnJobStopped(Job j)
        {
            // Debug.LogFormat("Standing at [{0},{1}] Finished job \"{2}\" at [{3},{4}]", CurrentTile.X, CurrentTile.Y, CurrentJob.Description, CurrentJob.Tile.X, CurrentJob.Tile.Y);

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