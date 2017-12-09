using System;
using Debug = UnityEngine.Debug;
using System.Timers;
using System.Diagnostics;
using System.Collections.Generic;

namespace FluentBehaviourTree
{
    /// <summary>
    /// A behaviour tree leaf node for running an action.
    /// </summary>
    public class ActionNode : IBehaviourTreeNode
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
#pragma warning disable 0414
        private string name;
#pragma warning restore 0414

        private Stopwatch timer = new Stopwatch();

        /// <summary>
        /// Function to invoke for the action.
        /// </summary>
        private Func<TimeData, BehaviourTreeStatus> fn;
        

        public ActionNode(string name, Func<TimeData, BehaviourTreeStatus> fn)
        {
            this.name=name;
            this.fn=fn;
        }

        public void GetTimes(Dictionary<string, long> times)
        {
            times.Add(name, timer.ElapsedMilliseconds);
            //if (timer.ElapsedMilliseconds > 0)
            //{
            //    Debug.LogFormat("{0}: {1}ms", name, timer.ElapsedMilliseconds);
            //}
        }

        public BehaviourTreeStatus Tick(TimeData time)
        {
            timer.Start();
            var result = fn(time);
            timer.Stop();

            //Debug.LogFormat("{0} returning {1}", name, result);
            return result;
        }
    }
}
