using Engine.Models;
using Engine.Renderer.SDLRenderer;
using Engine.Utilities;
using System;
using System.IO;

namespace RendererTests.scenes
{
    class TileScene: IScene
    {
        private GameObject tileGo;

        public TileScene()
        {
            var imagefile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("assets", "tile.png"));
            var texture = new SDLTexture();
            texture.Load(imagefile);

            // Sprite
            var rect = new Rect(0, 0, 64, 64);
            var pivot = new Vector2<float>(0, 0);
            var sprite = new Sprite(texture, rect, pivot)
            {
                Name = "Tile"
            };

            // Tile
            tileGo = new GameObject
            {
                Name = "Tile_0_0",
                Position = new WorldCoord(30, 30),
                IsActive = false,
                Sprite = sprite
            };
            tileGo.IsActive = true;
        }

        void IScene.Update() { }

        void IScene.Render()
        {
            tileGo.Sprite.Render(10, 10);
        }
    }
}
