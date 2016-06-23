using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class World
{
    private readonly Tile[,] _tiles;
    private Dictionary<string, Furniture> _installedObjectPrototypes;

    public int Width { get; private set; }
    public int Height { get; private set; }

    private Action<Furniture> cbFurnitureCreated;
    private Action<Tile> cbTileChanged;

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
                _tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
            }
        }
        Debug.Log("World (" + this.Width + "," + this.Height + ") created with " + (this.Width*this.Height) + " tiles.");

        CreateFurniturePrototypes();
    }

    private void CreateFurniturePrototypes()
    {
        _installedObjectPrototypes = new Dictionary<string, Furniture>();
        _installedObjectPrototypes.Add("Wall", Furniture.CreatePrototype("Wall", 0f, 1, 1, true));
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

        var obj = Furniture.PlaceInstance(_installedObjectPrototypes[objectType], t);

        if (obj == null)
        {
            // Failed to place object! Maybe something was already there.
            return;
        }

        if (cbFurnitureCreated != null)
        {
            cbFurnitureCreated(obj);
        }
    }

    public void RegisterFurnitureCreatedCb(Action<Furniture> cb)
    {
        cbFurnitureCreated += cb;
    }

    public void UnRegisterFurnitureCreatedCb(Action<Furniture> cb)
    {
        cbFurnitureCreated -= cb;
    }

    public void RegisterTileChanged(Action<Tile> cb)
    {
        cbTileChanged += cb;
    }

    public void UnRegisterTileChanged(Action<Tile> cb)
    {
        cbTileChanged -= cb;
    }

    private void OnTileChanged(Tile t)
    {
        cbTileChanged(t);
    }
}
