using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Furniture represents an object that is 'permanently' installed on a <see cref="Tile"/>.
/// </summary>
public class Furniture
{
    /// <summary>
    /// Base tile for this object. Large objects may occupy more tiles.
    /// </summary>
    public Tile Tile { get; private set; }

    /// <summary>
    /// The ObjectType wil lbe queried by the visual system to know what sprite to render for this object.
    /// </summary>
    public string ObjectType { get; private set; }

    /// <summary>
    /// Does this Furniture link to neighbouring furniture of the same type?
    /// </summary>
    public bool LinksToNeighbour { get; private set; }

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
    /// Create a new Furniture. This can only be done using the Factory methods.
    /// </summary>
    private Furniture()
    {
        LinksToNeighbour = false;
    }

    private Action<Furniture> cbOnChanged;

    private Func<Tile, bool> funcPositionValidation; 

    /// <summary>
    /// Gets a new Furniture Prototype.
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="movementCost"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Furniture CreatePrototype(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false)
    {
        var obj = new Furniture
        {
            ObjectType = objectType,
            _movementCost = movementCost,
            _width = width,
            _height = height,
            LinksToNeighbour = linksToNeighbour
        };
        obj.funcPositionValidation = obj.IsValidPosition;
        return obj;
    }

    /// <summary>
    /// Install a copy of the specified Prototype to the specified Tile.
    /// </summary>
    /// <param name="proto"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static Furniture PlaceInstance(Furniture proto, Tile tile)
    {
        if (proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance position validity function returned false.");
            return null;
        }

        var obj = new Furniture
        {
            ObjectType = proto.ObjectType,
            _movementCost = proto._movementCost,
            _width = proto._width,
            _height = proto._height,
            LinksToNeighbour = proto.LinksToNeighbour,
            Tile = tile
        };

        if (tile.PlaceFurniture(obj) == false)
        {
            // Something prevented the object being placed. Don't return it, so it gets GC'd out at some point.
            return null;
        }

        if (obj.LinksToNeighbour)
        {
            // Notify any linked neighbours of this new item.
            Tile t;
            int x = tile.X;
            int y = tile.Y;

            t = tile.World.GetTileAt(x, y + 1);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                // The North tile needs to be updated.
                t.Furniture.cbOnChanged(t.Furniture);
            }
            t = tile.World.GetTileAt(x + 1, y);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                // The East tile needs to be updated.
                t.Furniture.cbOnChanged(t.Furniture);
            }
            t = tile.World.GetTileAt(x, y - 1);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                // The South tile needs to be updated.
                t.Furniture.cbOnChanged(t.Furniture);
            }
            t = tile.World.GetTileAt(x - 1, y);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                // The West tile needs to be updated.
                t.Furniture.cbOnChanged(t.Furniture);
            }
        }

        return obj;
    }

    public void RegisterOnChangedCallback(Action<Furniture> cb)
    {
        cbOnChanged += cb;
    }

    public void UnRegisterOnChangedCallback(Action<Furniture> cb)
    {
        cbOnChanged -= cb;
    }

    public bool IsValidPosition(Tile t)
    {
        // Make sure Tile is of type Floor.
        if (t.Type != TileType.Floor)
        {
            Debug.Log("Tile is not a floor.");
            return false;
        }

        // Make sure Tile doesn't already have any Furniture.
        if (t.Furniture != null)
        {
            Debug.Log("Tile already has furniture.");
            return false;
        }

        return true;
    }

    //public bool IsValidPositionForDoor(Tile t)
    //{
    //    if (IsValidPosition(t) == false) return false;

    //    // TODO: Make sure we have either N/S walls or E/W walls.
    //    if (t.World.GetTileAt(t.X, t.Y + 1).Furniture.ObjectType == "Wall" &&
    //        t.World.GetTileAt(t.X, t.Y - 1).Furniture.ObjectType == "Wall")
    //    {
    //        return true;
    //    }
    //    if (t.World.GetTileAt(t.X + 1, t.Y).Furniture.ObjectType == "Wall" &&
    //        t.World.GetTileAt(t.X - 1, t.Y).Furniture.ObjectType == "Wall")
    //    {
    //        return true;
    //    }

    //    return true;
    //}
}
