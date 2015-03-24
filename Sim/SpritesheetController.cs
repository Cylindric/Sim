using System;
using System.Drawing;
using OpenTK;

namespace Sim
{
    class SpritesheetController
    {
        protected int Texture;
        protected int TextureWidth;
        protected int TextureHeight;
        protected int TextureColumns;
        protected int TextureRows;

        private string _datafile;
        private float _spriteDx;
        private float _spriteDy;
        private float _spriteWidth;
        private float _spriteHeight;

        public int SpriteSize { get; protected set; }

        public SpritesheetController(string filename, GraphicsController graphics)
        {
            _datafile = ResourceController.GetDataFile(filename);
            var datafileBitmap = "people.png";
            var datafileSize = 32;

            SpriteSize = datafileSize;

            var bitmap = new Bitmap(Image.FromFile(System.IO.Path.Combine("Resources", "Sprites", datafileBitmap)));
            TextureWidth = bitmap.Width;
            TextureHeight = bitmap.Height;
            TextureColumns = TextureWidth / SpriteSize;
            TextureRows = TextureHeight / SpriteSize;
 
            _spriteDx = 1.0f/TextureColumns;
            _spriteDy = 1.0f/TextureRows;
            _spriteWidth = (1.0f/TextureWidth)*SpriteSize;
            _spriteHeight = (1.0f/TextureHeight)*SpriteSize;

            Texture = graphics.LoadSpritesheet(bitmap);
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