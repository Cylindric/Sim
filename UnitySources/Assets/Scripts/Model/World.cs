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
        private Tile[,] _tiles;
        private Dictionary<string, Furniture> _furniturePrototypes;
        public List<Character> _characters;
        public List<Furniture> _furnitures;

        public Path_TileGraph TileGraph { get; set; } // TODO: this PathTileGraph really shouldn't be fully public like this.
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Action<Furniture> _cbFurnitureCreated;
        private Action<Character> _cbCharacterCreated;
        private Action<Tile> _cbTileChanged;

        public JobQueue JobQueue;

        public World()
        {
            JobQueue = new JobQueue();
            _characters = new List<Character>();
            _furnitures = new List<Furniture>();
        }

        public World(int width, int height) : this()
        {
            SetupWorld(width, height);
        }

        public void Update(float deltaTime)
        {
            //Debug.Log("Time: " + Time.deltaTime);

            foreach (var c in _characters)
            {
                c.Update(deltaTime);
            }
        }

        public Character CreateCharacter(Tile t)
        {
            var c = new Character(t);
            _characters.Add(c);

            if (_cbCharacterCreated != null)
            {
                _cbCharacterCreated(c);
            }

            return c;
        }

        private void CreateFurniturePrototypes()
        {
            _furniturePrototypes = new Dictionary<string, Furniture>();
            _furniturePrototypes.Add("Wall", Furniture.CreatePrototype("Wall", 0f, 1, 1, true));
        }

        private void SetupWorld(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            _tiles = new Tile[width, height];

            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    _tiles[x, y] = new Tile(this, x, y);
                    _tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
                }
            }
            Debug.Log("World (" + this.Width + "," + this.Height + ") created with " + (this.Width * this.Height) + " tiles.");

            CreateFurniturePrototypes();
        }

        public Tile GetTileAt(int x, int y)
        {
            if (x >= Width || x < 0 || y >= Height || y < 0)
            {
                return null;
            }

            return _tiles[x, y];
        }

        public Furniture PlaceFurniture(string objectType, Tile t)
        {
            if (_furniturePrototypes.ContainsKey(objectType) == false)
            {
                Debug.LogErrorFormat("Tried to place an object [{0}] for which we don't have a prototype.", objectType);
                return null;
            }

            var furn = Furniture.PlaceInstance(_furniturePrototypes[objectType], t);

            if (furn == null)
            {
                // Failed to place object! Maybe something was already there.
                return null;
            }

            _furnitures.Add(furn);

            if (_cbFurnitureCreated != null)
            {
                _cbFurnitureCreated(furn);
            }
            InvalidateTileGraph();

            return furn;
        }

        public void RegisterFurnitureCreatedCb(Action<Furniture> cb)
        {
            _cbFurnitureCreated += cb;
        }

        public void UnRegisterFurnitureCreatedCb(Action<Furniture> cb)
        {
            _cbFurnitureCreated -= cb;
        }

        public void RegisterCharacterCreatedCb(Action<Character> cb)
        {
            _cbCharacterCreated += cb;
        }

        public void UnRegisterCharacterCreatedCb(Action<Character> cb)
        {
            _cbCharacterCreated -= cb;
        }

        public void RegisterTileChanged(Action<Tile> cb)
        {
            _cbTileChanged += cb;
        }

        public void UnRegisterTileChanged(Action<Tile> cb)
        {
            _cbTileChanged -= cb;
        }

        private void OnTileChanged(Tile t)
        {
            if (_cbTileChanged != null)
            {
                _cbTileChanged(t);
            }
            InvalidateTileGraph();
        }

        /// <summary>
        /// Invalidates the current TileGraph.
        /// </summary>
        /// <remarks>
        /// Should be called whenever anything changes that affects the pathing.</remarks>
        public void InvalidateTileGraph()
        {
            TileGraph = null;
        }

        public bool IsFurniturePlacementValid(string furnitureType, Tile t)
        {
            return _furniturePrototypes[furnitureType].IsValidPosition(t);
        }

        public void SetupPathfindingTestMap()
        {
            var hMid = this.Width/2;
            var vMid = this.Height/2;

            for (int x = 5; x < this.Width - 5; x++)
            {
                for (int y = 5; y < this.Height - 5; y++)
                {
                    _tiles[x, y].Type = TileType.Floor;

                    // Place some walls
                    if ( (x == hMid - 3 || x == hMid + 3) || (y == vMid - 3 || y == vMid + 3))
                    {
                        if (x == hMid || y == vMid)
                        {

                        }
                        else
                        {
                            PlaceFurniture("Wall", _tiles[x, y]);
                        }
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////
        /// 
        ///                    LOADING / SAVING
        /// 
        ///////////////////////////////////////////////////////

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Width = int.Parse(reader.GetAttribute("Width"));
            Height = int.Parse(reader.GetAttribute("Height"));

            SetupWorld(Width, Height);

            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "Tiles":
                        ReadXml_Tiles(reader);
                        break;
                    case "Furnitures":
                        ReadXml_Furnitures(reader);
                        break;
                    case "Characters":
                        ReadXml_Characters(reader);
                        break;
                }
            }
        }

        private void ReadXml_Tiles(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name != "Tile")
                {
                    return;
                }

                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                _tiles[x, y].ReadXml(reader);
            }
        }

        private void ReadXml_Furnitures(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name != "Furniture")
                {
                    return;
                }

                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                var furn = PlaceFurniture(reader.GetAttribute("objectType"), _tiles[x, y]);
                furn.ReadXml(reader);
            }
        }

        private void ReadXml_Characters(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name != "Character")
                {
                    return;
                }

                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                var character = CreateCharacter(_tiles[x, y]);
                character.ReadXml(reader);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Width", Width.ToString());
            writer.WriteAttributeString("Height", Height.ToString());

            writer.WriteStartElement("Tiles");

            // Get a copy of a default Tile - we won't bother writing those out.
            var defaultTile = new Tile();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_tiles[x, y].Type == defaultTile.Type)
                    {
                        continue;
                    }
                    //writer.WriteStartElement("Tile");
                    //writer.WriteAttributeString("X", x.ToString());
                    //writer.WriteAttributeString("Y", y.ToString());
                    _tiles[x, y].WriteXml(writer);
                    //writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Furnitures");
            foreach (var furn in _furnitures)
            {
                furn.WriteXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Characters");
            foreach (var character in _characters)
            {
                character.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
    }
}
