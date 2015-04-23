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

        private Font _mousePositionLabel;
        private bool _showMousePosition;

        private Font _helpText;
        private bool _showHelpText;

        private const double UpdateStepTime = 1.0d / 60;
        private double _simulationAccumulator;

        private readonly List<string> _availableCharList = new List<string>
        {
            "beardman",
            "oldman",
            "redgirl",
            "blueboy",
            "capeboy"
        };

        public SimController()
            : base(880, 480, GraphicsMode.Default, "Sim", GameWindowFlags.Default)
        {
            VSync = VSyncMode.Adaptive;
        }

        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine("OnLoad");
            base.OnLoad(e);

            _graphics.Load(Color.White);
            _map = new Map("test", _graphics);
            _characters = new Character[1];
            for (var i = 0; i < _characters.Length; i++)
            {
                _characters[i] = new Character(Random.Instance.Next(_availableCharList), _graphics);
                while (_map.CheckCollision(_characters[i].Hitbox))
                {
                    _characters[i].Position = new Vector2(Random.Instance.Next(20, Width - 20), Random.Instance.Next(20, Height - 20));
                }
                _characters[i].State = Character.CharacterState.Standing;
                _characters[i].Direction= Random.Instance.Next<Character.CharacterDirection>();
                _characters[i].Name = i.ToString(CultureInfo.InvariantCulture);
            }

            _ai = new AiController(_map, _characters);

            _mousePositionLabel = new Font(_graphics)
            {
                FontSize = 16, 
                Text = string.Format("{0:00},{1:00}", 0, 0)
            };

            _helpText = new Font(_graphics)
            {
                FontSize = 20,
                Text = "h Show character hitboxes\n" +
                       "m Show mouse coordinates\n" +
                       "v Show character velocities\n" +
                       "? Show help",
                Position = new Vector2(10, 10)
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _graphics.ResetDisplay(0, 0, Width, Height);
            _mousePositionLabel.Position = new Vector2(10, Height - 20);
        }
 
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            Timer.Update();
            _simulationAccumulator += Timer.ElapsedSeconds;

            while (_simulationAccumulator >= UpdateStepTime)
            {
                _map.Update(UpdateStepTime);
                _ai.Update(UpdateStepTime);

                foreach (var c in _characters)
                {
                    c.Update(UpdateStepTime, _map);
                }

                _mousePositionLabel.Update(UpdateStepTime);

                _simulationAccumulator -= UpdateStepTime;
            }

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _graphics.BeginRender();
            _map.Render(_graphics);
            _ai.Render(_graphics);
            
            foreach (var c in _characters.OrderBy(c => c.Position.Y))
            {
                c.Render(_graphics);
            }

            if (_showMousePosition) 
                _mousePositionLabel.Render(_graphics);

            if (_showHelpText)
                _helpText.Render(_graphics);
            
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

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == '?')
            {
                _showHelpText = !_showHelpText;
            }
            else if (e.KeyChar == 'm')
            {
                _showMousePosition = !_showMousePosition;
            }

            _ai.OnKeyPress(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            _ai.OnMouseDown(e);

            var tile = _map.GetTileAtPosition(new Vector2(e.X, e.Y));
            Console.WriteLine("Clicked at {0},{1} ({2},{3})", e.X, e.Y, tile.Row, tile.Column);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            var x = Math.Min(Math.Max(0, e.X), _map.MapSize.X);
            var y = Math.Min(Math.Max(0, e.Y), _map.MapSize.Y);

            var tile = _map.GetTileAtPosition(new Vector2(x, y));
            _mousePositionLabel.Text = string.Format("{0:00},{1:00} ({2},{3})", e.X, e.Y, tile.Column, tile.Row);
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