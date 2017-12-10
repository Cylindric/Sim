using Engine.Controllers;
using Engine.Renderer.SDLRenderer;
using System;
using System.Collections.Generic;

namespace Engine
{
    public class Engine
    {
        #region Singleton
        private static readonly Lazy<Engine> _instance = new Lazy<Engine>(() => new Engine());

        public static Engine Instance { get { return _instance.Value; } }

        private Engine()
        {
        }
        #endregion

        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        public const int GRID_SIZE = 64;

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */
        public string AppPath
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public string SavePath
        {
            get { return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save"); }
        }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Run()
        {
            var controllers = new List<IController>
            {
                TimeController.Instance,
                MouseController.Instance,
                CameraController.Instance,
                SpriteManager.Instance,
                WorldController.Instance,
                TileSpriteController.Instance,
                CharacterSpriteController.Instance
            };

            SDLWindow.Instance.Start();
            SDLRenderer.Instance.Start();

            foreach(var c in controllers)
            {
                c.Start();
            }

            bool keepRunning = true;
            while (keepRunning)
            {
                SDLEvent.Update(); // This should be updated early, to make sure inputs are available to later activities.
                SDLWindow.Instance.Update();

                foreach (var c in controllers)
                {
                    c.Update();
                }

                foreach (var c in controllers)
                {
                    c.Render();
                }

                SDLWindow.Instance.Present();

                if (SDLEvent.Quit || SDLEvent.KeyUp(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE))
                {
                    keepRunning = false;
                }
            }
        }

        public string Path(params string[] parts)
        {
            return System.IO.Path.Combine(AppPath, System.IO.Path.Combine(parts));
        }
    }
}
