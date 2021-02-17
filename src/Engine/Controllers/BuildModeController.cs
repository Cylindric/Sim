using Engine.Utilities;
using Engine.Models;
using System;
using Engine.Renderer.SDLRenderer;
using static Engine.Engine;

namespace Engine.Controllers
{
    public class BuildModeController : IController
    {
        #region Singleton
        private static readonly Lazy<BuildModeController> _instance = new Lazy<BuildModeController>(() => new BuildModeController());

        public static BuildModeController Instance { get { return _instance.Value; } }

        private BuildModeController()
        {
        }
        #endregion

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        public BuildMode BuildMode = BuildMode.Floor;
        public string BuildModeObjectType;
        private TileType _buildModeTileType;

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Start()
        {
        }

        public void Update()
        {
            if (SDLEvent.KeyUp(SDL2.SDL.SDL_Keycode.SDLK_1))
            {
                SetMode_BuildFloor();
            }
            else if (SDLEvent.KeyUp(SDL2.SDL.SDL_Keycode.SDLK_2))
            {
                SetMode_BuildInstalledObject("steel_wall");
            }
        }

        public void Render(LAYER layer)
        {
        }

        public bool IsObjectDraggable()
        {
            if (BuildMode == BuildMode.Floor || BuildMode == BuildMode.Deconstruct || BuildMode == BuildMode.Colonist)
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
            Debug.Log("BMC Initiating floor building mode...");
            BuildMode = BuildMode.Floor;
            _buildModeTileType = TileType.Floor;

            MouseController.Instance.StartBuildMode();
        }

        public void SetMode_Clear()
        {
            Debug.Log("BMC Initiating clear mode...");
            BuildMode = BuildMode.Floor;
            _buildModeTileType = TileType.Empty;
        }

        public void SetMode_Deconstruct()
        {
            Debug.Log("BMC Initiating deconstruction mode...");
            BuildMode = BuildMode.Deconstruct;
            MouseController.Instance.StartBuildMode();
        }

        public void SetMode_BuildInstalledObject(string type)
        {
            Debug.Log($"BMC Initiating {type} building mode...");
            BuildModeObjectType = type;
            BuildMode = BuildMode.Furniture;
            MouseController.Instance.StartBuildMode();
        }

        public void SetMode_BuildColonist()
        {
            Debug.Log("BMC Initiating colonist spawning mode...");
            BuildMode = BuildMode.Colonist;
            MouseController.Instance.StartBuildMode();
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
                        j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null)
                        {
                            Name = "BuildFurniture"
                        };
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
            else if (BuildMode == BuildMode.Colonist)
            {
                World.Instance.CreateCharacter(t);
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
