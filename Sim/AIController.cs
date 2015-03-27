using OpenTK;
using OpenTK.Input;
using System;
namespace Sim
{
    class AiController
    {
        private const long MinTimeInState = 100;
        private const long MinTimeInDirection = 100;
        private const double StateChangeChance = 0.002;
        private const double DirectionChangeChance = 0.001;

        private readonly Map _map;
        private readonly Character[] _characters;

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
                _characters[0].State = Character.CharacterState.Standing;
            }
        }


        public void Update(double timeDelta)
        {
            HandleKeyboardState();

            foreach (var character in _characters)
            {
                UpdateCharacter(character, timeDelta);
            }
        }

        private void UpdateCharacter(Character character, double timeDelta)
        {
            // Decide wether to change what the Char is doing
            if (character.TimeInState > MinTimeInState && Random.Instance.NextDouble() <= StateChangeChance)
            {
                // Randomly flip to a different state
                character.State = Character.CharacterState.Walking;// Random.Instance.Next<Character.CharacterState>();
            }

            if (character.TimeInDirection > MinTimeInDirection && Random.Instance.NextDouble() <= DirectionChangeChance)
            {
                // Randomly flip to a different direction
                character.Direction = Random.Instance.Next<Character.CharacterDirection>();
            }

            // If the character is moving, check for map collisions
            //if (character.State == Character.CharacterState.Walking)
            //{
            //    var nextLocation = character.Position + (character.Velocity * (float)timeDelta);
            //    Console.WriteLine("AI:UC Moving character to {0:###.0}, {1:###.0}.", nextLocation.X, nextLocation.Y);
            //    var newHitbox = character.Hitbox;
            //    newHitbox.X = nextLocation.X;
            //    newHitbox.Y = nextLocation.Y;
            //    if (!_forceMove && _map.CheckCollision(newHitbox))
            //    {
            //        Console.WriteLine("AI.cs:UpdateCharacter Move cancelled, character collided with the map.");
            //        character.Stop();
            //    }
            //}
        }
    }
}
