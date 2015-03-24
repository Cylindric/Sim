using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Sim
{
    class MapController
    {
        private SpritesheetController _sprites;
        private int[] _tiles;
        private const int Width = 10;
        private const int Height = 10;

        public MapController(GraphicsController graphics)
        {
            //_sprites = new SpritesheetController(graphics);
            _tiles = new int[Width*Height];

            for (var row = 0; row < Height; row++)
            {
                for (var col= 0; col < Width; col++)
                {
                    _tiles[row*Width + col] = 0;
                }                
            }
        }

        public void Render(GraphicsController graphics)
        {
            for (var row = 0; row < Height; row++)
            {
                for (var col = 0; col < Width; col++)
                {
                //    _sprites.Render(_tiles[row*Width + col], new Vector2(col * _sprites.SpriteSize, row * _sprites.SpriteSize), graphics);
                }
            }            
        }
    }
}
