using System.Collections.Generic;
using Engine.Pathfinding;
using MoonSharp.Interpreter;
// using UnityEngine;
using System.Linq;

namespace Engine.Model
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

        public bool TransferInventory(Tile tile, Inventory source)
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

        public bool TransferInventory(Job job, Inventory source)
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

        public bool TransferInventory(Character character, Inventory source, int qty = -1)
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
            var path = GetClosestPathToInventoryOfType(objectType, t, desiredQty, searchInStockpiles);
            return path.EndTile().Inventory;
        }

        public Path_AStar GetClosestPathToInventoryOfType(string objectType, Tile t, int desiredQty, bool searchInStockpiles)
        {
            // If we've never seen any of this object type before, there aren't any.
            if (Inventories.ContainsKey(objectType) == false)
            {
                return null;
            }

            // If there aren't any stacks with items actually in them, give up
            if(Inventories.Where(o => o.Key == objectType && o.Value.Count > 0).Any() == false)
            {
                return null;
            }

            var path = new Path_AStar()
            {
                World = World.Instance,
                Start = t,
                ObjectType = objectType,
                CanTakeFromStockpile = true
            };
            path.Calculate();
            return path;
        }
    }
}
