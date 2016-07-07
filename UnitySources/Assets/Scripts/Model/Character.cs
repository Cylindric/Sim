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

        public Inventory inventory;

        private Tile _destTile;
        private Tile destTile
        {
            get { return _destTile; }
            set
            {
                if (_destTile != value)
                {
                    _destTile = value;
                    pathAStar = null;
                }
            }
        }

        private Tile nextTile; // The next Tile in the pathfinding sequence
        private Path_AStar pathAStar;
        private float movementPercentage; // Goes from 0 to 1 as we move from CurrentTile to destTile
        private float speed = 5f; // Tiles per second
        private Job myJob;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */
        public Character()
        { }

        public Character(Tile tile)
        {
            CurrentTile = destTile = nextTile = tile;
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
                if (nextTile == null) return CurrentTile.X;

                return Mathf.Lerp(CurrentTile.X, nextTile.X, movementPercentage);
            }
        }

        public float Y
        {
            get
            {
                if (nextTile == null) return CurrentTile.Y;

                return Mathf.Lerp(CurrentTile.Y, nextTile.Y, movementPercentage);
            }
        }

        public Tile CurrentTile { get; protected set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        public void AbandonJob()
        {
            nextTile = destTile = CurrentTile;
            //pathAStar = null;
            World.Current.JobQueue.Enqueue(myJob);
            myJob = null;
        }

        public void Update(float deltaTime)
        {
            Update_DoJob(deltaTime);

            Update_DoMovement(deltaTime);

            if (cbCharacterChanged != null)
            {
                cbCharacterChanged(this);
            }
        }

        public void SetDestination(Tile tile)
        {
            if (CurrentTile.IsNeighbour(tile, true) == false)
            {
                Debug.Log("Character::SetDestination -- Our destination Tile isn't actually our neighbour.");
            }

            destTile = tile;
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
                this.inventory = new Inventory();
                this.inventory.Character = this;
                this.inventory.ReadXml(reader);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Character");
            writer.WriteAttributeString("X", CurrentTile.X.ToString());
            writer.WriteAttributeString("Y", CurrentTile.Y.ToString());
            if (this.inventory != null)
            {
                this.inventory.WriteXml(writer);
            }
            writer.WriteEndElement();
        }

        private void GetNewJob()
        {
            // Grab a new job.
            myJob = World.Current.JobQueue.Dequeue();

            if (myJob == null) return;
            
            if (myJob.Furniture != null)
            {
                destTile = myJob.Furniture.GetJobSpotTile();
            }
            else
            {
                destTile = myJob.Tile;
            }

            myJob.RegisterOnJobStoppedCallback(OnJobStopped);

            // Check to see if the job is reachable from the Character's current position.
            // We mmight have to go somewhere else first to get materials.

            // If we are already at the worksite, just return, otherwise need to calculate a route there.
            if (destTile == CurrentTile)
            {
                return;
            }

            pathAStar = new Path_AStar(World.Current, CurrentTile, destTile);
            if (pathAStar.Length() == 0)
            {
                // Debug.LogError("Path_AStar returned no path to target job Tile!");
                AbandonJob();
                pathAStar = null;
                destTile = CurrentTile;
                return;
            }
        }

        private void Update_DoJob(float deltaTime)
        {
            // Do I have a job?
            if (myJob == null)
            {
                GetNewJob();

                if (myJob == null)
                {
                    // There is no job queued, so can just finish.
                    destTile = CurrentTile;
                    return;
                }

            }

            // We have a job, and it is reachable.
            if (myJob.HasAllMaterial() == false)
            {
                // We are missing resources required for the job
                
                // Are we carrying what we need?
                if (inventory != null)
                {
                    if (myJob.NeedsMaterial(inventory) > 0)
                    {
                        destTile = myJob.Tile;

                        if (CurrentTile == destTile)
                        {
                            // We are at the jobsite, so drop the Inventory.
                            World.Current.InventoryManager.PlaceInventory(myJob, inventory);
                            myJob.DoWork(0); // This will call all the cbJobWorked callbacks

                            if (inventory.StackSize == 0)
                            {
                                inventory = null;
                            }
                            else
                            {
                                Debug.LogError("Character is still carrying Inventory, which shouldn't be the case.");
                                inventory = null;
                            }
                        }
                        else
                        {
                            // Still walking to the site.
                            destTile = myJob.Tile;
                            return;
                        }
                    }
                    else
                    {
                        // We are carrying something, but the job doesn't want it.
                        // Dump it where we are.
                        // TODO: actually dump it to an empty Tile, as we might be stood on a job Tile.
                        if (World.Current.InventoryManager.PlaceInventory(CurrentTile, inventory) == false)
                        {
                            Debug.LogError("Character tried to dump Inventory to an invalid Tile.");
                            // TODO: At this point we should try to dump this inv somewhere else, but for now we're just deleting it.
                            inventory = null;
                        }
                    }
                }
                else
                {
                    // At this point, the job still requires Inventory, but we don't have it.
                    // That means we need to walk towards a Tile that does have the required items.

                    if (CurrentTile.Inventory != null && 
                        (myJob.CanTakeFromStockpile || CurrentTile.Furniture == null || CurrentTile.Furniture.IsStockpile() == false) &&
                        myJob.NeedsMaterial(CurrentTile.Inventory) != 0)
                    {
                        // The materials we need are right where we're stood!
                        World.Current.InventoryManager.PlaceInventory(
                            character: this, 
                            source: CurrentTile.Inventory, 
                            qty: myJob.NeedsMaterial(CurrentTile.Inventory));

                    }
                    else
                    {
                        // The Job needs some of this:
                        var unsatisfied = myJob.GetFirstRequiredInventory();

                        // Look for the first item that matches
                        var supply = World.Current.InventoryManager.GetClosestInventoryOfType(
                            objectType: unsatisfied.ObjectType,
                            t: CurrentTile,
                            desiredQty: unsatisfied.MaxStackSize - unsatisfied.StackSize,
                            searchInStockpiles: myJob.CanTakeFromStockpile);

                        if (supply == null)
                        {
                            //Debug.LogFormat("No Tile found containing the desired type ({0}).", unsatisfied.ObjectType);
                            AbandonJob();
                            return;
                        }

                        destTile = supply.Tile;
                        return;
                    }
                }

                // Cannot continue until we have everythign we need.
                return;
            }

            // We have all the material that we need
            // Make sure the destination Tile is the job Tile
            destTile = myJob.Tile;

            // Are we there yet?
            if (CurrentTile == myJob.Tile)
            {
                myJob.DoWork(deltaTime);
            }

            // Done.
        }

        private void Update_DoMovement(float deltaTime)
        {
            if (CurrentTile == destTile)
            {
                pathAStar = null;
                return; // We're already were we want to be.
            }
            
            if (nextTile == null || nextTile == CurrentTile)
            {
                // Get the next Tile from the pathfinder.
                if (pathAStar == null || pathAStar.Length() == 0)
                {
                    // Generate a path to our destination
                    pathAStar = new Path_AStar(World.Current, CurrentTile, destTile);
                    // This will calculate a path from curr to dest.
                    if (pathAStar.Length() == 0)
                    {
                        //Debug.LogError("Path_AStar returned no path to destination!");
                        AbandonJob();
                        pathAStar = null;
                        return;
                    }

                    // Ignore the first Tile in the path, as that's the Tile we are currently in,
                    // and we can always move out of our current Tile.
                    nextTile = pathAStar.Dequeue();
                }

                // Grab the next waypoint from the pathing system!
                nextTile = pathAStar.Dequeue();


                if (nextTile == CurrentTile)
                {
                    Debug.LogError("Update_DoMovement - nextTile is CurrentTile?");
                }
            }

            // At this point we should have a valid nextTile to move to.

            // What's the total distance from point A to point B?
            // We are going to use Euclidean distance FOR NOW...
            // But when we do the pathfinding system, we'll likely
            // switch to something like Manhattan or Chebyshev distance
            float distToTravel = 0;
            if (nextTile != CurrentTile)
            {
                distToTravel = Mathf.Sqrt(
                    Mathf.Pow(CurrentTile.X - nextTile.X, 2) +
                    Mathf.Pow(CurrentTile.Y - nextTile.Y, 2)
                    );
            }

            // Before entering a Tile, make sure it is not impassable.
            // This might happen if the Tile is changed (e.g. wall built) after the pathfinder runs.
            if (nextTile.IsEnterable() == Enterability.Never)
            {
                // Debug.LogError("Error - Character was strying to enter an impassable Tile!");
                nextTile = null;
                pathAStar = null;
            }
            else if(nextTile.IsEnterable() == Enterability.Soon)
            {
                // The next Tile we're trying to enter is walkable, but maybe for some reason
                // cannot be entered right now. Perhaps it is occupied, or contains a closed door.
                return;
            }

            // How much distance can be travel this Update?
            var distThisFrame = (speed / nextTile.MovementCost) * deltaTime;

            // How much is that in terms of percentage to our destination?
            var percThisFrame = distThisFrame/distToTravel;

            // Add that to overall percentage travelled.
            movementPercentage += percThisFrame;

            if (movementPercentage >= 1)
            {
                // We have reached our destination

                // TODO: Get the next Tile from the pathfinding system.
                //       If there are no more tiles, then we have TRULY
                //       reached our destination.

                CurrentTile = nextTile;
                movementPercentage = 0;
            }
        }

        private void OnJobStopped(Job j)
        {
            // Job completed or was cancelled.

            j.UnregisterOnJobStoppedCallback(OnJobStopped);

            if (j != myJob)
            {
                Debug.LogError("Character being told about job (" + j.Name + ") that isn't his. You forgot to unregister something.");
                return;
            }

            myJob = null;
        }

    }
}