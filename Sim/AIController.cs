using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using System;
using Sim.Primitives;
using Rectangle = Sim.Primitives.Rectangle;

namespace Sim
{
    public class AiController
    {
        private const long MinTimeInState = 100;
        private const long MinTimeInDirection = 100;
        private const double StateChangeChance = 0.002;
        private const double DirectionChangeChance = 0.001;

        private readonly Map _map;
        private readonly Character[] _characters;
        private readonly List<GameObject> _particles = new List<GameObject>();
        private Tile _clickedTile;

        private readonly Dictionary<Character, Astar> _pendingPathfinders = new Dictionary<Character, Astar>();
        private readonly Dictionary<Character, Astar> _completePathfinders = new Dictionary<Character, Astar>();

        public AiController(Map map, Character[] characters)
        {
            _map = map;
            _characters = characters;
        }

        public void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'h':
                    Console.WriteLine("Toggling Hitboxes.");
                    _map.DebugShowHitbox = !_map.DebugShowHitbox;
                    foreach (var character in _characters)
                    {
                        character.DebugShowHitbox = !character.DebugShowHitbox;
                    }
                    break;

                case 'v':
                    Console.WriteLine("Toggling Velocities.");
                    foreach (var character in _characters)
                    {
                        character.DebugShowVelocity = !character.DebugShowVelocity;
                    }
                    break;

                case 'p':
                    Console.WriteLine("Toggling Positions.");
                    foreach (var character in _characters)
                    {
                        character.DebugShowPosition = !character.DebugShowPosition;
                    }
                    break;
            }
        }

        public void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                _clickedTile = _map.GetTileAtPosition(new Vector2(e.X, e.Y));
            }
        }

        private void HandleKeyboardState()
        {
            var state = Input.GetState;

            if (state[Key.Right])
            {
                _characters[0].Direction = Character.CharacterDirection.Right;
                _characters[0].State = Character.CharacterState.Walking;
            }
            else if (state[Key.Left])
            {
                _characters[0].Direction = Character.CharacterDirection.Left;
                _characters[0].State = Character.CharacterState.Walking;
            }
            else if (state[Key.Down])
            {
                _characters[0].Direction = Character.CharacterDirection.Down;
                _characters[0].State = Character.CharacterState.Walking;
            }
            else if (state[Key.Up])
            {
                _characters[0].Direction = Character.CharacterDirection.Up;
                _characters[0].State = Character.CharacterState.Walking;
            }
            else
            {
                //_characters[0].State = Character.CharacterState.Standing;
            }
        }

        private void HandleMouseEvents()
        {
            if (_clickedTile != null)
            {
                if (_clickedTile.IsWalkable)
                {
                    GiveCharacterNewDestination(_characters[0], _clickedTile.LocationPx);
                    _clickedTile = null;
                }
            }
        }

        public void Update(double timeDelta)
        {
            HandleKeyboardState();
            HandleMouseEvents();

            // Handle one pending pathfinder
            if (_pendingPathfinders.Count > 0)
            {
                var pathfinder = _pendingPathfinders.Take(1).FirstOrDefault();
                pathfinder.Value.Calculate();
                _completePathfinders.Add(pathfinder.Key, pathfinder.Value);
                _pendingPathfinders.Remove(pathfinder.Key);
            }

            foreach (var p in _particles)
            {
                p.Update(timeDelta);
            }

            foreach (var character in _characters)
            {
                UpdateCharacter(character, timeDelta);
            }

            _particles.RemoveAll(p => p.IsDead);
        }


        private void UpdateCharacter(Character character, double timeDelta)
        {
            if (character.State == Character.CharacterState.HeadingToDestination)
            {
                if (_completePathfinders.ContainsKey(character))
                {
                    var route = _completePathfinders[character].Route;
                    if (route.Count == 0)
                    {
                        // unreachable destination!
                        character.State = Character.CharacterState.Standing;
                    }
                    else
                    {
                        // For debugging, draw the route
                        bool first = true;
                        foreach (var t in route)
                        {
                            var p = new Rectangle(t.LocationPx, t.SizePx);
                            p.Color = first ? Color.GreenYellow : Color.DarkRed;
                            p.TimeToLive = 0.1;
                            _particles.Add(p);
                            first = false;
                        }


                        var nextTile = route[route.Count - 1];
                        var distance = Vector2.Subtract(character.Position, nextTile.LocationPx).LengthFast;
                        if (distance < 1f)
                        {
                            // We have reached the target tile
                            route.RemoveAt(route.Count - 1);
                        }
                        character.Destination = nextTile.LocationPx;
                        _particles.Add(new Crosshair(character.Destination, 10) {TimeToLive = 0.1, Color = Color.Blue});
                    }
                }
                else
                {
                    // A character heading somewhere but without a route there, should be left to it.
                    return;
                }
            }

            if (character.State == Character.CharacterState.Standing)
            {
                // A character standing around has a chance of deciding to do something else.
                //if (character.TimeInState > MinTimeInState && Random.Instance.NextDouble() <= StateChangeChance)
                //{
                //    // Randomly flip to a different state
                //    GiveCharacterRandomDestination(character);
                //}
            }
        }


        public void Render(GraphicsController graphics)
        {
            foreach (var p in _particles)
            {
                p.Render(graphics);
            }
        }


        private void GiveCharacterRandomDestination(Character character)
        {
            // Pick a random place on the map that is reachable
            var destination = new Vector2(Random.Instance.Next(0, (int) _map.MapSize.X),
                Random.Instance.Next(0, (int) _map.MapSize.Y));

            while (!_map.GetTileAtPosition(destination).IsWalkable)
            {
                destination = new Vector2(Random.Instance.Next(0, (int)_map.MapSize.X),
                    Random.Instance.Next(0, (int) _map.MapSize.Y));
            }
            GiveCharacterNewDestination(character, destination);
        }


        private void GiveCharacterNewDestination(Character character, Vector2 destination)
        {
            character.Destination = destination;

            // Clear any already-completed routes
            if (_completePathfinders.ContainsKey(character))
            {
                _completePathfinders.Remove(character);
            }

            // Create a new pathfinder for the character
            var astar = new Astar(_map);
            astar.Navigate(character.Position, character.Destination);
            _pendingPathfinders[character] = astar;

            character.State = Character.CharacterState.HeadingToDestination;
        }


    }
}
