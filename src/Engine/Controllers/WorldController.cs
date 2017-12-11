using System.IO;
using System.Xml;
using Engine.Models;
using Engine.Utilities;
using System;

namespace Engine.Controllers
{
    public class WorldController : IController
    {
        #region Singleton
        private static readonly Lazy<WorldController> _instance = new Lazy<WorldController>(() => new WorldController());

        public static WorldController Instance { get { return _instance.Value; } }

        private WorldController()
        {
        }
        #endregion

        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */
        private static bool _loadWorld = false;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */
        public World World { get; protected set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Start()
        {
            MarkovNameGenerator.Start();

            var args = Environment.GetCommandLineArgs();

            if (_loadWorld)
            {
                CreateWorldFromSave();
            }
            else
            {
                CreateEmptyWorld();
            }
        }

        public void NewWorld()
        {
            // Debug.Log("New World clicked.");
            _loadWorld = false;
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void SaveWorld()
        {
            // Debug.Log("Save World clicked.");
            var filename = Path.Combine(Engine.Instance.SavePath, "SaveGame000.xml");

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
            _loadWorld = true;
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public Tile GetTileAtWorldCoordinates(WorldCoord coord)
        {
            var x = Mathf.FloorToInt(coord.X + 0.5f);
            var y = Mathf.FloorToInt(coord.Y + 0.5f);

            return World.Instance.GetTileAt(x, y);
        }

        private void CreateWorldFromSave()
        {
            Debug.Log("Creating world from save.");
            var filename = Path.Combine(Engine.Instance.SavePath, "SaveGame000.xml");
            if (File.Exists(filename) == false)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(filename);
            this.World = World.ReadXml(xml);

            // Center the camera.
            CameraController.Instance.Position = new ScreenCoord(World.Width / 2f, World.Height / 2f);

            Debug.LogFormat("Loaded game from {0}", filename);
        }

        private void CreateEmptyWorld()
        {
            Debug.Log("Creating empty world.");
            World = new World(10, 10);

            var middleX = World.Width / 2;
            var middleY = World.Height / 2;

            World.GetTileAt(middleX - 1, middleY - 1).Type = TileType.Floor;
            World.GetTileAt(middleX, middleY - 1).Type = TileType.Floor;
            World.GetTileAt(middleX + 1, middleY - 1).Type = TileType.Floor;

            World.GetTileAt(middleX - 1, middleY).Type = TileType.Floor;
            World.GetTileAt(middleX, middleY).Type = TileType.Floor;
            World.GetTileAt(middleX + 1, middleY).Type = TileType.Floor;

            World.GetTileAt(middleX - 1, middleY + 1).Type = TileType.Floor;
            World.GetTileAt(middleX, middleY + 1).Type = TileType.Floor;
            World.GetTileAt(middleX + 1, middleY + 1).Type = TileType.Floor;

            // Put some characters into the world
            // World.CreateCharacter(World.GetTileAt(World.Width / 2 - 1, World.Height / 2));
            World.CreateCharacter(World.GetTileAt(World.Width / 2, World.Height / 2));
            // BWorld.CreateCharacter(World.GetTileAt(World.Width / 2 + 1, World.Height / 2));

            CameraController.Instance.SetPosition(new ScreenCoord(World.Width / 2f, World.Height / 2f));
        }

        public void Update()
        {
            World.Update(TimeController.Instance.DeltaTime);

            if (World.TileGraph != null)
            {
                World.TileGraph.DebugVis();
            }
        }

        public void Render()
        {
            // throw new NotImplementedException();
        }

    }
}