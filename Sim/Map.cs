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
        public bool DebugShowHitbox { get; set; }

        /// <summary>
        /// The size of the map, in pixels.
        /// </summary>
        public Vector2 MapSize { get; set; }

        private int[] _tiles;
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
            for (var row = 0; row < _rows; row++)
            {
                for (var col = 0; col < _columns; col++)
                {
                    _spritesheet.Render(_tiles[row*_columns + col],
                        new Vector2(col*_spritesheet.SpriteWidth, row*_spritesheet.SpriteWidth), Graphics);

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
                    var tileRow = tile/_columns;
                    var tileCol = tile%_columns;

                    var p =
                        new Rectangle(
                            new Vector4(tileCol*Size.X, tileRow*Size.Y, _spritesheet.SpriteWidth,
                                _spritesheet.SpriteHeight), Graphics)
                        {
                            Color = Color.ForestGreen,
                            TimeToLive = 0.1
                        };
                    _particleList.Add(p);

                    if (IsWalkable(tile))
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
        private bool IsWalkable(int t)
        {
            if (t < 0) return false;
            if (t > _tiles.Length) return false;
            return _tiles[t] == 3;
        }

        public int ManhattanDistance(int t1, int t2)
        {
            var c1 = t1 % _columns;
            var r1 = t1 / _columns;

            var c2 = t2 % _columns;
            var r2 = t2 / _columns;

            return Math.Abs(c2 - c1) + Math.Abs(r2 - r1);
        }

        public IEnumerable<int> ReachableTiles(Vector2 start)
        {
            return ReachableTiles(GetTileIdFromPosition(start));
        }

        public IEnumerable<int> ReachableTiles(int start)
        {
            var tiles = new List<int>();

            // If the start tile isn't walkable, nothing is.
            if (!IsWalkable(start))
            {
                return tiles;
            }

            // Check the four adjacent tiles
            if (IsWalkable(start - 1)) tiles.Add(start - 1); // left
            if (IsWalkable(start + 1)) tiles.Add(start + 1); // right
            if (IsWalkable(start - _columns)) tiles.Add(start - _columns); // up
            if (IsWalkable(start + _columns)) tiles.Add(start + _columns); // down

            return tiles;
        }

        private IEnumerable<int> TilesInHitbox(Vector4 hitbox)
        {
            var left = (int) hitbox.X/_spritesheet.SpriteWidth;
            var top = (int) hitbox.Y/_spritesheet.SpriteHeight;
            var right = (int) (hitbox.X + hitbox.Z)/_spritesheet.SpriteWidth;
            var bottom = (int) (hitbox.Y + hitbox.W)/_spritesheet.SpriteHeight;

            var tiles = new List<int>();
            for (var row = top; row <= bottom; row++)
            {
                for (var col = left; col <= right; col++)
                {
                    if (row >= top && row <= bottom && col >= left && col <= right)
                    {
                        tiles.Add(row*_columns + col);
                    }
                }
            }
            return tiles;
        }

        public int GetTileIdFromPosition(Vector2 p)
        {
            var column = (int)(p.X/Size.X);
            var row = (int)(p.Y/Size.Y);
            var cell = (row*_columns) + column;
            return cell;
        }

        public Vector4 GetTileDimFromId(int id)
        {
            var column = id%_columns;
            var row = id/_columns;
            return new Vector4(column * Size.X, row * Size.Y, column * Size.X + Size.X, row * Size.Y + Size.Y);
        }


        private void LoadFromFile(string filename)
        {
            var data = new MapDatafile();
            try
            {
                data.LoadFromFile(filename);
                LoadSpritesheet(data.Spritesheet);
                _columns = data.Width;
                _rows = data.Height;
                _tiles = data.TileIds.ToArray();
                MapSize = new Vector2(_columns * _spritesheet.SpriteWidth, _rows * _spritesheet.SpriteHeight);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load map.", e);
            }
        }
    }
}