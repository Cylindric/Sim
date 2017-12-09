using System;
using System.Collections.Generic;
using Assets.Scripts.Model;
using Priority_Queue;

namespace Assets.Scripts.Pathfinding
{
    public class RoomFinder
    {
        public Tile FindClosestRoom(Tile startTile, Func<Room, bool> isValid)
        {
            // Use a simple Dijkstra search to find the nearest room matching the criteria.

            if (World.Instance.Rooms.Count <= 1)
            {
                return null;
            }

            return FindClosestTile(startTile, t =>
            {
                if (t.Room == null) return false;
                return isValid(t.Room);
            });
        }

        public Tile FindClosestTile(Tile startTile, Func<Tile, bool> isValid)
        {
            // Check to see if we have a valid Tile graph
            if (World.Instance.TileGraph == null)
            {
                World.Instance.TileGraph = new Path_TileGraph(World.Instance);
            }

            // A dictionary of all valid, walkable nodes.
            var nodes = World.Instance.TileGraph.Nodes;

            var start = nodes[startTile];

            var frontier = new SimplePriorityQueue<Path_Node<Tile>>();
            frontier.Enqueue(start, 0f);

            var costSoFar = new Dictionary<Path_Node<Tile>, float>();

            costSoFar.Add(start, 0f);

            Path_Node<Tile> current = null;

            var found = false;
            while (frontier.Count > 0)
            {
                current = frontier.Dequeue();

                if (isValid(current.data))
                {
                    found = true;
                    break;
                }

                foreach (var edgeNeighbour in current.edges)
                {
                    var next = edgeNeighbour.node;

                    var newCost = costSoFar[current] + next.data.MovementCost;

                    if (costSoFar.ContainsKey(next) == false || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        frontier.Enqueue(next, newCost);
                    }
                }
            }

            if (found == false || current == null) return null;

            return current.data;
        }
    }
}