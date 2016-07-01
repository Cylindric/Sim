using System.Collections.Generic;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    class InventorySpriteController : MonoBehaviour
    {
        public GameObject inventoryUIPrefab;

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

            foreach (var objectType in World.InventoryManager.Inventories.Keys)
            {
                foreach (var inv in World.InventoryManager.Inventories[objectType])
                {
                    OnInventoryCreated(inv);
                }
            }
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
            invGo.transform.position = new Vector3(inv.tile.X, inv.tile.Y, 0);
            invGo.transform.SetParent(this.transform, true);

            var sr = invGo.AddComponent<SpriteRenderer>();
            sr.sprite = _inventorySprites[inv.objectType];
            sr.sortingLayerName = "Inventory";

            if (inv.maxStackSize > 1)
            {
                var uiGo = Instantiate(inventoryUIPrefab);
                uiGo.transform.SetParent(invGo.transform);
                uiGo.transform.localPosition = Vector3.zero;
                uiGo.GetComponentInChildren<Text>().text = inv.stackSize.ToString();
            }

            // inv.RegisterOnChangedCallback(OnInventoryChanged);
        }

        private void OnInventoryChanged(Character character)
        {
            //if (_inventoryGameObjectMap.ContainsKey(character) == false)
            //{
            //    Debug.LogError("OnCharacterChanged failed - Character requested that is not in the map!");
            //    return;
            //}

            //var charGo = _inventoryGameObjectMap[character];
            //charGo.transform.position = new Vector3(character.X, character.Y, 0);
        }

        private void LoadSprites()
        {
            var sprites = Resources.LoadAll<Sprite>("Inventory/plate");
            if (sprites.Length == 0)
            {
                Debug.LogError("Failed to load any sprites from the spritesheet [Inventory/pate]");
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
