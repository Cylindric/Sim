using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Models;
using Priority_Queue;
using Engine.Utilities;
using Debug = Engine.Utilities.Debug;


namespace Engine.Pathfinding
{
    public class Path_AStar
    {
        public Path_AStar()
        {
            ForcedRoutableTiles = new List<Tile>();
        }

        public Path_AStar(Path_AStar other)
        {
            this._path = other._path;
            this._reachable = other._reachable;
        }

        public World World { get; set; }
        public Tile Start { get; set; }
        public Tile End { get; set; }
        public bool CanTakeFromStockpile { get; set; }
        public string ObjectType { get; set; }
        public List<Tile> ForcedRoutableTiles { get; set; } 

        private LinkedList<Tile> _path;
        private bool _reachable;
        private bool _debugVis = true;

        public bool IsReachable
        {
            get { return _path != null && _reachable; }
        }

        public bool IsUnReachable
        {
            get { return !IsReachable; }
        }
        
        public void Calculate() {
            if (World == null)
            {
                throw new InvalidOperationException("Invalid path calculation - no World set");
            }
            if (Start == null)
            {
                throw new InvalidOperationException("Invalid path calculation - no start set");
            }

            // If tileEnd is null, simply search for the nearest objectType, ignoring the A* Heuristic element.
            // Basically, use Dijkstra's algorithm.

            _reachable = true;

            // Check to see if we have a valid Tile graph
            if(this.World.TileGraph == null) {
                this.World.TileGraph = new Path_TileGraph(this.World);
            }

            // A dictionary of all valid, walkable nodes.
            var nodes = this.World.TileGraph.Nodes;

            if (_debugVis && End != null)
            {
                // UnityEngine.Debug.DrawLine(new Vector3(Start.X, Start.Y, -2), new Vector3(End.X, End.Y, -2), Colour.Red, TimeController.Instance.DeltaTime * 5, false);
                this.World.TileGraph.DebugVis();
            }

            // Make sure our start/end tiles are in the list of nodes!
            if (nodes.ContainsKey(Start) == false) {
                Debug.LogError("Path_AStar: The starting Tile isn't in the list of nodes!");
                _reachable = false;
                return;
            }

            var start = nodes[Start];
            Path_Node<Tile> goal = null;

            if (End != null)
            {
                if (nodes.ContainsKey(End) == false)
                {
                    Debug.LogError("Path_AStar: The ending Tile isn't in the list of nodes!");
                    _reachable = false;
                    return;
                }

                goal = nodes[End];
            }


            // Mostly following this pseusocode:
            // https://en.wikipedia.org/wiki/A*_search_algorithm

            var closedSet = new List<Path_Node<Tile>>();

            var openSet = new SimplePriorityQueue<Path_Node<Tile>>();
            openSet.Enqueue( start, 0);

            var cameFrom = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

            // Set up the array of G values
            var gScore = new Dictionary<Path_Node<Tile>, float>();
            foreach(var n in nodes.Values) {
                gScore[n] = Mathf.Infinity;
            }
            gScore[ start ] = 0;

            // Set up the array of F values
            var fScore = new Dictionary<Path_Node<Tile>, float>();
            foreach(var n in nodes.Values) {
                fScore[n] = Mathf.Infinity;
            }
            fScore[ start ] = HeuristicCostEstimate( start, goal );

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
                        ReconstructPath(cameFrom, current);
                        _reachable = true;
                        return;
                    }
                }
                else
                {
                    // Looking for inventory
                    if (current.data.Inventory != null && current.data.Inventory.ObjectType == ObjectType)
                    {
                        // Type is correct
                        if (CanTakeFromStockpile || current.data.Furniture == null ||
                            current.data.Furniture.IsStockpile() == false)
                        {
                            ReconstructPath(cameFrom, current);
                            _reachable = true;
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

                    // If the neighbour is impassable, give this node an infinite cost
                    var movementCostToNeighbor = Mathf.Infinity;
                    if (!Mathf.Approximately(neighbor.data.MovementCost, 0f))
                    {
                        movementCostToNeighbor = neighbor.data.MovementCost * DistanceBetween(current, neighbor);
                    }

                    var tentativeGScore = gScore[current] + movementCostToNeighbor;

                    if (openSet.Contains(neighbor) && tentativeGScore >= gScore[neighbor])
                        continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);

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
            _reachable = false;

            // We don't have a failure state, maybe? It's just that the
            // path list will be null.
        }

        public Tile Dequeue()
        {
            if (_path == null || _path.Count == 0)
            {
                return null;
            }
            var first = _path.First();
            _path.RemoveFirst();
            return first;
        }

        public int Length()
        {
            return _path == null ? 0 : _path.Count;
        }

        public Tile EndTile()
        {
            if (_path == null || _path.Count == 0) return null;
            return _path.Last();
        }

        public void DumpDebugInfo()
        {
            Debug.LogFormat("Dumping path with {0} nodes from [{1},{2}] to [{3}{4}].", _path.Count, Start.X, Start.Y, End == null ? -1 : End.X, End == null ? -1 : End.Y);
            foreach (var n in _path)
            {
                Debug.LogFormat("[{0},{1},{2}]", n.X, n.Y, n.MovementCost);
            }
        }

        private static float HeuristicCostEstimate(Path_Node<Tile> a, Path_Node<Tile> b)
        {

            if (b == null)
            {
                // We have no fixed destination, so probably just looking for an item.
                return 0f;
            }

            return (float)Math.Sqrt(
                Math.Pow(a.data.X - b.data.X, 2) +
                Math.Pow(a.data.Y - b.data.Y, 2)
                );

        }

        private static float DistanceBetween(Path_Node<Tile> a, Path_Node<Tile> b)
        {
            // We can make assumptions because we know we're working
            // on a grid at this point.

            // Hori/Vert neighbours have a distance of 1
            if (Math.Abs(a.data.X - b.data.X) + Math.Abs(a.data.Y - b.data.Y) == 1)
            {
                return 1f;
            }

            // Diag neighbours have a distance of 1.41421356237	
            if (Math.Abs(a.data.X - b.data.X) == 1 && Math.Abs(a.data.Y - b.data.Y) == 1)
            {
                return 1.41421356237f;
            }

            // Otherwise, do the actual math.
            return (float)Math.Sqrt(
                Math.Pow(a.data.X - b.data.X, 2) +
                Math.Pow(a.data.Y - b.data.Y, 2)
                );
        }

        private void ReconstructPath(IDictionary<Path_Node<Tile>, Path_Node<Tile>> cameFrom, Path_Node<Tile> current)
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
                // if (_debugVis) UnityEngine.Debug.DrawLine(new Vector3(current.data.X, current.data.Y, -2), new Vector3(cameFrom[current].data.X, cameFrom[current].data.Y, -2), Color.green, 3f);

                current = cameFrom[current];
                totalPath.AddLast(current.data);
            }

            // We don't need to have the start tile in the path, because that's where we already are.
            totalPath.RemoveLast();

            // At this point, total_path is a queue that is running
            // backwards from the END Tile to the START Tile, so let's reverse it.
            _path = new LinkedList<Tile>(totalPath.Reverse());
        }

    }
}
