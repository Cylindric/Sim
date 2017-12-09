namespace Engine.Utilities
{
    public class Colour
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public Colour()
        {

        }

        public Colour(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Colour Zero { get { return new Colour(0, 0, 0, 0); } }
        public static Colour White { get { return new Colour(1, 1, 1, 1); } }
    }
}
