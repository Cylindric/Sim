using System;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class World
    {
        private readonly Tile[,] _tiles;
        private Dictionary<string, Furniture> _furniturePrototypes;
        private List<Character> _characters;
        private PathTileGraph _tileGraph;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private Action<Furniture> _cbFurnitureCreated;
        private Action<Character> _cbCharacterCreated;
        private Action<Tile> _cbTileChanged;

        public JobQueue JobQueue;

        public World(int width, int height)
        {
            JobQueue = new JobQueue();

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
            Debug.Log("World (" + this.Width + "," + this.Height + ") created with " + (this.Width*this.Height) + " tiles.");

            CreateFurniturePrototypes();

            _characters = new List<Character>();
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

        public Tile GetTileAt(int x, int y)
        {
            if (x >= Width || x < 0 || y >= Height || y < 0)
            {
                return null;
            }

            return _tiles[x, y];
        }

        public void PlaceFurniture(string objectType, Tile t)
        {
            if (_furniturePrototypes.ContainsKey(objectType) == false)
            {
                Debug.LogErrorFormat("Tried to place an object [{0}] for which we don't have a prototype.", objectType);
                return;
            }

            var obj = Furniture.PlaceInstance(_furniturePrototypes[objectType], t);

            if (obj == null)
            {
                // Failed to place object! Maybe something was already there.
                return;
            }

            if (_cbFurnitureCreated != null)
            {
                _cbFurnitureCreated(obj);
            }
            InvalidateTileGraph();
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
            _cbTileChanged(t);
            InvalidateTileGraph();
        }

        /// <summary>
        /// Invalidates the current TileGraph.
        /// </summary>
        /// <remarks>
        /// Should be called whenever anything changes that affects the pathing.</remarks>
        public void InvalidateTileGraph()
        {
            _tileGraph = null;
        }

        public bool IsFurniturePlacementValid(string furnitureType, Tile t)
        {
            return _furniturePrototypes[furnitureType].IsValidPosition(t);
        }

        public void SetupPathfindingTestMap()
        {
            var hMid = this.Width/2;
            var vMid = this.Height/2;

            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
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
    }
}
