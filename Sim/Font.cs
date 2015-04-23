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
        public float LineHeight { get; set; }

        public Font(GraphicsController graphics)
        {
            LoadSpritesheet("font", graphics);
            Text = "";
            _baseScale = 1.0f / Spritesheet.SpriteHeight;
            FontSize = 10f;
            LineHeight = 0.5f;
        }

        public override void Update(double timeDelta)
        {
        }

        public override void Render(GraphicsController graphics)
        {
            var cursor = Position;
            Spritesheet.TintColour = Colour;
            foreach (var line in Text.Split('\n'))
            {
                foreach (var c in line)
                {
                    var sprite = Convert.ToInt32(c) - Convert.ToInt32('!');
                    if (sprite == ' ')
                    {
                        // A space simply advances the cursor, doesn't draw anything

                    } else if (sprite >= 0)
                    {
                        Spritesheet.Render(sprite, cursor, new Vector2(FontSize*_baseScale), graphics);
                    }

                    // Advance the cursor one character
                    cursor += new Vector2(Spritesheet.SpriteWidth, 0)*FontSize*_baseScale;

                }

                // At the end of the line, move the cursor back to the beginning 
                // of the line and move down one line.
                var lineHeight = FontSize * LineHeight;
                cursor = Vector2.Add(Position, new Vector2(0, cursor.Y + lineHeight));
            }
        }
    }
}
