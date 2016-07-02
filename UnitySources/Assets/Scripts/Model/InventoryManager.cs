using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets.Scripts.Pathfinding;
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

        private void CleanupInventory(Inventory inv)
        {
            if (inv.stackSize == 0)
            {
                if (Inventories.ContainsKey(inv.objectType))
                {
                    Inventories[inv.objectType].Remove(inv);
                }

                if (inv.tile != null)
                {
                    inv.tile.inventory = null;
                    inv.tile = null;
                }
                if (inv.character != null)
                {
                    inv.character.inventory = null;
                    inv.character = null;
                }
            }
        }

        public bool PlaceInventory(Tile tile, Inventory source)
        {
            bool tileWasEmpty = tile.inventory == null;

            if (tile.PlaceInventory(source) == false)
            {
                return false;
            }

            // If the stack-size is zero, remove it.
            CleanupInventory(source);

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

        public bool PlaceInventory(Job job, Inventory source)
        {
            if (job._inventoryRequirements.ContainsKey(source.objectType) == false)
            {
                Debug.LogError("Trying to add inventory to a character that doesn't want it.");
                return false;
            }

            job._inventoryRequirements[source.objectType].stackSize += source.stackSize;

            if (job._inventoryRequirements[source.objectType].stackSize >
                job._inventoryRequirements[source.objectType].maxStackSize)
            {
                source.stackSize = job._inventoryRequirements[source.objectType].stackSize - job._inventoryRequirements[source.objectType].maxStackSize;
                job._inventoryRequirements[source.objectType].stackSize =
                    job._inventoryRequirements[source.objectType].maxStackSize;
            }
            else
            {
                source.stackSize = 0;
            }

            // If the stack-size is zero, remove it.
            CleanupInventory(source);

            return true;
        }

        public bool PlaceInventory(Character character, Inventory source, int qty = -1)
        {
            // If no Quantity specified, assume 'everything'.
            if (qty < 0)
            {
                qty = source.stackSize;
            }
            
            // If the qty is greater than what's available, limit it to what is available
            qty = Mathf.Min(qty, source.stackSize);

            if (character.inventory == null)
            {
                character.inventory = source.Clone();
                character.inventory.stackSize = 0;
                Inventories[character.inventory.objectType].Add(character.inventory);

            } else if (character.inventory.objectType != source.objectType)
            {
                Debug.LogError("Character is trying to pick up inventory when already carrying something else.");
                return false;
            }

            // If the Qty is greater than the character's capacity, limit to capacity
            qty = Mathf.Min(character.inventory.Space, qty);
            
            // Transfer the items from the source to the character
            character.inventory.stackSize += qty;
            source.stackSize -= qty;

            // If the stack-size is zero, remove it.
            CleanupInventory(source);

            return true;
        }

        public Inventory GetClosestInventoryOfType(string objectType, Tile t, int desiredQty)
        {
            if (Inventories.ContainsKey(objectType) == false)
            {
                Debug.LogError("Trying to find closest inventory for a type we don't have.");
                return null;
            }

            foreach (var inv in Inventories[objectType])
            {
                if (inv.tile != null)
                {
                    return inv;
                }
            }

            return null;
        }
    }
}
