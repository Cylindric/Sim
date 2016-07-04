using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class Job
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        public float _jobTime { get; private set; }
        private Action<Job> cbOnComplete;
        private Action<Job> cbOnCancel;
        private Action<Job> cbJobWorked;
        public Dictionary<string, Inventory> _inventoryRequirements;
        public Furniture FurniturePrototype;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public Job(Tile tile, string jobObjectType, Action<Job> cb, float jobTime, IEnumerable<Inventory> requirements)
        {
            this.Tile = tile;
            this.JobObjectType = jobObjectType;
            this.cbOnComplete += cb;
            this._jobTime = jobTime;
            this._inventoryRequirements = new Dictionary<string, Inventory>();
            this.AcceptsAnyItemType = false;
            this.CanTakeFromStockpile = true;

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
            this.Tile = other.Tile;
            this.JobObjectType = other.JobObjectType;
            this.cbOnComplete += other.cbOnComplete;
            this.cbOnCancel += other.cbOnCancel;
            this._jobTime = other._jobTime;
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

        public void RegisterOnCompleteCallback(Action<Job> cb)
        {
            cbOnComplete += cb;
        }

        public void UnregisterOnCompleteCallback(Action<Job> cb)
        {
            cbOnComplete -= cb;
        }

        public void RegisteJobWorkedCallback(Action<Job> cb)
        {
            cbJobWorked += cb;
        }

        public void UnregisterJobworkedCallback(Action<Job> cb)
        {
            cbJobWorked -= cb;
        }

        public void RegisterOnCancelCallback(Action<Job> cb)
        {
            cbOnCancel += cb;
        }

        public void UnregisterOnCancelCallback(Action<Job> cb)
        {
            cbOnCancel -= cb;
        }

        public void DoWork(float workTime)
        {
            // Check to make sure we have everything we need.
            // If not, don't register the work time.
            if (HasAllMaterial() == false)
            {
                // Still call the callbacks though, so animations etc can be updated
                if (cbJobWorked != null)
                {
                    cbJobWorked(this);
                }

                return;
            }

            if (cbJobWorked != null)
            {
                cbJobWorked(this);
            }

            _jobTime -= workTime;

            if (_jobTime <= 0)
            {
                if (cbOnComplete != null)
                {
                    cbOnComplete(this);
                }
            }
        }

        public void CancelJob()
        {
            if (cbOnCancel != null)
            {
                cbOnCancel(this);
            }

            Tile.World.JobQueue.Remove(this);
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
