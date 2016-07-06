using System;
using System.Collections.Generic;
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
        private Action<Job> cbOnJobStopped;
        private Action<Job> cbOnJobWorked;

        /// <summary>
        /// The piece of Furniture that owns this Job. Will often be NULL.
        /// </summary>
        public Furniture Furniture { get; set; }

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public Job(string name, Tile tile, string jobObjectType, Action<Job> cb, float jobTime, IEnumerable<Inventory> requirements, bool repeats = false)
        {
            this.Name = name;
            this.Tile = tile;
            this.JobObjectType = jobObjectType;
            this.cbOnJobCompleted += cb;
            this._jobTime = jobTime;
            this.jobTimeRequired = jobTime;
            this._inventoryRequirements = new Dictionary<string, Inventory>();
            this.AcceptsAnyItemType = false;
            this.CanTakeFromStockpile = true;
            this.jobRepeats = repeats;

            // Make sure the Inventories are COPIED, as we will be making changes to them.
            if (requirements != null)
            {
                foreach (var inv in requirements)
                {
                    this._inventoryRequirements[inv.objectType] = inv.Clone();
                }
            }
        }

        private Job(Job other)
        {
            this.Name = other.Name + " (clone)";
            this.Tile = other.Tile;
            this.JobObjectType = other.JobObjectType;
            this.cbOnJobCompleted += other.cbOnJobCompleted;
            this.cbOnJobStopped += other.cbOnJobStopped;
            this._jobTime = other._jobTime;
            this.jobTimeRequired = other.jobTimeRequired;
            this.jobRepeats = other.jobRepeats;

            this._inventoryRequirements = new Dictionary<string, Inventory>();
            this.AcceptsAnyItemType = other.AcceptsAnyItemType;
            this.CanTakeFromStockpile = other.CanTakeFromStockpile;

            // Make sure the Inventories are COPIED, as we will be making changes to them.
            if (other._inventoryRequirements != null)
            {
                foreach (var inv in other._inventoryRequirements.Values)
                {
                    this._inventoryRequirements[inv.objectType] = inv.Clone();
                }
            }
        }

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        public Tile Tile { get; set; }

        public string JobObjectType { get; protected set; }

        public bool AcceptsAnyItemType { get; set; }
        public bool CanTakeFromStockpile { get; set; }


        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

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

        public void RegisterOnJobWorkedCallback(Action<Job> cb)
        {
            cbOnJobWorked += cb;
        }

        public void UnregisterOnJobWorkedCallback(Action<Job> cb)
        {
            cbOnJobWorked -= cb;
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

                return;
            }

            if (cbOnJobWorked != null) cbOnJobWorked(this);

            _jobTime -= workTime;

            if (_jobTime <= 0)
            {
                // Do whatever is supposed to happen when this Job completes.
                if (cbOnJobCompleted != null) cbOnJobCompleted(this);

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
                if (inv.maxStackSize > inv.stackSize)
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
                return inv.maxStackSize;
            }

            if (_inventoryRequirements.ContainsKey(inv.objectType) == false)
            {
                return 0;
            }

            if (_inventoryRequirements[inv.objectType].stackSize >= _inventoryRequirements[inv.objectType].maxStackSize)
            {
                // We already have all that we need.
                return 0;
            }

            return _inventoryRequirements[inv.objectType].maxStackSize - _inventoryRequirements[inv.objectType].stackSize;
        }

        public Inventory GetFirstRequiredInventory()
        {
            foreach (var inv in _inventoryRequirements.Values)
            {
                if (inv.maxStackSize > inv.stackSize)
                {
                    return inv;
                }
            }
            return null;
        }
    }
}
