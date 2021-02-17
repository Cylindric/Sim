using System.Collections.Generic;
using Engine.Models;
using Engine.Pathfinding;
using Debug = Engine.Utilities.Debug;
using Engine.Utilities;
using System;
using static Engine.Engine;

namespace Engine.Controllers
{
    class CharacterSpriteController : IController
    {
        #region Singleton
        private static readonly Lazy<CharacterSpriteController> _instance = new Lazy<CharacterSpriteController>(() => new CharacterSpriteController());

        public static CharacterSpriteController Instance { get { return _instance.Value; } }

        private CharacterSpriteController()
        {
        }
        #endregion

        public GameObject CharacterPrefab;
        private readonly Dictionary<Character, GameObject> _characterGameObjectMap = new Dictionary<Character, GameObject>();
        private readonly Dictionary<Character, Dictionary<string, GameObject>> _characterSpriteParts = new Dictionary<Character, Dictionary<string, GameObject>>();

        /// <summary>
        /// This is just a helper property to make it easier to access World.
        /// </summary>
        private static World World { get { return WorldController.Instance.World; } }

        public void Start()
        {
            CharacterPrefab = new GameObject()
            {
                Name = "Default",
                Sprite = SpriteManager.Instance.GetSprite("colonist", "default"),
                ActiveSprite = SpriteManager.Instance.GetSprite("colonist", "default"),
                IsActive = true,
                SortingLayerName = LAYER.DEFAULT
            };

            World.RegisterCharacterCreatedCb(OnCharacterCreated);

            foreach (var c in World.Characters)
            {
                OnCharacterCreated(c);
            }
        }

        public void Update() { }
        public void Render(LAYER layer) { }

        public void OnCharacterCreated(Character character)
        {
            var characterGo = GameObject.Instantiate(CharacterPrefab);
            characterGo.Name = "Character " + character.Name;
            _characterGameObjectMap.Add(character, characterGo);
            // characterGo.transform.SetParent(this.transform, true);

            SetSpriteForCharacter(character, characterGo, "default", true);
            SetSpriteForCharacter(character, characterGo, "shield", false);
            SetSpriteForCharacter(character, characterGo, "working", false);
            SetSpriteForCharacter(character, characterGo, "footstep", false);

            character.RegisterOnChangedCallback(OnCharacterChanged);
        }

         
        private void OnCharacterChanged(Character character)
        {
            if (_characterGameObjectMap.ContainsKey(character) == false)
            {
                Debug.LogError("OnCharacterChanged failed - Character requested that is not in the map!");
                return;
            }

            var charGo = _characterGameObjectMap[character];
            charGo.Position = new WorldCoord(character.X, character.Y);

            // Draw 'footsteps' for any current path on this character
            if (character.CurrentTile != null && character.Path != null && character.Path.Length() > 0)
            {
                var p = new Path_AStar(character.Path);
                Tile t;
                do
                {
                    t = p.Dequeue();
                    if (t != null)
                    {
                        var go = new GameObject
                        {
                            Name = "footstep",
                            Position = new WorldCoord(t.X, t.Y),
                            Sprite = SpriteManager.Instance.GetSprite("colonist", "footstep"),
                            // go.Sprite.sortingLayerName = "Characters";
                            IsActive = true
                        };
                        // Destroy(go, 1);
                    }
                } while (t != null);

            }

            // Set the various subsprite visibility depending on what the situation is for this character.
            _characterSpriteParts[character]["working"].IsActive = character.CurrentState == Character.State.WorkingJob;
            _characterSpriteParts[character]["shield"].IsActive = character.ShieldStatus;
        }

        public void SetSpriteForCharacter(Character character, GameObject go, string part, bool visible = true)
        {
            var subpartGo = new GameObject
            {
                Name = part,
                Sprite = SpriteManager.Instance.GetSprite("colonist", part)
            };

            //subpartGo.SpriteRenderer = new SpriteRenderer();
            //subpartGo.SpriteRenderer.sprite = SpriteManager.Instance.GetSprite("colonist", part);
            //subpartGo.SpriteRenderer.sortingLayerName = "Characters";
            //subpartGo.SpriteRenderer.enabled = visible;

            if (_characterSpriteParts.ContainsKey(character) == false)
            {
                _characterSpriteParts.Add(character, new Dictionary<string, GameObject>());
            }

            _characterSpriteParts[character].Add(part, subpartGo);
        }
    }
}
