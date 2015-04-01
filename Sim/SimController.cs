using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace Sim
{
    public class SimController : GameWindow
    {
        readonly GraphicsController _graphics = new GraphicsController();
        private Map _map;
        private Character[] _characters;
        private AiController _ai;
        private readonly List<string> _availableCharList = new List<string>() {"beardman", "oldman", "redgirl", "blueboy", "capeboy"};
        public SimController()
            : base(800, 600, GraphicsMode.Default, "Sim", GameWindowFlags.Default)
        {
            this.VSync = VSyncMode.Adaptive;
        }

        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine("OnLoad");
            base.OnLoad(e);

            _graphics.Load(Color.White);
            _map = new Map("test", _graphics);
            _characters = new Character[20];
            for (var i = 0; i < _characters.Length; i++)
            {
                _characters[i] = new Character(Random.Instance.Next<string>(_availableCharList), _graphics);
                while (_map.CheckCollision(_characters[i].Hitbox))
                {
                    //Console.WriteLine("Moving character {0} due to being in a wall ({1},{2}).", i, _characters[i].Position.X, _characters[i].Position.Y);
                    _characters[i].Position = new Vector2(Random.Instance.Next(20, Width - 20), Random.Instance.Next(20, Height - 20));
                    //Console.WriteLine("New position is ({0},{1}).", _characters[i].Position.X, _characters[i].Position.Y);
                }
                _characters[i].State = Character.CharacterState.Standing;
                _characters[i].Direction= Random.Instance.Next<Character.CharacterDirection>();
                _characters[i].Name = i.ToString(CultureInfo.InvariantCulture);
            }

            _ai = new AiController(_map, _characters);
        }

        protected override void OnResize(EventArgs e)
        {
            Console.WriteLine("OnResize");
            base.OnResize(e);
            _graphics.ResetDisplay(0, 0, Width, Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            Timer.Update();

            _map.Update(Timer.ElapsedSeconds);
            
            _ai.Update(Timer.ElapsedSeconds);
            
            foreach (var c in _characters)
            {
                c.Update(Timer.ElapsedSeconds, _map);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _graphics.BeginRender();
            _map.Render();
             foreach (var c in _characters.OrderBy(c => c.Position.Y))
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

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            _ai.OnKeyPress(e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _graphics.Dispose();
            }
        }
    }
}