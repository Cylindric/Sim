using Assets.Model;
using UnityEngine;

namespace Assets.Controllers
{
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; protected set; }

        public World World { get; protected set; }

        /// <summary>
        /// Called by Unity when the controller is created. 
        /// We're using OnEnable() instead of Start() just to make sure it's ready before anything else.
        /// </summary>
        private void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There shouldn't be an instance already!");
            }
            Instance = this;

            // Create an empty World.
            this.World = new World(15, 15);
        
            // Centre the view on the middle of the world.
            Camera.main.transform.position = new Vector3(World.Width/2f, World.Height/2f, Camera.main.transform.position.z);
        }

        /// <summary>
        /// This is called by Unity before every frame renderered.
        /// </summary>
        private void Update()
        {
            World.Update(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        public Tile GetTileAtWorldCoordinates(Vector3 coord)
        {
            var x = Mathf.FloorToInt(coord.x);
            var y = Mathf.FloorToInt(coord.y);

            return World.GetTileAt(x, y);
        }

        public void BuildTestMap()
        {
            World.SetupPathfindingTestMap();
        }

    }
}