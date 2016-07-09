using System.Collections.Generic;
using Assets.Scripts.Model;
using Assets.Scripts.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Controllers
{
    public class MouseController : MonoBehaviour
    {
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
        private Vector3 _lastFramePosition;
        private Vector3 _currentFramePosition;
        private Vector3 _dragStartPosition;
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
        public Vector3 GetMousePosition()
        {
            return _currentFramePosition;
        }

        public Tile GetTileUnderMouse()
        {
            return WorldController.Instance.GetTileAtWorldCoordinates(_currentFramePosition);
        }

        private void ShowFurnitureSpriteAtTile(string furnType, Tile t)
        {
            var go = new GameObject();
            go.transform.SetParent(this.transform, true);
            _dragPreviewGameObjects.Add(go);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _fsc.GetSpriteForFurniture(furnType);

            if (WorldController.Instance.World.IsFurniturePlacementValid(furnType, t))
            {
                sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
            }
            else
            {
                sr.color = new Color(1f, 0.5f, 0.5f, 1f);
            }

            sr.sortingLayerName = "Jobs";

            var proto = World.Instance.FurniturePrototypes[furnType];
            var posOffset = new Vector3((float) (proto.Width - 1)/2, (float) (proto.Height - 1)/2, 0);
            go.transform.position = new Vector3(t.X, t.Y, 0) + posOffset;
        }

        // Use this for initialization
        private void Start()
        {
            _fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
            _bmc = GameObject.FindObjectOfType<BuildModeController>();
            _dragPreviewGameObjects = new List<GameObject>();
        }

        // Update is called once per frame
        private void Update()
        {
            _currentFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetKeyUp(KeyCode.Escape))
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

            _lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private void UpdateCameraMovement()
        {
            // Handle screen dragging
            if (Input.GetMouseButton(2)) // 2:Middle Mouse Button
            {
                var diff = _lastFramePosition - _currentFramePosition;
                Camera.main.transform.Translate(diff);
            }

            // Zooming
            Camera.main.orthographicSize -= Camera.main.orthographicSize*Input.GetAxis("Mouse ScrollWheel")*2f;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 50f);
        }

        private void UpdateDragging()
        {
            // If over UI, do nothing
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

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
            if (Input.GetMouseButtonDown(0))
            {
                _dragStartPosition = _currentFramePosition;
                _isDragging = true;
            }
            else if (_isDragging == false)
            {
                _dragStartPosition = _currentFramePosition;
            }

            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape))
            {
                // The RIGHT mouse button came up or ESC was pressed, so cancel any dragging.
                _isDragging = false;
            }

            if (_bmc.IsObjectDraggable() == false)
            {
                _dragStartPosition = _currentFramePosition;
            }

            var startX = Mathf.FloorToInt(_dragStartPosition.x + 0.5f);
            var endX = Mathf.FloorToInt(_currentFramePosition.x + 0.5f);
            var startY = Mathf.FloorToInt(_dragStartPosition.y + 0.5f);
            var endY = Mathf.FloorToInt(_currentFramePosition.y + 0.5f);

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
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
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
                                var go = SimplePool.Spawn(CircleCursorPrefab, new Vector3(x, y, 0),
                                    Quaternion.identity);
                                go.transform.SetParent(this.transform, true);
                                _dragPreviewGameObjects.Add(go);
                            }
                        }
                    }
                }
            }

            // End Drag
            if (_isDragging && Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                for (var x = startX; x <= endX; x++)
                {
                    for (var y = startY; y <= endY; y++)
                    {
                        var actionTile = true;

                        // If shift is being held, just action the perimeter
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
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
