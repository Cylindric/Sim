using System;
using OpenTK;
using Sim.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Sim
{
    // http://www.policyalmanac.org/games/aStarTutorial.htm
    public class Astar
    {
        private class Node
        {
            public Map.Tile Tile;
            public Node Parent;

            /// <summary>
            /// The movement cost to move from the starting point A to a given
            /// square on the grid, following the path generated to get there. 
            /// </summary>
            public int StartToHereCostG;

            /// <summary>
            /// the estimated movement cost to move from that given square on 
            /// the grid to the final destination, point B. This is often 
            /// referred to as the heuristic, which can be a bit confusing. The 
            /// reason why it is called that is because it is a guess. We really
            /// don’t know the actual distance until we find the path, because 
            /// all sorts of things can be in the way (walls, water, etc.).
            /// </summary>
            public int HereToDestCostH;

            public int F
            {
                get { return StartToHereCostG + HereToDestCostH; }
            }
        }

        private readonly Map _map;
        private Vector2 _start;
        private Vector2 _destination;
        private readonly List<Node> _openList = new List<Node>();
        private readonly List<Node> _closedList = new List<Node>();
        private readonly List<Map.Tile> _finalRoute = new List<Map.Tile>(); 

        private Node _startNode;
        private Node _endNode;

        public bool Calculating { get; private set; }

        public List<Map.Tile> Route
        {
            get { return _finalRoute; }
        }

        public Astar(Map map)
        {
            _map = map;
            Calculating = false;
        }

        /// <summary>
        /// Begin a new navigation
        /// </summary>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        public void Navigate(Vector2 start, Vector2 destination)
        {
            _start = start;
            _destination = destination;
            Calculating = true;

            _startNode = new Node {Tile = _map.GetTileAtPosition(_start)};
            _endNode = new Node {Tile = _map.GetTileAtPosition(_destination)};
 
            // Begin at the starting point A and add it to an “open list” of 
            // squares to be considered. The open list is kind of like a 
            // shopping list. Right now there is just one item on the list, but
            // we will have more later. It contains squares that might fall 
            // along the path you want to take, but maybe not. Basically, this 
            // is a list of squares that need to be checked out.
           _openList.Add(_startNode);
        }

        /// <summary>
        /// Calculate the route loaded into A*.
        /// </summary>
        public void Calculate()
        {
            Console.WriteLine("Calculating route from ({0},{1}) to ({2},{3}).", _startNode.Tile.Row, _startNode.Tile.Column, _endNode.Tile.Row, _endNode.Tile.Column);
            if (!_endNode.Tile.IsWalkable)
            {
                Console.WriteLine("Destination tile is not walkable!");
                Calculating = false;
                return;
            }
            while (Calculating)
            {
                Step();
            }
        }

        /// <summary>
        /// Perform one step of the A* calculation.
        /// </summary>
        /// <remarks>
        /// Only performs a single iteration of the A* algorithm, so is useful 
        /// for limiting the run-time of the algorithm within a frame, or 
        /// displaying/demoing progress.
        /// </remarks>
        public void Step()
        {
            if (!Calculating)
            {
                return;
            }

            // To continue the search, we simply choose the lowest F score 
            // square from all those that are on the open list.
            var checkNode = _openList.OrderBy(n => n.F).Take(1).FirstOrDefault();
            if (checkNode == null)
            {
                Calculating = false;
                return;
            }

            // We then do the following with the selected square
            // Drop it from the open list and add it to the closed list.
            _openList.Remove(checkNode);
            _closedList.Add(checkNode);
            if (checkNode.Tile == _endNode.Tile)
            {
                // Done!
                var node = checkNode;
                _finalRoute.Add(node.Tile);
                while(true)
                {
                    node = node.Parent;
                    if (node == null)
                    {
                        _openList.Clear();
                        _closedList.Clear();
                        Calculating = false;
                        return;
                    }
                    _finalRoute.Add(node.Tile);
                }
            }

            // Check all of the adjacent squares. Ignoring those that are on 
            // the closed list or unwalkable (terrain with walls, water, or 
            // other illegal terrain), add squares to the open list if they 
            // are not on the open list already. Make the selected square the 
            // “parent” of the new squares.
            foreach (var t in _map.ReachableTiles(checkNode.Tile))
            {
                if (_closedList.Any(n => n.Tile == t))
                {
                    continue;
                }

                var existingOpenTile = _openList.FirstOrDefault(n => n.Tile == t);
                if (existingOpenTile != null)
                {
                    // If an adjacent square is already on the open list, 
                    // check to see if this path to that square is a better 
                    // one. In other words, check to see if the G score for
                    // that square is lower if we use the current square to get
                    // there. If not, don’t do anything.
                    // On the other hand, if the G cost of the new path is 
                    // lower, change the parent of the adjacent square to the 
                    // selected square (in the diagram above, change the 
                    // direction of the pointer to point at the selected 
                    // square). Finally, recalculate both the F and G scores
                    // of that square.
                    var newG = checkNode.StartToHereCostG + 1;
                    if (newG < existingOpenTile.StartToHereCostG)
                    {
                        existingOpenTile.Parent = checkNode;
                        existingOpenTile.StartToHereCostG = newG;
                    }
                }
                else
                {
                    var newNode = new Node
                    {
                        Tile = t,
                        Parent = checkNode,
                        StartToHereCostG= checkNode.StartToHereCostG+ 1,
                        HereToDestCostH = _map.ManhattanDistance(t, _endNode.Tile)
                    };

                    _openList.Add(newNode);                    
                }
            }    
        }
    }
}
