using System.Collections.Generic;

namespace Assets.Scripts.Model
{
    public class SpriteSet
    {
        public List<Sprite> Sprites { get; set; }

        public SpriteSet()
        {
            Sprites = new List<Sprite>();
        }
    }
}
