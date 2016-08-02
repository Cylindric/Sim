using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class SpriteSheet
    {
        public string Name { get; set; }
        public Dictionary<string, SpriteSet> SpriteSets; 

        public SpriteSheet()
        {
            SpriteSets = new Dictionary<string, SpriteSet>();
        }

        public UnityEngine.Sprite GetDefaultSprite()
        {
            return SpriteSets["default"].Sprites[0].GetSprite();
        }
    }
}
