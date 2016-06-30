using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class Room
    {
        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        private float _atmosO2 = 0f;

        private float _atmosN = 0f;

        private float _atmosCo2 = 0f;

        private List<Tile> _tiles = new List<Tile>();

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void AssignTile(Tile t)
        {
            if (_tiles.Contains(t))
            {
                return;
            }

            if (t.Room != null)
            {
                t.Room._tiles.Remove(t);
            }

            t.Room = this;
            _tiles.Add(t);
        }

        public void UnassignAllTiles()
        {
            foreach (var t in _tiles)
            {
                t.Room = t.World.GetOutsideRoom();
            }
            _tiles = new List<Tile>();
        }

        public static void DoRoomFloodfill(Furniture furniture)
        {
            // Get a reference to the World, for convenience.
            var world = furniture.Tile.World;

            var oldRoom = furniture.Tile.Room;

            // Create a new Room on each side of the selected Furniture.
            foreach (var t in furniture.Tile.GetNeighbours())
            {
                FloodFill(t, oldRoom);
            }

            // Tiles with unpassable furniture are not in a room.
            furniture.Tile.Room = null;
            oldRoom._tiles.Remove(furniture.Tile);

            // Unassign all Tiles from the Room that the Furniture is in.
            // Never delete the outside Room.
            if (oldRoom != world.GetOutsideRoom())
            {
                if (oldRoom._tiles.Count > 0)
                {
                    Debug.LogError("oldRoom still has tiles assigned to it!");
                }
                world.DeleteRoom(oldRoom);
            }
        }

        private static void FloodFill(Tile tile, Room oldRoom)
        {
            if (tile == null)
            {
                return;
            }

            if (tile.Room != oldRoom)
            {
                // This Tile was already assigned to another new Room, so the direction picked isn'tile isolated.
                return;
            }

            if (tile.Furniture != null && tile.Furniture.IsRoomEnclosure)
            {
                // This Tile has a wall or door or something, so doesn'tile have a Room.
                return;
            }

            if (tile.Type == TileType.Empty)
            {
                // Empty Tile, so must stay "outside"
                return;
            }

            // If we get this far, we know that we need to create a new Room.

            var newRoom = new Room();

            var tilesToCheck = new Queue<Tile>();
            tilesToCheck.Enqueue(tile);

            while (tilesToCheck.Count > 0)
            {
                var t = tilesToCheck.Dequeue();

                if (t.Room == oldRoom)
                {
                    newRoom.AssignTile(t);

                    foreach (var t2 in t.GetNeighbours())
                    {
                        if (t2 == null || t2.Type == TileType.Empty)
                        {
                            // We've hit an open-space Tile - either edge of the World, or an empty Tile.
                            // That means this new Room is actually now connected to the outside, so it's not valid.
                            newRoom.UnassignAllTiles();
                            return;
                        }

                        if (t2.Room == oldRoom && (t2.Furniture == null || t2.Furniture.IsRoomEnclosure == false))
                        {
                            tilesToCheck.Enqueue(t2);
                        }
                    }
                }
            }

            // Copy data from the old room to the new room.
            newRoom._atmosCo2 = oldRoom._atmosCo2;
            newRoom._atmosN = oldRoom._atmosN;
            newRoom._atmosO2 = oldRoom._atmosO2;

            // Tell the World that a new Room has been created.
            tile.World.AddRoom(newRoom);
        }
    }
}
