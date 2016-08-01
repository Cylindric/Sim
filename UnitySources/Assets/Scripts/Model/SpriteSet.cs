using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Model
{
    class SpriteSet
    {
        private List<SpriteFrame> _frames = new List<SpriteFrame>();

        public Sprite GetDefaultSprite()
        {
            if (_frames.Count == 0)
            {
                return null;
            }

            return _frames.First().Sprite;
        }

        public void SetSprite(Sprite sprite)
        {
            if (_frames.Count == 0)
            {
                _frames.Add(new SpriteFrame());
            }
            _frames.First().Sprite = sprite;
        }
    }
}
