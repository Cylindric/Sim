using System.Drawing;
using OpenTK;

namespace Sim.Primitives
{
    class Rectangle : GameObject
    {

        public Rectangle(Vector4 p)
        {
            Size = p.Zw;// new Vector2(p.Z, p.W);
            Position = p.Xy;// new Vector2(p.X, p.Y);
        }

        public Rectangle(Vector2 p1, Vector2 p2)
        {
            Size = p2;
            Position = p1;
        }

        public Color Color = Color.White;

        public override void Render(GraphicsController graphics)
        {
            graphics.SetColour(Color);
            graphics.RenderRectangle(new Vector4(Position.X, Position.Y, Position.X + Size.X, Position.Y + Size.Y));
            graphics.ClearColour();
        }
    }
}
