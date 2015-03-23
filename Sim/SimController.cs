using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;

namespace Sim
{
    public class SimController : GameWindow
    {
        readonly GraphicsController _graphics = new GraphicsController();
        private Sprite _sprite;

        public SimController()
            : base(800, 600, GraphicsMode.Default, "Sim", GameWindowFlags.Default)
        {
            this.VSync = VSyncMode.Off;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _graphics.Load(Color.White);

             _sprite = new Sprite(_graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _graphics.ResetDisplay(Width, Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            //shader.SetVariable("pixel_threshold", (((float)Mouse.X / (float)this.Width) + ((float)Mouse.Y / (float)this.Height)) / 30);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _graphics.BeginRender();
            _sprite.Render(new Vector2(50, 50), _graphics);
            _graphics.EndRender(this);
        }

    }
}