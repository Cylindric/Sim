using OpenTK;
using OpenTK.Input;
using System;
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
            if (character.State == Character.CharacterState.HeadingToDestination)
            {
                // A character heading somewhere should be left to it.
                return;
            }

            if (character.State == Character.CharacterState.Standing)
            {
                // A character standing around has a chance of deciding to do something else.
                if (character.TimeInState > MinTimeInState && Random.Instance.NextDouble() <= StateChangeChance)
                {
                    // Randomly flip to a different state
                    GiveCharacterRandomDestination(character);
                }
            }
        }

        private void GiveCharacterRandomDestination(Character character)
        {
            // Pick a random place on the map
            character.Destination = new Vector2(Random.Instance.Next(0, (int)_map.MapSize.X), Random.Instance.Next(0, (int)_map.MapSize.Y));
            character.State = Character.CharacterState.HeadingToDestination;
        }

    }
}
