using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace Sim
{
    public class SimController : GameWindow
    {
        readonly GraphicsController _graphics = new GraphicsController();
        private MapController _map;
        private Character _character;
        private readonly Timer _timer = new Timer();

        public SimController()
            : base(800, 600, GraphicsMode.Default, "Sim", GameWindowFlags.Default)
        {
            this.VSync = VSyncMode.Off;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _graphics.Load(Color.White);

             _map = new MapController(_graphics);
             _character = new Character("beardman", this, _graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _graphics.ResetDisplay(Width, Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            Timer.Update();

            _character.Update(Timer.ElapsedSeconds);

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _graphics.BeginRender();
            _map.Render(_graphics);
            _character.Render();
            _graphics.EndRender(this);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
 
            if (e.Key == Key.Escape)
            {
                Exit();
            }
        }
    }
}