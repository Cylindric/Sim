namespace Sim
{
    public class Particle : GameObject
    {
        public Particle(GraphicsController graphics)
            : base(graphics)
        {
            LoadSpritesheet("font");
        }

        public override void Update(double timeDelta)
        {
            base.Update(timeDelta);

            if(TimeToLive <= 0)
            {
                return;
            }

            _timeInFrame += timeDelta;

            if (_timeInFrame >= 0.2f)
            {
                _frameNumber++;
                _frameNumber = _frameNumber % 10;
                _timeInFrame = 0;
            }

        }

        public override void Render()
        {
            if(TimeToLive <= 0)
            {
                return;
            }
            _spritesheet.Render(10+_frameNumber, Position, Graphics);
        }

        private double _timeInFrame;
        private int _frameNumber;
    }
}
