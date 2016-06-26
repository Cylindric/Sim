using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Model;
using UnityEngine;

namespace Assets.Controllers
{
    class CharacterSpriteController : MonoBehaviour
    {
        private readonly Dictionary<Character, GameObject> _characterGameObjectMap = new Dictionary<Character, GameObject>();
        private readonly Dictionary<string, Sprite> _characterSprites = new Dictionary<string, Sprite>();

        /// <summary>
        /// This is just a helper property to make it easier to access World.
        /// </summary>
        private static World World { get { return WorldController.Instance.World; } }

        private void Start()
        {
            LoadSprites();
            World.RegisterCharacterCreatedCb(OnCharacterCreated);

            var c = World.CreateCharacter(World.GetTileAt(World.Width/2, World.Height/2));
        }

        void Update()
        {
            
        }

        public void OnCharacterCreated(Character character)
        {
            Debug.Log("CharacterSpriteController::OnCharacterCreated()");
            var characterGo = new GameObject();
            _characterGameObjectMap.Add(character, characterGo);

            characterGo.name = "Character";
            characterGo.transform.position = new Vector3(character.X, character.Y, 0);
            characterGo.transform.SetParent(this.transform, true);

            var sr = characterGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForCharacter(character);
            sr.sortingLayerName = "Characters";

            character.RegisterOnChangeCallback(OnCharacterChanged);
        }

        private void OnCharacterChanged(Character character)
        {
            if (_characterGameObjectMap.ContainsKey(character) == false)
            {
                Debug.LogError("OnCharacterChanged failed - Character requested that is not in the map!");
                return;
            }

            var charGo = _characterGameObjectMap[character];
            charGo.transform.position = new Vector3(character.X, character.Y, 0);
        }

        private void LoadSprites()
        {
            var sprites = Resources.LoadAll<Sprite>("Characters/Colonist");
            if (sprites.Length == 0)
            {
                Debug.LogError("Failed to load any sprites from the spritesheet [Characters/Colonist]2");
            }
            foreach (var sprite in sprites)
            {
                _characterSprites.Add(sprite.name, sprite);
            }
        }

        public Sprite GetSpriteForCharacter(Character obj)
        {
            var spriteName = "body";

            if (_characterSprites.ContainsKey(spriteName) == false)
            {
                Debug.LogErrorFormat("Attempt to load missing sprite [{0}] failed!", spriteName);
                return null;
            }

            return _characterSprites[spriteName];
        }
    }
}
