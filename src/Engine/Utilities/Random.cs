namespace Engine.Utilities
{
    public class Random : System.Random
    {
        private static System.Random _random = new System.Random();

        public static float Range(float v1, float v2)
        {
            return (float)_random.NextDouble() * (v2 - v1) + v1;
        }
    }
}
