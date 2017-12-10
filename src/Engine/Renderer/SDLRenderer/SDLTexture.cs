using Engine.Models;
using Engine.Utilities;
using SDL2;
using System;
using System.Diagnostics;
using static SDL2.SDL;
using Debug = Engine.Utilities.Debug;

namespace Engine.Renderer.SDLRenderer
{
    [DebuggerDisplay("SDLTexture [{_texturePtr}]")]
    public class SDLTexture
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */
        private IntPtr _texturePtr;
        private int _width;
        private int _height;
        private uint _format;
        private int _access;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */
        public SDLTexture()
        {
        }

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void CreateFromSurface(IntPtr surface)
        {
            if(_texturePtr != IntPtr.Zero)
            {
                SDL_DestroyTexture(_texturePtr);
            }
            _texturePtr = SDL_CreateTextureFromSurface(SDLRenderer.Instance.RenderPtr, surface);
            SDL_QueryTexture(_texturePtr, out _format, out _access, out _width, out _height);
        }

        public void Load(string filename)
        {
            var surface = SDL_image.IMG_Load(filename);
            if(surface == null)
            {
                Debug.Log($"Failed to load image! SDL error: {SDL.SDL_GetError()}");
                throw new InvalidOperationException(SDL.SDL_GetError());
            }

            _texturePtr = SDL.SDL_CreateTextureFromSurface(SDLRenderer.Instance.RenderPtr, surface);
            SDL.SDL_DestroyTexture(surface);

            if(_texturePtr == null)
            {
                Debug.Log($"Failed to create texture! SDL error: {SDL.SDL_GetError()}");
                throw new InvalidOperationException(SDL.SDL_GetError());
            }
        }

        public void Render(ScreenCoord position)
        {
            SDL_Rect srcrect = new SDL_Rect()
            {
                x = 0,
                y = 0,
                w = _width,
                h = _height
            };

            SDL_Rect dsrect = new SDL_Rect()
            {
                x = (int)position.X,
                y = (int)position.Y,
                w = _width,
                h = _height
            };

            if (SDL.SDL_RenderCopy(SDLRenderer.Instance.RenderPtr, _texturePtr, ref srcrect, ref dsrect) < 0)
            {
                Debug.Log($"Failed to render texture! SDL error: {SDL.SDL_GetError()}");
                throw new InvalidOperationException(SDL.SDL_GetError());
            }
        }

        public void RenderSprite(Sprite sprite, int x, int y, int width, int height)
        {
            SDL_Rect srcrect = new SDL_Rect()
            {
                x = sprite.X,
                y = sprite.Y,
                w = sprite.Width,
                h = sprite.Height
            };

            SDL_Rect dsrect = new SDL_Rect()
            {
                x = x,
                y = y,
                w = width,
                h = height
            };

            if(SDL.SDL_RenderCopy(SDLRenderer.Instance.RenderPtr, _texturePtr, ref srcrect, ref dsrect) < 0)
            {
                Debug.Log($"Failed to render texture! SDL error: {SDL.SDL_GetError()}");
                throw new InvalidOperationException(SDL.SDL_GetError());
            }
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

                SDL.SDL_DestroyTexture(_texturePtr);
                disposedValue = true;
            }
        }

        ~SDLTexture()
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
