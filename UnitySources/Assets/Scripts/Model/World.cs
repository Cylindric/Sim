using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Assets.Scripts.Pathfinding;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    public class World : IXmlSerializable
    {
        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */
        public JobQueue JobQueue;
        public List<Character> _characters;
        public List<Furniture> _furnitures;
        public List<Room> _rooms;
        public InventoryManager InventoryManager;

        private Tile[,] _tiles;
        private Dictionary<string, Furniture> _furniturePrototypes;

        /* #################################################################### */
        /* #                        CONSTRUCTORS                              # */
        /* #################################################################### */

        public World()
        {
            this.JobQueue = new JobQueue();
            this._characters = new List<Character>();
            this._furnitures = new List<Furniture>();
            this.InventoryManager = new InventoryManager();

            this._rooms = new List<Room>();
            this._rooms.Add(new Room()); // Add the default 'outside' room.
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
        public Path_TileGraph TileGraph { get; set; } // TODO: this PathTileGraph really shouldn't be fully public like this.
        public int Width { get; private set; }
        public int Height { get; private set; }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        public void Update(float deltaTime)
        {
            foreach (var c in this._characters)
            {
                c.Update(deltaTime);
            }

            foreach (var f in this._furnitures)
            {
                f.Update(deltaTime);
            }
        }

        public Character CreateCharacter(Tile t)
        {
            var c = new Character(t);
            this._characters.Add(c);

            if (this._cbCharacterCreated != null)
            {
                this._cbCharacterCreated(c);
            }

            return c;
        }

        public Room GetOutsideRoom()
        {
            return _rooms[0];
        }

        public void DeleteRoom(Room r)
        {
            if (r == GetOutsideRoom())
            {
                Debug.LogError("Tried to delete the outside room!");
                return;
            }

            // Remove the current room from the list of rooms.
            _rooms.Remove(r);

            // Make sure no tiles point to this room.
            // TODO: This probably isn't necessary, as the flood-fill will assign all these tiles to new rooms.
            r.UnassignAllTiles();
        }

        public void AddRoom(Room r)
        {
            _rooms.Add(r);
        }

        public Tile GetTileAt(int x, int y)
        {
            if (x >= this.Width || x < 0 || y >= this.Height || y < 0)
            {
                return null;
            }

            return this._tiles[x, y];
        }

        public Furniture PlaceFurniture(string objectType, Tile t)
        {
            if (this._furniturePrototypes.ContainsKey(objectType) == false)
            {
                Debug.LogErrorFormat("Tried to place an object [{0}] for which we don't have a prototype.", objectType);
                return null;
            }

            var furn = Furniture.PlaceInstance(this._furniturePrototypes[objectType], t);

            if (furn == null)
            {
                // Failed to place object! Maybe something was already there.
                return null;
            }

            this._furnitures.Add(furn);

            // Recalculate rooms?
            if (furn.IsRoomEnclosure)
            {
                Room.DoRoomFloodfill(furn);
            }

            if (this._cbFurnitureCreated != null)
            {
                this._cbFurnitureCreated(furn);
                
                if (Mathf.Approximately(furn.MovementCost, 1f) == false)
                {
                    // Tiles with a movement cost of exactly 1, don't affect the path-finding for their tile.
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
            return this._furniturePrototypes[furnitureType].IsValidPosition(t);
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
                            this.PlaceFurniture("Wall", this._tiles[x, y]);
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
                    case "Characters":
                        timer.Start();
                        this.ReadXml_Characters(reader);
                        Debug.LogFormat("Loading Characters took {0} ms.", timer.ElapsedMilliseconds);
                        timer.Stop();
                        timer.Reset();
                        break;
                }
            }

             // TODO: This is for testing only - remove it!
            var inv = new Inventory();
            inv.stackSize = 10;
            var t = GetTileAt(Width/2, Height/2);
            InventoryManager.PlaceInventory(t, inv);
            if (_cbInventoryCreated != null)
            {
                _cbInventoryCreated(t.inventory);
            }

            inv = new Inventory();
            inv.stackSize = 18;
            t = GetTileAt(Width / 2 + 2, Height / 2);
            InventoryManager.PlaceInventory(t, inv);
            if (_cbInventoryCreated != null)
            {
                _cbInventoryCreated(t.inventory);
            }

            inv = new Inventory();
            inv.stackSize = 14;
            t = GetTileAt(Width / 2 + 1, Height / 2 + 2);
            InventoryManager.PlaceInventory(t, inv);
            if (_cbInventoryCreated != null)
            {
                _cbInventoryCreated(t.inventory);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Width", this.Width.ToString());
            writer.WriteAttributeString("Height", this.Height.ToString());

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
            foreach (var furn in this._furnitures)
            {
                furn.WriteXml(writer);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("Characters");
            foreach (var character in this._characters)
            {
                character.WriteXml(writer);
            }

            writer.WriteEndElement();
        }

        private void CreateFurniturePrototypes()
        {
            this._furniturePrototypes = new Dictionary<string, Furniture>();

            this._furniturePrototypes.Add("Wall", new Furniture("Wall", 0f, 1, 1, true, true));
            this._furniturePrototypes.Add("Door", new Furniture("Door", 2f, 1, 1, false, true));

            this._furniturePrototypes["Door"].SetParameter("openness", 0.0f);
            this._furniturePrototypes["Door"].SetParameter("is_opening", 0.0f);
            this._furniturePrototypes["Door"].RegisterUpdateAction(FurnitureActions.Door_UpdateAction);
            this._furniturePrototypes["Door"].IsEntereable = FurnitureActions.Door_IsEnterable;
        }

        private void SetupWorld(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            this._tiles = new Tile[width, height];

            for (var x = 0; x < this.Width; x++)
            {
                for (var y = 0; y < this.Height; y++)
                {
                    this._tiles[x, y] = new Tile(this, x, y);
                    this._tiles[x, y].RegisterTileTypeChangedCallback(this.OnTileChanged);
                    this._tiles[x, y].Room = _rooms[0];
                }
            }

            Debug.Log("World (" + this.Width + "," + this.Height + ") created with " + (this.Width*this.Height) + " tiles.");

            this.CreateFurniturePrototypes();

            this._characters = new List<Character>();
            this._furnitures = new List<Furniture>();
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
            if (reader.ReadToDescendant("Tile"))
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    this._tiles[x, y].ReadXml(reader);

                } while (reader.ReadToNextSibling("Tile"));
            }
        }

        private void ReadXml_Furnitures(XmlReader reader)
        {
            if (reader.ReadToDescendant("Furniture"))
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    var furn = this.PlaceFurniture(reader.GetAttribute("objectType"), this._tiles[x, y]);
                    furn.ReadXml(reader);
                } while (reader.ReadToNextSibling("Furniture"));
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

    }
}
