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
        [DebuggerDisplay("id={id} parent={parent} G={startToHereCostG} H={hereToDestCostH} F={F}")]
        private class Node
        {
            public int Id;
            public int Parent;

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
        private readonly GraphicsController _graphics;
        private readonly List<GameObject> _particleList = new List<GameObject>();
        private Vector2 _start;
        private Vector2 _destination;
        private readonly List<Node> _openList = new List<Node>();
        private readonly List<Node> _closedList = new List<Node>();
        private readonly List<Node> _finalRoute = new List<Node>(); 

        private Node _startNode;
        private Node _endNode;

        public bool Calculating { get; private set; }

        public Astar(Map map, GraphicsController graphics)
        {
            _map = map;
            _graphics = graphics;
            Calculating = false;
        }

        public void Navigate(Vector2 start, Vector2 destination)
        {
            _start = start;
            _destination = destination;
            Calculating = true;

            var p = new Crosshair(_start, 20, _graphics)
            {
                Color = Color.Wheat,
            };
            _particleList.Add(p);

            p = new Crosshair(_destination, 20, _graphics)
            {
                Color = Color.Red,
            };
            _particleList.Add(p);

            _startNode = new Node {Id = _map.GetTileIdFromPosition(_start), Parent = -1};
            _endNode = new Node {Id = _map.GetTileIdFromPosition(_destination)};
 
            // Begin at the starting point A and add it to an “open list” of 
            // squares to be considered. The open list is kind of like a 
            // shopping list. Right now there is just one item on the list, but
            // we will have more later. It contains squares that might fall 
            // along the path you want to take, but maybe not. Basically, this 
            // is a list of squares that need to be checked out.
           _openList.Add(_startNode);

            // Look at all the reachable or walkable squares adjacent to the 
            // starting point, ignoring squares with walls, water, or other 
            // illegal terrain. Add them to the open list, too. For each of 
            // these squares, save point A as its “parent square”. This parent 
            // square stuff is important when we want to trace our path. It 
            // will be explained more later.
            foreach (var t in _map.ReachableTiles(_startNode.Id))
            {
                var g = _startNode.StartToHereCostG + 1;
                var h = _map.ManhattanDistance(t, _endNode.Id);

                var node = new Node
                {
                    Id = t,
                    Parent = _startNode.Id,
                    StartToHereCostG = g,
                    HereToDestCostH = h
                };
                _openList.Add(node);
            }

            // Drop the starting square A from your open list, and add it to a 
            // “closed list” of squares that you don’t need to look at again 
            // for now.
            _openList.Remove(_startNode);
            _closedList.Add(_startNode);
        }

        public void Update(double timeDelta)
        {
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
            if (checkNode.Id == _endNode.Id)
            {
                // Done!
                var node = checkNode;
                _finalRoute.Add(node);
                while(true)
                {
                    node = _closedList.FirstOrDefault(n => n.Id == node.Parent);
                    if (node == null)
                    {
                        Calculating = false;
                        return;
                    }
                    else
                    {
                        _finalRoute.Add(node);
                    }
                }
            }

            // Check all of the adjacent squares. Ignoring those that are on 
            // the closed list or unwalkable (terrain with walls, water, or 
            // other illegal terrain), add squares to the open list if they 
            // are not on the open list already. Make the selected square the 
            // “parent” of the new squares.
            foreach (var t in _map.ReachableTiles(checkNode.Id))
            {
                if (_closedList.Any(n => n.Id == t))
                {
                    continue;
                }

                var existingOpenTile = _openList.FirstOrDefault(n => n.Id == t);
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
                        existingOpenTile.Parent = checkNode.Id;
                        existingOpenTile.StartToHereCostG = newG;
                    }
                }
                else
                {
                    var newNode = new Node
                    {
                        Id = t,
                        Parent = checkNode.Id,
                        StartToHereCostG= checkNode.StartToHereCostG+ 1,
                        HereToDestCostH = _map.ManhattanDistance(t, _endNode.Id)

                    };

                    _openList.Add(newNode);                    
                }
            }    
        }

        public void Render()
        {
            foreach (var p in _particleList)
            {
                p.Render();
            }

            // Render the open list
            foreach (var t in _openList)
            {
                var dim = _map.GetTileDimFromId(t.Id);
                var centre = dim.Xy + ((dim.Zw - dim.Xy) / 2.0f);

                var dimParent = _map.GetTileDimFromId((t.Parent));
                var centreParent = dimParent.Xy + ((dimParent.Zw - dimParent.Xy) / 2.0f);

                var pointToParent = (centreParent - centre).Normalized() * 15f;

                _graphics.SetColour(Color.PaleGreen);
                _graphics.RenderRectangle(dim);
                _graphics.SetColour(Color.White);
                _graphics.RenderLine(centre, centre + pointToParent);
                _graphics.ClearColour();
            }

            foreach (var t in _closedList)
            {
                var dim = _map.GetTileDimFromId(t.Id);
                var centre = dim.Xy + ((dim.Zw - dim.Xy) / 2.0f);

                var dimParent = _map.GetTileDimFromId((t.Parent));
                var centreParent = dimParent.Xy + ((dimParent.Zw - dimParent.Xy) / 2.0f);

                var pointToParent = (centreParent - centre).Normalized() * 15f;

                _graphics.SetColour(Color.CornflowerBlue);
                _graphics.RenderRectangle(dim);
                _graphics.SetColour(Color.White);
                _graphics.RenderLine(centre, centre + pointToParent);
                _graphics.ClearColour();
            }

            foreach (var t in _finalRoute)
            {
                var dim = _map.GetTileDimFromId(t.Id);
                var centre = dim.Xy + ((dim.Zw - dim.Xy) / 2.0f);

                var dimParent = _map.GetTileDimFromId((t.Parent));
                var centreParent = dimParent.Xy + ((dimParent.Zw - dimParent.Xy) / 2.0f);

                var pointToParent = (centreParent - centre).Normalized() * 40f;

                _graphics.SetColour(Color.DarkRed);
                _graphics.RenderRectangle(dim);
                _graphics.SetColour(Color.Red);
                _graphics.RenderLine(centre, centre + pointToParent);
                _graphics.ClearColour();
            }
        }
    }
}
