using Sim;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShaderTest
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

        private int Program = 0;
        private Dictionary<string, int> Variables = new Dictionary<string, int>();

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
            int status_code = -1;
            string info = "";

            if (vertexSource == "" && fragmentSource == "")
            {
                Console.WriteLine("Failed to compile Shader." +
                    Environment.NewLine + "Nothing to Compile.", "Error");
                return false;
            }

            if (Program > 0)
                Renderer.Call(() => GL.DeleteProgram(Program));

            Variables.Clear();

            Program = GL.CreateProgram();

            if (vertexSource != "")
            {
                int vertexShader = GL.CreateShader(ShaderType.VertexShader);
                Renderer.Call(() => GL.ShaderSource(vertexShader, vertexSource));
                Renderer.Call(() => GL.CompileShader(vertexShader));
                Renderer.Call(() => GL.GetShaderInfoLog(vertexShader, out info));
                Renderer.Call(() => GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status_code));

                if (status_code != 1)
                {
                    Console.WriteLine("Failed to Compile Vertex Shader Source." +
                        Environment.NewLine + info + Environment.NewLine + "Status Code: " + status_code.ToString());

                    Renderer.Call(() => GL.DeleteShader(vertexShader));
                    Renderer.Call(() => GL.DeleteProgram(Program));
                    Program = 0;

                    return false;
                }

                Renderer.Call(() => GL.AttachShader(Program, vertexShader));
                Renderer.Call(() => GL.DeleteShader(vertexShader));
            }

            if (fragmentSource != "")
            {
                int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                Renderer.Call(() => GL.ShaderSource(fragmentShader, fragmentSource));
                Renderer.Call(() => GL.CompileShader(fragmentShader));
                Renderer.Call(() => GL.GetShaderInfoLog(fragmentShader, out info));
                Renderer.Call(() => GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status_code));

                if (status_code != 1)
                {
                    Console.WriteLine("Failed to Compile Fragment Shader Source." +
                        Environment.NewLine + info + Environment.NewLine + "Status Code: " + status_code.ToString());

                    Renderer.Call(() => GL.DeleteShader(fragmentShader));
                    Renderer.Call(() => GL.DeleteProgram(Program));
                    Program = 0;

                    return false;
                }

                Renderer.Call(() => GL.AttachShader(Program, fragmentShader));
                Renderer.Call(() => GL.DeleteShader(fragmentShader));
            }

            Renderer.Call(() => GL.LinkProgram(Program));
            Renderer.Call(() => GL.GetProgramInfoLog(Program, out info));
            Renderer.Call(() => GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out status_code));

            if (status_code != 1)
            {
                Console.WriteLine("Failed to Link Shader Program." +
                    Environment.NewLine + info + Environment.NewLine + "Status Code: " + status_code.ToString());

                Renderer.Call(() => GL.DeleteProgram(Program));
                Program = 0;

                return false;
            }

            return true;
        }

        private int GetVariableLocation(string name)
        {
            if (Variables.ContainsKey(name))
                return Variables[name];

            int location = GL.GetUniformLocation(Program, name);

            if (location != -1)
                Variables.Add(name, location);
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
            if (Program > 0)
            {
                Renderer.Call(() => GL.UseProgram(Program));

                int location = GetVariableLocation(name);
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
            if (Program > 0)
            {
                Renderer.Call(() => GL.UseProgram(Program));

                int location = GetVariableLocation(name);
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
            if (Program > 0)
            {
                Renderer.Call(() => GL.UseProgram(Program));

                int location = GetVariableLocation(name);
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
            if (Program > 0)
            {
                Renderer.Call(() => GL.UseProgram(Program));

                int location = GetVariableLocation(name);
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
            if (Program > 0)
            {
                Renderer.Call(() => GL.UseProgram(Program));

                int location = GetVariableLocation(name);
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
            if (shader != null && shader.Program > 0)
            {
                Renderer.Call(() => GL.UseProgram(shader.Program));
            }
            else
            {
                Renderer.Call(() => GL.UseProgram(0));
            }
        }

        public void Dispose()
        {
            if (Program != 0)
                Renderer.Call(() => GL.DeleteProgram(Program));
        }

    }
}
