using System;
using System.Collections.Generic;
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
        private Character[] _characters;
        private AiController _ai;
        private readonly Timer _timer = new Timer();
        private List<string> _availableCharList = new List<string>() {"beardman", "oldman", "redgirl", "blueboy"};
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
            _characters = new Character[50];
            for (var i = 0; i < _characters.Length; i++)
            {
                _characters[i] = new Character(Random.Instance.Next<string>(_availableCharList), this, _graphics);
                while (_map.CheckCollision(_characters[i].Hitbox))
                {
                    _characters[i].Position = new Vector2(Random.Instance.Next(20, Width - 20), Random.Instance.Next(20, Height - 20));
                }
                _characters[i].State = Random.Instance.Next<Character.CharacterState>();
                _characters[i].Direction= Random.Instance.Next<Character.CharacterDirection>();
            }

            _ai = new AiController(_map, _characters);
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

            _ai.Update();

            foreach (var c in _characters)
            {
                c.Update(Timer.ElapsedSeconds);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _graphics.BeginRender();
            _map.Render(_graphics);
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