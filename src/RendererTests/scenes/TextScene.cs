using Engine.Controllers;
using Engine.Renderer.SDLRenderer;
using Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RendererTests.scenes
{
    class TextScene : IScene
    {
        SDLText text;

        public TextScene()
        {
            text = new SDLText();
            text.Create("Hello, world!", "Robotica", 50, new SDL2.SDL.SDL_Color() { r = 255 });
        }

        void IScene.Update() {
            text.Position = MouseController.Instance.GetMousePosition();
        }

        void IScene.Render()
        {
            text.Render();
        }
    }
}
