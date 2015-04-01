using System;
using System.Drawing;
using OpenTK;
using Sim.DataFormats;
using System.Collections.Generic;

namespace Sim
{
    public class SpritesheetController
    {
        protected int Texture;
        protected int TextureWidth;
        protected int TextureHeight;
        protected int TextureColumns;
        protected int TextureRows;
        
        private readonly float _spriteDx;
        private readonly float _spriteDy;

        public struct SpritesheetData
        {
            public string Filename;
            public int TextureId;
            public int Width;
            public int Height;
            public int Columns;
            public int Rows;
            public int SpriteWidth;
            public int SpriteHeight;

            public float SpriteDx;
            public float SpriteDy;
        }

        public string Filename { get; set; }
        public int SpriteWidth { get; protected set; }
        public int SpriteHeight { get; protected set; }
        public Color TintColour { get; set; }
        public int Count { get; private set; }
 
        private static readonly Dictionary<string, SpritesheetData> TextureIds = new Dictionary<string, SpritesheetData>();
        private static Shader _shader;
 
        public SpritesheetController(string filename, GraphicsController graphics)
        {
            Filename = filename;
            _shader = new Shader(ResourceController.LoadShader("sprite.frag.glsl"), Shader.Type.Fragment);
            TintColour = Color.FromArgb(0, 0, 0, 0);

            if (!SpritesheetController.TextureIds.ContainsKey(filename))
            {
                Console.WriteLine("Loading new Spritesheet {0}", filename);
                var spritesheetData = new SpritesheetData {Filename = filename};

                var data =
                    ResourceController.Load<SpritesheetDatafile>(
                        ResourceController.GetDataFilename("spritesheet.{0}.txt", Filename));

                spritesheetData.SpriteWidth = data.SpriteWidth;
                spritesheetData.SpriteHeight = data.SpriteHeight;

                var bitmap = new Bitmap(ResourceController.GetSpriteFilename(data.BitmapFile));

                spritesheetData.Width = bitmap.Width;
                spritesheetData.Height = bitmap.Height;
                spritesheetData.Columns = spritesheetData.Width / spritesheetData.SpriteWidth;
                spritesheetData.Rows = spritesheetData.Height / spritesheetData.SpriteHeight;

                spritesheetData.SpriteDx = 1.0f / spritesheetData.Columns;
                spritesheetData.SpriteDy = 1.0f / spritesheetData.Rows;

                spritesheetData.TextureId = graphics.LoadSpritesheet(bitmap);
                SpritesheetController.TextureIds.Add(filename, spritesheetData);
            }

            var cachedData = SpritesheetController.TextureIds[filename];
            SpriteWidth = cachedData.SpriteWidth;
            SpriteHeight = cachedData.SpriteHeight;
            Texture = cachedData.TextureId;
            TextureWidth = cachedData.Width;
            TextureHeight = cachedData.Height;
            TextureColumns = cachedData.Columns;
            TextureRows = cachedData.Rows;
            Count = cachedData.Rows*cachedData.Columns;

            _spriteDx = cachedData.SpriteDx;
            _spriteDy = cachedData.SpriteDy;
        }

        public void Render(int sprite, Vector2 position, GraphicsController graphics)
        {
            Render(sprite, position, new Vector2(1), graphics);
        }

        public void Render(int sprite, Vector2 position, Vector2 scale, GraphicsController graphics)
        {
            var spriteCol = sprite % TextureColumns;
            var spriteRow = sprite / TextureColumns;

            var textureTop = spriteRow * _spriteDy;
            var textureLeft = spriteCol * _spriteDx;

            var vertexLeft = position.X;
            var vertexTop = position.Y;
            var vertexRight = position.X + (SpriteWidth * scale.X);
            var vertexBottom = position.Y + (SpriteHeight* scale.Y);

            var verts = new Vector2[4]
            {
                new Vector2(vertexLeft, vertexTop), new Vector2(vertexRight, vertexTop), 
                new Vector2(vertexRight, vertexBottom), new Vector2(vertexLeft, vertexBottom)
            };

            var tex = new Vector2[4]
            {
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(1, 1), new Vector2(0, 1)
            };

            _shader.SetVariable("TextureSize", _spriteDx, _spriteDy);
            _shader.SetVariable("TextureOffset", textureLeft, textureTop);
            _shader.SetVariable("Colour", TintColour);

            graphics.BindTexture(Texture);
            Shader.Bind(_shader);
            graphics.RenderQuad(verts, tex);
            graphics.BindTexture(0);
            Shader.Bind(null);
        }
    }
}