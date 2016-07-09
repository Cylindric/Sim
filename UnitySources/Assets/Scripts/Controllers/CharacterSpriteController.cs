using System.Collections.Generic;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    class CharacterSpriteController : MonoBehaviour
    {
        private readonly Dictionary<Character, GameObject> _characterGameObjectMap = new Dictionary<Character, GameObject>();

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

            characterGo.name = "Character";
            characterGo.transform.position = new Vector3(character.X, character.Y, 0);
            characterGo.transform.SetParent(this.transform, true);

            var sr = characterGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForCharacter(character);
            sr.sortingLayerName = "Characters";

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
        }

        public Sprite GetSpriteForCharacter(Character obj)
        {
            return SpriteManager.Instance.GetSprite("colonist_body");
        }
    }
}
