using System.Security.Policy;
using Assets.Scripts.Controllers;
using Assets.Scripts.Model;
using Assets.Scripts.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public enum BuildMode
    {
        Floor,
        Furniture,
        Deconstruct
    }

    public class BuildModeController : MonoBehaviour
    {
        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        public BuildMode BuildMode = BuildMode.Floor;
        public string BuildModeObjectType;
        private TileType _buildModeTileType;

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        void Awake()
        {
            //#if UNITY_EDITOR
            //QualitySettings.vSyncCount = 0;  // VSync must be disabled
            //Application.targetFrameRate = 10;
            //#endif
        }

        private void Start()
        {
        }

        private void Update()
        {
        }

        public bool IsObjectDraggable()
        {
            if (BuildMode == BuildMode.Floor || BuildMode == BuildMode.Deconstruct)
            {
                // Floors are draggable.
                return true;
            }

            var proto = WorldController.Instance.World.FurniturePrototypes[BuildModeObjectType];

            return (proto.Width == 1 && proto.Height == 1);
        }

        private void OnFurnitureJobComplete(string furnitureType, Tile t)
        {
            WorldController.Instance.World.PlaceFurniture(furnitureType, t);
        }

        public void SetMode_BuildFloor()
        {
            BuildMode = BuildMode.Floor;
            _buildModeTileType = TileType.Floor;

            GameObject.FindObjectOfType<MouseController>().StartBuildMode();
        }

        public void SetMode_Clear()
        {
            BuildMode = BuildMode.Floor;
            _buildModeTileType = TileType.Empty;
        }

        public void SetMode_Deconstruct()
        {
            BuildMode = BuildMode.Deconstruct;
            GameObject.FindObjectOfType<MouseController>().StartBuildMode();
        }

        public void SetMode_BuildInstalledObject(string type)
        {
            BuildModeObjectType = type;
            BuildMode = BuildMode.Furniture;
            GameObject.FindObjectOfType<MouseController>().StartBuildMode();
        }

        public void DoBuild(Tile t)
        {
            if (BuildMode == BuildMode.Furniture)
            {
                var furnitureType = BuildModeObjectType;

                // Check that we can build the object in the selected Tile.
                if (
                    t.PendingFurnitureJob == null
                    && WorldController.Instance.World.IsFurniturePlacementValid(furnitureType, t))
                {
                    // this Tile is valid for this furniture.
                    // Create a job to build it.

                    Job j;

                    if (WorldController.Instance.World.FurnitureJobPrototypes.ContainsKey(furnitureType))
                    {
                        // Make a clone of the Job Prototype.
                        j = WorldController.Instance.World.FurnitureJobPrototypes[furnitureType].Clone();

                        // Assign the correct Tile.
                        j.Tile = t;
                    }
                    else
                    {
                        Debug.LogError("There is no Furniture Job Prototype for '" + furnitureType + "'");
                        j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null);
                        j.Name = "BuildFurniture";
                    }

                    j.FurniturePrototype = WorldController.Instance.World.FurniturePrototypes[furnitureType];
                    j.MinRange = 1;
                    t.PendingFurnitureJob = j;
                    j.RegisterOnJobStoppedCallback((theJob) => { theJob.Tile.PendingFurnitureJob = null; });
                    j.RegisterOnJobCompletedCallback((theJob) => { theJob.Tile.PendingFurnitureJob = null; });

                    WorldController.Instance.World.JobQueue.Enqueue(j);
                }
            }
            else if (BuildMode == BuildMode.Floor)
            {
                // We are in Tile-changing mode.
                t.Type = _buildModeTileType;
            }
            else if (BuildMode == BuildMode.Deconstruct)
            {
                if (t.Furniture != null)
                {
                    t.Furniture.Deconstruct();
                }

            }
            else
            {
                Debug.LogError("Unimplemented build mode");
            }
        }

        public void BuildTestMap()
        {
            WorldController.Instance.World.SetupPathfindingTestMap();
        }

    }
}
