using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Sim;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ShaderTest
{
    class Program : GameWindow
    {
        /*
         * Shader Test
         */
        string vs, fs;
        Shader vsShader;

        int texture;
        Timer timer = new Timer();
        double timeInFrame = 0;
        int frameNumber;


        public Program()
            : base(800, 600, GraphicsMode.Default, "ShaderTest", GameWindowFlags.Default)
        {
            this.VSync = VSyncMode.Off;
        }

        static void Main(string[] args)
        {
            using (Program game = new Program())
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


            /*
             * Shader Test
             */
            texture = LoadBitmap(new Bitmap(Image.FromFile("shadertest.png")));
            vs = System.IO.File.ReadAllText("vertex.glsl");
            fs = System.IO.File.ReadAllText("fragment.glsl");
            vsShader = new Shader(fs, Shader.Type.Fragment);
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

            if(timeInFrame >= 0.2)
            {
                // flip
                frameNumber++;
                frameNumber = frameNumber % 5;
                timeInFrame = 0;
                Console.WriteLine("Flipping to {0}", frameNumber);
            }

            vsShader.SetVariable("TextureSize", (float)32 / 384, (float)32 / 256);
            vsShader.SetVariable("TextureOffset", ((float)32 / 384)*frameNumber, 0);
            
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
             * Shader Test
             */
            GL.BindTexture(TextureTarget.Texture2D, texture);
            Shader.Bind(vsShader);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(System.Drawing.Color.White);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(0, 0);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(32, 0);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(32, 32);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(0, 32);
            GL.End();
            Shader.Bind(null);
            GL.BindTexture(TextureTarget.Texture2D, 0);


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

        private int LoadBitmap(Bitmap bitmap, bool IsRepeated = false, bool IsSmooth = true)
        {
            try
            {
                int TextureID = 0;
                Renderer.Call(() => GL.GenTextures(1, out TextureID));

                Renderer.Call(() => GL.BindTexture(TextureTarget.Texture2D, TextureID));

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                Renderer.Call(() => GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0));

                bitmap.UnlockBits(data);

                // Setup filtering
                Renderer.Call(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, IsRepeated ? Convert.ToInt32(TextureWrapMode.Repeat) : Convert.ToInt32(TextureWrapMode.ClampToEdge)));
                Renderer.Call(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, IsRepeated ? Convert.ToInt32(TextureWrapMode.Repeat) : Convert.ToInt32(TextureWrapMode.ClampToEdge)));
                Renderer.Call(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, IsSmooth ? Convert.ToInt32(TextureMagFilter.Linear) : Convert.ToInt32(TextureMagFilter.Nearest)));
                Renderer.Call(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, IsSmooth ? Convert.ToInt32(TextureMinFilter.Linear) : Convert.ToInt32(TextureMinFilter.Nearest)));

                return TextureID;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating new Texture:" + Environment.NewLine + ex.Message, "Error");
                return 0;
            }
        }
    }
}
