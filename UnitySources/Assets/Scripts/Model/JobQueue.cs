﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Model
{
    public class JobQueue
    {
        private readonly Queue<Job> _jobQueue;
        private Action<Job> cbJobCreated;

        public JobQueue()
        {
            _jobQueue = new Queue<Job>();
        }

        public void Enqueue(Job j)
        {
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

    }
}
