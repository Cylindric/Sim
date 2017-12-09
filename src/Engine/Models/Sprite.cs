using UnityEngine;

namespace Assets.Scripts.Model
{
    public class Sprite
    {
        private readonly UnityEngine.Sprite _sprite;

        public UnityEngine.Sprite GetSprite()
        {
            return _sprite;
        }

        public Sprite(Texture2D texture, Rect rect, Vector2 pivot, int ppu)
        {
            _sprite = UnityEngine.Sprite.Create(texture, rect, pivot, ppu);
        }
    }
}
