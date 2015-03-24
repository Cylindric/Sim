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
        private Character[] _characters;
        private readonly Timer _timer = new Timer();
        private Random _random = new Random();


        public SimController()
            : base(800, 600, GraphicsMode.Default, "Sim", GameWindowFlags.Default)
        {
            this.VSync = VSyncMode.Off;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _graphics.Load(Color.White);

            _characters = new Character[100];
            for (var i = 0; i < _characters.Length; i++)
            {
                _characters[i] = new Character(_random.NextDouble() >= 0.5 ? "beardman" : "oldman", this, _graphics);
                _characters[i].SetPosition(new Vector2(_random.Next(20, Width-20), _random.Next(20, Height-20)));
            }
             _map = new MapController(_graphics);
             
            //_character = new Character("beardman", this, _graphics);
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

            //_character.Update(Timer.ElapsedSeconds);

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _graphics.BeginRender();
            _map.Render(_graphics);
            //_character.Render();
            foreach (var c in _characters)
            {
                c.Render();
            }

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