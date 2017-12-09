using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Pathfinding;

namespace Engine.Model
{
    public class JobQueue
    {
        private readonly List<Job> _jobQueue;
        private Action<Job> cbJobCreated;

        public JobQueue()
        {
            _jobQueue = new List<Job>();
        }

        public int Count()
        {
            return _jobQueue.Count;
        }

        public void Enqueue(Job j)
        {
            if (j.JobTime < 0)
            {
                // Jobs with a build-time less than zero get insta-built.
                j.DoWork(0);
                return;
            }

            _jobQueue.Add(j);

            if (cbJobCreated != null) cbJobCreated(j);
        }

        public Job TakeFirstJobFromQueue()
        {
            if (_jobQueue.Count == 0)
            {
                return null;
            }

            var job = _jobQueue.First();
            _jobQueue.Remove(job);
            return job;
        }

        public Job TakeClosestJobToTile(Tile t)
        {
            Job found = null;
            int distance = int.MaxValue;

            foreach (var job in _jobQueue)
            {
                var route = new Path_AStar()
                {
                    World = World.Instance,
                    Start = t,
                    End = job.Tile
                };
                route.Calculate();

                if (found == null || route.Length() < distance)
                {
                    found = job;
                    distance = route.Length();
                }
            }

            if (found != null)
            {
                _jobQueue.Remove(found);
            }

            return found;
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

            _jobQueue.Remove(job);
        }
    }
}
