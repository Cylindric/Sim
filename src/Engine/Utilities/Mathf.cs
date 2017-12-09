using System;

namespace Engine.Utilities
{
    public static class Mathf
    {
        public static float Infinity
        {
            get {
                return float.PositiveInfinity;
            }
        }

        internal static float Lerp(float a, float b, float f)
        {
            return (a * (1.0f - f)) + (b * f); // More precise. https://stackoverflow.com/questions/4353525/floating-point-linear-interpolation
            // return a + ((b - a) * f); // Less precise if a and b greatly differ
        }

        internal static bool Approximately(float a, float b, float epsilon = 0.0000001f)
        {
            // https://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp
            float absA = System.Math.Abs(a);
            float absB = System.Math.Abs(b);
            float diff = System.Math.Abs(a - b);

            if (a == b)
            { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < float.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < epsilon;
            }
            else
            { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        internal static float Clamp01(float v)
        {
            if (v < 0)
            {
                return 0;
            }
            else if (
              v > 1)
            {
                return 1;
            }
            else
            {
                return v;
            }
        }

        internal static int FloorToInt(float a)
        {
            return (int)Math.Floor(a);
        }
    }
}
