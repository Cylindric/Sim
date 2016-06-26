using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Model
{
    public class World
    {
        private readonly Tile[,] _tiles;
        private Dictionary<string, Furniture> _furniturePrototypes;
        private List<Character> _characters;

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

        public void CreateCharacter(Tile t)
        {
            var c = new Character(t);
            if (_cbCharacterCreated != null)
            {
                _cbCharacterCreated(c);
                
            }
        }

        private void CreateFurniturePrototypes()
        {
            _furniturePrototypes = new Dictionary<string, Furniture>();
            _furniturePrototypes.Add("Wall", Furniture.CreatePrototype("Wall", 0f, 1, 1, true));
        }

        public Tile GetTileAt(int x, int y)
        {
            if (x > Width || x < 0 || y > Height || y < 0)
            {
                //Debug.LogError("Tile (" + x + "," + y + ") is out of range");
                return null;
            }
            return _tiles[x, y];
        }

        public void PlaceInstalledObject(string objectType, Tile t)
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
        }

        public bool IsFurniturePlacementValid(string furnitureType, Tile t)
        {
            return _furniturePrototypes[furnitureType].IsValidPosition(t);
        }

        //public Furniture GetFurniturePrototype(string objectType)
        //{
        //    if (_furniturePrototypes.ContainsKey(objectType) == false)
        //    {
        //        Debug.LogErrorFormat("No furniture with type {0}", objectType);
        //        return null;
        //    }
        //    return _furniturePrototypes[objectType];
        //}
    }
}
