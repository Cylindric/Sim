using System.Diagnostics;

namespace Engine.Utilities
{
    [DebuggerDisplay("[{X},{Y}]")]
    public struct Vector2<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        public Vector2 (T x, T y)
        {
            X = x;
            Y = y;
        }

        public static Vector2<T> Zero
        {
            get
            {
                return new Vector2<T>(default(T), default(T));
            }
        }

    }
}
