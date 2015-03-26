using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sim
{
    class Shader : IDisposable
    {
        public enum Type
        {
            Vertex = 0x01,
            Fragment = 0x02
        }

        public static bool IsSupported
        {
            get
            {
                return (new Version(GL.GetString(StringName.Version).Substring(0, 3)) >= new Version(2, 0) ? true : false);
            }
        }

        private int _program = 0;
        private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();

        public Shader(string vs, string fs)
        {
            if (!IsSupported)
            {
                Console.WriteLine("Failed to create Shader." +
                    Environment.NewLine + "Your system doesn't support Shader.", "Error");
                return;
            }

            Compile(vs, fs);
        }

        public Shader(string source, Type type)
        {
            if (!IsSupported)
            {
                Console.WriteLine("Failed to create Shader." +
                    Environment.NewLine + "Your system doesn't support Shader.", "Error");
                return;
            }

            if (type == Type.Vertex)
                Compile(source, "");
            else
                Compile("", source);
        }

        private bool Compile(string vertexSource = "", string fragmentSource = "")
        {
            var statusCode = -1;
            var info = "";

            if (vertexSource == "" && fragmentSource == "")
            {
                Console.WriteLine("Failed to compile Shader." +
                    Environment.NewLine + "Nothing to Compile.", "Error");
                return false;
            }

            if (_program > 0)
                Renderer.Call(() => GL.DeleteProgram(_program));

            _variables.Clear();

            _program = GL.CreateProgram();

            if (vertexSource != "")
            {
                var vertexShader = GL.CreateShader(ShaderType.VertexShader);
                Renderer.Call(() => GL.ShaderSource(vertexShader, vertexSource));
                Renderer.Call(() => GL.CompileShader(vertexShader));
                Renderer.Call(() => GL.GetShaderInfoLog(vertexShader, out info));
                Renderer.Call(() => GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out statusCode));

                if (statusCode != 1)
                {
                    Console.WriteLine("Failed to Compile Vertex Shader Source." +
                        Environment.NewLine + info + Environment.NewLine + "Status Code: " + statusCode.ToString());

                    Renderer.Call(() => GL.DeleteShader(vertexShader));
                    Renderer.Call(() => GL.DeleteProgram(_program));
                    _program = 0;

                    return false;
                }

                Renderer.Call(() => GL.AttachShader(_program, vertexShader));
                Renderer.Call(() => GL.DeleteShader(vertexShader));
            }

            if (fragmentSource != "")
            {
                var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                Renderer.Call(() => GL.ShaderSource(fragmentShader, fragmentSource));
                Renderer.Call(() => GL.CompileShader(fragmentShader));
                Renderer.Call(() => GL.GetShaderInfoLog(fragmentShader, out info));
                Renderer.Call(() => GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out statusCode));

                if (statusCode != 1)
                {
                    Console.WriteLine("Failed to Compile Fragment Shader Source." +
                        Environment.NewLine + info + Environment.NewLine + "Status Code: " + statusCode.ToString());

                    Renderer.Call(() => GL.DeleteShader(fragmentShader));
                    Renderer.Call(() => GL.DeleteProgram(_program));
                    _program = 0;

                    return false;
                }

                Renderer.Call(() => GL.AttachShader(_program, fragmentShader));
                Renderer.Call(() => GL.DeleteShader(fragmentShader));
            }

            Renderer.Call(() => GL.LinkProgram(_program));
            Renderer.Call(() => GL.GetProgramInfoLog(_program, out info));
            Renderer.Call(() => GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out statusCode));

            if (statusCode != 1)
            {
                Console.WriteLine("Failed to Link Shader Program." +
                    Environment.NewLine + info + Environment.NewLine + "Status Code: " + statusCode.ToString());

                Renderer.Call(() => GL.DeleteProgram(_program));
                _program = 0;

                return false;
            }

            return true;
        }

        private int GetVariableLocation(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name];

            var location = GL.GetUniformLocation(_program, name);

            if (location != -1)
                _variables.Add(name, location);
            else
                Console.WriteLine("Failed to retrieve Variable Location." +
                    Environment.NewLine + "Variable Name not found.", "Error");

            return location;
        }

        /// <summary>
        /// Change a value Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="x">Value</param>
        public void SetVariable(string name, float x)
        {
            if (_program > 0)
            {
                Renderer.Call(() => GL.UseProgram(_program));

                var location = GetVariableLocation(name);
                if (location != -1)
                    Renderer.Call(() => GL.Uniform1(location, x));

                Renderer.Call(() => GL.UseProgram(0));
            }
        }

        /// <summary>
        /// Change a 2 value Vector Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="x">First Vector Value</param>
        /// <param name="y">Second Vector Value</param>
        public void SetVariable(string name, float x, float y)
        {
            if (_program > 0)
            {
                Renderer.Call(() => GL.UseProgram(_program));

                var location = GetVariableLocation(name);
                if (location != -1)
                    Renderer.Call(() => GL.Uniform2(location, x, y));

                Renderer.Call(() => GL.UseProgram(0));
            }
        }

        /// <summary>
        /// Change a 3 value Vector Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="x">First Vector Value</param>
        /// <param name="y">Second Vector Value</param>
        /// <param name="z">Third Vector Value</param>
        public void SetVariable(string name, float x, float y, float z)
        {
            if (_program > 0)
            {
                Renderer.Call(() => GL.UseProgram(_program));

                var location = GetVariableLocation(name);
                if (location != -1)
                    Renderer.Call(() => GL.Uniform3(location, x, y, z));

                Renderer.Call(() => GL.UseProgram(0));
            }
        }

        /// <summary>
        /// Change a 4 value Vector Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="x">First Vector Value</param>
        /// <param name="y">Second Vector Value</param>
        /// <param name="z">Third Vector Value</param>
        /// <param name="w">Fourth Vector Value</param>
        public void SetVariable(string name, float x, float y, float z, float w)
        {
            if (_program > 0)
            {
                Renderer.Call(() => GL.UseProgram(_program));

                var location = GetVariableLocation(name);
                if (location != -1)
                    Renderer.Call(() => GL.Uniform4(location, x, y, z, w));

                Renderer.Call(() => GL.UseProgram(0));
            }
        }

        /// <summary>
        /// Change a Matrix4 Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="matrix">Matrix</param>
        public void SetVariable(string name, Matrix4 matrix)
        {
            if (_program > 0)
            {
                Renderer.Call(() => GL.UseProgram(_program));

                var location = GetVariableLocation(name);
                if (location != -1)
                {
                    // Well cannot use ref on lambda expression Lol
                    // So we need to call Check error manually
                    GL.UniformMatrix4(location, false, ref matrix);
                    Renderer.CheckError();
                }

                Renderer.Call(() => GL.UseProgram(0));
            }
        }

        /// <summary>
        /// Change a 2 value Vector Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="vector">Vector Value</param>
        public void SetVariable(string name, Vector2 vector)
        {
            SetVariable(name, vector.X, vector.Y);
        }

        /// <summary>
        /// Change a 3 value Vector Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="vector">Vector Value</param>
        public void SetVariable(string name, Vector3 vector)
        {
            SetVariable(name, vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Change a 4 value Vector Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="vector">Vector Value</param>
        public void SetVariable(string name, Vector4 vector)
        {
            SetVariable(name, vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Change a Color Variable of the Shader
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="color">Color Value</param>
        public void SetVariable(string name, Color color)
        {
            SetVariable(name, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        /// <summary>
        /// Bind a Shader for Rendering
        /// </summary>
        /// <param name="shader">Shader to bind</param>
        public static void Bind(Shader shader)
        {
            if (shader != null && shader._program > 0)
            {
                Renderer.Call(() => GL.UseProgram(shader._program));
            }
            else
            {
                Renderer.Call(() => GL.UseProgram(0));
            }
        }

        public void Dispose()
        {
            if (_program != 0)
                Renderer.Call(() => GL.DeleteProgram(_program));
        }

    }
}
