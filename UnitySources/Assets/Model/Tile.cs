using UnityEngine;
using System.Collections;
using System;

public class Tile {

    private World world;
    private TileType type = TileType.Empty;
    private LooseObject looseObject;
    private InstalledObject installedObject;

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
}
