using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Controllers
{
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; protected set; }

        public World World { get; protected set; }
        private static bool loadWorld = false;

        public void NewWorld()
        {
            Debug.Log("New World clicked.");
            loadWorld = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void SaveWorld()
        {
            Debug.Log("Save World clicked.");
            var filename = Path.Combine(Application.persistentDataPath, "SaveGame000.xml");

            var serializer = new XmlSerializer(typeof(World));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, World);
            writer.Close();

            Debug.Log("World saved to " + filename);
        }

        public void LoadWorld()
        {
            Debug.Log("Load World clicked.");
            loadWorld = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public Tile GetTileAtWorldCoordinates(Vector3 coord)
        {
            var x = Mathf.FloorToInt(coord.x);
            var y = Mathf.FloorToInt(coord.y);

            return World.GetTileAt(x, y);
        }

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

            if (loadWorld)
            {
                CreateWorldFromSave();
            }
            else
            {
                CreateEmptyWorld();
            }
        }

        private void CreateWorldFromSave()
        {
            Debug.Log("Creating world from save.");
            var filename = Path.Combine(Application.persistentDataPath, "SaveGame000.xml");
            if (File.Exists(filename) == false)
            {
                return;
            }

            var serializer = new XmlSerializer(typeof(World));
            var reader = new StreamReader(filename);
            World = (World)serializer.Deserialize(reader);
            reader.Close();

            Camera.main.transform.position = new Vector3(World.Width / 2f, World.Height / 2f, Camera.main.transform.position.z);

            //  Use this call for wherever a player triggers a custom event
            Analytics.CustomEvent("GameLoad", new Dictionary<string, object>() { {"save", "SaveGame000.xml"} });
        }

        private void CreateEmptyWorld()
        {
            Debug.Log("Creating empty world.");
            this.World = new World(100, 100);

            World.CreateCharacter(World.GetTileAt(World.Width / 2, World.Height / 2)); // First test character
            // World.CreateCharacter(World.GetTileAt(World.Width / 2 + 1, World.Height / 2)); // Another test character

            Camera.main.transform.position = new Vector3(World.Width / 2f, World.Height / 2f, Camera.main.transform.position.z);
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

            float scrollSpeed = 4f;
            if (Input.GetKey(KeyCode.A))
            {
                Camera.main.transform.position += Vector3.left * Time.deltaTime * scrollSpeed;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Camera.main.transform.position += Vector3.right * Time.deltaTime * scrollSpeed;
            }
            if (Input.GetKey(KeyCode.W))
            {
                Camera.main.transform.position += Vector3.up * Time.deltaTime * scrollSpeed;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Camera.main.transform.position += Vector3.down * Time.deltaTime * scrollSpeed;
            }
        }

    }
}