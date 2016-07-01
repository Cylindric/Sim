using System.Collections.Generic;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    class InventorySpriteController : MonoBehaviour
    {
        private readonly Dictionary<Inventory, GameObject> _inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();
        private readonly Dictionary<string, Sprite> _inventorySprites = new Dictionary<string, Sprite>();

        /// <summary>
        /// This is just a helper property to make it easier to access World.
        /// </summary>
        private static World World { get { return WorldController.Instance.World; } }

        private void Start()
        {
            LoadSprites();
            World.RegisterInventoryCreatedCb(OnInventoryCreated);

            //foreach (var c in World._characters)
            //{
            //    OnInventoryCreated(c);
            //}
        }

        void Update()
        {
            
        }

        public void OnInventoryCreated(Inventory inv)
        {
            Debug.Log("InventorySpriteController::OnInventoryCreated()");
            var invGo = new GameObject();
            _inventoryGameObjectMap.Add(inv, invGo);

            invGo.name = "Inv" + inv.objectType;
            invGo.transform.position = new Vector3(inv.X, inv.Y, 0);
            invGo.transform.SetParent(this.transform, true);

            var sr = invGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForCharacter(inv);
            sr.sortingLayerName = "Characters";

            inv.RegisterOnChangedCallback(OnCharacterChanged);
        }

        private void OnCharacterChanged(Character character)
        {
            if (_inventoryGameObjectMap.ContainsKey(character) == false)
            {
                Debug.LogError("OnCharacterChanged failed - Character requested that is not in the map!");
                return;
            }

            var charGo = _inventoryGameObjectMap[character];
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
                _inventorySprites.Add(sprite.name, sprite);
            }
        }

        public Sprite GetSpriteForCharacter(Character obj)
        {
            var spriteName = "body";

            if (_inventorySprites.ContainsKey(spriteName) == false)
            {
                Debug.LogErrorFormat("Attempt to load missing sprite [{0}] failed!", spriteName);
                return null;
            }

            return _inventorySprites[spriteName];
        }
    }
}
