﻿using Assets.Model;
using UnityEngine;

namespace Assets.Controllers
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
            WorldController.Instance.World.PlaceInstalledObject(furnitureType, t);
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

                    var j = new Job(t, furnitureType, (theJob) =>
                    {
                        WorldController.Instance.World.PlaceInstalledObject(furnitureType, t);
                        t.PendingFurnitureJob = null;
                    });
                    t.PendingFurnitureJob = j;
                    j.RegisterJobCancelledCallback((theJob) => { theJob.Tile.PendingFurnitureJob = null; });
                    j.RegisterJobCompleteCallback((theJob) => { theJob.Tile.PendingFurnitureJob = null; });

                    WorldController.Instance.World.JobQueue.Enqueue(j);
                }
            }
            else
            {
                // We are in Tile-changing mode.
                t.Type = _buildModeTileType;
            }
        }
    }
}
