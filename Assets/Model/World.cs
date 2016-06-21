using UnityEngine;
using System.Collections;

public class World {

    Tile[,] tiles;
    
    public int Width { get; private set; }
    public int Height { get; private set; }

    public World(int width, int height)
    {
        this.Width = width;
        this.Height = height;

        tiles = new Tile[width, height];

        for (int x = 0; x < this.Width; x++)
        {
            for (int y = 0; y < this.Height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
            }
        }

        Debug.Log("World (" + this.Width + "," + this.Height + ") created with " + (this.Width * this.Height) + " tiles.");
    }

    public void RandomiseTiles()
    {
        for (int x = 0; x < this.Width; x++)
        {
            for (int y = 0; y < this.Height; y++)
            {
                if (Random.Range(0, 2) == 0)
                {
                    tiles[x, y].Type = Tile.TileType.Empty;
                }else
                {
                    tiles[x, y].Type = Tile.TileType.Floor;
                }
            }
        }

        Debug.Log("Randomised tiles");
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x > Width || x < 0 || y > Height || y < 0)
        {
            Debug.LogError("Tile (" + x + "," + y + ") is out of range");
            return null;
        }
        return tiles[x, y];
    }
}
