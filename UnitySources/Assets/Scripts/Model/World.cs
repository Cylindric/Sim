using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Utilities;
using MoonSharp.Interpreter;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    [MoonSharpUserData]
    public class World : IXmlSerializable
    {
        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */
        public JobQueue JobQueue;
        public List<Character> Characters;
        public List<Furniture> Furnitures;
        public List<Room> Rooms;
        public InventoryManager InventoryManager;

        public Dictionary<string, Furniture> FurniturePrototypes;
        public Dictionary<string, Job> FurnitureJobPrototypes;

        private Tile[,] _tiles;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        public World()
        {
            this.JobQueue = new JobQueue();
            this.Characters = new List<Character>();
            this.Furnitures = new List<Furniture>();
            this.InventoryManager = new InventoryManager();

            this.Rooms = new List<Room>();
            this.Rooms.Add(new Room()); // Add the default 'outside' room.
        }

        public World(int width, int height) : this()
        {
            this.SetupWorld(width, height);
        }

        /* #################################################################### */
        /* #                         DELEGATES                                # */
        /* #################################################################### */
        private Action<Furniture> _cbFurnitureCreated;
        private Action<Character> _cbCharacterCreated;
        private Action<Inventory> _cbInventoryCreated;
        private Action<Tile> _cbTileChanged;

        /* #################################################################### */
        /* #                         PROPERTIES                               # */
        /* #################################################################### */
        public static World Instance { get; private set; }
        public Path_TileGraph TileGraph { get; set; } // TODO: this PathTileGraph really shouldn't be fully public like this.
        public int Width { get; private set; }
        public int Height { get; private set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        public void Update(float deltaTime)
        {
            foreach (var c in this.Characters)
            {
                c.Update(deltaTime);
            }

            foreach (var f in this.Furnitures)
            {
                f.Update(deltaTime);
            }
        }

        public Character CreateCharacter(Tile t)
        {
            var c = new Character(t);
            this.Characters.Add(c);

            if (this._cbCharacterCreated != null)
            {
                this._cbCharacterCreated(c);
            }

            return c;
        }

        public Room GetOutsideRoom()
        {
            return Rooms[0];
        }

        public void DeleteRoom(Room r)
        {
            if (r == GetOutsideRoom())
            {
                Debug.LogError("Tried to delete the outside room!");
                return;
            }

            // Remove the current room from the list of rooms.
            Rooms.Remove(r);

            // Make sure no tiles point to this room.
            r.ReturnTilesToOutsideRoom();
        }

        public void AddRoom(Room r)
        {
            Rooms.Add(r);
        }

        public Tile GetTileAt(int x, int y)
        {
            if (x >= this.Width || x < 0 || y >= this.Height || y < 0)
            {
                return null;
            }

            return this._tiles[x, y];
        }

        public Furniture PlaceFurniture(string objectType, Tile t, bool doRoomFloodFill = true)
        {
            if (this.FurniturePrototypes.ContainsKey(objectType) == false)
            {
                Debug.LogErrorFormat("Tried to place an object [{0}] for which we don't have a prototype.", objectType);
                return null;
            }

            var furn = Furniture.PlaceInstance(this.FurniturePrototypes[objectType], t);

            if (furn == null)
            {
                // Failed to place object! Maybe something was already there.
                return null;
            }

            furn.RegisterOnRemovedCallback(OnFurnitureRemoved);
            this.Furnitures.Add(furn);

            // Recalculate rooms?
            if (furn.IsRoomEnclosure & doRoomFloodFill)
            {
                Room.DoRoomFloodfill(furn.Tile);
            }

            if (this._cbFurnitureCreated != null)
            {
                this._cbFurnitureCreated(furn);
                
                if (Mathf.Approximately(furn.MovementCost, 1f) == false)
                {
                    // Tiles with a movement cost of exactly 1, don't affect the path-finding for their job.
                    this.InvalidateTileGraph();
                }
            }

            return furn;
        }

        public void RegisterFurnitureCreatedCb(Action<Furniture> cb)
        {
            this._cbFurnitureCreated += cb;
        }

        public void UnRegisterFurnitureCreatedCb(Action<Furniture> cb)
        {
            this._cbFurnitureCreated -= cb;
        }

        public void RegisterCharacterCreatedCb(Action<Character> cb)
        {
            this._cbCharacterCreated += cb;
        }

        public void UnRegisterCharacterCreatedCb(Action<Character> cb)
        {
            this._cbCharacterCreated -= cb;
        }

        public void RegisterInventoryCreatedCb(Action<Inventory> cb)
        {
            this._cbInventoryCreated += cb;
        }

        public void UnRegisterInventoryCreatedCb(Action<Inventory> cb)
        {
            this._cbInventoryCreated -= cb;
        }

        public void RegisterTileChanged(Action<Tile> cb)
        {
            this._cbTileChanged += cb;
        }

        public void UnRegisterTileChanged(Action<Tile> cb)
        {
            this._cbTileChanged -= cb;
        }

        /// <summary>
        /// Invalidates the current TileGraph.
        /// </summary>
        /// <remarks>
        /// Should be called whenever anything changes that affects the pathing.</remarks>
        public void InvalidateTileGraph()
        {
            this.TileGraph = null;
        }

        public bool IsFurniturePlacementValid(string furnitureType, Tile t)
        {
            return this.FurniturePrototypes[furnitureType].IsValidPosition(t);
        }

        public void SetupPathfindingTestMap()
        {
            var hMid = this.Width/2;
            var vMid = this.Height/2;

            for (int x = hMid-20; x < hMid+20; x++)
            {
                for (int y = vMid-20; y < vMid+20; y++)
                {
                    this._tiles[x, y].Type = TileType.Floor;

                    // Place some walls
                    if ((x == hMid - 3 || x == hMid + 3) || (y == vMid - 3 || y == vMid + 3))
                    {
                        if (x == hMid || y == vMid)
                        {

                        }
                        else
                        {
                            this.PlaceFurniture("furn_wall_steel", this._tiles[x, y]);
                        }
                    }
                }
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this.Width = int.Parse(reader.GetAttribute("Width"));
            this.Height = int.Parse(reader.GetAttribute("Height"));

            this.SetupWorld(this.Width, this.Height);

            var timer = new Stopwatch();

            while (reader.Read())
            {
                Debug.LogFormat("Parsing section {0}.", reader.Name);
                switch (reader.Name)
                {
                    case "Tiles":
                        timer.Start();
                        this.ReadXml_Tiles(reader);
                        Debug.LogFormat("Loading Tiles took {0} ms.", timer.ElapsedMilliseconds);
                        timer.Stop();
                       timer.Reset();
                        break;
                    case "Furnitures":
                        timer.Start();
                        this.ReadXml_Furnitures(reader);
                        Debug.LogFormat("Loading Furniture took {0} ms.", timer.ElapsedMilliseconds);
                        timer.Stop();
                        timer.Reset();
                        break;
                    case "Rooms":
                        timer.Start();
                        this.ReadXml_Rooms(reader);
                        Debug.LogFormat("Loading Rooms took {0} ms.", timer.ElapsedMilliseconds);
                        timer.Stop();
                        timer.Reset();
                        break;
                    case "Characters":
                        timer.Start();
                        this.ReadXml_Characters(reader);
                        Debug.LogFormat("Loading Characters took {0} ms.", timer.ElapsedMilliseconds);
                        timer.Stop();
                        timer.Reset();
                        break;
                }
            }
            reader.Close();

            Inventory inv = new Inventory("steel_plate", 50, 50);
            Tile t = GetTileAt(Width / 2, Height / 2);
            InventoryManager.PlaceInventory(t, inv);
            if (_cbInventoryCreated != null)
            {
                _cbInventoryCreated(t.Inventory);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Width", this.Width.ToString());
            writer.WriteAttributeString("Height", this.Height.ToString());


            writer.WriteStartElement("Rooms");
            foreach (var room in this.Rooms.Skip(1)) // Don't save the outside room
            {
                room.WriteXml(writer);
            }
            writer.WriteEndElement();


            writer.WriteStartElement("Tiles");

            // Get a copy of a default Tile - we won't bother writing those out.
            var defaultTile = new Tile();

            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    // Write out each Tile, if it isn't empty.
                    if (this._tiles[x, y].Type == defaultTile.Type)
                    {
                        continue;
                    }
                    this._tiles[x, y].WriteXml(writer);
                }
            }

            writer.WriteEndElement();

            writer.WriteStartElement("Furnitures");
            foreach (var furn in this.Furnitures)
            {
                furn.WriteXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Characters");
            foreach (var character in this.Characters)
            {
                character.WriteXml(writer);
            }
            writer.WriteEndElement();
        }

        public void SetFurnitureJobPrototype(Job j, Furniture f)
        {
            FurnitureJobPrototypes[f.ObjectType] = j;
        }

        public void LoadFurnitureLua()
        {
            var filepath = Application.streamingAssetsPath;
            filepath = Path.Combine(filepath, "Base");
            filepath = Path.Combine(filepath, "LUA");
            filepath = Path.Combine(filepath, "Furniture");
            foreach (var filename in Directory.GetFiles(filepath, "*.lua"))
            {
                Debug.Log("Loading LUA file " + filename);
                var myLuaCode = System.IO.File.ReadAllText(filename);

                FurnitureActions.LoadLua(myLuaCode);
            }
        }

        private void CreateFurniturePrototypes()
        {
            LoadFurnitureLua();

            this.FurniturePrototypes = new Dictionary<string, Furniture>();
            this.FurnitureJobPrototypes = new Dictionary<string, Job>();

            var filepath = Application.streamingAssetsPath;
            filepath = Path.Combine(filepath, "Base");
            filepath = Path.Combine(filepath, "Data");
            filepath = Path.Combine(filepath, "Furniture.xml");

            var furnText = System.IO.File.ReadAllText(filepath);

            var xml = new XmlDocument();
            xml.LoadXml(furnText);
            var furnitures = xml.DocumentElement.SelectSingleNode("/Furnitures");
            foreach (XmlNode furniture in furnitures.ChildNodes)
            {
                var objectType = XmlParser.ParseAttributeString(furniture, "objectType");
                // Debug.LogFormat("Loading Furniture {0}...", ObjectType);

                var furn = new Furniture(
                    objectType: objectType,
                    movementCost: XmlParser.ParseFloat(furniture, ".//MovementCost", 1),
                    width: XmlParser.ParseInt(furniture, ".//Width", 1),
                    height: XmlParser.ParseInt(furniture, ".//Height", 1),
                    linksToNeighbour: XmlParser.ParseBool(furniture, ".//LinksToNeighbours", false),
                    isRoomEnclosure: XmlParser.ParseBool(furniture, ".//EnclosesRoom", false)
                    );
                furn.Name = XmlParser.ParseString(furniture, ".//Name");
                furn.JobSpotOffset = XmlParser.ParseVector2(furniture, ".//JobSpotOffset");
                furn.JobSpawnOffset = XmlParser.ParseVector2(furniture, ".//JobSpawnOffset");

                Debug.Log("Adding Furniture Prototype " + objectType);
                this.FurniturePrototypes.Add(objectType, furn);

                var parameters = furniture.SelectSingleNode(".//Params");
                if (parameters != null)
                {
                    foreach (XmlNode param in parameters.ChildNodes)
                    {
                        if (param.Attributes == null) continue;

                        var name = param.Attributes["name"].InnerText;
                        var value = float.Parse(param.InnerText);
                        this.FurniturePrototypes[objectType].SetParameter(name, value);
                    }
                }

                var callbacks = furniture.SelectNodes(".//OnUpdate");
                if (callbacks != null)
                {
                    foreach (XmlNode callback in callbacks)
                    {
                        var name = callback.InnerText;
                        furn.RegisterUpdateAction(name);
                    }
                }


                callbacks = furniture.SelectNodes(".//IsEnterable");
                if (callbacks != null)
                {
                    foreach (XmlNode callback in callbacks)
                    {
                        var name = callback.InnerText;
                        furn.RegisterIsEnterableAction(name);
                    }
                }

                foreach (XmlNode buildJob in furniture.SelectNodes(".//BuildingJob"))
                {
                    Debug.LogFormat("Adding Job to Furniture {0}...", objectType);
                    var time = float.Parse(buildJob.Attributes["time"].InnerText);

                    var inventory = new List<Inventory>();
                    foreach (XmlNode inv in buildJob.SelectNodes(".//Inventory"))
                    {
                        var newReq = new Inventory(
                            objectType: inv.Attributes["objectType"].InnerText,
                            maxStackSize: int.Parse(inv.Attributes["amount"].InnerText),
                            stackSize: 0
                            );

                        inventory.Add(newReq);
                    }

                    var newJob = new Job(
                        tile: null,
                        jobObjectType: objectType,
                        cbJobComplete: FurnitureActions.JobComplete_FurnitureBuilding,
                        jobTime: time,
                        inventoryRequirements: inventory.ToArray()
                        )
                    {
                        Name = "Build_" + objectType
                    };

                    this.FurnitureJobPrototypes.Add(
                        key: objectType,
                        value: newJob
                    );
                }

                Debug.LogFormat("Loaded Furniture {0} succeeded.", objectType);
            }
        }

        private void SetupWorld(int width, int height)
        {
            Instance = this;

            this.Width = width;
            this.Height = height;

            this._tiles = new Tile[width, height];

            for (var x = 0; x < this.Width; x++)
            {
                for (var y = 0; y < this.Height; y++)
                {
                    this._tiles[x, y] = new Tile(this, x, y);
                    this._tiles[x, y].RegisterTileTypeChangedCallback(this.OnTileChanged);
                    this._tiles[x, y].Room = Rooms[0];
                }
            }

            Debug.Log("World (" + this.Width + "," + this.Height + ") created with " + (this.Width*this.Height) + " tiles.");

            this.CreateFurniturePrototypes();

            this.Characters = new List<Character>();
            this.Furnitures = new List<Furniture>();
            this.InventoryManager = new InventoryManager();
        }

        private void OnTileChanged(Tile t)
        {
            if (this._cbTileChanged != null)
            {
                this._cbTileChanged(t);
            }

            this.InvalidateTileGraph();
        }

        private void ReadXml_Tiles(XmlReader reader)
        {
            var count = 0;
            if (reader.ReadToDescendant("Tile"))
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    this._tiles[x, y].ReadXml(reader);
                    count++;

                } while (reader.ReadToNextSibling("Tile"));
            }
            Debug.LogFormat("Loaded {0} Tiles from save file.", count);
        }

        private void ReadXml_Furnitures(XmlReader reader)
        {
            if (reader.ReadToDescendant("Furniture"))
            {
                var count = 0;
                do
                {
                    count++;
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    string type = reader.GetAttribute("ObjectType");
                    var furn = this.PlaceFurniture(type, this._tiles[x, y], false);
                    furn.ReadXml(reader);
                } while (reader.ReadToNextSibling("Furniture"));
                Debug.LogFormat("Loaded {0} Furnitures from save file.", count);
            }
            else
            {
                Debug.LogWarning("No Furniture found in save file!");
            }
        }

        private void ReadXml_Rooms(XmlReader reader)
        {
            if (reader.ReadToDescendant("Room"))
            {
                var count = 0;
                do
                {
                    count++;

                    var r = new Room();
                    Rooms.Add(r);
                    r.ReadXml(reader);

                    //int x = int.Parse(reader.GetAttribute("X"));
                    //int y = int.Parse(reader.GetAttribute("Y"));
                    //string type = reader.GetAttribute("ObjectType");
                    //var furn = this.PlaceFurniture(type, this._tiles[x, y], false);
                    //furn.ReadXml(reader);
                } while (reader.ReadToNextSibling("Room"));
                Debug.LogFormat("Loaded {0} Rooms from save file.", count);
            }
            else
            {
                Debug.LogWarning("No Rooms found in save file!");
            }
        }

        private void ReadXml_Characters(XmlReader reader)
        {
            if (reader.ReadToDescendant("Character"))
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    var character = this.CreateCharacter(this._tiles[x, y]);
                    character.ReadXml(reader);
                } while (reader.ReadToNextSibling("Character"));
            }
        }

        public void OnInventoryCreated(Inventory inv)
        {
            if (_cbInventoryCreated != null) _cbInventoryCreated(inv);
        }

        public void OnFurnitureRemoved(Furniture furn)
        {
            Furnitures.Remove(furn);
        }

        public int GetRoomId(Room room)
        {
            return Rooms.IndexOf(room);
        }

        public Room GetRoomFromId(int id)
        {
            if (id < 0 || id > Rooms.Count - 1)
            {
                return null;
            }
            return Rooms[id];
        }
    }
}
