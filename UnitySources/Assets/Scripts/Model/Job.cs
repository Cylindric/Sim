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

        private float _jobTime;
        private Action<Job> cbOnComplete;
        private Action<Job> cbOnCancel;
        private Dictionary<string, Inventory> _inventoryRequirements;

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
            _jobTime -= workTime;

            if (_jobTime <= 0)
            {
                cbOnComplete(this);
            }
        }

        public void CancelJob()
        {
            cbOnCancel(this);
        }
    }
}
