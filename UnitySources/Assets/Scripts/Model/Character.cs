using System;
using System.Diagnostics;
using Assets.Scripts.Pathfinding;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    [DebuggerDisplay("Character at [{X}, {Y}]")]
    public class Character
    {
        public float X
        {
            get { return Mathf.Lerp(currTile.X, nextTile.X, movementPercentage); }
        }

        public float Y
        {
            get { return Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage); }
        }

        public Tile currTile { get; protected set; }


        Tile destTile; // If we aren't moving, then destTile = currTile
        Tile nextTile; // The next tile in the pathfinding sequence
        Path_AStar pathAStar;
        float movementPercentage; // Goes from 0 to 1 as we move from currTile to destTile

        float speed = 5f; // Tiles per second

        Action<Character> cbCharacterChanged;

        Job myJob;

        public Character(Tile tile)
        {
            currTile = destTile = nextTile = tile;
        }

        void Update_DoJob(float deltaTime)
        {
            // Do I have a job?
            if (myJob == null)
            {
                // Grab a new job.
                myJob = currTile.World.JobQueue.Dequeue();

                if (myJob != null)
                {
                    // We have a job!

                    // TODO: Check to see if the job is REACHABLE!

                    destTile = myJob.Tile;
                    myJob.RegisterOnCompleteCallback(OnJobEnded);
                    myJob.RegisterOnCancelCallback(OnJobEnded);
                }
            }

            // Are we there yet?
            if (myJob != null && currTile == myJob.Tile)
            {
                //if(pathAStar != null && pathAStar.Length() == 1)	{ // We are adjacent to the job site.
                if (myJob != null)
                {
                    myJob.DoWork(deltaTime);
                }
            }

        }

        public void AbandonJob()
        {
            nextTile = destTile = currTile;
            pathAStar = null;
            currTile.World.JobQueue.Enqueue(myJob);
            myJob = null;
        }

        void Update_DoMovement(float deltaTime)
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
                }

                // Grab the next waypoint from the pathing system!
                nextTile = pathAStar.Dequeue();

                if (nextTile == currTile)
                {
                    Debug.LogError("Update_DoMovement - nextTile is currTile?");
                }
            }

            /*		if(pathAStar.Length() == 1) {
                        return;
                    }
            */
            // At this point we should have a valid nextTile to move to.

            // What's the total distance from point A to point B?
            // We are going to use Euclidean distance FOR NOW...
            // But when we do the pathfinding system, we'll likely
            // switch to something like Manhattan or Chebyshev distance
            float distToTravel = Mathf.Sqrt(
                Mathf.Pow(currTile.X - nextTile.X, 2) +
                Mathf.Pow(currTile.Y - nextTile.Y, 2)
                );


            // How much distance can be travel this Update?
            float distThisFrame = speed*deltaTime;

            // How much is that in terms of percentage to our destination?
            float percThisFrame = distThisFrame/distToTravel;

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
                // FIXME?  Do we actually want to retain any overshot movement?
            }


        }

        public void Update(float deltaTime)
        {
            //Debug.Log("Character Update");

            Update_DoJob(deltaTime);

            Update_DoMovement(deltaTime);

            if (cbCharacterChanged != null)
                cbCharacterChanged(this);

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

        void OnJobEnded(Job j)
        {
            // Job completed or was cancelled.

            if (j != myJob)
            {
                Debug.LogError("Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }

            myJob = null;
        }
    }
}