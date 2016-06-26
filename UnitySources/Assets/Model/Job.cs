using System;
using UnityEngine;

namespace Assets.Model
{
    public class Job
    {
        public Tile Tile { get; private set; }
        public string JobObjectType { get; protected set; }

        private float _jobTime;
        private Action<Job> cbOnComplete;
        private Action<Job> cbOnCancel;

        public Job(Tile tile, string jobObjectType, Action<Job> cb, float jobTime = 1f)
        {
            this.Tile = tile;
            this.JobObjectType = jobObjectType;
            this.cbOnComplete += cb;
            this._jobTime = jobTime;
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
            Debug.Log("Work done.");
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
