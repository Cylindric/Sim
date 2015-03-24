using System.ComponentModel;
using OpenTK;
using OpenTK.Input;
using System.Linq;

namespace Sim
{
    class Character
    {
        private readonly string _datafile;
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
            _datafile = ResourceController.GetDataFile(string.Format("character.{0}.txt", name));
            var datafileSpritesheet = "people";
            _walkDownFrames = new int[4] { 0, 1, 2, 1 };
            _walkLeftFrames = new int[4] { 12, 13, 14, 13 };
            _walkRightFrames = new int[4] { 24, 25, 26, 25 };
            _walkUpFrames = new int[4] { 36, 37, 38, 37 };
            _idleDownFrames = new int[1] { 1 };
            _idleLeftFrames = new int[1] { 13 };
            _idleRightFrames = new int[1] { 25 };
            _idleUpFrames = new int[1] { 37 };

            _sim = sim;
            _graphics = graphics;

            _spritesheet = new SpritesheetController(string.Format("spritesheet.{0}.txt", datafileSpritesheet), graphics);
            _position = new Vector2(0, 0);
            _thisFrame = 0;
            _nextFrame = 0;
            _frameNum = 0;
        }

        public void Update(float timeDelta)
        {
            _frameTime += timeDelta;

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
