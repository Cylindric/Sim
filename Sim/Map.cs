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

        private Tile[,] _tiles2;
  
        //private int[] _tiles;

        private int _columns;
        private int _rows;

        private readonly List<GameObject> _particleList = new List<GameObject>();

        public Map(string filename, GraphicsController graphics) :
            base(graphics)
        {
            LoadFromFile(filename);
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
            foreach (var t in _tiles2)
            {
                _spritesheet.Render(t.SpriteNum, t.LocationPx, Graphics);

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
            if (hitbox.X < 0 || hitbox.X > _columns*_spritesheet.SpriteWidth)
            {
                return true;
            }
            if (hitbox.Y < 0 || hitbox.Y > _rows*_spritesheet.SpriteWidth)
            {
                return true;
            }

            try
            {
                // The only walkable tile is currently #3. Anything else is a wall
                var collision = false;
                foreach (var tile in TilesInHitbox(hitbox))
                {
                    //var tileRow = tile/_columns;
                    //var tileCol = tile%_columns;

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
                        //break;
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

        /// <summary>
        /// Check to determine if specified cell is walkable.
        /// </summary>
        /// <param name="t">The cell id to check.</param>
        /// <returns>true if cell is walkable; otherwise false.</returns>
        /// <remarks>An invalid passed in id will always return false.</remarks>
        //private bool IsWalkable(int t)
        //{
        //    if (t < 0) return false;
        //    if (t > _tiles.Length) return false;
        //    return _tiles[t] == 3;
        //}

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
            if (start.Column > 0 && _tiles2[start.Row, start.Column - 1].IsWalkable) tiles.Add(_tiles2[start.Row, start.Column - 1]); // left
            if (start.Column < _columns && _tiles2[start.Row, start.Column + 1].IsWalkable) tiles.Add(_tiles2[start.Row, start.Column + 1]); // right
            if (start.Row > 0 && _tiles2[start.Row - 1, start.Column].IsWalkable) tiles.Add(_tiles2[start.Row - 1, start.Column]); // up
            if (start.Row < _columns && _tiles2[start.Row + 1, start.Column].IsWalkable) tiles.Add(_tiles2[start.Row + 1, start.Column]); // down

            return tiles;
        }

        private IEnumerable<Tile> TilesInHitbox(Vector4 hitbox)
        {
            var left = (int) hitbox.X/_spritesheet.SpriteWidth;
            var top = (int) hitbox.Y/_spritesheet.SpriteHeight;
            var right = (int) (hitbox.X + hitbox.Z)/_spritesheet.SpriteWidth;
            var bottom = (int) (hitbox.Y + hitbox.W)/_spritesheet.SpriteHeight;

            var tiles = new List<Tile>();
            for (var row = top; row <= bottom; row++)
            {
                for (var col = left; col <= right; col++)
                {
                    if (row >= top && row <= bottom && col >= left && col <= right)
                    {
                        tiles.Add(_tiles2[row, col]);
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
            var tile = _tiles2[row, column];
            return _tiles2[row, column];
        }

        //public Vector4 GetTileDimFromId(int id)
        //{
        //    var column = id%_columns;
        //    var row = id/_columns;
        //    return new Vector4(column * Size.X, row * Size.Y, column * Size.X + Size.X, row * Size.Y + Size.Y);
        //}


        private void LoadFromFile(string filename)
        {
            var data = new MapDatafile();
            try
            {
                data.LoadFromFile(filename);
                LoadSpritesheet(data.Spritesheet);
                _columns = data.Width;
                _rows = data.Height;
                //_tiles = data.TileIds.ToArray();
                //_tiles2 = data.Tiles;
                MapSize = new Vector2(_columns * _spritesheet.SpriteWidth, _rows * _spritesheet.SpriteHeight);

                _tiles2 = new Tile[_rows, _columns];

                // Now that we've loaded the spritesheet, we can update the tiles with pixel data
                foreach (var t in data.Tiles)
                {
                    t.LocationPx = new Vector2(t.Column * _spritesheet.SpriteWidth, t.Row * _spritesheet.SpriteHeight);
                    t.SizePx = new Vector2(_spritesheet.SpriteWidth);
                    t.CentrePx = new Vector2(t.LocationPx.X + t.SizePx.X / 2, t.LocationPx.Y + t.SizePx.Y / 2);
                    t.IsWalkable = t.SpriteNum == 3;
                    _tiles2[t.Row, t.Column] = t;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load map.", e);
            }
        }
    }
}