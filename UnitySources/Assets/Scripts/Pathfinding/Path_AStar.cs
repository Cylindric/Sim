using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using Priority_Queue;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public class Path_AStar {

        private LinkedList<Tile> _path;

        public Path_AStar(World world, Tile tileStart, Tile tileEnd, string objectType = null, bool canTakeFromStockpile = false) {
            // If tileEnd is null, simply search for the nearest objectType, ignoring the A* Heuristic element.
            // Basically, use Dijkstra's algorithm.

            // Check to see if we have a valid Tile graph
            if(world.TileGraph == null) {
                world.TileGraph = new Path_TileGraph(world);
            }

            // A dictionary of all valid, walkable nodes.
            var nodes = world.TileGraph.nodes;

            // Make sure our start/end tiles are in the list of nodes!
            if(nodes.ContainsKey(tileStart) == false) {
                Debug.LogError("Path_AStar: The starting Tile isn't in the list of nodes!");

                return;
            }

            var start = nodes[tileStart];
            Path_Node<Tile> goal = null;

            if (tileEnd != null)
            {
                if (nodes.ContainsKey(tileEnd) == false)
                {
                    Debug.LogError("Path_AStar: The ending Tile isn't in the list of nodes!");
                    return;
                }

                goal = nodes[tileEnd];
            }


            // Mostly following this pseusocode:
            // https://en.wikipedia.org/wiki/A*_search_algorithm

            var closedSet = new List<Path_Node<Tile>>();

            var openSet = new SimplePriorityQueue<Path_Node<Tile>>();
            openSet.Enqueue( start, 0);

            var cameFrom = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

            var gScore = new Dictionary<Path_Node<Tile>, float>();
            foreach(var n in nodes.Values) {
                gScore[n] = Mathf.Infinity;
            }
            gScore[ start ] = 0;

            var fScore = new Dictionary<Path_Node<Tile>, float>();
            foreach(var n in nodes.Values) {
                fScore[n] = Mathf.Infinity;
            }
            fScore[ start ] = heuristic_cost_estimate( start, goal );

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (goal != null)
                {
                    if (current == goal)
                    {
                        // We have reached our goal!
                        // Let's convert this into an actual sequene of
                        // tiles to walk on, then end this constructor function!
                        reconstruct_path(cameFrom, current);
                        return;
                    }
                }
                else
                {
                    // Looking for inventory
                    if (current.data.Inventory != null && current.data.Inventory.ObjectType == objectType)
                    {
                        // Type is correct
                        if (canTakeFromStockpile || current.data.Furniture == null ||
                            current.data.Furniture.IsStockpile() == false)
                        {
                            reconstruct_path(cameFrom, current);
                            return;
                        }
                    }
                }

                closedSet.Add(current);

                foreach (var edgeNeighbor in current.edges)
                {
                    var neighbor = edgeNeighbor.node;

                    if (closedSet.Contains(neighbor))
                        continue; // ignore this already completed neighbor

                    var movementCostToNeighbor = neighbor.data.MovementCost*dist_between(current, neighbor);

                    var tentativeGScore = gScore[current] + movementCostToNeighbor;

                    if (openSet.Contains(neighbor) && tentativeGScore >= gScore[neighbor])
                        continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + heuristic_cost_estimate(neighbor, goal);

                    if (openSet.Contains(neighbor) == false)
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                    else
                    {
                        openSet.UpdatePriority(neighbor, fScore[neighbor]);
                    }
                }
            }

            // If we reached here, it means that we've burned through the entire
            // OpenSet without ever reaching a point where current == goal.
            // This happens when there is no path from start to goal
            // (so there's a wall or missing floor or something).

            // We don't have a failure state, maybe? It's just that the
            // path list will be null.
        }

        float heuristic_cost_estimate( Path_Node<Tile> a, Path_Node<Tile> b ) {

            if (b == null)
            {
                // We have no fixed destination, so probably just looking for an item.
                return 0f;
            }

            return Mathf.Sqrt(
                Mathf.Pow(a.data.X - b.data.X, 2) +
                Mathf.Pow(a.data.Y - b.data.Y, 2)
                );

        }

        float dist_between( Path_Node<Tile> a, Path_Node<Tile> b ) {
            // We can make assumptions because we know we're working
            // on a grid at this point.

            // Hori/Vert neighbours have a distance of 1
            if( Mathf.Abs( a.data.X - b.data.X ) + Mathf.Abs( a.data.Y - b.data.Y ) == 1 ) {
                return 1f;
            }

            // Diag neighbours have a distance of 1.41421356237	
            if( Mathf.Abs( a.data.X - b.data.X ) == 1 && Mathf.Abs( a.data.Y - b.data.Y ) == 1 ) {
                return 1.41421356237f;
            }

            // Otherwise, do the actual math.
            return Mathf.Sqrt(
                Mathf.Pow(a.data.X - b.data.X, 2) +
                Mathf.Pow(a.data.Y - b.data.Y, 2)
                );
        }

        void reconstruct_path(Dictionary<Path_Node<Tile>, Path_Node<Tile>> cameFrom, Path_Node<Tile> current)
        {
            // So at this point, current IS the goal.
            // So what we want to do is walk backwards through the Came_From
            // map, until we reach the "end" of that map...which will be
            // our starting node!
            var totalPath = new LinkedList<Tile>();
            totalPath.AddLast(current.data); // This "final" step is the path is the goal!

            while (cameFrom.ContainsKey(current))
            {
                // Came_From is a map, where the
                //    key => value relation is real saying
                //    some_node => we_got_there_from_this_node

                current = cameFrom[current];
                totalPath.AddLast(current.data);
            }

            // We don't need to have the start tile in the path, because that's where we already are.
            totalPath.RemoveLast();

            // At this point, total_path is a queue that is running
            // backwards from the END Tile to the START Tile, so let's reverse it.
            _path = new LinkedList<Tile>(totalPath.Reverse());
        }

        public Tile Dequeue()
        {
            var first = _path.First();
            _path.RemoveFirst();
            return first;
        }

        public int Length() {
            if (_path == null)
            {
                return 0;
            }

            return _path.Count;
        }

        public Tile EndTile()
        {
            if (_path == null) return null;
            if (_path.Count == 0) return null;

            return _path.Last();
        }
    }
}
