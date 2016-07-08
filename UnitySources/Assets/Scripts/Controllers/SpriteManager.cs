using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Assets.Scripts.Model;
using Assets.Scripts.Model.Import;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Rect = UnityEngine.Rect;
using Sprite = UnityEngine.Sprite;

namespace Assets.Scripts.Controllers
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Current;

        private Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        private void OnEnable ()
        {
            Current = this;
            LoadSprites();
        }
	
        public Sprite GetSprite(string spriteName)
        {
            if (_sprites.ContainsKey(spriteName) == false)
            {
                Debug.LogErrorFormat("No prite with name {0}!", spriteName);
                return null;
            }
            return _sprites[spriteName];
        }

        private void LoadSprites()
        {
            var filepath = Application.streamingAssetsPath;
            filepath = Path.Combine(filepath, "Base");
            filepath = Path.Combine(filepath, "Images");

            LoadSpritesheet(Path.Combine(filepath, "Furniture/orange_walls"));
            LoadSpritesheet(Path.Combine(filepath, "Furniture/furn_mining_station"));
            LoadSpritesheet(Path.Combine(filepath, "Furniture/furn_oxygen"));
            LoadSpritesheet(Path.Combine(filepath, "Furniture/furn_stockpile"));
        }

        private void LoadSpritesheet(string filepath)
        {
            // First get the data about the sprites
            var filestream = new StreamReader(filepath + ".xml");
            var serializer = new XmlSerializer(typeof(Model.Import.Sprites));

            var sprites = new Sprites();
            try
            {
                sprites = (Sprites) serializer.Deserialize(filestream);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            
            // Now load the image itself
            var bytes = File.ReadAllBytes(filepath + ".png");
            var texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);

            // For every sprite defined in the datafile, create a new Sprite object
            foreach (var s in sprites.SpriteList)
            {
                var sprite = Sprite.Create(texture, s.Rect.ToRect(), s.Pivot.ToVector2(), s.pixelsPerUnit);
                _sprites.Add(s.name, sprite);
            }
        }
    }
}
