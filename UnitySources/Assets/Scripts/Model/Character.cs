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
    public class Character : IXmlSerializable
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

        private Tile nextTile; // The next tile in the pathfinding sequence
        private Path_AStar pathAStar;
        private float movementPercentage; // Goes from 0 to 1 as we move from currTile to destTile
        private float speed = 5f; // Tiles per second
        private Job myJob;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */
        public Character()
        { }

        public Character(Tile tile)
        {
            currTile = destTile = nextTile = tile;
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
            get { return Mathf.Lerp(currTile.X, nextTile.X, movementPercentage); }
        }

        public float Y
        {
            get { return Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage); }
        }

        public Tile currTile { get; protected set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        public void AbandonJob()
        {
            nextTile = destTile = currTile;
            pathAStar = null;
            currTile.World.JobQueue.Enqueue(myJob);
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
            if (currTile.IsNeighbour(tile, true) == false)
            {
                Debug.Log("Character::SetDestination -- Our destination tile isn't actually our neighbour.");
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
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Character");
            writer.WriteAttributeString("X", currTile.X.ToString());
            writer.WriteAttributeString("Y", currTile.Y.ToString());
            writer.WriteEndElement();
        }

        private void GetNewJob()
        {
            // Grab a new job.
            myJob = currTile.World.JobQueue.Dequeue();
            if (myJob == null)
            {
                return;
            }

            destTile = myJob.Tile;
            myJob.RegisterOnCompleteCallback(OnJobEnded);
            myJob.RegisterOnCancelCallback(OnJobEnded);

            // Check to see if the job is reachable from the character's current position.
            // We mmight have to go somewhere else first to get materials.

            pathAStar = new Path_AStar(currTile.World, currTile, destTile);
            if (pathAStar.Length() == 0)
            {
                // Debug.LogError("Path_AStar returned no path to target job tile!");
                AbandonJob();
                pathAStar = null;
                destTile = currTile;
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
                    destTile = currTile;
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

                        if (currTile == destTile)
                        {
                            // We are at the jobsite, so drop the inventory.
                            currTile.World.InventoryManager.PlaceInventory(myJob, inventory);
                            myJob.DoWork(0); // This will call all the cbJobWorked callbacks

                            if (inventory.stackSize == 0)
                            {
                                inventory = null;
                            }
                            else
                            {
                                Debug.LogError("Character is still carrying inventory, which shouldn't be the case.");
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
                        // TODO: actually dump it to an empty tile, as we might be stood on a job tile.
                        if (currTile.World.InventoryManager.PlaceInventory(currTile, inventory) == false)
                        {
                            Debug.LogError("Character tried to dump inventory to an invalid tile.");
                            // TODO: At this point we should try to dump this inv somewhere else, but for now we're just deleting it.
                            inventory = null;
                        }
                    }
                }
                else
                {
                    // At this point, the job still requires inventory, but we don't have it.
                    // That means we need to walk towards a Tile that does have the required items.

                    if (currTile.inventory != null && 
                        (myJob.CanTakeFromStockpile || currTile.Furniture == null || currTile.Furniture.IsStockpile() == false) &&
                        myJob.NeedsMaterial(currTile.inventory) != 0)
                    {
                        // The materials we need are right where we're stood!
                        currTile.World.InventoryManager.PlaceInventory(
                            character: this, 
                            source: currTile.inventory, 
                            qty: myJob.NeedsMaterial(currTile.inventory));

                    }
                    else
                    {
                        // The Job needs some of this:
                        var unsatisfied = myJob.GetFirstRequiredInventory();

                        // Look for the first item that matches
                        var supply = currTile.World.InventoryManager.GetClosestInventoryOfType(
                            objectType: unsatisfied.objectType,
                            t: currTile,
                            desiredQty: unsatisfied.maxStackSize - unsatisfied.stackSize,
                            searchInStockpiles: myJob.CanTakeFromStockpile);

                        if (supply == null)
                        {
                            //Debug.LogFormat("No Tile found containing the desired type ({0}).", unsatisfied.objectType);
                            AbandonJob();
                            return;
                        }

                        destTile = supply.tile;
                        return;
                    }
                }

                // Cannot continue until we have everythign we need.
                return;
            }

            // We have all the material that we need
            // Make sure the destination tile is the job tile
            destTile = myJob.Tile;

            // Are we there yet?
            if (currTile == myJob.Tile)
            {
                myJob.DoWork(deltaTime);
            }

            // Done.
        }

        private void Update_DoMovement(float deltaTime)
        {
            if (currTile == destTile)
            {
                pathAStar = null;
                return; // We're already were we want to be.
            }
            
            if (nextTile == null || nextTile == currTile)
            {
                // Get the next tile from the pathfinder.
                if (pathAStar == null || pathAStar.Length() == 0)
                {
                    // Generate a path to our destination
                    pathAStar = new Path_AStar(currTile.World, currTile, destTile);
                    // This will calculate a path from curr to dest.
                    if (pathAStar.Length() == 0)
                    {
                        //Debug.LogError("Path_AStar returned no path to destination!");
                        AbandonJob();
                        pathAStar = null;
                        return;
                    }

                    // Ignore the first tile in the path, as that's the tile we are currently in,
                    // and we can always move out of our current tile.
                    nextTile = pathAStar.Dequeue();
                }

                // Grab the next waypoint from the pathing system!
                nextTile = pathAStar.Dequeue();


                if (nextTile == currTile)
                {
                    Debug.LogError("Update_DoMovement - nextTile is currTile?");
                }
            }

            // At this point we should have a valid nextTile to move to.

            // What's the total distance from point A to point B?
            // We are going to use Euclidean distance FOR NOW...
            // But when we do the pathfinding system, we'll likely
            // switch to something like Manhattan or Chebyshev distance
            float distToTravel = 0;
            if (nextTile != currTile)
            {
                distToTravel = Mathf.Sqrt(
                    Mathf.Pow(currTile.X - nextTile.X, 2) +
                    Mathf.Pow(currTile.Y - nextTile.Y, 2)
                    );
            }

            // Before entering a tile, make sure it is not impassable.
            // This might happen if the tile is changed (e.g. wall built) after the pathfinder runs.
            if (nextTile.IsEnterable() == Enterability.Never)
            {
                // Debug.LogError("Error - character was strying to enter an impassable tile!");
                nextTile = null;
                pathAStar = null;
            }
            else if(nextTile.IsEnterable() == Enterability.Soon)
            {
                // The next tile we're trying to enter is walkable, but maybe for some reason
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

                // TODO: Get the next tile from the pathfinding system.
                //       If there are no more tiles, then we have TRULY
                //       reached our destination.

                currTile = nextTile;
                movementPercentage = 0;
            }
        }

        private void OnJobEnded(Job j)
        {
            // Job completed or was cancelled.

            j.UnregisterOnCancelCallback(OnJobEnded);
            j.UnregisterOnCompleteCallback(OnJobEnded);

            if (j != myJob)
            {
                Debug.LogError("Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }

            myJob = null;
        }

    }
}