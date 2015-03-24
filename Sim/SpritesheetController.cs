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

        private float _spriteDx;
        private float _spriteDy;
        private float _spriteWidth;
        private float _spriteHeight;

        public struct SpritesheetData
        {
            public string filename;
            public int textureId;
            public int width;
            public int height;
            public int columns;
            public int rows;

            public int spriteSize;
            public float spriteDx;
            public float spriteDy;
            public float spriteWidth;
            public float spriteHeight;
        }

        public int SpriteSize { get; protected set; }

        private static readonly Dictionary<string, SpritesheetData> TextureIds = new Dictionary<string, SpritesheetData>();

        public SpritesheetController(string filename, GraphicsController graphics)
        {
            if (!SpritesheetController.TextureIds.ContainsKey(filename))
            {
                Console.WriteLine("Loading new Spritesheet {0}", filename);
                var spritesheetData = new SpritesheetData();
                spritesheetData.filename = filename;

                var data =
                    ResourceController.Load<SpritesheetDatafile>(
                        ResourceController.GetDataFilename("spritesheet.{0}.txt", filename));

                spritesheetData.spriteSize = data.SpriteSize;

                var bitmap = new Bitmap(ResourceController.GetSpriteFilename(data.BitmapFile));

                spritesheetData.width = bitmap.Width;
                spritesheetData.height = bitmap.Height;
                spritesheetData.columns = spritesheetData.width / spritesheetData.spriteSize;
                spritesheetData.rows = spritesheetData.height / spritesheetData.spriteSize;

                spritesheetData.spriteDx = 1.0f / spritesheetData.columns;
                spritesheetData.spriteDy = 1.0f / spritesheetData.rows;
                spritesheetData.spriteWidth = (1.0f / spritesheetData.width) * spritesheetData.spriteSize;
                spritesheetData.spriteHeight = (1.0f / spritesheetData.height) * spritesheetData.spriteSize;

                spritesheetData.textureId = graphics.LoadSpritesheet(bitmap);
                SpritesheetController.TextureIds.Add(filename, spritesheetData);
            }

            var cachedData = SpritesheetController.TextureIds[filename];
            SpriteSize = cachedData.spriteSize;
            Texture = cachedData.textureId;
            TextureWidth = cachedData.width;
            TextureHeight = cachedData.height;
            TextureColumns = cachedData.columns;
            TextureRows = cachedData.rows;

            _spriteDx = cachedData.spriteDx;
            _spriteDy = cachedData.spriteDy;
            _spriteWidth = cachedData.spriteWidth;
            _spriteHeight = cachedData.spriteHeight;
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