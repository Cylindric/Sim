﻿using System;
using UnityEngine;

namespace Assets.Model
{
    public class Tile {

        public World World { get; private set; }

        private TileType type = TileType.Empty;

        //private Inventory _inventory;

        public Furniture Furniture { get; private set; }
        public Job PendingFurnitureJob { get; set; }

        private Action<Tile> cbTileChanged;

        public int X { get; private set; }
        public int Y { get; private set; }

        public TileType Type
        {
            get
            {
                return type;
            }

            set
            {
                var oldType = type;
                type = value;

                if (cbTileChanged != null && oldType != type)
                {
                    cbTileChanged(this);
                }
            }
        }

        public Tile(World world, int x, int y)
        {
            this.World = world;
            this.X = x;
            this.Y = y;
        }

        public void UnRegisterTileTypeChangedCallback(Action<Tile> callback)
        {
            cbTileChanged -= callback;
        }

        public void RegisterTileTypeChangedCallback(Action<Tile> callback)
        {
            cbTileChanged += callback;
        }

        public bool PlaceFurniture(Furniture objectInstance)
        {
            // If a null objectInstance is provided, clear the current object.
            if (objectInstance == null)
            {
                Furniture = null;
                return true;
            }

            if (Furniture != null)
            {
                Debug.LogError("Trying to assign a Furniture to a Tile that already has one.");
                return false;
            }

            Furniture = objectInstance;
            return true;
        }

        public bool IsNeighbour(Tile tile, bool allowDiagonal = false)
        {
            if (this.X == tile.X && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1))
            {
                return true;
            }

            if (this.Y == tile.Y && (this.X == tile.X + 1 || this.X == tile.X - 1))
            {
                return true;
            }

            if (allowDiagonal)
            {
                if (this.X == tile.X + 1 && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1))
                {
                    return true;
                }
                if (this.X == tile.X - 1 && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
