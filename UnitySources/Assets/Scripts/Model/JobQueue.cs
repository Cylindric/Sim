using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Model
{
    public class JobQueue
    {
        private Queue<Job> _jobQueue;
        private Action<Job> cbJobCreated;

        public JobQueue()
        {
            _jobQueue = new Queue<Job>();
        }

        public void Enqueue(Job j)
        {
            if (j._jobTime < 0)
            {
                // Jobs with a build-time less than zero get insta-built.
                j.DoWork(0);
                return;
            }

            _jobQueue.Enqueue(j);

            if (cbJobCreated != null)
            {
                cbJobCreated(j);
            }
        }

        public Job Dequeue()
        {
            if (_jobQueue.Count == 0)
            {
                return null;
            }

            return _jobQueue.Dequeue();
        }

        public void RegisterJobCreationCallback(Action<Job> cb)
        {
            cbJobCreated += cb;
        }

        public void UnRegisterJobCreationCallback(Action<Job> cb)
        {
            cbJobCreated -= cb;
        }

        public void Remove(Job job)
        {
            if (_jobQueue.Contains(job) == false)
            {
                // Debug.LogError("Trying to remove a job that isn't in the queue!");
                // A Character was probably working this job, so it's not on the queue.
                return;
            }

            var jobs = new List<Job>(_jobQueue);
            jobs.Remove(job);
            _jobQueue = new Queue<Job>(jobs);
        }
    }
}
