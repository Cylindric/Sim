﻿using System;
using UnityEngine;

namespace Assets.Model
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
        public Action<Character> cbCharacterChanged;

        private Job myJob;

        private Tile destTile;
        private float movementPercentage;
        private float speed = 2f; // Tiles Per Second

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
                    myJob.RegisterJobCancelledCallback(OnJobEnded);
                    myJob.RegisterJobCompleteCallback(OnJobEnded);
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

            float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - destTile.X, 2) + Mathf.Pow(currTile.Y - destTile.Y, 2));
            float distThisFrame = speed*deltaTime;
            float percThisFrame = distThisFrame/distToTravel;

            movementPercentage += percThisFrame;
            if (movementPercentage >= 1)
            {
                currTile = destTile;
                movementPercentage = 0;
            }

            if (cbCharacterChanged != null)
            {
                cbCharacterChanged(this);
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

        public void RegisterCharacterChangedCallback(Action<Character> cb)
        {
            cbCharacterChanged += cb;
        }

        public void UnRegisterCharacterChangedCallback(Action<Character> cb)
        {
            cbCharacterChanged -= cb;
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