using OpenTK;
using Sim.DataFormats;
using System;
using System.Collections.Generic;
using Sim.Primitives;

namespace Sim
{
    public class Map : GameObject
    {
        public bool DebugShowHitbox { get; set; }

        /// <summary>
        /// The size of the map, in pixels.
        /// </summary>
        public Vector2 MapSize { get; set; }

        private Tile[,] _tiles;
        public int Columns { get; private set; }
        public int Rows { get; private set; }

        private readonly List<GameObject> _particleList = new List<GameObject>();

        internal Tile[,] Tiles
        {
            get { return _tiles; }
        }

        public Map(string filename, GraphicsController graphics, bool fullFilename = false)
        {
            LoadFromFile(filename, graphics, fullFilename);
        }

        public override void Update(double timeDelta)
        {
            foreach (var p in _particleList)
            {
                p.Update(timeDelta);
            }
            _particleList.RemoveAll(p => p.IsDead);
        }

        public override void Render(GraphicsController graphics)
        {
            foreach (var t in _tiles)
            {
                Spritesheet.Render(t.SpriteNum, t.LocationPx, graphics);

                // render the hitbox
                if (DebugShowHitbox)
                {
                    foreach (var p in _particleList)
                    {
                        p.Render(graphics);
                    }
                }                
            }
        }

        /// <summary>
        /// Check if hitbox collides with unwalkable parts of the map.
        /// </summary>
        /// <param name="hitbox">The hitbox to check. (x,y,width,height)</param>
        /// <returns>Returns true on collision; else false.</returns>
        public bool CheckCollision(Vector4 hitbox)
        {
            if (hitbox.X < 0 || hitbox.X > Columns*Spritesheet.SpriteWidth)
            {
                return true;
            }
            if (hitbox.Y < 0 || hitbox.Y > Rows*Spritesheet.SpriteWidth)
            {
                return true;
            }

            try
            {
                // The only walkable tile is currently #3. Anything else is a wall
                var collision = false;
                foreach (var tile in TilesInHitbox(hitbox))
                {
                    if (!tile.IsWalkable)
                    {
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
            if (start.Column < Columns && _tiles[start.Row, start.Column + 1].IsWalkable) tiles.Add(_tiles[start.Row, start.Column + 1]); // right
            if (start.Row > 0 && _tiles[start.Row - 1, start.Column].IsWalkable) tiles.Add(_tiles[start.Row - 1, start.Column]); // up
            if (start.Row < Columns && _tiles[start.Row + 1, start.Column].IsWalkable) tiles.Add(_tiles[start.Row + 1, start.Column]); // down

            return tiles;
        }

        /// <summary>
        /// Return all tiles that intersect with the hitbox.
        /// </summary>
        /// <param name="hitbox">The hitbox to check. (x,y,width,height)</param>
        /// <returns>A List of the intersecting tiles.</returns>
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
            try
            {
                var column = (int)(p.X/Size.X);
                var row = (int)(p.Y/Size.Y);
                return _tiles[row, column];
            }
            catch (IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(String.Format("Invalid position specified. No tile exists at {0},{1}.", p.X, p.Y), e);
            }
        }

        private void LoadFromFile(string filename, GraphicsController graphics, bool fullFilename = false)
        {
            var data = new MapDatafile();
            try
            {
                data.LoadFromFile(filename, fullFilename);
                LoadSpritesheet(data.Spritesheet, graphics);
                Columns = data.Width;
                Rows = data.Height;
                MapSize = new Vector2(Columns * Spritesheet.SpriteWidth, Rows * Spritesheet.SpriteHeight);

                _tiles = new Tile[Rows, Columns];

                // Now that we've loaded the spritesheet, we can update the tiles with pixel data
                foreach (var t in data.Tiles)
                {
                    t.SizePx = new Vector2(Spritesheet.SpriteWidth);
                    t.MoveTile(t.Column, t.Row);
                    t.IsWalkable = t.SpriteNum == 3;
                    _tiles[t.Row, t.Column] = t;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load map.", e);
            }
        }

        #region Map Editing
        
        /// <summary>
        /// Change the Sprite ID of the tile at this location.
        /// </summary>
        /// <param name="p">The position.</param>
        /// <param name="sprite">The new sprite ID.</param>
        public void SetTileSprite(Vector2 p, int sprite)
        {
            var tile = GetTileAtPosition(p);
            tile.SpriteNum = sprite;
        }

        /// <summary>
        /// Create new rows of tiles.
        /// </summary>
        /// <param name="rows">The number of rows to add. If negative, adds to the top.</param>
        /// <param name="defaultSprite">The Sprite to use for the new tiles.</param>
        public void AddRows(int rows, int defaultSprite = 0)
        {
            var rowsToAdd = Math.Abs(rows);
            var newRowCount = Rows + rowsToAdd;

            var newtiles = new Tile[newRowCount, Columns];

            if (rows < 0)
            {
                // Add new rows first
                for (var row = 0; row < rowsToAdd; row++)
                {
                    for (var col = 0; col < Columns; col++)
                    {
                        newtiles[row, col] = new Tile(row, col, Spritesheet.SpriteWidth, Spritesheet.SpriteHeight)
                        {
                            SpriteNum = defaultSprite
                        };
                    }
                }
                // Add all the existing rows
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Columns; col++)
                    {
                        newtiles[row + rowsToAdd, col] = _tiles[row, col];
                        newtiles[row + rowsToAdd, col].MoveTile(col, row + rowsToAdd);
                    }
                }
            }
            else
            {
                // Add all the existing rows first
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Columns; col++)
                    {
                        newtiles[row, col] = _tiles[row, col];
                    }
                }
                // Add new rows
                for (var row = 0; row < rowsToAdd; row++)
                {
                    for (var col = 0; col < Columns; col++)
                    {
                        newtiles[Rows + row, col] = new Tile(Rows + row, col, Spritesheet.SpriteWidth, Spritesheet.SpriteHeight)
                        {
                            SpriteNum = defaultSprite
                        };
                    }
                }
            }

            Rows += rowsToAdd;
            MapSize = new Vector2(MapSize.X, Rows * Spritesheet.SpriteHeight);
            _tiles = newtiles;
        }

        /// <summary>
        /// Create new column of tiles.
        /// </summary>
        /// <param name="columns">The number of columns to add. If negative, adds to the left.</param>
        /// <param name="defaultSprite">The Sprite to use for the new tiles.</param>
        public void AddColumns(int columns, int defaultSprite = 0)
        {
            var columnsToAdd = Math.Abs(columns);
            var newColumnCount = Columns + columnsToAdd;

            var newtiles = new Tile[Rows, newColumnCount];

            if (columns < 0)
            {
                // Add new columns first
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < columnsToAdd; col++)
                    {
                        newtiles[row, col] = new Tile(row, col, Spritesheet.SpriteWidth, Spritesheet.SpriteHeight)
                        {
                            SpriteNum = defaultSprite
                        };
                    }
                }
                // Add all the existing columns
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Columns; col++)
                    {
                        newtiles[row, col + columnsToAdd] = _tiles[row, col];
                        newtiles[row, col + columnsToAdd].MoveTile(col + columnsToAdd, row);
                    }
                }
            }
            else
            {
                // Add all the existing columns first
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Columns; col++)
                    {
                        newtiles[row, col] = _tiles[row, col];
                    }
                }
                // Add new columns
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < columnsToAdd; col++)
                    {
                        newtiles[row, Columns + col] = new Tile(row, Columns + col, Spritesheet.SpriteWidth, Spritesheet.SpriteHeight)
                        {
                            SpriteNum = defaultSprite
                        };
                    }
                }
            }

            Columns += columnsToAdd;
            MapSize = new Vector2(Columns * Spritesheet.SpriteWidth, MapSize.Y);
            _tiles = newtiles;
        }

        #endregion
    }
}