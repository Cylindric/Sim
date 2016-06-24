using Assets.Model;
using UnityEngine;

namespace Assets.Controllers
{
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; protected set; }

        public World World { get; protected set; }

        private void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There shouldn't be an instance already!");
            }
            Instance = this;

            // Create an empty World.
            this.World = new World(100, 100);
        
            // Centre the view on the middle of the world.
            Camera.main.transform.position = new Vector3(World.Width/2f, World.Height/2f, Camera.main.transform.position.z);
        }

        private void Update()
        {
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

    }
}