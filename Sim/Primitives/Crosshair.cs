using System.Drawing;
using OpenTK;

namespace Sim.Primitives
{
    class Crosshair : GameObject
    {

        public Crosshair(Vector2 p, int size)
        {
            Size = new Vector2(size);
            Position = p;
        }

        public Color Color = Color.White;

        public override void Render(GraphicsController graphics)
        {
            graphics.SetColour(Color);
            graphics.RenderLine(new Vector2(Position.X - Size.X / 2, Position.Y), new Vector2(Position.X + Size.X / 2, Position.Y));
            graphics.RenderLine(new Vector2(Position.X, Position.Y - Size.Y / 2), new Vector2(Position.X, Position.Y + Size.Y / 2));
            graphics.ClearColour();
        }
    }
}
