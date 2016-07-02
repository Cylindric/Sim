using Assets.Scripts.Controllers;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public static class FurnitureActions
    {
        public static void Door_UpdateAction(Furniture furn, float deltaTime)
        {
            if (furn.GetParameter("is_opening") >= 1f)
            {
                furn.OffsetParameter("openness", deltaTime*4);

                if (furn.GetParameter("openness") >= 1f)
                {
                    furn.SetParameter("is_opening", 0);
                }
            }
            else
            {
                furn.OffsetParameter("openness", deltaTime * -4);
            }

            furn.SetParameter("openness", Mathf.Clamp01(furn.GetParameter("openness")));
            furn.cbOnChanged(furn);
        }

        public static Enterability Door_IsEnterable(Furniture furn)
        {
            furn.SetParameter("is_opening", 1);

            if (furn.GetParameter("openness") >= 1)
            {
                return Enterability.Yes;
            }

            return Enterability.Soon;
        }

        public static void JobComplete_FurnitureBuilding(Job theJob)
        {
            WorldController.Instance.World.PlaceFurniture(theJob.JobObjectType, theJob.Tile);
            theJob.Tile.PendingFurnitureJob = null;
        }

        public static Inventory[] Stockpile_GetItemsFromFilter()
        {
            return new Inventory[1] { new Inventory("Steel Plate", 50, 0) };
        }

        /// <summary>
        /// Ensures that there is Job on the queue asking for Inventory for this Stockpile.
        /// </summary>
        /// <remarks>
        /// This doesn't need to run on every Update. It only needs to run whenever:
        ///   -- It gets created
        ///   -- An item gets delivered
        ///   -- An item gets picked up
        ///   -- This stockpile is reconfigured
        /// </remarks>
        /// <param name="furn"></param>
        /// <param name="deltaTime"></param>
        public static void Stockpile_UpdateAction(Furniture furn, float deltaTime)
        {
            if (furn.Tile.inventory != null && furn.Tile.inventory.stackSize >= furn.Tile.inventory.maxStackSize)
            {
                // We are full!
                furn.ClearJobs();
                return;
            }

            // Maybe we already have a job queued up?
            if (furn.GetJobCount() > 0)
            {
                // All done.
                return;
            }

            // We currently are NOT full, but we don't have a job either.
            // We either have some inventory already, or no inventory yet.
            // Alternatively, something has gone wrong.
            if (furn.Tile.inventory != null && furn.Tile.inventory.stackSize == 0)
            {
                Debug.LogError("Stockpile has a zero-size stack.");
                furn.ClearJobs();
                return;
            }

            Inventory[] itemsDesired;

            // Make sure there's a job on the queue asking for inventory to be brought to us.
            if (furn.Tile.inventory == null)
            {
                itemsDesired = Stockpile_GetItemsFromFilter();
            }
            else
            {
                // Some stuff is here, but we're not full.
                var desiredInv = furn.Tile.inventory.Clone();
                desiredInv.maxStackSize -= desiredInv.stackSize;
                desiredInv.stackSize = 0;

                itemsDesired = new Inventory[1] {desiredInv};
            }

            var j = new Job(
                tile: furn.Tile,
                jobObjectType: null,
                cb: null,
                jobTime: 0f,
                requirements: itemsDesired);

            j.CanTakeFromStockpile = false;
            j.RegisteJobWorkedCallback(Stockpile_JobWorked);

            furn.AddJob(j);
        }

        private static void Stockpile_JobWorked(Job j)
        {
            j.Tile.Furniture.RemoveJob(j);

            foreach (var inv in j._inventoryRequirements.Values)
            {
                if (inv.stackSize > 0)
                {
                    j.Tile.World.InventoryManager.PlaceInventory(j.Tile, inv);
                    return;
                }
            }
        }
    }
}
