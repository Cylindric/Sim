using System;
using System.Drawing;
using OpenTK;
using Sim.DataFormats;
using System.Collections.Generic;

namespace Sim
{
    class SpritesheetController
    {
        protected int Texture;
        protected int TextureWidth;
        protected int TextureHeight;
        protected int TextureColumns;
        protected int TextureRows;

        private readonly float _spriteDx;
        private readonly float _spriteDy;
        private readonly float _spriteWidth;
        private readonly float _spriteHeight;

        public struct SpritesheetData
        {
            public string Filename;
            public int TextureId;
            public int Width;
            public int Height;
            public int Columns;
            public int Rows;
            public int SpriteSize;

            public float SpriteDx;
            public float SpriteDy;
            public float SpriteWidth;
            public float SpriteHeight;
        }

        public int SpriteSize { get; protected set; }

        private static readonly Dictionary<string, SpritesheetData> TextureIds = new Dictionary<string, SpritesheetData>();

        public SpritesheetController(string filename, GraphicsController graphics)
        {
            if (!SpritesheetController.TextureIds.ContainsKey(filename))
            {
                Console.WriteLine("Loading new Spritesheet {0}", filename);
                var spritesheetData = new SpritesheetData {Filename = filename};

                var data =
                    ResourceController.Load<SpritesheetDatafile>(
                        ResourceController.GetDataFilename("spritesheet.{0}.txt", filename));

                spritesheetData.SpriteSize = data.SpriteSize;

                var bitmap = new Bitmap(ResourceController.GetSpriteFilename(data.BitmapFile));

                spritesheetData.Width = bitmap.Width;
                spritesheetData.Height = bitmap.Height;
                spritesheetData.Columns = spritesheetData.Width / spritesheetData.SpriteSize;
                spritesheetData.Rows = spritesheetData.Height / spritesheetData.SpriteSize;

                spritesheetData.SpriteDx = 1.0f / spritesheetData.Columns;
                spritesheetData.SpriteDy = 1.0f / spritesheetData.Rows;
                spritesheetData.SpriteWidth = (1.0f / spritesheetData.Width) * spritesheetData.SpriteSize;
                spritesheetData.SpriteHeight = (1.0f / spritesheetData.Height) * spritesheetData.SpriteSize;

                spritesheetData.TextureId = graphics.LoadSpritesheet(bitmap);
                SpritesheetController.TextureIds.Add(filename, spritesheetData);
            }

            var cachedData = SpritesheetController.TextureIds[filename];
            SpriteSize = cachedData.SpriteSize;
            Texture = cachedData.TextureId;
            TextureWidth = cachedData.Width;
            TextureHeight = cachedData.Height;
            TextureColumns = cachedData.Columns;
            TextureRows = cachedData.Rows;

            _spriteDx = cachedData.SpriteDx;
            _spriteDy = cachedData.SpriteDy;
            _spriteWidth = cachedData.SpriteWidth;
            _spriteHeight = cachedData.SpriteHeight;
        }

        public void Render(int sprite, Vector2 position, GraphicsController graphics)
        {
            var spriteCol = sprite % TextureColumns;
            var spriteRow = sprite / TextureColumns;

            var top = spriteRow * _spriteDy;
            var left = spriteCol * _spriteDx;

            var verts = new Vector2[4] { new Vector2(0, 0) + position, new Vector2(SpriteSize, 0) + position, new Vector2(SpriteSize, SpriteSize) + position, new Vector2(0, SpriteSize) + position };

            var tex = new Vector2[4]
            {
                new Vector2(left, top), new Vector2(left + _spriteWidth, top),
                new Vector2(left + _spriteWidth, top + _spriteHeight), new Vector2(left, top + _spriteHeight)
            };

            graphics.BindTexture(Texture);
            graphics.RenderQuad(verts, tex);
            graphics.BindTexture(0);

        }

    }
}