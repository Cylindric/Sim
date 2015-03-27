using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sim
{
    class Rectangle : GameObject
    {

        public Rectangle(Vector4 p, GraphicsController graphics) : base(graphics)
        {
            Size = p.Zw;// new Vector2(p.Z, p.W);
            Position = p.Xy;// new Vector2(p.X, p.Y);
        }

        public Color Color = Color.White;

        public override void Update(double timeDelta)
        {
            base.Update(timeDelta);
        }

        public override void Render()
        {
            Graphics.SetColour(Color);
            Graphics.RenderRectangle(new Vector4(Position.X, Position.Y, Position.X + Size.X, Position.Y + Size.Y));
            Graphics.ClearColour();
        }
    }
}
