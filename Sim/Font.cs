using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Sim
{
    public class Font
    {
        private readonly SpritesheetController _spritesheet;
        private readonly GraphicsController _graphics;
        private readonly float _baseScale;

        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public float Size { get; set; }
        public Color Colour { get; set; }

        public Font(GraphicsController graphics)
        {
            _graphics = graphics;
            _spritesheet = new SpritesheetController("font", graphics);
            Text = "";
            _baseScale = 1.0f / _spritesheet.SpriteHeight;
            Size = 10f;
        }

        public void Render()
        {
            var cursor = Position;
            foreach (var c in Text)
            {
                var sprite = Convert.ToInt32(c) - 48;
                _spritesheet.TintColour = Colour;
                _spritesheet.Render(sprite, cursor, new Vector2(Size * _baseScale), _graphics);
                cursor += new Vector2(_spritesheet.SpriteWidth, 0) * Size * _baseScale;
            }

        }
    }
}
