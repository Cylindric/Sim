using System;
using OpenTK;
using OpenTK.Input;
using System.Linq;
using Sim.DataFormats;

namespace Sim
{
    class Character : GameObject
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
                Hitbox = new Vector4(_position.X, _position.Y, _spritesheet.SpriteWidth, _spritesheet.SpriteWidth);
            }
        }

        public Vector4 Hitbox { get; private set; }
        public long TimeInState { get; private set; }
        public long TimeInDirection { get; private set; }
        public Vector2 Velocity { get; set; }
        public string Name { get; set; }

        private readonly SpritesheetController _spritesheet;
        private const float MaxSpeed = 50.0f;
        private SimController _sim;
        private float _frameTime;
        private int _thisFrame;
        private int _nextFrame;
        private int _frameNum;
        private readonly Font _font;

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
        public bool DebugShowPosition { get; set; }

        public Character(string name, SimController sim, GraphicsController graphics) : 
            base(graphics)
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

            _spritesheet = new SpritesheetController(data.SpritesheetName, Graphics);
            Position = new Vector2(0, 0);
            _font = new Font(Graphics);
            _thisFrame = _idleDownFrames[0];
            _nextFrame = _thisFrame;
            _frameNum = 0;
        }

        public override void Update(float timeDelta)
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

            // render the hitbox
            if (DebugShowHitbox)
            {
                _font.Position = Position - new Vector2(0, 12);
                _font.Size = 16;
                _font.Text = Name;
            }
        }


        public override void Render()
        {
            _spritesheet.Render(_thisFrame, Position, Graphics);
            
            // render the hitbox
            if (DebugShowHitbox)
            {
                Graphics.SetColour(new Vector4(1, 0, 0, 0.5f));
                Graphics.RenderRectangle(new Vector4(Hitbox.X, Hitbox.Y, Hitbox.X + Hitbox.Z, Hitbox.Y + Hitbox.W));
                Graphics.ClearColour();
                _font.Render();
            }

            // render the direction
            if (DebugShowVelocity)
            {
                Graphics.SetColour(new Vector4(0, 0, 1, 0.5f));
                var centre = new Vector2(Hitbox.X + Hitbox.Z / 2, Hitbox.Y + Hitbox.W / 2);
                Graphics.RenderLine(centre, new Vector2(centre.X + Velocity.X, centre.Y + Velocity.Y));
            }

            // render the position
            if (DebugShowPosition)
            {
                Graphics.SetColour(new Vector4(0, 1, 0, 0.5f));
                Graphics.RenderLine(Position + new Vector2(-5, 0), Position + new Vector2(5, 0));
                Graphics.RenderLine(Position + new Vector2(0, -5), Position + new Vector2(0, 5));
            }

            Graphics.ClearColour();

        }

    }
}
