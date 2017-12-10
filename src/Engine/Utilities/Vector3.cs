using System.Diagnostics;

namespace Engine.Utilities
{
    [DebuggerDisplay("[{X},{Y},{Z}]")]
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Adds the terms of the two vectors together and returns the result.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        /// <summary>
        /// Subtracts the terms of the second vector from the first and returns the result.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// Multiplies the terms of the vector v by f and returns the result.
        /// </summary>
        /// <param name="v">The vector</param>
        /// <param name="f">The multiplier</param>
        /// <returns>The vector multiplied by f.</returns>
        public static Vector3 operator *(Vector3 v, float f)
        {
            return new Vector3(v.X * f, v.Y * f, v.Z * f);
        }

        public static Vector3 operator /(Vector3 v, int i)
        {
            return new Vector3(v.X / i, v.Y / i, v.Z / i);
        }

        /// <summary>
        /// The vector 0,0,0.
        /// </summary>
        public static Vector3 Zero { get { return new Vector3(0, 0, 0); } }

        /// <summary>
        /// The vector -1,0,0.
        /// </summary>
        public static Vector3 Left { get { return new Vector3(-1, 0, 0); } }

        /// <summary>
        /// The vector 1,0,0.
        /// </summary>
        public static Vector3 Right { get { return new Vector3(1, 0, 0); } }

        /// <summary>
        /// The vector 0,1,0.
        /// </summary>
        public static Vector3 Up { get { return new Vector3(0, 1, 0); } }

        /// <summary>
        /// The vector 0,-1,0.
        /// </summary>
        public static Vector3 Down { get { return new Vector3(0, -1, 0); } }
    }
}
