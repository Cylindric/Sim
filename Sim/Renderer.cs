using OpenTK.Graphics.OpenGL;
using System;

// http://www.opentk.com/node/3691

namespace Sim
{
    // Include this if you are at .NET 2.0
    public delegate void Action();
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult Func<TResult>();
    public delegate TResult Func<T, TResult>(T arg);
    public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // Well you can name anything as you like Lol.
    public static class Renderer
    {
        /// <summary>
        /// Lastest Error Code that Occurred.
        /// </summary>
        public static ErrorCode LastError { get; private set; }

        /// <summary>
        /// Call OpenGL function and check for the error
        /// </summary>
        /// <param name="callback">OpenGL Function to be called</param>
        public static void Call(Action callback)
        {
            callback();
            CheckError();
        }

        /// <summary>
        /// OpenGL function and check for the error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">OpenGL Function to be called</param>
        /// <param name="parameter">Parameters of OpenGL Function</param>
        public static void Call<T>(Action<T> func, T parameter)
        {
            func(parameter);
            CheckError();
        }

        /// <summary>
        /// Check for the OpenGL Error
        /// </summary>
        public static void CheckError()
        {
            ErrorCode errorCode = GL.GetError();

            if (errorCode == ErrorCode.NoError)
                return;

            LastError = errorCode;
            string error = "Unknown Error";
            string description = "No Description";

            // Decode the error code
            switch (errorCode)
            {
                case ErrorCode.InvalidEnum:
                    {
                        error = "GL_INVALID_ENUM";
                        description = "An unacceptable value has been specified for an enumerated argument";
                        break;
                    }

                case ErrorCode.InvalidValue:
                    {
                        error = "GL_INVALID_VALUE";
                        description = "A numeric argument is out of range";
                        break;
                    }

                case ErrorCode.InvalidOperation:
                    {
                        error = "GL_INVALID_OPERATION";
                        description = "The specified operation is not allowed in the current state";
                        break;
                    }

                case ErrorCode.StackOverflow:
                    {
                        error = "GL_STACK_OVERFLOW";
                        description = "This command would cause a stack overflow";
                        break;
                    }

                case ErrorCode.StackUnderflow:
                    {
                        error = "GL_STACK_UNDERFLOW";
                        description = "This command would cause a stack underflow";
                        break;
                    }

                case ErrorCode.OutOfMemory:
                    {
                        error = "GL_OUT_OF_MEMORY";
                        description = "there is not enough memory left to execute the command";
                        break;
                    }

                case ErrorCode.InvalidFramebufferOperationExt:
                    {
                        error = "GL_INVALID_FRAMEBUFFER_OPERATION_EXT";
                        description = "The object bound to FRAMEBUFFER_BINDING_EXT is not \"framebuffer complete\"";
                        break;
                    }
                default:
                    {
                        error = errorCode.ToString();
                        break;
                    }
            }

            // Log the error
            Console.WriteLine("An internal OpenGL call failed: " + error + " (" + description + ")", "Fatal Error");
        }
    }
}