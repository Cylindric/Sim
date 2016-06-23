using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class World
{
    private readonly Tile[,] _tiles;
    private Dictionary<string, InstalledObject> _installedObjectPrototypes; 
    
    public int Width { get; private set; }
    public int Height { get; private set; }

    private Action<InstalledObject> cbInstalledObjectCreated;

    public World(int width, int height)
    {
        this.Width = width;
        this.Height = height;

        _tiles = new Tile[width, height];

        for (int x = 0; x < this.Width; x++)
        {
            for (int y = 0; y < this.Height; y++)
            {
                _tiles[x, y] = new Tile(this, x, y);
            }
        }
        Debug.Log("World (" + this.Width + "," + this.Height + ") created with " + (this.Width * this.Height) + " tiles.");

        CreateInstalledObjectPrototypes();
    }

    private void CreateInstalledObjectPrototypes()
    {
        _installedObjectPrototypes = new Dictionary<string, InstalledObject>();
        _installedObjectPrototypes.Add("Wall", InstalledObject.CreatePrototype("Wall", 0f, 1, 1, true));
    }
    
    public void RandomiseTiles()
    {
        for (int x = 0; x < this.Width; x++)
        {
            for (int y = 0; y < this.Height; y++)
            {
                if (Random.Range(0, 2) == 0)
                {
                    _tiles[x, y].Type = TileType.Empty;
                }
                else
                {
                    _tiles[x, y].Type = TileType.Floor;
                }
            }
        }

        Debug.Log("Randomised tiles");
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x > Width || x < 0 || y > Height || y < 0)
        {
            //Debug.LogError("Tile (" + x + "," + y + ") is out of range");
            return null;
        }
        return _tiles[x, y];
    }

    public void PlaceInstalledObject(string objectType, Tile t)
    {
        if (_installedObjectPrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogErrorFormat("Tried to place an object [{0}] for which we don't have a prototype.", objectType);
            return;
        }

        var obj = InstalledObject.PlaceInstance(_installedObjectPrototypes[objectType], t);

        if (obj == null)
        {
            // Failed to place object! Maybe something was already there.
            return;
        }

        if (cbInstalledObjectCreated != null)
        {
            cbInstalledObjectCreated(obj);
        }
    }

    public void RegisterInstalledObjectCreatedCb(Action<InstalledObject> cb)
    {
        cbInstalledObjectCreated += cb;
    }

    public void UnRegisterInstalledObjectCreatedCb(Action<InstalledObject> cb)
    {
        cbInstalledObjectCreated -= cb;
    }
}
