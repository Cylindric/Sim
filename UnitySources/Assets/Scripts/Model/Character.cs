using System;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class Character
    {
        public float X
        {
            get { return Mathf.Lerp(currTile.X, destTile.X, movementPercentage); }
        }

        public float Y
        {
            get { return Mathf.Lerp(currTile.Y, destTile.Y, movementPercentage); }
        }

        public Tile currTile { get; private set; }

        private Job myJob;
        private Tile destTile;
        private float movementPercentage;
        private float speed = 2f; // Tiles Per Second
        private Action<Character> cbOnChange;

        public Character(Tile tile)
        {
            currTile = tile;
            destTile = tile;
        }

        public void Update(float deltaTime)
        {
            if (myJob == null)
            {
                // Get a new job from the queue.
                myJob = currTile.World.JobQueue.Dequeue();
                if (myJob != null)
                {
                    destTile = myJob.Tile;
                    myJob.RegisterOnCancelCallback(OnJobEnded);
                    myJob.RegisterOnCompleteCallback(OnJobEnded);
                }
            }

            if (currTile == destTile)
            {
                if (myJob != null)
                {
                    myJob.DoWork(deltaTime);
                }
                return;
            }

            float distToTravel = Mathf.Sqrt(
                Mathf.Pow(currTile.X - destTile.X, 2) +
                Mathf.Pow(currTile.Y - destTile.Y, 2)
                );

            
            float distThisFrame = speed*deltaTime;
            float percThisFrame = distThisFrame/distToTravel;

            movementPercentage += percThisFrame;
            if (movementPercentage >= 1)
            {
                // We have reached our destination.

                // Get the next tile from the pathfinder. If there aren't any, we have arrived.

                currTile = destTile;
                movementPercentage = 0;
            }

            if (cbOnChange != null)
            {
                cbOnChange(this);
            }
        }

        public void SetDestination(Tile t)
        {
            if (currTile.IsNeighbour(t, true) == false)
            {
                Debug.Log("Character::SetDestination - our destination tile must be adjacent to the current tile.");
            }
            destTile = t;
        }

        public void RegisterOnChangeCallback(Action<Character> cb)
        {
            cbOnChange += cb;
        }

        public void UnregisterOnChangeCallback(Action<Character> cb)
        {
            cbOnChange -= cb;
        }

        private void OnJobEnded(Job j)
        {
            if (j != myJob)
            {
                Debug.LogError("Character being told about job that isn't his.");
                return;
            }

            myJob = null;
        }
    }
}
