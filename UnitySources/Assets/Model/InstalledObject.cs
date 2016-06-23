using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// An InstalledObject represents an object that is 'permanently' installed on a <see cref="Tile"/>.
/// </summary>
public class InstalledObject
{
    /// <summary>
    /// Base tile for this object. Large objects may occupy more tiles.
    /// </summary>
    public Tile Tile { get; protected set; }

    /// <summary>
    /// The ObjectType wil lbe queried by the visual system to know what sprite to render for this object.
    /// </summary>
    public string ObjectType { get; protected set; }

    /// <summary>
    /// Cost of moving through this object.
    /// </summary>
    /// <remarks>
    /// If this is zero, the tile is impassable.</remarks>
    private float _movementCost = 1f;

    /// <summary>
    /// Width of the Object in Tiles.
    /// </summary>
    private int _width = 1;

    /// <summary>
    /// Height of the Object in Tiles.
    /// </summary>
    private int _height = 1;

    /// <summary>
    /// Create a new InstalledObject. This can only be done using the Factory methods.
    /// </summary>
    private InstalledObject() { }

    private Action<InstalledObject> cbOnChanged;

    /// <summary>
    /// Gets a new InstalledObject Prototype.
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="movementCost"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static InstalledObject CreatePrototype(string objectType, float movementCost = 1f, int width = 1, int height = 1)
    {
        var obj = new InstalledObject
        {
            ObjectType = objectType,
            _movementCost = movementCost,
            _width = width,
            _height = height
        };
        return obj;
    }

    /// <summary>
    /// Install a copy of the specified Prototype to the specified Tile.
    /// </summary>
    /// <param name="proto"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static InstalledObject PlaceInstance(InstalledObject proto, Tile tile)
    {
        var obj = new InstalledObject
        {
            ObjectType = proto.ObjectType,
            _movementCost = proto._movementCost,
            _width = proto._width,
            _height = proto._height,
            Tile = tile
        };

        if (tile.PlaceObject(obj) == false)
        {
            // Something prevented the object being placed. Don't return it, so it gets GC'd out at some point.
            return null;
        }

        return obj;
    }

    public void RegisterOnChangedCallback(Action<InstalledObject> cb)
    {
        cbOnChanged += cb;
    }

    public void UnRegisterOnChangedCallback(Action<InstalledObject> cb)
    {
        cbOnChanged -= cb;
    }
}
