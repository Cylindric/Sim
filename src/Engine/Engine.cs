using Engine.Controllers;
using Engine.Models;
using Engine.Renderer.SDLRenderer;
using System;

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

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Run()
        {
            SDLWindow.Instance.Start();
            SDLRenderer.Instance.Start();

            bool keepRunning = true;
            while (keepRunning)
            {
                Time.Update();
                SDLEvent.Update(); // This should be updated early, to make sure inputs are available to later activities.
                SDLWindow.Instance.Update(); // This needs to be called before anything that might draw anything, as it clears the backbuffer.
                MouseController.Instance.Update();
                CameraController.Instance.Update();
                WorldController.Instance.Update();

                WorldController.Instance.Render();
                MouseController.Instance.Render();
                CameraController.Instance.Render();

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
