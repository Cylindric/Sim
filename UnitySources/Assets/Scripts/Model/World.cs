using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Assets.Scripts.Pathfinding;
using UnityEngine;

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

            if (this._cbFurnitureCreated != null)
            {
                this._cbFurnitureCreated(furn);
            }

            this.InvalidateTileGraph();

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

            for (int x = 5; x < this.Width - 5; x++)
            {
                for (int y = 5; y < this.Height - 5; y++)
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

            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "Tiles":
                        this.ReadXml_Tiles(reader);
                        break;
                    case "Furnitures":
                        this.ReadXml_Furnitures(reader);
                        break;
                    case "Characters":
                        this.ReadXml_Characters(reader);
                        break;
                }
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
                    if (this._tiles[x, y].Type == defaultTile.Type)
                    {
                        continue;
                    }

                    //writer.WriteStartElement("Tile");
                    //writer.WriteAttributeString("X", x.ToString());
                    //writer.WriteAttributeString("Y", y.ToString());
                    this._tiles[x, y].WriteXml(writer);
                    //writer.WriteEndElement();
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

            this._furniturePrototypes.Add("Wall", new Furniture("Wall", 0f, 1, 1, true));
            this._furniturePrototypes.Add("Door", new Furniture("Door", 2f, 1, 1, false));

            this._furniturePrototypes["Door"].furnParameters["openness"] = 0.0f;
            this._furniturePrototypes["Door"].furnParameters["is_opening"] = 0.0f;
            this._furniturePrototypes["Door"].updateActions += FurnitureActions.Door_UpdateAction;
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
                }
            }

            Debug.Log("World (" + this.Width + "," + this.Height + ") created with " + (this.Width*this.Height) +
                      " tiles.");

            this.CreateFurniturePrototypes();
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
