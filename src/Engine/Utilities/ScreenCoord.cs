using Engine.Controllers;
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
            X = x;
            Y = y;
        }

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        public float X { get; set; }
        public float Y { get; set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public static ScreenCoord operator +(ScreenCoord c1, ScreenCoord c2)
        {
            return new ScreenCoord(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static ScreenCoord operator +(ScreenCoord c1, WorldCoord c2)
        {
            return new ScreenCoord(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static ScreenCoord operator -(ScreenCoord c1, ScreenCoord c2)
        {
            return new ScreenCoord(c1.X - c2.X, c1.Y - c2.Y);
        }

        public WorldCoord ToWorld()
        {
            return CameraController.Instance.ScreenToWorldPoint(this);
        }

        /// <summary>
        /// Flip the coordinate top-to-bottom (negates the Y value)
        /// </summary>
        /// <returns>X, Y × -1</returns>
        public ScreenCoord Flip()
        {
            return new ScreenCoord(X, Y * -1);
        }

        public static ScreenCoord Zero
        {
            get
            {
                return new ScreenCoord(0, 0);
            }
        }

        public override string ToString() {
            return $"[{X},{Y}]";
        }
    }
}
