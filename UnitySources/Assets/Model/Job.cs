using System;

namespace Assets.Model
{
    public class Job
    {
        public Tile Tile { get; private set; }
        public string JobObjectType { get; protected set; }

        private float jobTime;
        private Action<Job> cbJobComplete;
        private Action<Job> cbJobCancelled;

        public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime = 1f)
        {
            this.Tile = tile;
            this.JobObjectType = jobObjectType;
            this.cbJobComplete += cbJobComplete;
        }

        public void RegisterJobCompleteCallback(Action<Job> cb)
        {
            cbJobComplete += cb;
        }

        public void UnRegisterJobCompleteCallback(Action<Job> cb)
        {
            cbJobComplete -= cb;
        }

        public void RegisterJobCancelledCallback(Action<Job> cb)
        {
            cbJobCancelled += cb;
        }

        public void UnRegisterJobCancelledCallback(Action<Job> cb)
        {
            cbJobCancelled -= cb;
        }

        public void DoWork(float workTime)
        {
            jobTime -= workTime;

            if (jobTime <= 0)
            {
                cbJobComplete(this);
            }
        }

        public void CancelJob()
        {
            cbJobCancelled(this);
        }
    }
}
