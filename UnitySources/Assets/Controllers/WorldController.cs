using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }

    public Sprite FloorSprite;

    public World World { get; protected set; }

    private readonly Dictionary<Tile, GameObject> _tileGameObjectMap = new Dictionary<Tile, GameObject>();
    private readonly Dictionary<InstalledObject, GameObject> _installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();
    private readonly Dictionary<string, Sprite> _wallSprites = new Dictionary<string, Sprite>();

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

        World.RegisterInstalledObjectCreatedCb(OnInstalledObjectCreated);

        // Cache some sprite stuff
        Sprite[] sprites = Resources.LoadAll<Sprite>("Tiles/mapPack_spritesheet");
        foreach (Sprite sprite in sprites)
        {
            _wallSprites.Add(sprite.name, sprite);
        }

        // Create a game object for every tile
        for (var x = 0; x < World.Width; x++)
        {
            for (var y = 0; y < World.Height; y++)
            {
                var tileData = this.World.GetTileAt(x, y);
                var tileGo = new GameObject();
                _tileGameObjectMap.Add(tileData, tileGo);

                tileGo.name = "Tile_" + x + "_" + y;
                tileGo.transform.localScale = new Vector3(1.01f, 1.01f); // little bit of extra size to help prevent gaps between tiles. TODO: must be a cleverer way of doing this ;)
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
            var tileData = _tileGameObjectMap.Keys.First();
            var tileGo = _tileGameObjectMap[tileData];
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
            tileGo.GetComponent<SpriteRenderer>().sprite = null;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged - Unrecognised tile type");
        }
    }

    public Tile GetTileAtWorldCoordinates(Vector3 coord)
    {
        var x = Mathf.FloorToInt(coord.x);
        var y = Mathf.FloorToInt(coord.y);

        return World.GetTileAt(x, y);
    }

    public void OnInstalledObjectCreated(InstalledObject obj)
    {
        GameObject objGo = new GameObject();
        _installedObjectGameObjectMap.Add(obj, objGo);

        objGo.name = obj.ObjectType + "_" + obj.Tile.X + "_" + obj.Tile.Y;
        objGo.transform.position = new Vector3(obj.Tile.X, obj.Tile.Y, 0);
        objGo.transform.SetParent(this.transform, true);
        
        objGo.AddComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(obj);

        obj.RegisterOnChangedCallback(OnInstalledObjectChanged);
    }

    private Sprite GetSpriteForInstalledObject(InstalledObject obj)
    {
        string spriteName = obj.ObjectType.ToLower();

        if (obj.LinksToNeighbour == true)
        {
            spriteName = spriteName + "_";

            // check for neighbours NESW
            var x = obj.Tile.X;
            var y = obj.Tile.Y;
            var t = World.GetTileAt(x, y + 1);
            if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == obj.ObjectType)
            {
                spriteName += "N";
            }
            t = World.GetTileAt(x + 1, y);
            if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == obj.ObjectType)
            {
                spriteName += "E";
            }
            t = World.GetTileAt(x, y - 1);
            if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == obj.ObjectType)
            {
                spriteName += "S";
            }
            t = World.GetTileAt(x - 1, y);
            if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == obj.ObjectType)
            {
                spriteName += "W";
            }
        }

        if (_wallSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogErrorFormat("Attempt to load missing sprite [{0}] failed!", spriteName);
        }

        return _wallSprites[spriteName];
    }

    private void OnInstalledObjectChanged(InstalledObject obj)
    {
        Debug.LogError("NOT IMPLEMENTED");
    }
}