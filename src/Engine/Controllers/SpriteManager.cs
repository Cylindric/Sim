using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Models;
using Debug = Engine.Utilities.Debug;
using System;
using static Engine.Engine;

namespace Engine.Controllers
{
    public class SpriteManager : IController
    {
        #region Singleton
        private static readonly Lazy<SpriteManager> _instance = new Lazy<SpriteManager>(() => new SpriteManager());

        public static SpriteManager Instance { get { return _instance.Value; } }

        private SpriteManager()
        {
        }
        #endregion

        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        private readonly Dictionary<string, SpriteSheet> _spritesheets = new Dictionary<string, SpriteSheet>();

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public void Start()
        {
            var filepath = Engine.Instance.Path("assets", "base", "images");
            LoadSprites(filepath);
        }

        public void Update() { }
        public void Render(LAYER layer) { }
	
        public Sprite GetSprite(string atlasName, string spriteName)
        {
            if (_spritesheets.ContainsKey(atlasName) == false || _spritesheets[atlasName]._sprites.ContainsKey(spriteName) == false)
            {
                Debug.LogErrorFormat("No sprite with name {0} in atlas {1}!", spriteName, atlasName);
                return null;
            }
            return _spritesheets[atlasName]._sprites[spriteName];
        }

        /// <summary>
        /// Iterate through the entire directory tree looking for spritesheet XML files.
        /// </summary>
        /// <param name="filepath"></param>
        private void LoadSprites(string filepath)
        {
            foreach (var dir in Directory.GetDirectories(filepath))
            {
                LoadSprites(dir);
            }

            foreach (var file in Directory.GetFiles(filepath).Where(f => f.EndsWith(".xml")))
            {
                var spritesheet = new SpriteSheet().Load(file);
                _spritesheets.Add(spritesheet.Name, spritesheet);
            }
        }
    }
}
