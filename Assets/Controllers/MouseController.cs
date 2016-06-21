using UnityEngine;
using System.Collections.Generic;

public class MouseController : MonoBehaviour
{

    public GameObject CircleCursorPrefab;

    private Vector3 _lastFramePosition;
    private Vector3 _currentFramePosition;
    private Vector3 _dragStartPosition;
    private List<GameObject> _dragPreviewGameObjects;

    // Use this for initialization
    private void Start()
    {
        _dragPreviewGameObjects = new List<GameObject>();
    }

    // Update is called once per frame
	void Update ()
    {
        _currentFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
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
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel") * 2f;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 50f);
    }

    private void UpdateDragging()
    {
        // Start Drag
        if (Input.GetMouseButtonDown(0))
        {
            _dragStartPosition = _currentFramePosition;
        }

        var startX = Mathf.FloorToInt(_dragStartPosition.x);
        var endX = Mathf.FloorToInt(_currentFramePosition.x);
        var startY = Mathf.FloorToInt(_dragStartPosition.y);
        var endY = Mathf.FloorToInt(_currentFramePosition.y);

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

        // Clear the drag-area markers
        while (_dragPreviewGameObjects.Count > 0)
        {
            var go = _dragPreviewGameObjects[0];
            _dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }

        if (Input.GetMouseButton(0))
        {
            // Display dragged area
            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY; y <= endY; y++)
                {
                    var t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        var go = SimplePool.Spawn(CircleCursorPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                        go.transform.SetParent(this.transform, true);
                        _dragPreviewGameObjects.Add(go);
                    }
                }

            }
        }

        // End Drag
        if (Input.GetMouseButtonUp(0))
        {
            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY; y <= endY; y++)
                {
                    var t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        t.Type = Tile.TileType.Floor;
                    }
                }
            }
        }
    }

}
