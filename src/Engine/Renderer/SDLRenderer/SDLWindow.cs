using SDL2;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Debug = Engine.Utilities.Debug;

namespace Engine.Renderer.SDLRenderer
{
    [DebuggerDisplay("SDLWindow [{ptr}]")]
    public class SDLWindow : IDisposable
    {
        #region Singleton
        private static readonly Lazy<SDLWindow> _instance = new Lazy<SDLWindow>(() => new SDLWindow());

        public static SDLWindow Instance { get { return _instance.Value; } }

        private SDLWindow()
        {
        }
        #endregion

        private int _x = 100;
        private int _y = 100;
        private int _width = 800;
        private int _height = 600;

        public IntPtr ptr;

        public void Start()
        {
            Start(_x, _y, _width, _height);
        }

        public void Start(int x, int y, int w, int h)
        {
            _x = x;
            _y = y;
            _width = w;
            _height = h;

            /*
             When running C# applications under the Visual Studio debugger, native code that
            names threads with the 0x406D1388 exception will silently exit. To prevent this
            exception from being thrown by SDL, add this line before your SDL_Init call:
            */
            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
            
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING)  < 0)
            {
                Debug.Log($"Failed to initialise SDL! SDL error: {SDL.SDL_GetError()}");
                throw new InvalidOperationException(SDL.SDL_GetError());
            }
            ptr = SDL.SDL_CreateWindow("CylSim", _x, _y, _width, _height, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if(ptr == null)
            {
                Debug.Log($"Failed to create window! SDL error: {SDL.SDL_GetError()}");
                throw new InvalidOperationException(SDL.SDL_GetError());
            }
        }

        public void Update()
        {
            SDLRenderer.Instance.Clear();
        }

        public void Present()
        {
            SDLRenderer.Instance.Present();
        }

        public void Screenshot(string filename)
        {
            var ssSurface = SDL.SDL_CreateRGBSurface(0, _width, _height, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000);

            var surface = Marshal.PtrToStructure<SDL.SDL_Surface>(ssSurface);
            var rect = new SDL.SDL_Rect()
            {
                x = 0,
                y = 0,
                w = surface.w,
                h = surface.h
            };

            SDL.SDL_RenderReadPixels(SDLRenderer.Instance.RenderPtr, ref rect, SDL.SDL_PIXELFORMAT_ARGB8888, surface.pixels, surface.pitch);
            SDL.SDL_SaveBMP(ssSurface, filename);
            SDL.SDL_FreeSurface(ssSurface);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                SDL.SDL_DestroyWindow(ptr);
                SDL.SDL_Quit();
                disposedValue = true;
            }
        }

        ~SDLWindow()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
