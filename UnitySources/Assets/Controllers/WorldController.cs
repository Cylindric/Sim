using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }

    public Sprite floorSprite;
    public Sprite emptySprite;

    public World World { get; protected set; }

    private readonly Dictionary<Tile, GameObject> _tileGameObjectMap = new Dictionary<Tile, GameObject>(); 

    // Use this for initialization
    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("There shouldn't be an instance already!");
        }
        Instance = this;

        // Create an empty World
        this.World = new World(100, 100);

        // Create a game object for every tile
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                Tile tileData = this.World.GetTileAt(x, y);
                GameObject tileGo = new GameObject();
                _tileGameObjectMap.Add(tileData, tileGo);

                tileGo.name = "Tile_" + x + "_" + y;
                tileGo.transform.localScale = new Vector3(2, 2);
                tileGo.transform.position = new Vector3(tileData.X, tileData.Y, 0);
                tileGo.transform.SetParent(this.transform, true);

                tileGo.AddComponent<SpriteRenderer>();
                tileData.RegisterTileTypeChangedCallback(OnTileTypeChanged);
            }
        }

        World.RandomiseTiles();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void DestroyAllTileGameObjects()
    {
        while (_tileGameObjectMap.Count > 0)
        {
            Tile tileData = _tileGameObjectMap.Keys.First();
            GameObject tileGo = _tileGameObjectMap[tileData];
            _tileGameObjectMap.Remove(tileData);
            tileData.UnRegisterTileTypeChangedCallback(OnTileTypeChanged);
            Destroy(tileGo);
        }
    }

    private void OnTileTypeChanged(Tile tileData)
    {
        if (_tileGameObjectMap.ContainsKey(tileData) == false)
        {
            Debug.LogError("TileGameObjectMap doesn't contain the tile_data.");
            return;
        }

        GameObject tileGo = _tileGameObjectMap[tileData];

        if (tileGo == null)
        {
            Debug.LogError("TileGameObjectMap returnd a null GameObject.");
            return;
        }

        if (tileData.Type == TileType.Floor)
        {
            tileGo.GetComponent<SpriteRenderer>().sprite = floorSprite;
        }
        else if (tileData.Type == TileType.Empty)
        {
            tileGo.GetComponent<SpriteRenderer>().sprite = emptySprite;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged - Unrecognised tile type");
        }
    }

    public Tile GetTileAtWorldCoordinates(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x);
        int y = Mathf.FloorToInt(coord.y);

        return World.GetTileAt(x, y);
    }

}