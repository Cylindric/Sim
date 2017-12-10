using System;
using System.Collections.Generic;
using Engine.Pathfinding;
using Engine.Utilities;
using FluentBehaviourTree;
using System.Linq;
using System.Diagnostics;
using Debug = Engine.Utilities.Debug;

namespace Engine.Models
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
            WaitingForAccess,
            CoolingDown
        }

        private const float TimeBetweenJobSearches = 3f;
        private const float BaseMovementSpeed = 5f;

        // The public magic fusion core mcguffin charges the character at this rate.
        private const float EnergyChargeRate = (1f/300);

        // The shield uses power at this rate - should be larger than EnergyChargeRate, otherwise it'll never run out.
        private const float ShieldEnergyUsageRate = (2f/300);

        // Below this value, jobs will be abandoned in favour of seeking breathable air.
        private const float TankDangerLevel = 0.2f;

        // The air tank will be charged to this level before moving off and doing something else.
        private const float TankMinLevel = 0.5f;

        private const int TankTopupRate = 10;

        public bool DebugAi = false;
        
        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        public Inventory Inventory;

        private Tile _destinationTile;

        public Tile DestinationTile
        {
            get { return _destinationTile; }
            set
            {
                if (_destinationTile != value)
                {
                    // Debug.LogFormat("{0} changed destination from {1} to {2}", Name, _destinationTile, value);
                    _destinationTile = value;
                    _path = null;
                }
            }
        }

        public Path_AStar Path
        {
            get { return _path; }
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

            _conditions = new Dictionary<string, float> {{"energy", 1f}, {"health", 1f}, {"suit_air", 1f} };
            _timeSinceLastJobSearch = TimeBetweenJobSearches;

            // This Behaviour Tree handles the environmental state of this character.
            var environmentBehaviour = new BehaviourTreeBuilder()
                .Selector("breathe_or_flee")
                    .Sequence("breathe")
                        .Condition("breathable", t => CanBreathe_Condition())
                        .Do("do_breathing", t => Breathe_Action(t.deltaTime))
                        .Do("replenish_suit", t => ReplenishSuit_Action(t.deltaTime))
                    .End()
                    .Do("breathe_suit", t => BreatheSuit_Action(t.deltaTime))
                    .Sequence("flee")
                        .Do("find_safety", t => FindSafety_Action())
                        .Do("flee", t => MoveTowardsDestination_Action(t.deltaTime))
                    .End()
                .End()
                .Build();

            // This Behaviour Tree specifies the process for getting a job, fetching any materials it needs, and then executing that job.
            var jobBehaviour = new BehaviourTreeBuilder()
                .Succeeder()
                    .Sequence("work")

                        // Fails if there are no jobs.
                        .Selector("get_job")
                            .Condition("have_a_job", t => DoesCharacterHaveAJob_Condition())
                            .Do("get_next_job", t => GetNextJob_Action(t.deltaTime)) // Fails if there are no jobs.
                        .End()

                        // Fails if no materials available
                        .Selector("get_materials")
                            .Condition("job_has_materials", t => JobHasAllNeedMaterials_Condition())
                            .Condition("am_carrying_materials", t => IsCarryingMaterials_Condition())
                            .Inverter().Do("find_material", t => FindRequiredStock_Action()).End()
                            .Inverter().Do("move_to_material", t => MoveTowardsDestination_Action(t.deltaTime)).End()
                            .Do("pickup_material", t => PickUpStock_Action())
                        .End()

                        // Succeeds when job complete
                        .Sequence("work_job")
                            .Do("movesetup_move_to_jobsite", t => SetupMoveToJobSite_Action())
                            .Do("move_to_jobsite", t => MoveTowardsDestination_Action(t.deltaTime))
                            .Do("drop_stock", t => TransferStockToJob_Action())
                            .Do("do_work", t => DoWork_Action(t.deltaTime))
                        .End()
                    .End()
                .End()
                .Build();

            var idleBehaviour = new BehaviourTreeBuilder()
                .Selector("ensure_in_room")
                    .Condition("is_in_room", t => AmIndoors_Condition())
                    .Inverter("").Do("find_nearrest_room", t => FindNearestRoom_Action(t.deltaTime)).End()
                    .Do("move_to_room", t => MoveTowardsDestination_Action(t.deltaTime))
                .End()
                .Build();

            // Combine all the BTs.
            _tree = new BehaviourTreeBuilder()
                .Sequence("worker")
                    .Do("drain_suit", t => DrainSuit_Action(t.deltaTime))
                    .Splice(environmentBehaviour)
                    .Splice(jobBehaviour)
                    .Splice(idleBehaviour) 
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
            if (CurrentJob != null)
            {
                if (CurrentJob.Furniture != null)
                {
                    CurrentJob.Furniture.WorkingCharacter = null;
                }
                World.Instance.JobQueue.Enqueue(CurrentJob);
                CurrentJob = null;
            }
            CurrentState = State.Idle;
        }

        private void AbandonMove()
        {
            DestinationTile = CurrentTile;
        }

        private bool CanBreathe_Condition()
        {
            if (CurrentTile == null) return false;
            if (CurrentTile.Room == null) return false;
            return CurrentTile.Room.Atmosphere.IsBreathable();
        }

        private bool AmIndoors_Condition()
        {
            // If we're nowhere, we are not in a room
            if (CurrentTile == null || CurrentTile.Room == null)
            {
                if (DebugAi) Debug.Log(this.Name + ": checking if I am in a room; not in any room");
                return false;
            }

            // Is the room the default room?
            if (CurrentTile.Room.IsOutsideRoom())
            {
                if (DebugAi) Debug.Log(this.Name + ": checking if I am in a room; no, is an outside area");
                return false;
            }

            if (DebugAi) Debug.Log(this.Name + ": checking if I am in a room; yes");
            return true;
        }

        private bool DoesCharacterHaveAJob_Condition()
        {
            if (DebugAi) Debug.Log(this.Name + ": checking if I have a job; " + (CurrentJob == null ? "nope" : "yes"));
            return CurrentJob != null;
        }

        private bool JobHasAllNeedMaterials_Condition()
        {
            if (CurrentJob.HasAllMaterial())
            {
                if (DebugAi) Debug.Log(this.Name + ": job has all materials");
                return true;
            }

            if (DebugAi) Debug.Log(this.Name + ": job need materials");
            return false;
        }

        private bool IsCarryingMaterials_Condition()
        {
            if (Inventory != null)
            {
                if (CurrentJob.NeedsMaterial(Inventory) > 0)
                {
                    // We are carrying at least some of what the current job needs, so take it to the job site.
                    if (DebugAi) Debug.Log(this.Name + ": I have the materials the job needs");
                    return true;
                }
            }
            if (DebugAi) Debug.Log(this.Name + ": I need materials for this job");
            return false;
        }

        private BehaviourTreeStatus FindNearestRoom_Action(float deltaTime)
        {
            if (DebugAi) Debug.Log(this.Name + " Looking for nearest room");

            _timeSinceLastJobSearch += deltaTime;
            if (_timeSinceLastJobSearch < TimeBetweenJobSearches)
            {
                if (DebugAi) Debug.Log(this.Name + " Searched too soon, failing");
                return BehaviourTreeStatus.Failure;
            }

            // Are we already in a valid room?
            if (CurrentTile != null && CurrentTile.Room != null)
            {
                if (DebugAi) Debug.Log(this.Name + " Already in a room, success!");
                return BehaviourTreeStatus.Success;
            }

            // Is the destination tile already a valid room?
            if (DestinationTile != null && DestinationTile.Room != null)
            {
                if (DebugAi) Debug.Log(this.Name + " Already going to a room, success!");
                return BehaviourTreeStatus.Success;
            }

            // Look for a safe room nearby.
            var targetRoomTile = FindNearestRoom();
            if (targetRoomTile == null)
            {
                if (DebugAi) Debug.Log(this.Name + " Could not find a room. Failure!");
                return BehaviourTreeStatus.Failure;
            }

            DestinationTile = targetRoomTile;

            // Find room
            if (DebugAi) Debug.Log(this.Name + " found a new room.");
            return BehaviourTreeStatus.Success;
        }

        private BehaviourTreeStatus GetNextJob_Action(float deltaTime)
        {
            //Debug.Log("GetNextJob_Action");

            _timeSinceLastJobSearch += deltaTime;
            if (_timeSinceLastJobSearch < TimeBetweenJobSearches)
            {
                if (DebugAi) Debug.Log(Name + ": GetNextJob returning failure");
                return BehaviourTreeStatus.Failure;
            }

            // If there are no jobs, just fail.
            if (World.Instance.JobQueue.Count() == 0)
            {
                if (DebugAi) Debug.Log(Name + ": GetNextJob returning failure");
                return BehaviourTreeStatus.Failure;
            }

            CurrentJob = World.Instance.JobQueue.TakeFirstJobFromQueue();

            // Try to get a job to do, if that fais, return a failure
            if (CurrentJob == null)
            {
                if (DebugAi) Debug.Log(Name + ": GetNextJob returning failure");
                return BehaviourTreeStatus.Failure;
            }

            if (DebugAi) Debug.LogFormat("{0} got new job {1} at [{2},{3}]", Name, CurrentJob.Name, CurrentJob.Tile.X, CurrentJob.Tile.Y);
            if (CurrentJob.Furniture != null)
            {
                CurrentJob.Furniture.WorkingCharacter = this;
            }
            CurrentJob.RegisterOnJobStoppedCallback(OnJobStopped);
            if (DebugAi) Debug.Log(Name + ": GetNextJob returning success");
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

        private BehaviourTreeStatus MoveTowardsDestination_Action(float deltaTime)
        {
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
                    _path = new Path_AStar()
                    {
                        World = World.Instance,
                        Start = CurrentTile,
                        End = DestinationTile
                    };
                    _path.Calculate();
                }

                // If Path is still null, we were not able to find a route to our goal
                if (_path == null)
                {
                    AbandonJob();
                    Debug.LogFormat("MoveTowardsDestination_Action: Could not find a route to the next tile!");
                    return BehaviourTreeStatus.Failure;
                }

                // See if we're "close enough" to the job.
                // _path.Debug();
                if (CurrentJob == null || (_path.Length()) <= CurrentJob.MinRange)
                {
                    // Debug.LogFormat("{0}: Close enough to job (is {1} needs to be <= {2})", Name, (_path.Length()), CurrentJob.MinRange);
                    // Set dest to current, just in case it was the proximity-check that got us here
                    DestinationTile = CurrentTile;
                    return BehaviourTreeStatus.Success;
                }
                else
                {
                    // Debug.LogFormat("{0}: Distance to job is {1}, needs to be <={2}", Name, (_path.Length()), CurrentJob.MinRange);
                }

                _nextTile = _path.Dequeue();
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
                distToTravel = (float)Math.Sqrt(
                    Math.Pow(CurrentTile.X - _nextTile.X, 2) +
                    Math.Pow(CurrentTile.Y - _nextTile.Y, 2)
                    );
            }

            // Before entering a Tile, make sure it is not impassable.
            // This might happen if the Tile is changed (e.g. wall built) after the pathfinder runs.
            if (_nextTile.IsEnterable() == Enterability.Never)
            {
                _nextTile = null;
                _path = null;
                Debug.LogFormat("{0}: MoveTowardsDestination_Action failed trying to move into a blocked tile.", Name);
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

        private BehaviourTreeStatus PickUpStock_Action()
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

        private BehaviourTreeStatus SetupMoveToJobSite_Action()
        {
            if (CurrentJob == null)
            {
                if (DebugAi) Debug.LogFormat("{0}: SetupMoveToJobSite_Action returning failure due to no job", Name);
                AbandonJob();
                AbandonMove();
                return BehaviourTreeStatus.Failure;
            }

            if (DestinationTile == CurrentJob.Tile)
            {
                // Already moving towards job, so just carry on.
                return BehaviourTreeStatus.Success;
            }

            // Make sure we're heading for the site.
            DestinationTile = CurrentJob.Tile;

            // See if we're already stood on the tile.
            if (CurrentTile == DestinationTile)
            {
                Debug.LogFormat("{0}: SetupMoveToJobSite_Action returning success due to being at the job", Name);
                return BehaviourTreeStatus.Success;
            }

            // Make sure we have a route to the job.
            if (_path == null || _path.EndTile() != DestinationTile)
            {
                _path = new Path_AStar()
                {
                    World = World.Instance,
                    Start = CurrentTile,
                    End = DestinationTile
                };
                _path.Calculate();
            }

            // If we still don't have a path, there is no path.
            if (_path.IsUnReachable)
            {
                Debug.LogFormat("{0}: SetupMoveToJobSite_Action returning failure due to not being able to find a route", Name);
                AbandonJob();
                return BehaviourTreeStatus.Failure;
            }

            // See if we're "close enough" to the job.
            // _path.Debug();
            if ((_path.Length()) <= CurrentJob.MinRange)
            {
                // Debug.LogFormat("{0}: Close enough to job (is {1} needs to be <= {2})", Name, (_path.Length()), CurrentJob.MinRange);
                // Set dest to current, just in case it was the proximity-check that got us here
                DestinationTile = CurrentTile;
                return BehaviourTreeStatus.Success;
            }
            else
            {
                // Debug.LogFormat("{0}: Distance to job is {1}, needs to be <={2}", Name, (_path.Length()), CurrentJob.MinRange);
            }

            // At this point we should have a valid _nextTile to move to.
            return BehaviourTreeStatus.Success;
        }

        private BehaviourTreeStatus TransferStockToJob_Action()
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

            // Debug.Log("TransferStockToJob_Action is transfering stock to job.");

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

        private BehaviourTreeStatus DoWork_Action(float deltaTime)
        {
            // Debug.Log("DoWork_Action");

            // If there isn't a current job, that probably means while this step was running, the job was completed.
            if (CurrentJob == null)
            {
                if (DebugAi) Debug.Log(Name + ": DoWork returning success");
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
                if (DebugAi) Debug.Log(Name + ": DoWork returning failure");
                return BehaviourTreeStatus.Failure;
            }

            // Debug.LogFormat("DoWork_Action working job \"{3}\" at [{0},{1}] ({2:F2})", CurrentTile.X, CurrentTile.Y, CurrentJob.JobTime, CurrentJob.Name);

            CurrentState = State.WorkingJob;

            // Set dest to current, just in case it was the proximity-check that got us here.
            DestinationTile = CurrentTile;
            _path = null;

            // Do some work.
            CurrentJob.DoWork(deltaTime);
            if (DebugAi) Debug.Log(Name + ": DoWork returning running");
            return BehaviourTreeStatus.Running;
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
        private BehaviourTreeStatus Breathe_Action(float deltaTime)
        {
            if (CurrentTile == null) return BehaviourTreeStatus.Failure;
            if (CurrentTile.Room == null) return BehaviourTreeStatus.Failure;

            // Consume some oxygen.
            CurrentTile.Room.Atmosphere.ChangeGas("O2", -this.BreathVolume() * deltaTime);

            // In each breath in, we take in about 18mg of O2, and release back out 36mg of CO2 and 20mg of H2O, which is 0.8 molecules of CO2 for every molecule of O2.
            // I'm not sure how to convert that into a sensible "CO2 produced" number, so this is MADE UP. TODO: Don't make this up.
            CurrentTile.Room.Atmosphere.ChangeGas("CO2", this.BreathVolume() * deltaTime);

            return BehaviourTreeStatus.Success;
        }

        /// <summary>
        /// Try to breathe suit air.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns>Failure if reserve &lt; 20%; otherwise success.</returns>
        private BehaviourTreeStatus BreatheSuit_Action(float deltaTime)
        {
            this.ChangeCondition("suit_air", -this.BreathVolume() * deltaTime);
            this.SetCondition("suit_air", Mathf.Clamp01(this.GetCondition("suit_air")));

            if (this.GetCondition("suit_air") < TankDangerLevel)
            {
                // Debug.LogFormat("{0} Running out of air. ({1})", this.Name, this.GetCondition("suit_air"));
                return BehaviourTreeStatus.Failure;
            }

            return BehaviourTreeStatus.Success;
        }

        private BehaviourTreeStatus ReplenishSuit_Action(float deltaTime)
        {
            this.ChangeCondition("suit_air", this.BreathVolume() * TankTopupRate * deltaTime);
            this.SetCondition("suit_air", Mathf.Clamp01(this.GetCondition("suit_air")));

            // If we have a job, stop charging at the min level
            if (this.CurrentJob != null && this.GetCondition("suit_air") >= TankMinLevel)
            {
                return BehaviourTreeStatus.Success;
            }

            // If we don't have a job, just carry on until the tank is full.
            if (this.GetCondition("suit_air") >= 1)
            {
                return BehaviourTreeStatus.Success;
            }

            // Debug.LogFormat("{0} Topping up suit air. ({1})", this.Name, this.GetCondition("suit_air"));
            return BehaviourTreeStatus.Running;
        }

        private BehaviourTreeStatus FindSafety_Action()
        {
            if (this.RoomIsSafe(CurrentTile))
            {
                // Already in a safe place.
                AbandonMove();
                return BehaviourTreeStatus.Success;
            }

            if (this.RoomIsSafe(DestinationTile))
            {
                // Already heading to a safe place.
                return BehaviourTreeStatus.Success;
            }

            // Look for a safe room nearby.
            var targetRoomTile = FindNearestSafeRoom();
            if (targetRoomTile == null)
            {
                // Debug.Log("Could not find a safe room!");
                return BehaviourTreeStatus.Failure;
            }

            // Debug.LogFormat("{0} finding safety", Name);

            DestinationTile = targetRoomTile;
            return BehaviourTreeStatus.Success;
        }

        private BehaviourTreeStatus DrainSuit_Action(float deltaTime)
        {
            // Consume energy if the shield is enabled.
            if (ShieldStatus == true)
            {
                this.ChangeCondition("energy", -ShieldEnergyUsageRate * deltaTime);
            }

            // Charge up the energy store
            this.ChangeCondition("energy", EnergyChargeRate * deltaTime);

            return BehaviourTreeStatus.Success;
        }

        public void Update(float deltaTime)
        {
            _tree.Tick(new TimeData(deltaTime));

            var times = new Dictionary<string, long>();
            _tree.GetTimes(times);
            var tstring = this.Name;
            foreach(var t in times.Where(o => o.Value > 0).OrderByDescending(o => o.Value).Take(10))
            {
                tstring = tstring + "; " + t.Key + ':' + t.Value;
            }
            if (tstring != this.Name)
            {
                // Debug.Log(tstring);
            }   

            // Hack in some environmental effects.
            // Currently breathability is all that affects if the shield is up or not.
            if (this.CanBreathe())
            {
                // this.Breathe(deltaTime);
                ShieldStatus = false;
            }
            else
            {
                ShieldStatus = true;
            }

            _cbCharacterChanged?.Invoke(this);
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