using Engine.Utilities;
using SDL2;
using System;
using static SDL2.SDL;

namespace Engine.Renderer.SDLRenderer
{
    public class SDLText : IDisposable
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */
        private SDLTexture _texture = new SDLTexture();

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */
        public ScreenCoord Position { get; set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Create(string text, string font, int size, SDL_Color colour)
        {
            var surface = SDL_ttf.TTF_RenderUTF8_Solid(SDLTtf.Instance.GetFont(font, size), text, colour);
            _texture.CreateFromSurface(surface);
            SDL.SDL_FreeSurface(surface);
            Position = new ScreenCoord();
        }

        public void Render()
        {
            _texture.Render(Position);
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

                disposedValue = true;
            }
        }

        ~SDLText()
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
