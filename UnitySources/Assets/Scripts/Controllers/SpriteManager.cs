using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static SpriteManager Instance;

        private Dictionary<string, SpriteSet> _sprites = new Dictionary<string, SpriteSet>();

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
            return _sprites[spriteName].GetDefaultSprite();
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
                //var sprites = new Sprites();
                //var sprite1 = new Model.Import.Sprite() { name = "colonist_body", pixelsPerUnit = 64 };
                //sprite1.Rect.Add(new Model.Import.Rect() { x = 0, y = 64, width = 64, height = 64 });
                //sprite1.Pivot = new Model.Import.Pivot() { x = 0.5f, y = 0.5f };
                //sprites.SpriteList.Add(sprite1);
                //var sprite2 = new Model.Import.Sprite() { name = "colonist_shield", pixelsPerUnit = 64 };
                //sprite2.Rect.Add(new Model.Import.Rect() { x = 64, y = 64, width = 64, height = 64 });
                //sprite2.Pivot = new Model.Import.Pivot() { x = 0.5f, y = 0.5f };
                //sprites.SpriteList.Add(sprite1);
                //var serializer = new XmlSerializer(typeof(Model.Import.Sprites));
                //var txt = new StringWriter();
                //serializer.Serialize(txt, sprites);
                
                LoadSpritesheet(file);
            }
        }

        private void LoadSpritesheet(string filepath)
        {
            Debug.LogFormat("Loading Spritesheet {0}.", filepath);

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
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(bytes);

            // For every sprite defined in the datafile, create a new Sprite object
            foreach (var s in sprites.SpriteList)
            {
                foreach (var r in s.Rects)
                {
                    var sprite = Sprite.Create(texture, r.ToRect(), s.Pivot.ToVector2(), s.pixelsPerUnit);
                    if (_sprites.ContainsKey(s.name))
                    {
                        Debug.LogWarningFormat("Duplicate sprite ({0}) found in data; overwriting... ", s.name);
                        _sprites[s.name].SetSprite(sprite);
                    }
                    else
                    {
                        _sprites.Add(s.name, new SpriteSet());
                        _sprites[s.name].SetSprite(sprite);
                    }
                }
            }
        }
    }
}
