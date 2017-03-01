using System.Collections.Generic;
using Assets.Scripts.Model;
using Assets.Scripts.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    class CharacterSpriteController : MonoBehaviour
    {
#pragma warning disable 0649
        public GameObject CharacterPrefab;
#pragma warning restore 0649

        private readonly Dictionary<Character, GameObject> _characterGameObjectMap = new Dictionary<Character, GameObject>();
        private readonly Dictionary<Character, Dictionary<string, SpriteRenderer>> _characterSpriteParts = new Dictionary<Character, Dictionary<string, SpriteRenderer>>();

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

        public void OnCharacterCreated(Character character)
        {
            var characterGo = Instantiate(CharacterPrefab);
            characterGo.name = "Character " + character.Name;
            _characterGameObjectMap.Add(character, characterGo);
            characterGo.transform.SetParent(this.transform, true);

            var script = (CharacterCollider)characterGo.GetComponentInChildren<MonoBehaviour>();
            script.Character = character;

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
            charGo.transform.position = new Vector3(character.X, character.Y, 0);

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
                        var go = new GameObject();
                        go.name = "footstep";
                        go.transform.position = new Vector3(t.X, t.Y, 0);
                        var sprite = go.AddComponent<SpriteRenderer>();
                        sprite.sprite = SpriteManager.Instance.GetSprite("colonist", "footstep");
                        sprite.sortingLayerName = "Characters";
                        sprite.enabled = true;
                        Destroy(go, 1);
                    }
                } while (t != null);

            }

            // Set the various subsprite visibility depending on what the situation is for this character.
            _characterSpriteParts[character]["working"].enabled = character.CurrentState == Character.State.WorkingJob;
            _characterSpriteParts[character]["shield"].enabled = character.ShieldStatus;
        }

        public void SetSpriteForCharacter(Character character, GameObject go, string part, bool visible = true)
        {
            var subpartGo = new GameObject();
            subpartGo.name = part;
            subpartGo.transform.SetParent(go.transform, false);

            var sprite = subpartGo.AddComponent<SpriteRenderer>();
            sprite.sprite = SpriteManager.Instance.GetSprite("colonist", part);
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
