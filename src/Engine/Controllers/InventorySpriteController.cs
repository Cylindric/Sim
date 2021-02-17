using System.Collections.Generic;
using Engine.Models;
using Engine.Utilities;

namespace Engine.Controllers
{
    class InventorySpriteController
    {
        public GameObject inventoryUIPrefab;

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
            var invGo = new GameObject();
            _inventoryGameObjectMap.Add(inv, invGo);

            invGo.Name = "Inv" + inv.ObjectType;
            invGo.Position = new WorldCoord(inv.Tile.X, inv.Tile.Y);
            invGo.Sprite = GetSpriteForInventory(inv);
            invGo.SortingLayerName = Engine.LAYER.DEFAULT;

            if (inv.MaxStackSize > 1)
            {
                var uiGo = GameObject.Instantiate(inventoryUIPrefab);
                uiGo.Position = WorldCoord.Zero;
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

            if (inv.StackSize == 0)
            {
                // the stack size is now zero, so remove the sprite
                _inventoryGameObjectMap.Remove(inv);
                inv.UnRegisterInventoryChangedCallback(OnInventoryChanged);
            }

            invGo.Position = new WorldCoord(inv.Tile.X, inv.Tile.Y);
        }

        public Sprite GetSpriteForInventory(Inventory obj)
        {
            return SpriteManager.Instance.GetSprite("inventory_" + obj.ObjectType, "default");
        }
    }
}
