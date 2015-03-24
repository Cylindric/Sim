using System.ComponentModel;
using System.Runtime.Hosting;
using OpenTK;
using OpenTK.Input;
using System.Linq;
using Sim.DataFormats;

namespace Sim
{
    class Character
    {
        private readonly SpritesheetController _spritesheet;
        private Vector2 _position;
        private Vector2 _velocity;
        private const float MaxSpeed = 50.0f;
        private SimController _sim;
        private readonly GraphicsController _graphics;
        private float _frameTime;
        private int _thisFrame;
        private int _nextFrame;
        private int _frameNum;
        private State _state;
        private Direction _direction;

        private int[] _walkDownFrames = { 0 };
        private int[] _walkLeftFrames = { 0 };
        private int[] _walkRightFrames = { 0 };
        private int[] _walkUpFrames = { 0 };

        private int[] _idleDownFrames = { 0 };
        private int[] _idleLeftFrames = { 0 };
        private int[] _idleRightFrames = { 0 };
        private int[] _idleUpFrames = { 0 };

        private enum State
        {
            Standing,
            Walking
        }

        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public Character(string name, SimController sim, GraphicsController graphics)
        {
            var data = ResourceController.Load<CharacterDatafile>(ResourceController.GetDataFilename("character.{0}.txt", name));
            
            _walkDownFrames = data.WalkDownFrames;
            _walkLeftFrames = data.WalkLeftFrames;
            _walkRightFrames = data.WalkRightFrames;
            _walkUpFrames = data.WalkUpFrames;
            _idleDownFrames = data.IdleDownFrames;
            _idleLeftFrames = data.IdleLeftFrames;
            _idleRightFrames = data.IdleRightFrames;
            _idleUpFrames = data.IdleUpFrames;
            
            _sim = sim;
            _graphics = graphics;

            _spritesheet = new SpritesheetController(data.SpritesheetName, graphics);
            _position = new Vector2(0, 0);
            _thisFrame = _idleDownFrames[0];
            _nextFrame = _thisFrame;
            _frameNum = 0;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
        }

        public void Update(float timeDelta)
        {
            _frameTime += timeDelta;

            var originalVelocity = _velocity;

            var state = Keyboard.GetState();
            if (state[Key.Right])
            {
                _velocity.X = MaxSpeed;
                _velocity.Y = 0;
                _state = State.Walking;
                _direction = Direction.Right;
             }
            else if (state[Key.Left])
            {
                _velocity.X = -MaxSpeed;
                _velocity.Y = 0;
                _state = State.Walking;
                _direction = Direction.Left;
            }
            else if (state[Key.Up])
            {
                _velocity.X = 0;
                _velocity.Y = -MaxSpeed;
                _state = State.Walking;
                _direction = Direction.Up;
            }
            else if (state[Key.Down])
            {
                _velocity.X = 0;
                _velocity.Y = MaxSpeed;
                _state = State.Walking;
                _direction = Direction.Down;
            }
            else
            {
                _velocity.X = 0;
                _velocity.Y = 0;
                _state = State.Standing;
            }

            if (_frameTime > 1.0f/4)
            {
                _frameNum++;
                switch (_state)
                {
                    case State.Standing:
                        switch (_direction)
                        {
                            case Direction.Down:
                                _nextFrame = _idleDownFrames[_frameNum % _idleDownFrames.Count()];
                                break;
                            case Direction.Left:
                                _nextFrame = _idleLeftFrames[_frameNum % _idleLeftFrames.Count()];
                                break;
                            case Direction.Right:
                                _nextFrame = _idleRightFrames[_frameNum % _idleRightFrames.Count()];
                                break;
                            case Direction.Up:
                                _nextFrame = _idleUpFrames[_frameNum % _idleUpFrames.Count()];
                                break;
                        }
                        break;

                    case State.Walking:
                        switch (_direction)
                        {
                            case Direction.Down:
                                _nextFrame = _walkDownFrames[_frameNum % _walkDownFrames.Count()];
                                break;
                            case Direction.Left:
                                _nextFrame = _walkLeftFrames[_frameNum % _walkLeftFrames.Count()];
                                break;
                            case Direction.Right:
                                _nextFrame = _walkRightFrames[_frameNum % _walkRightFrames.Count()];
                                break;
                            case Direction.Up:
                                _nextFrame = _walkUpFrames[_frameNum % _walkUpFrames.Count()];
                                break;
                        }
                        break;
                }

                _thisFrame = _nextFrame;
                _frameTime = 0;
            }

            _position += (_velocity * timeDelta);
        }

        public void Render()
        {
            _spritesheet.Render(_thisFrame, _position, _graphics);
        }

    }
}
