using System;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Sim
{
    class GraphicsController
    {
        private int _width;
        private int _height;

        public void Load(Color clearColor)
        {
            Renderer.Call(() => GL.ClearColor(clearColor));
            Renderer.Call(() => GL.Enable(EnableCap.Texture2D));

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            var viewPort = new int[4];
            Renderer.Call(() => GL.GetInteger(GetPName.Viewport, viewPort));
            Renderer.Call(() => GL.Enable(EnableCap.Blend));

            Renderer.Call(() => GL.Enable(EnableCap.Blend));
            Renderer.Call(() => GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha));

            Renderer.Call(() => GL.MatrixMode(MatrixMode.Projection));
            Renderer.Call(() => GL.Viewport(viewPort[0], viewPort[1], viewPort[2], viewPort[3]));
            Renderer.Call(() => GL.Ortho(viewPort[0], viewPort[0] + viewPort[2], viewPort[1] + viewPort[3], viewPort[1], -1, 1));   
        }

        public void ResetDisplay(int width, int height)
        {
            _width = width;
            _height = height;
            Renderer.Call(() => GL.Ortho(0, _width, _height, 0, -1, 1));
            Renderer.Call(() => GL.Viewport(0, 0, _width, _height));
        }

        public void BeginRender()
        {
            Renderer.Call(() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Renderer.Call(() => GL.MatrixMode(MatrixMode.Projection));
            Renderer.Call(GL.LoadIdentity);
            Renderer.Call(() => GL.Ortho(0, _width, _height, 0, -1, 1));
        }

        public void EndRender(GameWindow window)
        {
            window.SwapBuffers();
        }

        public void BindTexture(int textureId)
        {
            Renderer.Call(() => GL.BindTexture(TextureTarget.Texture2D, textureId));
        }

        public void RenderQuad(Vector2[] p, Vector2[] t)
        {
            if (p.Length != 4 || p.Length != 4)
            {
                throw new ArgumentException("Exactly four elements must be passed in both p and t.");
            }

            GL.Begin(PrimitiveType.Quads);

            for (var i = 0; i < p.Length; i++)
            {
                GL.TexCoord2(t[i]);
                GL.Vertex2(p[i]);
            }

            GL.End();
        }

        public int LoadSpritesheet(Bitmap bitmap, bool isRepeated = false, bool isSmooth = true)
        {
            try
            {
                var textureId = 0;
                Renderer.Call(() => GL.GenTextures(1, out textureId));

                Renderer.Call(() => GL.BindTexture(TextureTarget.Texture2D, textureId));

                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                Renderer.Call(() => GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0));

                bitmap.UnlockBits(data);

                // Setup filtering
                Renderer.Call(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, isRepeated ? Convert.ToInt32(TextureWrapMode.Repeat) : Convert.ToInt32(TextureWrapMode.ClampToEdge)));
                Renderer.Call(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, isRepeated ? Convert.ToInt32(TextureWrapMode.Repeat) : Convert.ToInt32(TextureWrapMode.ClampToEdge)));
                Renderer.Call(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, isSmooth ? Convert.ToInt32(TextureMagFilter.Linear) : Convert.ToInt32(TextureMagFilter.Nearest)));
                Renderer.Call(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, isSmooth ? Convert.ToInt32(TextureMinFilter.Linear) : Convert.ToInt32(TextureMinFilter.Nearest)));

                return textureId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating new Texture:" + Environment.NewLine + ex.Message, "Error");
                return 0;
            }
        }

        public Shader LoadFragmentShader(string filename)
        {
            return LoadShader(filename, Shader.Type.Fragment);
        }

        public Shader LoadVertexShader(string filename)
        {
            return LoadShader(filename, Shader.Type.Vertex);
        }

        public Shader LoadShader(string vertexFile, string fragmentFile)
        {
            var vertex = System.IO.File.ReadAllText(System.IO.Path.Combine("Resources", "Shaders", vertexFile));
            var fragment = System.IO.File.ReadAllText(System.IO.Path.Combine("Resources", "Shaders", fragmentFile));
            var shader = new Shader(vertex, fragment);
            return shader;
        }

        private Shader LoadShader(string filename, Shader.Type type)
        {
            var code = System.IO.File.ReadAllText(System.IO.Path.Combine("Resources", "Shaders", filename));
            var shader = new Shader(code, type);
            return shader;
        }

    }
}
