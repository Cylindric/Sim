using OpenTK;
using Sim.DataFormats;
using System.Drawing;
using System.Linq;

namespace Sim
{
    public class Character : GameObject
    {
        public enum CharacterState
        {
            Standing,
            Walking,
            HeadingToDestination
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

        public long TimeInState { get; private set; }
        public long TimeInDirection { get; private set; }
        public Vector2 Velocity { get; set; }
        public string Name { get; set; }
        public Vector2 Destination { get; set; }

        private const float MaxSpeed = 50.0f;
        private double _frameTime;
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

        public bool DebugShowHitbox {get; set; }
        public bool DebugShowVelocity { get; set; }
        public bool DebugShowPosition { get; set; }

        public Character(string name, GraphicsController graphics) : 
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
            
            LoadSpritesheet(data.SpritesheetName);
            
            _font = new Font(Graphics);
            Position = new Vector2(0, 0);
            _thisFrame = _idleDownFrames[0];
            _nextFrame = _thisFrame;
            _frameNum = 0;
        }

        public void Stop()
        {
            //Console.WriteLine("Character.cs:Stop Character stopped!");
            Velocity = new Vector2(0);
            State = CharacterState.Standing;
        }

        public void Update(double timeDelta, Map map)
        {
            base.Update(timeDelta);

             // Step timing data
            _frameTime += timeDelta;
            TimeInState = Timer.GetTime() - _lastStateChange;
            TimeInDirection = Timer.GetTime() - _lastDirectionChange;

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
                if (Velocity.Length > 0)
                {
                    //Console.WriteLine("Character.cs:Step {0:###.0},{1:###.0} Character stopped.", Position.X, Position.Y);
                    Velocity = new Vector2(0);
                }
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

            if(Velocity.X != 0 || Velocity.Y != 0)
            {
                var newPos = Position + (Velocity * (float)timeDelta);
                var newHitbox = new Vector4(Hitbox) {X = newPos.X, Y = newPos.Y};
                if (map.CheckCollision(newHitbox))
                {
                    //Console.WriteLine("C:U Move cancelled, character collided with the map.");
                    Stop();
                }
                else
                {
                    //Console.WriteLine("C:U {0:###.0},{1:###.0} > {2:###.0},{3:###.0} Character moving.", Position.X, Position.Y, newPos.X, newPos.Y);
                    Position = newPos;
                }
            }

            // render the hitbox
            if (DebugShowHitbox)
            {
                _font.Position = Position - new Vector2(0, 12);
                _font.FontSize = 16;
                _font.Text = Name;
            }
        }

        public override void Render()
        {
            Spritesheet.Render(_thisFrame, Position, Graphics);
            
            // render the hitbox
            if (DebugShowHitbox)
            {
                Graphics.SetColour(Color.Red);
                Graphics.RenderRectangle(new Vector4(Hitbox.X, Hitbox.Y, Hitbox.X + Hitbox.Z, Hitbox.Y + Hitbox.W));
                Graphics.ClearColour();
                _font.Render();
            }

            // render the direction
            if (DebugShowVelocity)
            {
                Graphics.SetColour(Color.Blue);
                var centre = new Vector2(Hitbox.X + Hitbox.Z / 2, Hitbox.Y + Hitbox.W / 2);
                Graphics.RenderLine(centre, new Vector2(centre.X + Velocity.X, centre.Y + Velocity.Y));
                if ((Destination - centre).LengthFast > 1 && State == CharacterState.HeadingToDestination)
                {
                    Graphics.RenderLine(centre, Destination);
                }
            }

            // render the position
            if (DebugShowPosition)
            {
                Graphics.SetColour(Color.Green);
                Graphics.RenderLine(Position + new Vector2(-5, 0), Position + new Vector2(5, 0));
                Graphics.RenderLine(Position + new Vector2(0, -5), Position + new Vector2(0, 5));
            }

            Graphics.ClearColour();

        }

    }
}
