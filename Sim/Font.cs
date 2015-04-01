using System;
using System.Drawing;
using OpenTK;

namespace Sim
{
    public class Font : GameObject
    {
        private readonly float _baseScale;

        public string Text { get; set; }
        public float FontSize { get; set; }
        public Color Colour { get; set; }

        public Font(GraphicsController graphics) : base(graphics)
        {
            LoadSpritesheet("font");
            Text = "";
            _baseScale = 1.0f / Spritesheet.SpriteHeight;
            FontSize = 10f;
        }

        public override void Update(double timeDelta)
        {
        }

        public override void Render()
        {
            var cursor = Position;
            foreach (var c in Text)
            {
                var sprite = Convert.ToInt32(c) - 38;
                Spritesheet.TintColour = Colour;
                Spritesheet.Render(sprite, cursor, new Vector2(FontSize * _baseScale), Graphics);
                cursor += new Vector2(Spritesheet.SpriteWidth, 0) * FontSize * _baseScale;
            }

        }
    }
}
