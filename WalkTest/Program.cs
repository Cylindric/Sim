using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Sim;
using System;

namespace WalkTest
{
    class Program : GameWindow
    {
        /*
        * Test
        */
        /**/

        private GraphicsController _graphics;
        Timer timer = new Timer();
        double timeInFrame = 0;
        int frameNumber;


        public Program()
            : base(800, 600, GraphicsMode.Default, "Test", GameWindowFlags.Default)
        {
            this.VSync = VSyncMode.Off;
        }

        static void Main(string[] args)
        {
            using (var game = new Program())
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

            timeInFrame += Timer.ElapsedSeconds;

            if (timeInFrame >= 0.5)
            {
                // flip
                frameNumber++;
                frameNumber = frameNumber % 999;
                timeInFrame = 0;
            }

            /*
             * Test
             */
            /**/

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Renderer.Call(() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Renderer.Call(() => GL.MatrixMode(MatrixMode.Projection));
            Renderer.Call(GL.LoadIdentity);
            Renderer.Call(() => GL.Ortho(0, Width, Height, 0, -1, 1));

            Timer.Update();

            /*
            * Test
            */
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
