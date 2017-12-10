using System.Diagnostics;

namespace Engine.Utilities
{
    [DebuggerDisplay("[{X},{Y}] [{Width},{Height}]")]
    public class SpriteClip
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public SpriteClip()
        {
        }

        public SpriteClip(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }
    }
}
