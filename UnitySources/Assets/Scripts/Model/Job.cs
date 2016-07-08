using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Assets.Scripts.Model
{
    [MoonSharpUserData]
    public class Job
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        public float _jobTime { get; private set; }
        public string Name { get; set; }
        public Dictionary<string, Inventory> _inventoryRequirements;
        public Furniture FurniturePrototype;

        private float jobTimeRequired;
        private bool jobRepeats = false;

        private Action<Job> cbOnJobCompleted;
        private List<string> cbOnJobCompletedLua;
        private Action<Job> cbOnJobStopped;
        private Action<Job> cbOnJobWorked;
        private List<string> cbOnJobWorkedLua;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public Job()
        {
            this._inventoryRequirements = new Dictionary<string, Inventory>();
            this.AcceptsAnyItemType = false;
            this.CanTakeFromStockpile = true;
            this.cbOnJobCompletedLua = new List<string>();
            this.cbOnJobWorkedLua = new List<string>();
        }

        public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirements, bool jobRepeats = false) : this()
        {
            this.Tile = tile;
            this.JobObjectType = jobObjectType;
            this.cbOnJobCompleted += cbJobComplete;
            this._jobTime = jobTime;
            this.jobTimeRequired = jobTime;
            this.jobRepeats = jobRepeats;

            // Make sure the Inventories are COPIED, as we will be making changes to them.
            if (inventoryRequirements != null)
            {
                foreach (var inv in inventoryRequirements)
                {
                    this._inventoryRequirements[inv.ObjectType] = inv.Clone();
                }
            }
        }

        private Job(Job other) : this()
        {
            this.Name = other.Name + " (clone)";
            this.Tile = other.Tile;
            this.JobObjectType = other.JobObjectType;
            this.cbOnJobCompleted += other.cbOnJobCompleted;
            this.cbOnJobStopped += other.cbOnJobStopped;
            this._jobTime = other._jobTime;
            this.jobTimeRequired = other.jobTimeRequired;
            this.jobRepeats = other.jobRepeats;
            this.AcceptsAnyItemType = other.AcceptsAnyItemType;
            this.CanTakeFromStockpile = other.CanTakeFromStockpile;
            this.cbOnJobCompletedLua = new List<string>(other.cbOnJobCompletedLua);
            this.cbOnJobWorkedLua = new List<string>(other.cbOnJobWorkedLua);

            // Make sure the Inventories are COPIED, as we will be making changes to them.
            if (other._inventoryRequirements != null)
            {
                foreach (var inv in other._inventoryRequirements.Values)
                {
                    this._inventoryRequirements[inv.ObjectType] = inv.Clone();
                }
            }
        }

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        /// <summary>
        /// The piece of Furniture that owns this Job. Will often be NULL.
        /// </summary>
        public Furniture Furniture { get; set; }

        public Tile Tile { get; set; }

        public string JobObjectType { get; protected set; }

        public bool AcceptsAnyItemType { get; set; }
        public bool CanTakeFromStockpile { get; set; }


        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public static Job CreateNew(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirements, bool jobRepeats = false)
        {
            return new Job(tile, jobObjectType, cbJobComplete, jobTime, inventoryRequirements, jobRepeats);
        }

        public virtual Job Clone()
        {
            return new Job(this);
        }

        public void RegisterOnJobCompletedCallback(Action<Job> cb)
        {
            cbOnJobCompleted += cb;
        }

        public void UnregisterOnJobCompletedCallback(Action<Job> cb)
        {
            cbOnJobCompleted -= cb;
        }

        public void RegisterOnJobCompletedCallback(string cb)
        {
            cbOnJobCompletedLua.Add(cb);
        }

        public void UnregisterOnJobCompletedCallback(string cb)
        {
            cbOnJobCompletedLua.Remove(cb);
        }

        public void RegisterOnJobWorkedCallback(Action<Job> cb)
        {
            cbOnJobWorked += cb;
        }

        public void UnregisterOnJobWorkedCallback(Action<Job> cb)
        {
            cbOnJobWorked -= cb;
        }

        public void RegisterOnJobWorkedCallback(string cb)
        {
            cbOnJobWorkedLua.Add(cb);
        }

        public void UnregisterOnJobWorkedCallback(string cb)
        {
            cbOnJobWorkedLua.Remove(cb);
        }

        public void RegisterOnJobStoppedCallback(Action<Job> cb)
        {
            cbOnJobStopped += cb;
        }

        public void UnregisterOnJobStoppedCallback(Action<Job> cb)
        {
            cbOnJobStopped -= cb;
        }

        public void DoWork(float workTime)
        {
            // Check to make sure we have everything we need.
            // If not, don't register the work time.
            if (HasAllMaterial() == false)
            {
                // Still call the callbacks though, so animations etc can be updated
                if (cbOnJobWorked != null) cbOnJobWorked(this);

                foreach (var funcname in cbOnJobWorkedLua)
                {
                    FurnitureActions.CallFunction(funcname, this);
                }

                return;
            }

            if (cbOnJobWorked != null) cbOnJobWorked(this);
            foreach (var funcname in cbOnJobWorkedLua)
            {
                FurnitureActions.CallFunction(funcname, this);
            }

            _jobTime -= workTime;

            if (_jobTime <= 0)
            {
                // Do whatever is supposed to happen when this Job completes.
                if (cbOnJobCompleted != null) cbOnJobCompleted(this);
                foreach (var funcname in cbOnJobCompletedLua)
                {
                    FurnitureActions.CallFunction(funcname, this);
                }

                if (jobRepeats == false)
                {
                    // If the Job is completely done, notify everything.
                    if (cbOnJobStopped != null) cbOnJobStopped(this);
                }
                else
                {
                    // This is a repeating Job, and must be reset.
                    _jobTime += jobTimeRequired;
                }
            }
        }

        public void CancelJob()
        {
            if (cbOnJobStopped != null) cbOnJobStopped(this);

            World.Current.JobQueue.Remove(this);
        }

        public bool HasAllMaterial()
        {
            foreach (var inv in _inventoryRequirements.Values)
            {
                if (inv.MaxStackSize > inv.StackSize)
                {
                    return false;
                }
            }
            return true;
        }

        public int NeedsMaterial(Inventory inv)
        {
            if (AcceptsAnyItemType)
            {
                return inv.MaxStackSize;
            }

            if (_inventoryRequirements.ContainsKey(inv.ObjectType) == false)
            {
                return 0;
            }

            if (_inventoryRequirements[inv.ObjectType].StackSize >= _inventoryRequirements[inv.ObjectType].MaxStackSize)
            {
                // We already have all that we need.
                return 0;
            }

            return _inventoryRequirements[inv.ObjectType].MaxStackSize - _inventoryRequirements[inv.ObjectType].StackSize;
        }

        public Inventory GetFirstRequiredInventory()
        {
            foreach (var inv in _inventoryRequirements.Values)
            {
                if (inv.MaxStackSize > inv.StackSize)
                {
                    return inv;
                }
            }
            return null;
        }
    }
}
