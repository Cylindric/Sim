using OpenTK;
using Sim.DataFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using Rectangle = Sim.Primitives.Rectangle;

namespace Sim
{
    public class Map : GameObject
    {
        public class Tile
        {
            public Vector2 LocationPx;
            public Vector2 CentrePx;
            public Vector2 SizePx;
            public int Row;
            public int Column;
            public bool IsWalkable;
            public int SpriteNum;
        }

        public bool DebugShowHitbox { get; set; }

        /// <summary>
        /// The size of the map, in pixels.
        /// </summary>
        public Vector2 MapSize { get; set; }

        private Tile[,] _tiles;
        private int _columns;
        private int _rows;

        private readonly List<GameObject> _particleList = new List<GameObject>();

        public Map(string filename, GraphicsController graphics, bool fullFilename = false) :
            base(graphics)
        {
            LoadFromFile(filename, fullFilename);
        }

        public override void Update(double timeDelta)
        {
            foreach (var p in _particleList)
            {
                p.Update(timeDelta);
            }
            _particleList.RemoveAll(p => p.IsDead);
        }

        public override void Render()
        {
            foreach (var t in _tiles)
            {
                Spritesheet.Render(t.SpriteNum, t.LocationPx, Graphics);

                // render the hitbox
                if (DebugShowHitbox)
                {
                    foreach (var p in _particleList)
                    {
                        p.Render();
                    }
                }                
            }
        }

        public bool CheckCollision(Vector4 hitbox)
        {
            if (hitbox.X < 0 || hitbox.X > _columns*Spritesheet.SpriteWidth)
            {
                return true;
            }
            if (hitbox.Y < 0 || hitbox.Y > _rows*Spritesheet.SpriteWidth)
            {
                return true;
            }

            try
            {
                // The only walkable tile is currently #3. Anything else is a wall
                var collision = false;
                foreach (var tile in TilesInHitbox(hitbox))
                {
                    var p =
                        new Rectangle(tile.LocationPx, tile.SizePx, Graphics)
                        {
                            Color = Color.ForestGreen,
                            TimeToLive = 0.1
                        };
                    _particleList.Add(p);

                    if (tile.IsWalkable)
                    {
                        p.Color = Color.Red;
                        collision = true;
                    }
                }
                return collision;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Invalid location provided for collision detection!");
                return true;
            }
        }

        public int ManhattanDistance(Tile t1, Tile t2)
        {
            return Math.Abs(t2.Column - t1.Column) + Math.Abs(t2.Row - t1.Row);
        }

        public IEnumerable<Tile> ReachableTiles(Tile start)
        {
            var tiles = new List<Tile>();

            // If the start tile isn't walkable, nothing is.
            if (!start.IsWalkable)
            {
                return tiles;
            }

            // Check the four adjacent tiles
            if (start.Column > 0 && _tiles[start.Row, start.Column - 1].IsWalkable) tiles.Add(_tiles[start.Row, start.Column - 1]); // left
            if (start.Column < _columns && _tiles[start.Row, start.Column + 1].IsWalkable) tiles.Add(_tiles[start.Row, start.Column + 1]); // right
            if (start.Row > 0 && _tiles[start.Row - 1, start.Column].IsWalkable) tiles.Add(_tiles[start.Row - 1, start.Column]); // up
            if (start.Row < _columns && _tiles[start.Row + 1, start.Column].IsWalkable) tiles.Add(_tiles[start.Row + 1, start.Column]); // down

            return tiles;
        }

        private IEnumerable<Tile> TilesInHitbox(Vector4 hitbox)
        {
            var left = (int) hitbox.X/Spritesheet.SpriteWidth;
            var top = (int) hitbox.Y/Spritesheet.SpriteHeight;
            var right = (int) (hitbox.X + hitbox.Z)/Spritesheet.SpriteWidth;
            var bottom = (int) (hitbox.Y + hitbox.W)/Spritesheet.SpriteHeight;

            var tiles = new List<Tile>();
            for (var row = top; row <= bottom; row++)
            {
                for (var col = left; col <= right; col++)
                {
                    if (row >= top && row <= bottom && col >= left && col <= right)
                    {
                        tiles.Add(_tiles[row, col]);
                    }
                }
            }
            return tiles;
        }

        public Tile GetTileAtPosition(Vector2 p)
        {
            var column = (int)(p.X/Size.X);
            var row = (int)(p.Y/Size.Y);
            var cell = (row*_columns) + column;
            var tile = _tiles[row, column];
            return _tiles[row, column];
        }

        private void LoadFromFile(string filename, bool fullFilename = false)
        {
            var data = new MapDatafile();
            try
            {
                data.LoadFromFile(filename, fullFilename);
                LoadSpritesheet(data.Spritesheet);
                _columns = data.Width;
                _rows = data.Height;
                MapSize = new Vector2(_columns * Spritesheet.SpriteWidth, _rows * Spritesheet.SpriteHeight);

                _tiles = new Tile[_rows, _columns];

                // Now that we've loaded the spritesheet, we can update the tiles with pixel data
                foreach (var t in data.Tiles)
                {
                    t.LocationPx = new Vector2(t.Column * Spritesheet.SpriteWidth, t.Row * Spritesheet.SpriteHeight);
                    t.SizePx = new Vector2(Spritesheet.SpriteWidth);
                    t.CentrePx = new Vector2(t.LocationPx.X + t.SizePx.X / 2, t.LocationPx.Y + t.SizePx.Y / 2);
                    t.IsWalkable = t.SpriteNum == 3;
                    _tiles[t.Row, t.Column] = t;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load map.", e);
            }
        }

    }
}