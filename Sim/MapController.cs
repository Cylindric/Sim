using System;
using System.Collections.Generic;
using OpenTK;

namespace Sim
{
    internal class MapController
    {
        private readonly SpritesheetController _sprites;
        private readonly int[] _tiles;
        private const int Width = 800/40;
        private const int Height = 600/40;

        public MapController(GraphicsController graphics)
        {
            _sprites = new SpritesheetController("grass", graphics);
            _tiles = new int[Width*Height];

            for (var row = 0; row < Height; row++)
            {
                for (var col = 0; col < Width; col++)
                {
                    if (row == 0 || row == Height - 1 || col == 0 || col == Width - 1)
                    {
                        _tiles[row*Width + col] = 6;

                    }
                    else
                    {
                        _tiles[row*Width + col] = 3;
                    }
                }
            }
        }

        public void Render(GraphicsController graphics)
        {
            for (var row = 0; row < Height; row++)
            {
                for (var col = 0; col < Width; col++)
                {
                    _sprites.Render(_tiles[row*Width + col],
                        new Vector2(col*_sprites.SpriteSize, row*_sprites.SpriteSize), graphics);
                }
            }
        }

        public bool CheckCollision(Vector4 hitbox)
        {
            try
            {
                // The only walkable tile is currently #3. Anything else is a wall
                var collision = false;
                foreach (var tile in TilesInHitbox(hitbox))
                {
                    if (_tiles[tile] != 3)
                    {
                        collision = true;
                        break;
                    }
                }
                return collision;
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Invalid location provided for collision detection!");
                return true;
            }
        }

        private int TileFromCoord(Vector2 location)
        {
            var col = (int) location.X/_sprites.SpriteSize;
            var row = (int) location.Y/_sprites.SpriteSize;
            return (row*Width) + col;
        }

        private IEnumerable<int> TilesInHitbox(Vector4 hitbox)
        {
            var left = (int) hitbox.X/_sprites.SpriteSize;
            var top = (int) hitbox.Y/_sprites.SpriteSize;
            var right = (int) (hitbox.X + hitbox.Z)/_sprites.SpriteSize;
            var bottom = (int) (hitbox.Y + hitbox.W)/_sprites.SpriteSize;

            var tiles = new List<int>();
            for (var row = top; row <= bottom; row++)
            {
                for (var col = left; col <= right; col++)
                {
                    if (row >= top && row <= bottom && col >= left && col <= right)
                    {
                        tiles.Add(row * Width + col);
                    }
                }
            }
            return tiles;
        }
    }
}