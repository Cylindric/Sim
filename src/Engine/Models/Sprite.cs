using Engine.Utilities;
using Engine.Renderer.SDLRenderer;
using System.Diagnostics;

namespace Engine.Models
{
    [DebuggerDisplay("{Name} ({Width} × {Height})")]
    public class Sprite
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */
        private SpriteClip _clip = new SpriteClip();
        private SDLTexture _texture;

        public Sprite(SDLTexture texture)
        {
            _texture = texture;
        }

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public Sprite(SDLTexture texture, Rect rect, Vector2<float> pivot, int ppu)
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
        public SDLTexture Texture { get; set; }

        public string Name { get; set; }

        public Colour Colour { get; set; }

        public int X
        {
            get
            {
                return _clip.X;
            }
            internal set
            {
                _clip.X = value;
            }
        }

        public int Y
        {
            get
            {
                return _clip.Y;
            }
            internal set
            {
                _clip.Y = value;
            }
        }

        public int Width
        {
            get
            {
                return _clip.Width;
            }
            internal set
            {
                _clip.Width = value;
            }
        }

        public int Height
        {
            get
            {
                return _clip.Height;
            }
            internal set
            {
                _clip.Height = value;
            }
        }

        public float Px { get; set; }

        public float Py { get; set; }

        public SpriteClip Clip
        {
            get
            {
                return _clip;
            }
        }

        public bool Centered { get; set; }

        public float Rotation { get; set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Render(int x, int y)
        {
            if (Centered)
            {
                x = x - (Width / 2);
                y = y - (Width / 2);
            }
            int width = Width;
            int height = Height;
            _texture.RenderSprite(this, x, y, width, height);
        }

        public void Rotate(float v)
        {
            Rotation = Mathf.Clamp(v, 0, 360);
        }
    }
}
