using System.Runtime.InteropServices;
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
            if (currTile == destTile)
            {
                return;
            }

            float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - destTile.X, 2) + Mathf.Pow(currTile.Y - destTile.Y, 2));
            float distThisFrame = speed*deltaTime;
            float percThisFrame = distToTravel/distThisFrame;

            movementPercentage += percThisFrame;
            if (movementPercentage >= 1)
            {
                currTile = destTile;
                movementPercentage = 0;
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
    }
}
