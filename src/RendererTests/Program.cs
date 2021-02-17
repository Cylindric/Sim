using Engine.Controllers;
using Engine.Models;
using Engine.Renderer.SDLRenderer;
using Engine.Utilities;
using RendererTests.scenes;
using SDL2;
using System;
using System.IO;

namespace RendererTests
{
    class Program
    {
        static void Main()
        {
            SDLWindow.Instance.Start();
            SDLRenderer.Instance.Start();
            SDL_ttf.TTF_Init();

            // Scenes
            IScene[] scenes = { null, new TileScene(), new TextScene() };

            // Render loop
            int currentScene = 2;
            MouseController.Instance.Start();
            bool keepRunning = true;
            while (keepRunning)
            {
                SDLEvent.Update();
                SDLWindow.Instance.Update();
                MouseController.Instance.Update();
                scenes[currentScene]?.Update();
                scenes[currentScene]?.Render();
                SDLWindow.Instance.Present();

                if (SDLEvent.Quit || SDLEvent.KeyUp(SDL.SDL_Keycode.SDLK_ESCAPE))
                {
                    keepRunning = false;
                }
                else if (SDLEvent.KeyUp(SDL.SDL_Keycode.SDLK_0))
                {
                    currentScene = 0;
                }
                else if (SDLEvent.KeyUp(SDL.SDL_Keycode.SDLK_1))
                {
                    currentScene = 1;
                }
                else if (SDLEvent.KeyUp(SDL.SDL_Keycode.SDLK_2))
                {
                    currentScene = 2;
                }
            }
        }
    }
}