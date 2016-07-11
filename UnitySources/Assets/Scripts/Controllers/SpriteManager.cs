using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Assets.Scripts.Model.Import;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Sprite = UnityEngine.Sprite;

namespace Assets.Scripts.Controllers
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance;

        private Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        private void OnEnable ()
        {
            Instance = this;
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
            LoadSprites(filepath);
        }

        private void LoadSprites(string filepath)
        {
            foreach (var dir in Directory.GetDirectories(filepath))
            {
                LoadSprites(dir);
            }

            foreach (var file in Directory.GetFiles(filepath).Where(f => f.EndsWith(".xml")))
            {
                LoadSpritesheet(file);
            }
        }

        private void LoadSpritesheet(string filepath)
        {
            // Full filename information
            var datafile = filepath;
            var imagefile = Path.Combine(Path.GetDirectoryName(datafile), Path.GetFileNameWithoutExtension(datafile) + ".png");

            // First get the data about the sprites
            var filestream = new StreamReader(filepath);
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
            var bytes = File.ReadAllBytes(imagefile);
            var texture = new Texture2D(1, 1);
            texture.filterMode = FilterMode.Point;
            texture.LoadImage(bytes);

            // For every sprite defined in the datafile, create a new Sprite object
            foreach (var s in sprites.SpriteList)
            {
                var sprite = Sprite.Create(texture, s.Rect.ToRect(), s.Pivot.ToVector2(), s.pixelsPerUnit);
                if (_sprites.ContainsKey(s.name))
                {
                    Debug.LogWarningFormat("Duplicate sprite ({0}) found in data; overwriting... ", s.name);
                    _sprites[s.name] = sprite;
                }
                else
                {
                    _sprites.Add(s.name, sprite);
                }
            }
        }
    }
}
