using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class InventoryManager
    {
        public Dictionary<string, List<Inventory>> Inventories { get; private set; }

        public InventoryManager()
        {
            Inventories = new Dictionary<string, List<Inventory>>();
        }

        public bool PlaceInventory(Tile tile, Inventory inv)
        {
            bool tileWasEmpty = tile.inventory == null;

            if (tile.PlaceInventory(inv) == false)
            {
                return false;
            }

            // If the stack-size is zero, remove it.
            if (inv.stackSize == 0)
            {
                if (Inventories.ContainsKey(tile.inventory.objectType))
                {
                    Inventories[inv.objectType].Remove(inv);
                }
            }

            if (tileWasEmpty)
            {
                if (Inventories.ContainsKey(tile.inventory.objectType) == false)
                {
                 Inventories[tile.inventory.objectType] = new List<Inventory>();   
                }
                Inventories[tile.inventory.objectType].Add(tile.inventory);
            }

            return true;
        }

    }
}
