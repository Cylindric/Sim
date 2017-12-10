using SDL2;
using System;
using System.Diagnostics;
using Debug = Engine.Utilities.Debug;

namespace Engine.Renderer.SDLRenderer
{
    [DebuggerDisplay("SDLRenderer [{ptr}]")]
    public class SDLRenderer : IDisposable
    {
        #region Singleton
        private static readonly Lazy<SDLRenderer> _instance = new Lazy<SDLRenderer>(() => new SDLRenderer());

        public static SDLRenderer Instance { get { return _instance.Value; } }

        private SDLRenderer()
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
        public IntPtr RenderPtr;

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */
        public void Start()
        {
            RenderPtr = SDL.SDL_CreateRenderer(SDLWindow.Instance.ptr, -1, 0);
            if (RenderPtr == null)
            {
                Debug.Log($"Failed to create renderer! SDL error: {SDL.SDL_GetError()}");
                throw new InvalidOperationException(SDL.SDL_GetError());
            }
        }

        public void Clear()
        {
            SDL.SDL_RenderClear(RenderPtr);
        }

        public void Present()
        {
            SDL.SDL_RenderPresent(RenderPtr);
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

                SDL.SDL_DestroyRenderer(RenderPtr);
                disposedValue = true;
            }
        }

        ~SDLRenderer()
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
