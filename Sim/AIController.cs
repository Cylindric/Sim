using OpenTK;
using OpenTK.Input;
using System;
namespace Sim
{
    class AiController
    {
        private const long MinTimeInState = 2000;
        private const long MinTimeInDirection = 2000;
        private const double StateChangeChance = 0.01;
        private const double DirectionChangeChance = 0.04;

        private readonly MapController _map;
        private readonly Character[] _characters;

        public AiController(MapController map, Character[] characters)
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
            if (character.State == Character.CharacterState.Walking)
            {
                var nextLocation = character.Position + character.Velocity * timeDelta;
                var newHitbox = character.Hitbox;
                newHitbox.X = nextLocation.X;
                newHitbox.Y = nextLocation.Y;
                if (_map.CheckCollision(newHitbox))
                {
                    character.State = Character.CharacterState.Standing;
                }
            }

        }

    }
}
