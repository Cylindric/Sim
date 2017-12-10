using System.Diagnostics;

namespace Engine.Utilities
{
    [DebuggerDisplay("[{X},{Y}]")]
    public struct ScreenCoord
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

        public ScreenCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public ScreenCoord(float x, float y)
        {
            X = (int)x;
            Y = (int)y;
        }

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        public float X { get; set; }
        public float Y { get; set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public static ScreenCoord operator +(ScreenCoord c1, WorldCoord c2)
        {
            return new ScreenCoord(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static ScreenCoord Zero
        {
            get
            {
                return new ScreenCoord(0, 0);
            }
        }
    }
}
