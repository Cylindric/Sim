using System.Collections.Generic;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    class CharacterSpriteController : MonoBehaviour
    {
        private readonly Dictionary<Character, GameObject> _characterGameObjectMap = new Dictionary<Character, GameObject>();
        private Dictionary<Character, Dictionary<string, SpriteRenderer>> _characterSpriteParts = new Dictionary<Character, Dictionary<string, SpriteRenderer>>();

        /// <summary>
        /// This is just a helper property to make it easier to access World.
        /// </summary>
        private static World World { get { return WorldController.Instance.World; } }

        private void Start()
        {
            World.RegisterCharacterCreatedCb(OnCharacterCreated);

            foreach (var c in World.Characters)
            {
                OnCharacterCreated(c);
            }
        }

        void Update()
        {
            
        }

        public void OnCharacterCreated(Character character)
        {
            var characterGo = new GameObject();
            _characterGameObjectMap.Add(character, characterGo);

            characterGo.name = "Character " + character.Name;

            characterGo.transform.position = new Vector3(character.X, character.Y, 0);
            characterGo.transform.SetParent(this.transform, true);

            SetSpriteForCharacter(character, characterGo, "body", true);
            SetSpriteForCharacter(character, characterGo, "shield", false);
            SetSpriteForCharacter(character, characterGo, "working", false);

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
            charGo.transform.position = new Vector3(character.X, character.Y, 0);

            // Set the various subsprite visibility depending on what the situation is for this character.
            _characterSpriteParts[character]["working"].enabled = character.IsWorking;
            _characterSpriteParts[character]["shield"].enabled = !character.CanBreathe();
        }

        public void SetSpriteForCharacter(Character character, GameObject go, string part, bool visible = true)
        {
            var subpartGo = new GameObject();
            subpartGo.transform.SetParent(go.transform, false);

            var sprite = subpartGo.AddComponent<SpriteRenderer>();
            sprite.sprite = SpriteManager.Instance.GetSprite("colonist_" + part);
            sprite.sortingLayerName = "Characters";
            sprite.enabled = visible;

            if (_characterSpriteParts.ContainsKey(character) == false)
            {
                _characterSpriteParts.Add(character, new Dictionary<string, SpriteRenderer>());
            }

            _characterSpriteParts[character].Add(part, sprite);
        }
    }
}
