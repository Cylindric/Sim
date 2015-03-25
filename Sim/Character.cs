using System;
using OpenTK;
using OpenTK.Input;
using System.Linq;
using Sim.DataFormats;

namespace Sim
{
    class Character
    {
        public enum CharacterState
        {
            Standing,
            Walking
        }

        public enum CharacterDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        
        public CharacterState State
        {
            get { return _state; }
            set
            {
                if (value != _state)
                {
                    _lastStateChange = Timer.GetTime();
                }
                _state = value;
            }
        }

        public CharacterDirection Direction
        {
            get { return _direction; }
            set
            {
                if (value != _direction)
                {
                    _lastDirectionChange = Timer.GetTime();
                }
                _direction = value;
            }
        }

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                Hitbox = new Vector4(_position.X, _position.Y, _spritesheet.SpriteSize, _spritesheet.SpriteSize);
            }
        }

        public Vector4 Hitbox { get; private set; }
        public long TimeInState { get; private set; }
        public long TimeInDirection { get; private set; }
        public Vector2 Velocity { get; set; }

        private readonly SpritesheetController _spritesheet;
        private const float MaxSpeed = 50.0f;
        private SimController _sim;
        private readonly GraphicsController _graphics;
        private float _frameTime;
        private int _thisFrame;
        private int _nextFrame;
        private int _frameNum;

        private readonly int[] _walkDownFrames = { 0 };
        private readonly int[] _walkLeftFrames = { 0 };
        private readonly int[] _walkRightFrames = { 0 };
        private readonly int[] _walkUpFrames = { 0 };

        private readonly int[] _idleDownFrames = { 0 };
        private readonly int[] _idleLeftFrames = { 0 };
        private readonly int[] _idleRightFrames = { 0 };
        private readonly int[] _idleUpFrames = { 0 };
        private CharacterDirection _direction;
        private long _lastDirectionChange;
        private CharacterState _state;
        private long _lastStateChange;
        private Vector2 _position;

        public bool DebugShowHitbox {get; set; }
        public bool DebugShowVelocity { get; set; }

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
            Position = new Vector2(0, 0);
            _thisFrame = _idleDownFrames[0];
            _nextFrame = _thisFrame;
            _frameNum = 0;
        }

        public void Update(float timeDelta)
        {
            // Update timing data
            _frameTime += timeDelta;
            TimeInState = Timer.GetTime() - _lastStateChange;
            TimeInDirection = Timer.GetTime() - _lastDirectionChange;

            var originalVelocity = Velocity;

            if (_state == CharacterState.Walking)
            {
                switch (_direction)
                {
                    case CharacterDirection.Down:
                        Velocity = new Vector2(0, MaxSpeed);
                        break;

                    case CharacterDirection.Left:
                        Velocity = new Vector2(-MaxSpeed, 0);
                        break;

                    case CharacterDirection.Right:
                        Velocity = new Vector2(MaxSpeed, 0);
                        break;

                    case CharacterDirection.Up:
                        Velocity = new Vector2(0, -MaxSpeed);
                        break;
                }
            }
            else
            {
                Velocity = new Vector2(0);
            }

            //var state = Keyboard.GetState();
            //if (state[Key.Right])
            //{
            //    _velocity.X = MaxSpeed;
            //    _velocity.Y = 0;
            //    State = CharacterState.Walking;
            //    Direction = CharacterDirection.Right;
            // }
            //else if (state[Key.Left])
            //{
            //    _velocity.X = -MaxSpeed;
            //    _velocity.Y = 0;
            //    State = CharacterState.Walking;
            //    Direction = CharacterDirection.Left;
            //}
            //else if (state[Key.Up])
            //{
            //    _velocity.X = 0;
            //    _velocity.Y = -MaxSpeed;
            //    State = CharacterState.Walking;
            //    Direction = CharacterDirection.Up;
            //}
            //else if (state[Key.Down])
            //{
            //    _velocity.X = 0;
            //    _velocity.Y = MaxSpeed;
            //    State = CharacterState.Walking;
            //    Direction = CharacterDirection.Down;
            //}
            //else
            //{
            //    _velocity.X = 0;
            //    _velocity.Y = 0;
            //    State = CharacterState.Standing;
            //}

            if (_frameTime > 1.0f/4)
            {
                _frameNum++;
                switch (State)
                {
                    case CharacterState.Standing:
                        switch (Direction)
                        {
                            case CharacterDirection.Down:
                                _nextFrame = _idleDownFrames[_frameNum % _idleDownFrames.Count()];
                                break;
                            case CharacterDirection.Left:
                                _nextFrame = _idleLeftFrames[_frameNum % _idleLeftFrames.Count()];
                                break;
                            case CharacterDirection.Right:
                                _nextFrame = _idleRightFrames[_frameNum % _idleRightFrames.Count()];
                                break;
                            case CharacterDirection.Up:
                                _nextFrame = _idleUpFrames[_frameNum % _idleUpFrames.Count()];
                                break;
                        }
                        break;

                    case CharacterState.Walking:
                        switch (Direction)
                        {
                            case CharacterDirection.Down:
                                _nextFrame = _walkDownFrames[_frameNum % _walkDownFrames.Count()];
                                break;
                            case CharacterDirection.Left:
                                _nextFrame = _walkLeftFrames[_frameNum % _walkLeftFrames.Count()];
                                break;
                            case CharacterDirection.Right:
                                _nextFrame = _walkRightFrames[_frameNum % _walkRightFrames.Count()];
                                break;
                            case CharacterDirection.Up:
                                _nextFrame = _walkUpFrames[_frameNum % _walkUpFrames.Count()];
                                break;
                        }
                        break;
                }

                _thisFrame = _nextFrame;
                _frameTime = 0;
            }

            Position += (Velocity * timeDelta);
        }


        public void Render()
        {
            _spritesheet.Render(_thisFrame, Position, _graphics);
            
            // render the hitbox
            if (DebugShowHitbox)
            {
                _graphics.SetColour(new Vector4(1, 0, 0, 1));
                _graphics.RenderRectangle(new Vector4(Hitbox.X, Hitbox.Y, Hitbox.X + Hitbox.Z, Hitbox.Y + Hitbox.W));
                _graphics.ClearColour();
            }

            // render the direction
            if (DebugShowVelocity)
            {
                _graphics.SetColour(new Vector4(0, 0, 1, 1));
                var centre = new Vector2(Hitbox.X + Hitbox.Z / 2, Hitbox.Y + Hitbox.W / 2);
                _graphics.RenderLine(centre, new Vector2(centre.X + Velocity.X, centre.Y + Velocity.Y));
            }

            _graphics.ClearColour();

        }

    }
}
