using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Controllers;
using MoonSharp.Interpreter;
using MoonSharp.RemoteDebugger;
using Debug = UnityEngine.Debug;

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

        //RemoteDebuggerService remoteDebugger;

        //private void ActivateRemoteDebugger(Script script)
        //{
        //    if (remoteDebugger == null)
        //    {
        //        remoteDebugger = new RemoteDebuggerService();

        //        // the last boolean is to specify if the script is free to run 
        //        // after attachment, defaults to false
        //        remoteDebugger.Attach(script, "Description of the script", false);
        //    }

        //    // start the web-browser at the correct url. Replace this or just
        //    // pass the url to the user in some way.
        //    Process.Start(remoteDebugger.HttpUrlStringLocalHost);
        //}

        public static void LoadLua(string rawLuaCode)
        {
            // Tell LUA to load all the classes that we have marked as MoonSharpUserData
            var result = _instance.myLuaScript.DoString(rawLuaCode);
            if (result.Type == DataType.String)
            {
                Debug.LogError(result.String);
            }
            //_instance.ActivateRemoteDebugger(_instance.myLuaScript);
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
                if (result.Type != DataType.Void)
                {
                    if (result.Type == DataType.Number)
                        Debug.LogFormat("{0} {1}", fname, result.Number);

                    if (result.Type == DataType.String)
                        Debug.LogFormat("{0} {1}", fname, result.String);

                    if (result.Type == DataType.UserData)
                    {
                        Debug.LogFormat("{0} {1}", fname, result.UserData.Object.ToString());
                        var j = new Job();
                    }

                }
            }
        }

        public static DynValue CallFunction(string fname, params object[] args)
        {
            var func = _instance.myLuaScript.Globals[fname];

            if (func == null)
            {
                Debug.LogErrorFormat("Function {0} is not a Lua function.", fname);
                return DynValue.Nil;
            }

            return _instance.myLuaScript.Call(func, args);
        }

        public static void JobComplete_FurnitureBuilding(Job theJob)
        {
            WorldController.Instance.World.PlaceFurniture(theJob.JobObjectType, theJob.Tile);
            theJob.Tile.PendingFurnitureJob = null;
        }

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