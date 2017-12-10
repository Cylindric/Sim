using System.Diagnostics;

namespace Engine.Utilities
{
    [DebuggerDisplay("[{X},{Y}]")]
    public struct WorldCoord
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public WorldCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public WorldCoord(float x, float y)
        {
            X = (int)x;
            Y = (int)y;
        }

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        public float X { get; set; }
        public float Y { get; set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public static WorldCoord operator +(WorldCoord c1, Vector2<float> c2)
        {
            return new WorldCoord(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static WorldCoord operator -(WorldCoord c1, WorldCoord c2)
        {
            return new WorldCoord(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static WorldCoord Zero
        {
            get
            {
                return new WorldCoord(0, 0);
            }
        }
    }
}
