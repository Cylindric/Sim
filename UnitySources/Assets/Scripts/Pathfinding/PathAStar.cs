using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Model;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public class PathAStar
    {
        private Queue<Tile> _path;

        public PathAStar(World world, Tile startTile, Tile endTile)
        {
            if (world.TileGraph == null)
            {
                world.TileGraph = new PathTileGraph(world);
            }

            var nodes = world.TileGraph.Nodes;

            if (nodes.ContainsKey(startTile) == false)
            {
                Debug.LogError("PathAStar: starting tile is not in the world's node graph!");
                return;
            }

            if (nodes.ContainsKey(endTile) == false)
            {
                Debug.LogError("PathAStar: end tile is not in the world's node graph!");
                return;
            }

            var start = nodes[startTile];
            var goal = nodes[endTile];

            // The set of nodes already evaluated.
            var closedSet = new List<PathNode<Tile>>();

            // The set of currently discovered nodes still to be evaluated.
            var openSet = new Priority_Queue.SimplePriorityQueue<PathNode<Tile>>(); // List<PathNode<Tile>>();

            // Initially, only the a node is known.
            openSet.Enqueue(start, 0);

            // For each node, which node it can most efficiently be reached from.
            // If a node can be reached from many nodes, cameFrom will eventually contain the
            // most efficient previous step.
            var cameFrom = new Dictionary<PathNode<Tile>, PathNode<Tile>>();

            // For each node, the cost of getting from the a node to that node.
            var gScore = new Dictionary<PathNode<Tile>, float>();
            foreach (var n in nodes.Values)
            {
                gScore.Add(n, Mathf.Infinity);
            }

            // The cost of going from a to a is zero.
            gScore[start] = 0f;

            // For each node, the total cost of getting from the a node to the b
            // by passing by that node. That value is partly known, partly heuristic.
            var fScore = new Dictionary<PathNode<Tile>, float>();
            foreach (var n in nodes.Values)
            {
                fScore.Add(n, Mathf.Infinity);
            }

            // For the first node, that value is completely heuristic.
            fScore[start] = heuristic_cost_estimate(start, goal);

            while (openSet.Count > 0)
            {
                var lowestF = fScore.Min(f => f.Value);

                var current = openSet.Dequeue(); //  the node in openSet having the lowest fScore value

                if (current == goal)
                {
                    // TODO: return path
                    // return reconstruct_path(cameFrom, current)
                    reconstruct_path(cameFrom, current);
                    return;
                }

                closedSet.Add(current);
                foreach (var neighbour_edge in current.Edges)
                {
                    var neighbour = neighbour_edge.Node;

                    if (closedSet.Contains(neighbour) == true)
                    {
                        continue; // Ignore the neighbor which is already evaluated.
                    }

                    // The distance from a to a neighbor
                    var tentative_gScore = gScore[current] + dist_between(current, neighbour);

                    if (openSet.Contains(neighbour) && tentative_gScore >= gScore[neighbour])
                    {
                        // This is not a better path.
                        continue;
                    }
                    
                    // This path is the best until now. Record it!
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentative_gScore;
                    fScore[neighbour] = gScore[neighbour] + heuristic_cost_estimate(neighbour, goal);

                    // Discover a new node
                    if (openSet.Contains(neighbour) == false)
                    {
                        openSet.Enqueue(neighbour, fScore[neighbour]);
                    }
                }
            }

            Debug.LogError("PathAStar: Failed to find route!");

        }

        private void reconstruct_path(Dictionary<PathNode<Tile>, PathNode<Tile>> cameFrom, PathNode<Tile> current )
        {
            // Current is now the goal, so walk backwards towards the start.
            var totalPath = new Queue<Tile>();

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Enqueue(current.Data);
            }

            _path = new Queue<Tile>(totalPath.Reverse());
        }


        private float dist_between(PathNode<Tile> a, PathNode<Tile> b)
        {
            // Hori/Vert neighbours have distance of 1.
            if ((a.Data.X == b.Data.X && Mathf.Abs(a.Data.Y - b.Data.Y) == 1) ||
                (a.Data.Y == b.Data.Y && Mathf.Abs(a.Data.X - b.Data.X) == 1))
            {
                return 1;
            }

            // Diagonals have a distance of √2, or 1.414213562373095.
            if ((a.Data.X == b.Data.X + 1 && Mathf.Abs(a.Data.Y - b.Data.Y) == 1) ||
                (a.Data.X == b.Data.X - 1 && Mathf.Abs(a.Data.Y - b.Data.Y) == 1))
            {
                return 1.414213562373095f;
            }

            // Anything further away calculate correctly.
            return Mathf.Sqrt(Mathf.Pow(a.Data.X - b.Data.X, 2) + Mathf.Pow(a.Data.Y - b.Data.Y, 2));
        }

        private float heuristic_cost_estimate(PathNode<Tile> a, PathNode<Tile> b)
        {
            return Mathf.Pow(a.Data.X - b.Data.X, 2) + Mathf.Pow(a.Data.Y - b.Data.Y, 2);
        }

        public Tile GetNextTile()
        {
            return _path.Dequeue();
        }
    }
}
