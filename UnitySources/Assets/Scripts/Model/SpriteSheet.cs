using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class SpriteSheet
    {
        public string Name { get; set; }
        public Dictionary<string, Sprite> Sprites;

        public SpriteSheet()
        {
            Sprites = new Dictionary<string, Sprite>();
        }

        public UnityEngine.Sprite GetDefaultSprite()
        {
            return Sprites[Name].GetSprite();
        }
    }
}
