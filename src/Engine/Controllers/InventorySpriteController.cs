using System.Collections.Generic;
using Engine.Models;
// using UnityEngine;
using UnityEngine.UI;
using Sprite = UnityEngine.Sprite;

namespace Engine.Controllers
{
    class InventorySpriteController// : MonoBehaviour
    {
#pragma warning disable 0649
        public GameObject inventoryUIPrefab;
#pragma warning restore 0649

        private readonly Dictionary<Inventory, GameObject> _inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        /// <summary>
        /// This is just a helper property to make it easier to access World.
        /// </summary>
        private static World World { get { return WorldController.Instance.World; } }

        private void Start()
        {
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
            // Debug.Log("InventorySpriteController::OnInventoryCreated()");
            var invGo = new GameObject();
            _inventoryGameObjectMap.Add(inv, invGo);

            invGo.name = "Inv" + inv.ObjectType;
            invGo.transform.position = new Vector3(inv.Tile.X, inv.Tile.Y, 0);
            invGo.transform.SetParent(this.transform, true);

            var sr = invGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForInventory(inv);
            sr.sortingLayerName = "Inventory";

            if (inv.MaxStackSize > 1)
            {
                var uiGo = Instantiate(inventoryUIPrefab);
                uiGo.transform.SetParent(invGo.transform);
                uiGo.transform.localPosition = Vector3.zero;
                uiGo.GetComponentInChildren<Text>().text = inv.StackSize.ToString();
            }

            inv.RegisterInventoryChangedCallback(OnInventoryChanged);
        }

        private void OnInventoryChanged(Inventory inv)
        {
            if (_inventoryGameObjectMap.ContainsKey(inv) == false)
            {
                Debug.LogError("OnInventoryChanged failed - Inventory requested that is not in the map!");
                return;
            }

            var invGo = _inventoryGameObjectMap[inv];

            if (inv.StackSize > 0)
            {
                var text = invGo.GetComponentInChildren<Text>();

                if (text != null)
                {
                    text.text = inv.StackSize.ToString();
                }
            }
            else
            {
                // the stack size is now zero, so remove the sprite
                Destroy(invGo);
                _inventoryGameObjectMap.Remove(inv);
                inv.UnRegisterInventoryChangedCallback(OnInventoryChanged);
            }

            invGo.transform.position = new Vector3(inv.Tile.X, inv.Tile.Y, 0);
        }

        public Sprite GetSpriteForInventory(Inventory obj)
        {
            return SpriteManager.Instance.GetSprite("inventory_" + obj.ObjectType, "default");
        }
    }
}
