using System.Collections.Generic;
using Assets.Scripts.Model;

namespace Assets.Scripts.Pathfinding
{
    public class PathAStar
    {
        private Queue<Tile> _path;

        public PathAStar(World world, Tile start, Tile end)
        {
            
        }

        public Tile GetNextTile()
        {
            return _path.Dequeue();
        }
    }
}
