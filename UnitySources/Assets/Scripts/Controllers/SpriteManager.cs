using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Assets.Scripts.Model;
using Assets.Scripts.Model.Import;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Controllers
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance;

        private const int PPU = 64;

        private readonly Dictionary<string, SpriteSheet> _sprites = new Dictionary<string, SpriteSheet>();

        private void OnEnable ()
        {
            Instance = this; // TODO: Not sure why this gets called twice
            LoadSprites();
        }
	
        public UnityEngine.Sprite GetSprite(string atlasName, string spriteName)
        {
            if (_sprites.ContainsKey(atlasName) == false || _sprites[atlasName].Sprites.ContainsKey(spriteName) == false)
            {
                Debug.LogErrorFormat("No sprite with name {0} in atlas {1}!", spriteName, atlasName);
                return null;
            }
            return _sprites[atlasName].Sprites[spriteName].GetSprite();
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
            // Debug.LogFormat("Loading Spritesheet {0}.", filepath);

            // First get the data about the sprites
            var filestream = new StreamReader(filepath);
            var serializer = new XmlSerializer(typeof (XmlTextureAtlas));
            var atlas = (XmlTextureAtlas) serializer.Deserialize(filestream);

            // Full filename information
            var imagefile = Path.Combine(Path.GetDirectoryName(filepath), atlas.imagePath);

            // Now load the image itself
            var bytes = File.ReadAllBytes(imagefile);
            var texture = new Texture2D(1, 1);
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(bytes);

            // Set up the basic SpriteSheet data.
            var spritesheet = new SpriteSheet()
            {
                Name = Path.GetFileNameWithoutExtension(atlas.imagePath)
            };

            // Add each sprite-set to the sheet.
            foreach (var data in atlas.Sprites)
            {
                data.y = atlas.height - data.y - data.height;

                var sprite = new Model.Sprite(texture,
                    new Rect(data.x, data.y, data.width, data.height),
                    new Vector2(data.pivotX, data.pivotY), PPU);

                spritesheet.Sprites.Add(data.name, sprite);
            }

            _sprites.Add(spritesheet.Name, spritesheet);
        }
    }
}
