using OpenTK;
using OpenTK.Input;
using System;
namespace Sim
{
    class AiController
    {
        private const long MinTimeInState = 1000;
        private const long MinTimeInDirection = 1000;
        private const double StateChangeChance = 0.2;
        private const double DirectionChangeChance = 0.1;

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

        public void Update(float timeDelta)
        {
            foreach (var character in _characters)
            {
                UpdateCharacter(character, timeDelta);
            }
        }

        private void UpdateCharacter(Character character, float timeDelta)
        {
            // Decide wether to change what the Char is doing
            if (character.TimeInState > MinTimeInState && Random.Instance.NextDouble() <= StateChangeChance)
            {
                // Randomly flip to a different state
                character.State = Random.Instance.Next<Character.CharacterState>();
            }

            if (character.TimeInDirection > MinTimeInDirection && Random.Instance.NextDouble() <= DirectionChangeChance)
            {
                // Randomly flip to a different direction
                character.Direction = Random.Instance.Next<Character.CharacterDirection>();
            }

            // If the character is moving, check for map collisions
            if (character.State == Character.CharacterState.Walking && character.Velocity.Length > 0)
            {
                var nextLocation = character.Position + character.Velocity * timeDelta;
                //Console.WriteLine("Char at {0},{1} checking for move to {2},{3}.", character.Position.X, character.Position.Y, nextLocation.X, nextLocation.Y);
                var newHitbox = character.Hitbox;
                newHitbox.X = nextLocation.X;
                newHitbox.Y = nextLocation.Y;
                if (_map.CheckCollision(newHitbox))
                {
                    //Console.WriteLine("Move cancelled, character collided with the map.");
                    character.State = Character.CharacterState.Standing;
                }
            }
        }
    }
}
