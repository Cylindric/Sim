using Engine.Renderer.SDLRenderer;
using Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Models
{
    public class Text
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */
        private SpriteClip _clip = new SpriteClip();
        private SDLText _text;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */
        public Text(SDLText texture, Rect rect, Vector2<float> pivot, string text)
        {
            Texture = texture;
            _clip.X = rect.X;
            _clip.Y = rect.Y;
            _clip.Width = rect.Width;
            _clip.Height = rect.Height;
            Px = pivot.X;
            Py = pivot.Y;
        }

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */
        public SDLText Texture { get; set; }
        public float Px { get; set; }
        public float Py { get; set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */
        public void Render(int x, int y)
        {
            _text.Render();
        }
    }
}
