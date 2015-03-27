using System;
using System.Collections.Generic;
using OpenTK;

namespace Sim
{
    class Map : GameObject
    {
        public bool DebugShowHitbox { get; set; }

        private readonly SpritesheetController _sprites;
        private readonly Font _font;
        private readonly int[] _tiles;
        private const int Width = 800/40;
        private const int Height = 600/40;

        public Map(GraphicsController graphics) : 
            base(graphics)
        {
            _font = new Font(Graphics);
            _sprites = new SpritesheetController("grass", Graphics);
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

        public override void Update(float timeDelta)
        {
        }

        public override void Render()
        {
            for (var row = 0; row < Height; row++)
            {
                for (var col = 0; col < Width; col++)
                {
                    _sprites.Render(_tiles[row*Width + col],
                        new Vector2(col*_sprites.SpriteWidth, row*_sprites.SpriteWidth), Graphics);

                    // render the hitbox
                    if (DebugShowHitbox)
                    {
                        Graphics.SetColour(new Vector4(1, 0, 0, 0.5f));
                        Graphics.RenderRectangle(new Vector4(col * _sprites.SpriteWidth, row*_sprites.SpriteWidth, (col + 1) * _sprites.SpriteWidth, (row + 1)*_sprites.SpriteWidth));
                        Graphics.ClearColour();

                        _font.Position = new Vector2(col * _sprites.SpriteWidth, row * _sprites.SpriteWidth + 16);
                        _font.Size = 12;
                        _font.Text = string.Format("{0},{1}", col, row);
                        _font.Render();
                    }
                }
            }
        }

        public bool CheckCollision(Vector4 hitbox)
        {
            if (hitbox.X < 0 || hitbox.X > Width * _sprites.SpriteWidth)
            {
                return true;
            }
            if (hitbox.Y < 0 || hitbox.Y > Height * _sprites.SpriteWidth)
            {
                return true;
            }

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
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Invalid location provided for collision detection!");
                return true;
            }
        }

        private int TileFromCoord(Vector2 location)
        {
            var col = (int) location.X/_sprites.SpriteWidth;
            var row = (int) location.Y/_sprites.SpriteWidth;
            return (row*Width) + col;
        }

        private IEnumerable<int> TilesInHitbox(Vector4 hitbox)
        {
            var left = (int) hitbox.X/_sprites.SpriteWidth;
            var top = (int) hitbox.Y/_sprites.SpriteWidth;
            var right = (int) (hitbox.X + hitbox.Z)/_sprites.SpriteWidth;
            var bottom = (int) (hitbox.Y + hitbox.W)/_sprites.SpriteWidth;

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