using System.Collections.Generic;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    /// <summary>
    /// Constructs a simple pathfinding-compatible graph of our World.
    /// Each Tile is a Node, and each walkable neighbour from a Tile is linked via an Edge.
    /// </summary>
    public class PathTileGraph
    {
        public Dictionary<Tile, PathNode<Tile>> Nodes { get; private set; }

        public PathTileGraph(World world)
        {
            Debug.Log("Building new PathTileGraph.");

            Nodes = new Dictionary<Tile, PathNode<Tile>>();

            // Build a node for every walkable Tile on the map.
            // Do not build nodes for empty tiles.
            for (int x = 0; x < world.Width; x++)
            {
                for (int y = 0; y < world.Height; y++)
                {
                    var t = world.GetTileAt(x, y);
                    if (t.MovementCost > 0)
                    {
                        var n = new PathNode<Tile>();
                        n.Data = t;
                        Nodes.Add(t, n);
                    }
                }
            }

            Debug.LogFormat("Added {0} nodes.", Nodes.Count);

            // Build edges for every neighbour
            var edgeCount = 0;
            foreach (var t in Nodes.Keys)
            {
                PathNode<Tile> n = Nodes[t];
                List<PathEdge<Tile>> edges = new List<PathEdge<Tile>>();

                Tile[] neighbours = t.GetNeighbours(true); // NOTE: some array spots may be null.

                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (neighbours[i] != null && neighbours[i].MovementCost > 0)
                    {
                        PathEdge<Tile> e = new PathEdge<Tile>();
                        e.Cost = neighbours[i].MovementCost;
                        e.Node = Nodes[neighbours[i]];
                        edges.Add(e);
                        edgeCount++;
                    }
                }
                n.Edges = edges.ToArray();
            }
            Debug.LogFormat("Added {0} edges.", edgeCount);

            Debug.Log("Built new PathTileGraph.");
        }
    }
}
