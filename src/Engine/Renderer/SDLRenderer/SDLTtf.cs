using SDL2;
using System;
using System.Collections.Generic;

namespace Engine.Renderer.SDLRenderer
{
    internal class SDLTtf
    {
        #region Singleton
        private static readonly Lazy<SDLTtf> _instance = new Lazy<SDLTtf>(() => new SDLTtf());

        public static SDLTtf Instance { get { return _instance.Value; } }

        private SDLTtf()
        {
            SDL_ttf.TTF_Init();
        }
        #endregion

        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        private readonly Dictionary<string, IntPtr> _fonts = new Dictionary<string, IntPtr>();

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public IntPtr GetFont(string name, int size)
        {
            var key = $"{name}__{size}";
            if (!_fonts.ContainsKey(key))
            {
                var font = SDL_ttf.TTF_OpenFont(Engine.Instance.Path("assets", "base", "fonts", $"{name}.ttf"), size);
                _fonts.Add(key, font);
            }
            return _fonts[key];
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

                foreach (var font in _fonts)
                {
                    SDL_ttf.TTF_CloseFont(font.Value);
                }

                SDL_ttf.TTF_Quit();
                disposedValue = true;
            }
        }

        ~SDLTtf()
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
