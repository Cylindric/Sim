using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }

    public Sprite FloorSprite;
    public Sprite EmptySprite;

    public World World { get; protected set; }

    private readonly Dictionary<Tile, GameObject> _tileGameObjectMap = new Dictionary<Tile, GameObject>();
    private readonly Dictionary<Furniture, GameObject> _furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();
    private readonly Dictionary<string, Sprite> _wallSprites = new Dictionary<string, Sprite>();

    private void OnEnable()
    {
        if (Instance != null)
        {
            Debug.LogError("There shouldn't be an instance already!");
        }
        Instance = this;

        // Create an empty World.
        this.World = new World(100, 100);

        // Centre the view on the middle of the world.
        Camera.main.transform.position = new Vector3(World.Width/2f, World.Height/2f, Camera.main.transform.position.z);

        World.RegisterFurnitureCreatedCb(OnFurnitureCreated);
        
        // Cache some sprite stuff.
        Sprite[] sprites = Resources.LoadAll<Sprite>("Furniture/Stone Walls");
        foreach (Sprite sprite in sprites)
        {
            _wallSprites.Add(sprite.name, sprite);
        }

        // Create a game object for every tile.
        for (var x = 0; x < World.Width; x++)
        {
            for (var y = 0; y < World.Height; y++)
            {
                var tileData = this.World.GetTileAt(x, y);
                var tileGo = new GameObject();
                _tileGameObjectMap.Add(tileData, tileGo);

                tileGo.name = "Tile_" + x + "_" + y;
                //tileGo.transform.localScale = new Vector3(1.01f, 1.01f); // little bit of extra size to help prevent gaps between tiles. TODO: must be a cleverer way of doing this ;)
                tileGo.transform.position = new Vector3(tileData.X, tileData.Y, 0);
                tileGo.transform.SetParent(this.transform, true);

                tileGo.AddComponent<SpriteRenderer>().sprite=  EmptySprite;
                //tileData.RegisterTileTypeChangedCallback(OnTileChanged);
            }
        }

        World.RegisterTileChanged(OnTileChanged);

        //World.RandomiseTiles();
    }

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
            var tileData = _tileGameObjectMap.Keys.First();
            var tileGo = _tileGameObjectMap[tileData];
            _tileGameObjectMap.Remove(tileData);
            tileData.UnRegisterTileTypeChangedCallback(OnTileChanged);
            Destroy(tileGo);
        }
    }

    private void OnTileChanged(Tile tileData)
    {
        if (_tileGameObjectMap.ContainsKey(tileData) == false)
        {
            Debug.LogError("TileGameObjectMap doesn't contain the tile_data.");
            return;
        }

        var tileGo = _tileGameObjectMap[tileData];

        if (tileGo == null)
        {
            Debug.LogError("TileGameObjectMap returned a null GameObject.");
            return;
        }

        if (tileData.Type == TileType.Floor)
        {
            tileGo.GetComponent<SpriteRenderer>().sprite = FloorSprite;
        }
        else if (tileData.Type == TileType.Empty)
        {
            tileGo.GetComponent<SpriteRenderer>().sprite = EmptySprite;
        }
        else
        {
            Debug.LogError("OnTileChanged - Unrecognised tile type");
        }
    }

    public Tile GetTileAtWorldCoordinates(Vector3 coord)
    {
        var x = Mathf.FloorToInt(coord.x);
        var y = Mathf.FloorToInt(coord.y);

        return World.GetTileAt(x, y);
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        var furnGo = new GameObject();
        _furnitureGameObjectMap.Add(furn, furnGo);

        furnGo.name = furn.ObjectType + "_" + furn.Tile.X + "_" + furn.Tile.Y;
        furnGo.transform.position = new Vector3(furn.Tile.X, furn.Tile.Y, 0);
        furnGo.transform.SetParent(this.transform, true);
        
        furnGo.AddComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

        furn.RegisterOnChangedCallback(OnFurnitureChanged);
    }

    private Sprite GetSpriteForFurniture(Furniture obj)
    {
        var spriteName = obj.ObjectType;

        if (obj.LinksToNeighbour == true)
        {
            spriteName = spriteName + "_";

            // check for neighbours NESW
            var x = obj.Tile.X;
            var y = obj.Tile.Y;

            Tile t;

            t = World.GetTileAt(x, y + 1);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                spriteName += "N";
            }
            t = World.GetTileAt(x + 1, y);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                spriteName += "E";
            }
            t = World.GetTileAt(x, y - 1);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                spriteName += "S";
            }
            t = World.GetTileAt(x - 1, y);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                spriteName += "W";
            }
        }

        if (_wallSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogErrorFormat("Attempt to load missing sprite [{0}] failed!", spriteName);
            return null;
        }

        return _wallSprites[spriteName];
    }

    private void OnFurnitureChanged(Furniture furn)
    {
        if (_furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged failed - Furniture requested that is not in the map!");
            return;
        }

        var furnGo = _furnitureGameObjectMap[furn];
        furnGo.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
    }
}