using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Assets.Scripts.Model;
using MoonSharp.RemoteDebugger;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Controllers
{
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; protected set; }

        public World World { get; protected set; }
        private static bool loadWorld = false;

        public void NewWorld()
        {
            // Debug.Log("New World clicked.");
            loadWorld = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void SaveWorld()
        {
            // Debug.Log("Save World clicked.");
            var filename = Path.Combine(Application.persistentDataPath, "SaveGame000.xml");

            var xml = new XmlDocument();
            var xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = xml.DocumentElement;
            xml.InsertBefore(xmlDeclaration, root);
            var world = this.World.WriteXml(xml);
            xml.AppendChild(world);
            xml.Save(filename);

            Debug.Log("World saved to " + filename);
        }

        public void LoadWorld()
        {
            // Debug.Log("Load World clicked.");
            loadWorld = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public Tile GetTileAtWorldCoordinates(Vector3 coord)
        {
            var x = Mathf.FloorToInt(coord.x + 0.5f);
            var y = Mathf.FloorToInt(coord.y + 0.5f);

            return World.Instance.GetTileAt(x, y);
        }

        // RemoteDebuggerService remoteDebugger;

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
            // Debug.Log("Creating world from save.");
            var filename = Path.Combine(Application.persistentDataPath, "SaveGame000.xml");
            if (File.Exists(filename) == false)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(filename);
            this.World = World.ReadXml(xml);

            // Center the camera.
            Camera.main.transform.position = new Vector3(World.Width / 2f, World.Height / 2f, Camera.main.transform.position.z);
        }

        private void CreateEmptyWorld()
        {
            // Debug.Log("Creating empty world.");
            this.World = new World(100, 100);

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    World.CreateCharacter(World.GetTileAt(World.Width / 2 + x, World.Height / 2 + y));
                }
            }

            Camera.main.transform.position = new Vector3(World.Width / 2f, World.Height / 2f, Camera.main.transform.position.z);
        }

        /// <summary>
        /// This is called by Unity before every frame renderered.
        /// </summary>
        private void Update()
        {
            World.Update(Time.deltaTime);

            var scrollSpeed = 4f;
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