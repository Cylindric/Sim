using System.Collections.Generic;
using Engine.Models;
using Engine.Utilities;
using System;
using Engine.Renderer.SDLRenderer;

namespace Engine.Controllers
{
    public class MouseController : IController
    {
        #region Singleton
        private static readonly Lazy<MouseController> _instance = new Lazy<MouseController>(() => new MouseController());

        public static MouseController Instance { get { return _instance.Value; } }

        private MouseController()
        {
        }
        #endregion

        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        private enum MouseMode
        {
            Select,
            Build
        }

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */
        public GameObject CircleCursorPrefab;
        private ScreenCoord _lastFramePosition;
        private ScreenCoord _currentFramePosition;
        private ScreenCoord _dragStartPosition;
        private List<GameObject> _dragPreviewGameObjects;
        private BuildModeController _bmc;
        private FurnitureSpriteController _fsc;
        private bool _isDragging = false;
        private MouseMode _mode = MouseMode.Select;

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

        public void StartBuildMode()
        {
            _mode = MouseMode.Build;
        }

        public void StartSelectMode()
        {
            _mode = MouseMode.Select;
        }

        /// <summary>
        /// Gets the current mouse position, in World-space coordinates.
        /// </summary>
        /// <returns></returns>
        public ScreenCoord GetMousePosition()
        {
            return _currentFramePosition;
        }

        public Tile GetTileUnderMouse()
        {
            return WorldController.Instance.GetTileAtWorldCoordinates(_currentFramePosition.ToWorld());
        }

        private void ShowFurnitureSpriteAtTile(string furnType, Tile t)
        {
            var go = new GameObject();
            // go.transform.SetParent(this.transform, true);
            _dragPreviewGameObjects.Add(go);

            go.Sprite = _fsc.GetSpriteForFurniture(furnType);

            if (WorldController.Instance.World.IsFurniturePlacementValid(furnType, t))
            {
                go.Sprite.Colour = new Colour(0.5f, 1f, 0.5f, 0.25f);
            }
            else
            {
                go.Sprite.Colour = new Colour(1f, 0.5f, 0.5f, 1f);
            }

            go.SortingLayerName = "Jobs";

            var proto = World.Instance.FurniturePrototypes[furnType];
            var posOffset = new Vector3((float) (proto.Width - 1)/2, (float) (proto.Height - 1)/2, 0);
            go.Position = new WorldCoord(t.X + posOffset.X, t.Y + posOffset.Y);
        }

        // Use this for initialization
        public void Start()
        {
            _fsc = FurnitureSpriteController.Instance;
            _bmc = BuildModeController.Instance;
            _dragPreviewGameObjects = new List<GameObject>();
        }

        // Update is called once per frame
        public void Update()
        {
            _currentFramePosition = SDLEvent.MousePosition;

            if (SDLEvent.KeyUp(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                if (_mode == MouseMode.Build)
                {
                    _mode = MouseMode.Select;
                }
                else if (_mode == MouseMode.Select)
                {
                    Debug.Log("TODO: Show Game Menu");
                }
            }

            UpdateDragging();
            UpdateCameraMovement();

            _lastFramePosition = SDLEvent.MousePosition;
        }

        public void Render() {}

        private void UpdateCameraMovement()
        {
            // Handle screen dragging
            if (SDLEvent.MouseButtonIsDown(SDL2.SDL.SDL_BUTTON_MIDDLE))
            {
                var diff = _lastFramePosition - _currentFramePosition;
                CameraController.Instance.Position += diff.Flip();
                Debug.Log($"Camera moved {diff} to {CameraController.Instance.Position}");
            }

            // Zooming
            //Camera.main.orthographicSize -= Camera.main.orthographicSize*Input.GetAxis("Mouse ScrollWheel")*2f;
            //Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 50f);
        }

        private void UpdateDragging()
        {
            // If over UI, do nothing
            //if (EventSystem.current.IsPointerOverGameObject())
            //{
            //    return;
            //}

            // Clear the drag-area markers
            while (_dragPreviewGameObjects.Count > 0)
            {
                var go = _dragPreviewGameObjects[0];
                _dragPreviewGameObjects.RemoveAt(0);
                SimplePool.Despawn(go);
            }

            if (_mode != MouseMode.Build)
            {
                return;
            }

            // Start Drag
            if (SDLEvent.MouseButtonIsDown(SDL2.SDL.SDL_BUTTON_LEFT))
            {
                _dragStartPosition = _currentFramePosition;
                _isDragging = true;
            }
            else if (_isDragging == false)
            {
                _dragStartPosition = _currentFramePosition;
            }

            if (SDLEvent.MouseButtonWentUp(SDL2.SDL.SDL_BUTTON_RIGHT) || SDLEvent.KeyUp(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                // The RIGHT mouse button came up or ESC was pressed, so cancel any dragging.
                _isDragging = false;
            }

            if (_bmc.IsObjectDraggable() == false)
            {
                _dragStartPosition = _currentFramePosition;
            }

            var start = _dragStartPosition.ToWorld();
            var end = _dragStartPosition.ToWorld();

            var startX = Mathf.FloorToInt(start.X + 0.5f);
            var endX = Mathf.FloorToInt(end.X + 0.5f);
            var startY = Mathf.FloorToInt(start.Y + 0.5f);
            var endY = Mathf.FloorToInt(end.Y + 0.5f);

            if (endX < startX)
            {
                var temp = endX;
                endX = startX;
                startX = temp;
            }

            if (endY < startY)
            {
                var temp = endY;
                endY = startY;
                startY = temp;
            }

            // Display dragged area
            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY; y <= endY; y++)
                {
                    var t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        var actionTile = true;

                        // If shift is being held, just action the perimeter
                        if (SDLEvent.KeyState(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) || SDLEvent.KeyState(SDL2.SDL.SDL_Keycode.SDLK_RSHIFT))
                        {
                            actionTile = (x == startX || x == endX || y == startY || y == endY);
                        }

                        if (actionTile)
                        {
                            if (_bmc.BuildMode == BuildMode.Furniture)
                            {
                                ShowFurnitureSpriteAtTile(_bmc.BuildModeObjectType, t);
                            }
                            else
                            {
                                var go = SimplePool.Spawn(CircleCursorPrefab, new WorldCoord(x, y));
                                // go.transform.SetParent(this.transform, true);
                                _dragPreviewGameObjects.Add(go);
                            }
                        }
                    }
                }
            }

            // End Drag
            if (_isDragging && SDLEvent.MouseButtonWentUp(SDL2.SDL.SDL_BUTTON_LEFT))
            {
                _isDragging = false;
                for (var x = startX; x <= endX; x++)
                {
                    for (var y = startY; y <= endY; y++)
                    {
                        var actionTile = true;

                        // If shift is being held, just action the perimeter
                        if (SDLEvent.KeyState(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) || SDLEvent.KeyState(SDL2.SDL.SDL_Keycode.SDLK_RSHIFT))
                        {
                            actionTile = (x == startX || x == endX || y == startY || y == endY);
                        }

                        if (actionTile)
                        {
                            var t = WorldController.Instance.World.GetTileAt(x, y);
                            if (t != null)
                            {
                                _bmc.DoBuild(t);
                            }
                        }
                    }
                }
            }
        }

    }
}
