using System.Collections.Generic;
using Engine.Models;
// using UnityEngine;

namespace Engine.Pathfinding
{
    public class Path_TileGraph {

        // This class constructs a simple path-finding compatible graph
        // of our world.  Each Tile is a node. Each WALKABLE neighbour
        // from a Tile is linked via an edge connection.

        private bool _debugVis = false;

        public Dictionary<Tile, Path_Node<Tile>> Nodes;

        public Path_TileGraph(World world) {

            // Loop through all tiles of the world
            // For each Tile, create a node
            //  Do we create nodes for non-floor tiles?  NO!
            //  Do we create nodes for tiles that are completely unwalkable (i.e. walls)?  NO!

            Nodes = new Dictionary<Tile, Path_Node<Tile>>();

            for (var x = 0; x < world.Width; x++) {
                for (var y = 0; y < world.Height; y++) {

                    var t = world.GetTileAt(x,y);
                    var n = new Path_Node<Tile>();
                    n.data = t;
                    Nodes.Add(t, n);
                }
            }

            // Now loop through all nodes again, creating edges for neighbours
            foreach(var t in Nodes.Keys) {
                var n = Nodes[t];

                var edges = new List<Path_Edge<Tile>>();

                // Get a list of neighbours for the Tile
                var neighbours = t.GetNeighbours(true);	// NOTE: Some of the array spots could be null.

                // If neighbour is walkable, create an edge to the relevant node.
                foreach (var neighbour in neighbours)
                {
                    if(neighbour != null) {
                        var e = new Path_Edge<Tile>();
                        e.cost = neighbour.MovementCost;
                        if (IsClippingCorner(t, neighbour)) // Discourage routing through diagonal gaps by resetting the movement cost to 0.
                        {
                            e.cost = 0f;
                        }
                        e.node = Nodes[ neighbour ];

                        // Add the edge to our temporary (and growable!) list
                        edges.Add(e);
                    }
                }

                n.edges = edges.ToArray();
            }
        }

        /// <summary>
        /// Overlay a visualisation of the Tile Graph.
        /// </summary>
        /// <remarks>
        /// Blue lines from the centre of each tile indicate possible routes out of that tile.
        /// Red lines indicate an unsuitable path out of a tile, but they exist for special path-finding purposes.
        /// </remarks>
        public void DebugVis()
        {
            if (!_debugVis) return;

            foreach (var  n in Nodes)
            {
                var start = new Vector3(n.Key.X, n.Key.Y, -2);
                foreach (var n2 in n.Value.edges)
                {
                    var end = new Vector3(n2.node.data.X, n2.node.data.Y, -2);
                    var end2 = start + ((end - start)/3);
                    end2.z = -2;
                    var c = new Color(0f, 0f, 1f, 0.5f);
                    if (Mathf.Approximately(n2.cost, 0f))
                    {
                        c = new Color(1f, 0f, 0f, 0.2f);
                    }
                    Debug.DrawLine(start, end2, c);
                }
            }
        }

        /// <summary>
        /// Return true if moving from the current tile to the neighbour tile would be clipping through a diagonal corner
        /// </summary>
        /// <param name="curr">The start tile</param>
        /// <param name="neigh">The neighbouring tile</param>
        /// <returns>True if clipping; else false.</returns>
        /// <remarks>
        /// Moving from A to B here would be clipping.
        /// 
        ///       A ██████
        ///   ██████B
        /// 
        /// </remarks>
        private static bool IsClippingCorner(Tile curr, Tile neigh)
        {

            // If the distance is not "2", then this can't be a clipping move
            if (Mathf.Abs(curr.X - neigh.X) + Mathf.Abs(curr.Y - neigh.Y) != 2) return false;

            // We are diagonal
            var dX = curr.X - neigh.X;
            var dY = curr.Y - neigh.Y;

            if (Mathf.Approximately(World.Instance.GetTileAt(curr.X - dX, curr.Y).MovementCost, 0))
            {
                // E or W is unwalkable, so this would be a clipped movement.
                return true;
            }

            if (Mathf.Approximately(World.Instance.GetTileAt(curr.X, curr.Y - dY).MovementCost, 0))
            {
                // N or S is unwalkable, so this would be a clipped movement.
                return true;
            }

            return false;
        }
    }
}
