using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }

    public Sprite floorSprite;
    public Sprite emptySprite;

    public World World { get; protected set; }

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
                Tile tile_data = this.World.GetTileAt(x, y);
                GameObject tile_go = new GameObject();
                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.localScale = new Vector3(2, 2);
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_go.transform.SetParent(this.transform, true);

                tile_go.AddComponent<SpriteRenderer>();

                tile_data.RegisterTileTypeChangedCallback((tile) => { OnTileTypeChanged(tile, tile_go); });
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

    private void OnTileTypeChanged(Tile tile_data, GameObject tile_go)
    {
        if (tile_data.Type == Tile.TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        }
        else if (tile_data.Type == Tile.TileType.Empty)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = emptySprite;
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