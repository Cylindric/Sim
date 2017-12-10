using Engine.Renderer.SDLRenderer;
using Engine.Utilities;
using System;
using static SDL2.SDL;

namespace Engine.Controllers
{
    public class CameraController : IController
    {
        #region Singleton
        private static readonly Lazy<CameraController> _instance = new Lazy<CameraController>(() => new CameraController());

        public static CameraController Instance { get { return _instance.Value; } }

        private CameraController()
        {
            //_text.Create("Testing 1..2..3..", "Robotica", 20, new SDL_Color() { r = 1, g = 1, b = 0, a = 0 });
            //_text.Position = new Vector2<int>(5, 5);
        }
        #endregion

        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        /// <summary>
        /// The position of the camera. Stored internally as a float to allow for
        /// scrolling less than a single pixel in one frame.
        /// </summary>
        private ScreenCoord _position = new ScreenCoord();

        // private Text _text = new Text();

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        /// <summary>
        /// The current position of the camera.
        /// </summary>
        public ScreenCoord Position {
            get {
                return new ScreenCoord(_position.X, _position.Y);
            }
            set
            {
                _position.X = value.X;
                _position.Y = value.Y;
            }
        }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Start()
        {

        }

        public void SetPosition(int x, int y)
        {
            _position = new ScreenCoord(x, y);
        }

        public void SetPosition(ScreenCoord pos)
        {
            _position = new ScreenCoord(pos.X, pos.Y);
        }

        public void Update()
        {
            // Check for movement input
            float scrollSpeed = 100f;
            var dragDistance = TimeController.Instance.DeltaTime * scrollSpeed;

            if (SDLEvent.KeyState(SDL_Keycode.SDLK_a))
            {
                _position.X -= dragDistance;
            }
            if (SDLEvent.KeyState(SDL_Keycode.SDLK_d))
            {
                _position.X += dragDistance;
            }
            if (SDLEvent.KeyState(SDL_Keycode.SDLK_w))
            {
                _position.Y += dragDistance;
            }
            if (SDLEvent.KeyState(SDL_Keycode.SDLK_s))
            {
                _position.Y -= dragDistance;
            }

        }

        public void Render()
        {
            // _text.Render();
        }

        internal WorldCoord ScreenToWorldPoint(ScreenCoord screenPosition)
        {
            return new WorldCoord()
            {
                X = (float)(screenPosition.X + Position.X) / Engine.GRID_SIZE,
                Y = (float)(screenPosition.Y - Position.Y) / Engine.GRID_SIZE
            };
        }

        internal ScreenCoord WorldToScreenPoint(Vector2<float> worldCoordinate)
        {
            return new ScreenCoord()
            {
                X = (int)(worldCoordinate.X * Engine.GRID_SIZE - Position.X),
                Y = (int)(worldCoordinate.Y * Engine.GRID_SIZE + Position.Y)
            };
        }
    }
}
