using UnityEngine;
using System;

public class Tile {

    private World world;
    private TileType type = TileType.Empty;
    private LooseObject _looseObject;
    public InstalledObject InstalledObject { get; protected set; }

    private Action<Tile> tileTypeChangedCallback;

    public int X { get; private set; }
    public int Y { get; private set; }

    public TileType Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;

            if (tileTypeChangedCallback != null)
            {
                tileTypeChangedCallback(this);
            }
        }
    }

    public Tile(World world, int x, int y)
    {
        this.world = world;
        this.X = x;
        this.Y = y;
    }

    public void UnRegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        tileTypeChangedCallback -= callback;
    }

    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        tileTypeChangedCallback += callback;
    }

    public bool PlaceObject(InstalledObject objectInstance)
    {
        // If a null objectInstance is provided, clear the current object.
        if (objectInstance == null)
        {
            InstalledObject = null;
            return true;
        }

        if (InstalledObject != null)
        {
            Debug.LogError("Trying to assign an InstalledObject to a Tile that already has one.");
            return false;
        }

        InstalledObject = objectInstance;
        return true;
    }
}
