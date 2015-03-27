using System.Globalization;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Sim;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleTest
{
    class ParticleTest : GameWindow
    {
        /*
        * Test
        */
        private Particle _particle;
        private List<Particle> _particleList = new List<Particle>();
        /**/

        private GraphicsController _graphics;
        Timer timer = new Timer();

        public ParticleTest()
            : base(800, 600, GraphicsMode.Default, "Test", GameWindowFlags.Default)
        {
            this.VSync = VSyncMode.Off;
        }

        static void Main(string[] args)
        {
            using (var game = new ParticleTest())
                game.Run();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Renderer.Call(() => GL.ClearColor(Color4.SkyBlue));
            Renderer.Call(() => GL.Enable(EnableCap.Texture2D));

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            var viewPort = new int[4];
            Renderer.Call(() => GL.GetInteger(GetPName.Viewport, viewPort));
            Renderer.Call(() => GL.Enable(EnableCap.Blend));

            Renderer.Call(() => GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha));

            Renderer.Call(() => GL.MatrixMode(MatrixMode.Projection));
            Renderer.Call(() => GL.Viewport(viewPort[0], viewPort[1], viewPort[2], viewPort[3]));
            Renderer.Call(() => GL.Ortho(viewPort[0], viewPort[0] + viewPort[2], viewPort[1] + viewPort[3], viewPort[1], -1, 1));

            _graphics = new Sim.GraphicsController();
            
            /*
             * Test
             */
            _particle = new Particle(_graphics);
            _particle.Position = new Vector2(Width / 2, Height / 2);
            _particle.TimeToLive = 5;
            _particleList.Add(_particle);
            /**/

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Renderer.Call(() => GL.Ortho(0, Width, Height, 0, -1, 1));
            Renderer.Call(() => GL.Viewport(0, 0, Width, Height));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            Timer.Update();

            /*
             * Test
             */
            foreach(var obj in _particleList)
            {
                obj.Update(Timer.ElapsedSeconds);
            }

            _particleList.RemoveAll(p => p.TimeToLive <= 0);
            /**/

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Renderer.Call(() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Renderer.Call(() => GL.MatrixMode(MatrixMode.Projection));
            Renderer.Call(GL.LoadIdentity);
            Renderer.Call(() => GL.Ortho(0, Width, Height, 0, -1, 1));

            /*
            * Test
            */
            foreach (var obj in _particleList)
            {
                obj.Render();
            }
            /**/


            SwapBuffers();
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
