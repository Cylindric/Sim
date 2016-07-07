using System.Collections.Generic;
using Assets.Scripts.Controllers;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class FurnitureActions
    {
        private static FurnitureActions _instance;
        private Script myLuaScript;

        static FurnitureActions()
        {
            UserData.RegisterAssembly();

            _instance = new FurnitureActions();
            _instance.myLuaScript = new Script();
        }

        public static void LoadLua(string rawLuaCode)
        {
            // Tell LUA to load all the classes that we have marked as MoonSharpUserData
            var result = _instance.myLuaScript.DoString(rawLuaCode);
            if (result.Type == DataType.String)
            {
                Debug.LogError(result.String);
            }
        }

        public static void CallFunctionsWithFurniture(IEnumerable<string> functionNames, Furniture furn, float deltaTime)
        {
            foreach (var fname in functionNames)
            {
                var func = _instance.myLuaScript.Globals[fname];

                if (func == null)
                {
                    Debug.LogErrorFormat("Function {0} is not a LUA function.", fname);
                    return;
                }

                var result = _instance.myLuaScript.Call(func, new object[] { furn, deltaTime });
                if (result.Type == DataType.String)
                {
                    Debug.Log(result.String);
                }
            }
        }

        public static DynValue CallFunction(string fname, params object[] args)
        {
            var func = _instance.myLuaScript.Globals[fname];

            if (func == null)
            {
                Debug.LogErrorFormat("Function {0} is not a LUA function.", fname);
                return DynValue.Nil;
            }

            return _instance.myLuaScript.Call(func, args);
        }

        public static void JobComplete_FurnitureBuilding(Job theJob)
        {
            WorldController.Instance.World.PlaceFurniture(theJob.JobObjectType, theJob.Tile);
            theJob.Tile.PendingFurnitureJob = null;
        }

        //public static Inventory[] Stockpile_GetItemsFromFilter()
        //{
        //    return new Inventory[1] {new Inventory("steel_plate", 50, 0)};
        //}

        ///// <summary>
        ///// Ensures that there is Job on the queue asking for Inventory for this Stockpile.
        ///// </summary>
        ///// <remarks>
        ///// This doesn't need to run on every Update. It only needs to run whenever:
        /////   -- It gets created
        /////   -- An item gets delivered
        /////   -- An item gets picked up
        /////   -- This stockpile is reconfigured
        ///// </remarks>
        ///// <param name="furn"></param>
        ///// <param name="deltaTime"></param>
        //public static void Stockpile_UpdateAction(Furniture furn, float deltaTime)
        //{
        //    if (furn.Tile.inventory != null && furn.Tile.inventory.stackSize >= furn.Tile.inventory.maxStackSize)
        //    {
        //        // We are full!
        //        furn.CancelJobs();
        //        return;
        //    }

        //    // Maybe we already have a job queued up?
        //    if (furn.GetJobCount() > 0)
        //    {
        //        // All done.
        //        return;
        //    }

        //    // We currently are NOT full, but we don't have a job either.
        //    // We either have some inventory already, or no inventory yet.
        //    // Alternatively, something has gone wrong.
        //    if (furn.Tile.inventory != null && furn.Tile.inventory.stackSize == 0)
        //    {
        //        Debug.LogError("Stockpile has a zero-size stack.");
        //        furn.CancelJobs();
        //        return;
        //    }

        //    Inventory[] itemsDesired;

        //    // Make sure there's a job on the queue asking for inventory to be brought to us.
        //    if (furn.Tile.inventory == null)
        //    {
        //        itemsDesired = Stockpile_GetItemsFromFilter();
        //    }
        //    else
        //    {
        //        // Some stuff is here, but we're not full.
        //        var desiredInv = furn.Tile.inventory.Clone();
        //        desiredInv.maxStackSize -= desiredInv.stackSize;
        //        desiredInv.stackSize = 0;

        //        itemsDesired = new Inventory[1] {desiredInv};
        //    }

        //    var j = new Job(
        //        name: "StockpileFetch" + itemsDesired[0].objectType,
        //        tile: furn.Tile,
        //        jobObjectType: null,
        //        cb: null,
        //        jobTime: 0f,
        //        requirements: itemsDesired);

        //    j.CanTakeFromStockpile = false;
        //    j.RegisterOnJobWorkedCallback(Stockpile_JobWorked);

        //    furn.AddJob(j);
        //}

        //private static void Stockpile_JobWorked(Job j)
        //{
        //    j.CancelJob();

        //    foreach (var inv in j._inventoryRequirements.Values)
        //    {
        //        if (inv.stackSize > 0)
        //        {
        //            World.Current.InventoryManager.PlaceInventory(j.Tile, inv);
        //            return;
        //        }
        //    }
        //}

        //public static void MiningConsole_UpdateAction(Furniture furn, float deltaTime)
        //{
        //    var spawnSpot = furn.GetSpawnSpotTile();

        //    if (furn.GetJobCount() > 0)
        //    {
        //        // If the destination Tile is full of Iron, stop the job.
        //        if (spawnSpot.inventory != null && (spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize))
        //        {
        //            // the job spot is full, so cancel
        //            furn.CancelJobs();
        //        }

        //        // There's already a Job, so nothing to do.
        //        return;
        //    }

        //    // If we get here, then we have no current Job. Check to see if our destination is full
        //    if (spawnSpot.inventory != null && (spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize))
        //    {
        //        // the job spot is full!
        //        return;
        //    }

        //    var jobSpot = furn.GetJobSpotTile();

        //    var j = new Job(
        //        name: "MiningConsole_Work",
        //        tile: jobSpot, 
        //        jobObjectType: null,
        //        cb: MiningConsole_JobComplete,
        //        jobTime: 0.3f,
        //        requirements: null,
        //        repeats: true);
        //    //j.RegisterOnJobStoppedCallback(MiningConsole_JobStopped);

        //    furn.AddJob(j);
        //}

        //public static void MiningConsole_JobComplete(Job job)
        //{
        //    // Spawn some Steel Plates from the console
        //    var steel = new Inventory("steel_plate", 50, 5);
        //    World.Current.InventoryManager.PlaceInventory(job.Furniture.GetSpawnSpotTile(), steel);
        //}

        ////public static void MiningConsole_JobStopped(Job job)
        ////{
        ////    job.UnregisterOnJobStoppedCallback(MiningConsole_JobStopped);
        ////    job.Furniture.RemoveJob(job);
        ////}
    }
}