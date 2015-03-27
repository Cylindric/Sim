using System;
using System.Collections.Generic;
using OpenTK;
using System.IO;
using Sim.DataFormats;
using System.Drawing;

namespace Sim
{
    public class Map : GameObject
    {
        public bool DebugShowHitbox { get; set; }

        private readonly Font _font;
        private int[] _tiles;
        private int _width = 800/40;
        private int _height = 600/40;

        private List<GameObject> _particleList = new List<GameObject>();

        public Map(string filename, GraphicsController graphics) : 
            base(graphics)
        {
            _font = new Font(Graphics);
            LoadFromFile(filename);
        }

        public override void Update(double timeDelta)
        {
            foreach(var p in _particleList)
            {
                p.Update(timeDelta);
            }
            _particleList.RemoveAll(p => p.IsDead);
        }

        public override void Render()
        {
            for (var row = 0; row < _height; row++)
            {
                for (var col = 0; col < _width; col++)
                {
                    _spritesheet.Render(_tiles[row*_width + col],
                        new Vector2(col*_spritesheet.SpriteWidth, row*_spritesheet.SpriteWidth), Graphics);

                    // render the hitbox
                    if (DebugShowHitbox)
                    {
                        Graphics.SetColour(Color.Pink);
                        Graphics.RenderRectangle(new Vector4(col * _spritesheet.SpriteWidth, row*_spritesheet.SpriteWidth, (col + 1) * _spritesheet.SpriteWidth, (row + 1)*_spritesheet.SpriteWidth));
                        Graphics.ClearColour();

                        _font.Position = new Vector2(col * _spritesheet.SpriteWidth, row * _spritesheet.SpriteWidth + 16);
                        _font.FontSize = 12;
                        _font.Text = string.Format("{0},{1}", col, row);
                        _font.Render();

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
            if (hitbox.X < 0 || hitbox.X > _width * _spritesheet.SpriteWidth)
            {
                return true;
            }
            if (hitbox.Y < 0 || hitbox.Y > _height * _spritesheet.SpriteWidth)
            {
                return true;
            }

            try
            {
                // The only walkable tile is currently #3. Anything else is a wall
                var collision = false;
                foreach (var tile in TilesInHitbox(hitbox))
                {
                    var tileRow = tile / _width;
                    var tileCol = tile % _width;

                    var p = new Rectangle(new Vector4(tileCol * Size.X, tileRow * Size.Y, _spritesheet.SpriteWidth, _spritesheet.SpriteHeight), Graphics);
                    p.Color = Color.ForestGreen;
                    p.TimeToLive = 0.5;
                    _particleList.Add(p);

                    if (_tiles[tile] != 3)
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

        private int TileFromCoord(Vector2 location)
        {
            var col = (int) location.X/_spritesheet.SpriteWidth;
            var row = (int)location.Y / _spritesheet.SpriteHeight;
            return (row*_width) + col;
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
                        tiles.Add(row * _width + col);
                    }
                }
            }
            return tiles;
        }

        private void LoadFromFile(string filename)
        {
            var data = new MapDatafile();
            try
            {
                data.LoadFromFile(filename);
                LoadSpritesheet(data.Spritesheet);
                _width = data.Width;
                _height = data.Height;
                _tiles = data.TileIds.ToArray();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load map.", e);
            }
        }
    }
}