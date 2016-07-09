using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Assets.Scripts.Model
{
    [MoonSharpUserData]
    public class InventoryManager
    {
        public Dictionary<string, List<Inventory>> Inventories { get; private set; }

        public InventoryManager()
        {
            Inventories = new Dictionary<string, List<Inventory>>();
        }

        private void CleanupInventory(Inventory inv)
        {
            if (inv.StackSize == 0)
            {
                if (Inventories.ContainsKey(inv.ObjectType))
                {
                    Inventories[inv.ObjectType].Remove(inv);
                }

                if (inv.Tile != null)
                {
                    inv.Tile.Inventory = null;
                    inv.Tile = null;
                }
                if (inv.Character != null)
                {
                    inv.Character.Inventory = null;
                    inv.Character = null;
                }
            }
        }

        public bool PlaceInventory(Tile tile, Inventory source)
        {
            bool tileWasEmpty = tile.Inventory == null;

            if (tile.PlaceInventory(source) == false)
            {
                return false;
            }

            // If the stack-size is zero, remove it.
            CleanupInventory(source);

            if (tileWasEmpty)
            {
                if (Inventories.ContainsKey(tile.Inventory.ObjectType) == false)
                {
                    Inventories[tile.Inventory.ObjectType] = new List<Inventory>();
                }
                Inventories[tile.Inventory.ObjectType].Add(tile.Inventory);

                World.Instance.OnInventoryCreated(tile.Inventory);
            }

            return true;
        }

        public bool PlaceInventory(Job job, Inventory source)
        {
            if (job.InventoryRequirements.ContainsKey(source.ObjectType) == false)
            {
                Debug.LogError("Trying to add Inventory to a Character that doesn't want it.");
                return false;
            }

            job.InventoryRequirements[source.ObjectType].StackSize += source.StackSize;

            if (job.InventoryRequirements[source.ObjectType].StackSize >
                job.InventoryRequirements[source.ObjectType].MaxStackSize)
            {
                source.StackSize = job.InventoryRequirements[source.ObjectType].StackSize - job.InventoryRequirements[source.ObjectType].MaxStackSize;
                job.InventoryRequirements[source.ObjectType].StackSize =
                    job.InventoryRequirements[source.ObjectType].MaxStackSize;
            }
            else
            {
                source.StackSize = 0;
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
                qty = source.StackSize;
            }
            
            // If the qty is greater than what's available, limit it to what is available
            qty = Mathf.Min(qty, source.StackSize);

            if (character.Inventory == null)
            {
                character.Inventory = source.Clone();
                character.Inventory.StackSize = 0;
                Inventories[character.Inventory.ObjectType].Add(character.Inventory);

            } else if (character.Inventory.ObjectType != source.ObjectType)
            {
                Debug.LogError("Character is trying to pick up Inventory when already carrying something else.");
                return false;
            }

            // If the Qty is greater than the Character's capacity, limit to capacity
            qty = Mathf.Min(character.Inventory.Space, qty);
            
            // Transfer the items from the source to the Character
            character.Inventory.StackSize += qty;
            source.StackSize -= qty;

            // If the stack-size is zero, remove it.
            CleanupInventory(source);

            return true;
        }

        public Inventory GetClosestInventoryOfType(string objectType, Tile t, int desiredQty, bool searchInStockpiles)
        {
            if (Inventories.ContainsKey(objectType) == false)
            {
                // Debug.LogError("Trying to find closest Inventory for a type we don't have.");
                return null;
            }

            foreach (var inv in Inventories[objectType])
            {
                if (inv.Tile != null)
                {
                    if (inv.Tile.Furniture != null && inv.Tile.Furniture.IsStockpile() && searchInStockpiles == false)
                    {
                        return null;
                    }
                    return inv;
                }
            }

            return null;
        }
    }
}
