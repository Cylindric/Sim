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

        private Dictionary<string, SpriteSheet> _sprites = new Dictionary<string, SpriteSheet>();

        private void OnEnable ()
        {
            Instance = this;
            LoadSprites();
        }
	
        public UnityEngine.Sprite GetSprite(string spriteName)
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
            var serializer = new XmlSerializer(typeof(Model.Import.XmlSpriteSheet));

            var spriteSheetData = new XmlSpriteSheet();
            try
            {
                spriteSheetData = (XmlSpriteSheet) serializer.Deserialize(filestream);
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

            // Set up the basic SpriteSheet data.
            var spritesheet = new SpriteSheet()
            {
                Name = spriteSheetData.name
            };

            // Add each sprite-set to the sheet.
            foreach (var spritesetData in spriteSheetData.SpriteSets)
            {
                var spriteset = new SpriteSet();

                // Add all the sprites to the sprite-set
                foreach (var spriteData in spritesetData.Sprites)
                {
                    var sprite = new Model.Sprite(texture,
                        new Rect(spriteData.x, spriteData.y, spriteSheetData.width, spriteSheetData.height),
                        new Vector2(spriteSheetData.Pivot.x, spriteSheetData.Pivot.y), spriteSheetData.pixelsPerUnit);

                    spriteset.Sprites.Add(sprite);
                }

                spritesheet.SpriteSets.Add(spritesetData.name, spriteset);
            }
        }
    }
}
