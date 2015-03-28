using System.Drawing;
using OpenTK;

namespace Sim.Primitives
{
    class Crosshair : GameObject
    {

        public Crosshair(Vector2 p, int size, GraphicsController graphics)
            : base(graphics)
        {
            Size = new Vector2(size);
            Position = p;
        }

        public Color Color = Color.White;

        public override void Update(double timeDelta)
        {
            base.Update(timeDelta);
        }

        public override void Render()
        {
            Graphics.SetColour(Color);
            Graphics.RenderLine(new Vector2(Position.X - Size.X / 2, Position.Y), new Vector2(Position.X + Size.X / 2, Position.Y));
            Graphics.RenderLine(new Vector2(Position.X, Position.Y - Size.Y / 2), new Vector2(Position.X, Position.Y + Size.Y / 2));
            Graphics.ClearColour();
        }
    }
}
