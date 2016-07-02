using System.Security.Policy;
using Assets.Scripts.Controllers;
using Assets.Scripts.Model;
using Assets.Scripts.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class BuildModeController : MonoBehaviour
    {
        private bool _buildModeIsObjects = false;
        private TileType _buildModeTileType;
        private string _buildModeObjectType;

        private void Start()
        {
        }
    
        private void OnFurnitureJobComplete(string furnitureType, Tile t)
        {
            WorldController.Instance.World.PlaceFurniture(furnitureType, t);
        }

        public void SetMode_BuildFloor()
        {
            _buildModeIsObjects = false;
            _buildModeTileType = TileType.Floor;
        }

        public void SetMode_Clear()
        {
            _buildModeIsObjects = false;
            _buildModeTileType = TileType.Empty;
        }

        public void SetMode_BuildInstalledObject(string type)
        {
            _buildModeObjectType = type;
            _buildModeIsObjects = true;
        }

        public void DoBuild(Tile t)
        {
            if (_buildModeIsObjects == true)
            {
                string furnitureType = _buildModeObjectType;

                // Check that we can build the object in the selected tile.
                if (
                    t.PendingFurnitureJob == null
                    && WorldController.Instance.World.IsFurniturePlacementValid(furnitureType, t))
                {
                    // this tile is valid for this furniture.
                    // Create a job to build it.

                    Job j;

                    if (WorldController.Instance.World._furnitureJobPrototypes.ContainsKey(furnitureType))
                    {
                        // Make a clone of the Job Prototype.
                        j = WorldController.Instance.World._furnitureJobPrototypes[furnitureType].Clone();

                        // Assign the correct tile.
                        j.Tile = t;
                    }
                    else
                    {
                        Debug.LogError("There is no Furniture Job Prototype for '" + furnitureType + "'");
                        j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null);
                    }

                    t.PendingFurnitureJob = j;
                    j.RegisterOnCancelCallback((theJob) => { theJob.Tile.PendingFurnitureJob = null; });
                    j.RegisterOnCompleteCallback((theJob) => { theJob.Tile.PendingFurnitureJob = null; });

                    WorldController.Instance.World.JobQueue.Enqueue(j);
                }
            }
            else
            {
                // We are in Tile-changing mode.
                t.Type = _buildModeTileType;
            }
        }

        public void BuildTestMap()
        {
            WorldController.Instance.World.SetupPathfindingTestMap();
        }

    }
}
