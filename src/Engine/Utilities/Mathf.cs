using System;

namespace Engine.Utilities
{
    public static class Mathf
    {
        public static float Infinity
        {
            get
            {
                return float.PositiveInfinity;
            }
        }

        /// <summary>
        /// Returns a simple linear interpolation between a and b, at position f.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float f)
        {
            return (a * (1.0f - f)) + (b * f); // More precise. https://stackoverflow.com/questions/4353525/floating-point-linear-interpolation
            // return a + ((b - a) * f); // Less precise if a and b greatly differ
        }

        public static bool Approximately(float a, float b, float epsilon = 0.0000001f)
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

        /// <summary>
        /// Clamps the given value between min and max. Values less than min return min. Values greater than max return max.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(float value, float min, float max)
        {
            if (value > max) return max;
            if (value < min) return min;
            return value;
        }

        /// <summary>
        /// Clamps the given value between 0 and 1.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// <seealso cref="Clamp(float, float, float)"/>
        public static float Clamp01(float v)
        {
            return Clamp(v, 0, 1);
        }

        public static int FloorToInt(float a)
        {
            return (int)Math.Floor(a);
        }

        /// <summary>
        /// Normalizes any number to an arbitrary range by assuming the range wraps around when going below min or above max 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        /// <seealso cref="https://stackoverflow.com/a/2021986"/>
        public static float Wrap(float value, float start, float end)
        {
            float width = end - start;   // 
            float offsetValue = value - start;   // value relative to 0

            return (offsetValue - (float)(Math.Floor(offsetValue / width) * width)) + start;
            // + start to reset back to start of original range
        }

        /// <summary>
        /// Normalizes any number to an arbitrary range by assuming the range wraps around when going below min or above max 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        /// <seealso cref="https://stackoverflow.com/a/2021986"/>
        public static float Wrap(int value, int start, int end)
        {
            int newValue = value - start;
            int newEnd = end - start;

            int result = newValue % newEnd;
            if(result < 0)
            {
                result += newEnd;
            }

            result += start;
            return result;
        }
    }
}