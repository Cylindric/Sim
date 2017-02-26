using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

        /// <summary>
        /// Function to invoke for the action.
        /// </summary>
        private Func<TimeData, BehaviourTreeStatus> fn;
        

        public ActionNode(string name, Func<TimeData, BehaviourTreeStatus> fn)
        {
            this.name=name;
            this.fn=fn;
        }

        public BehaviourTreeStatus Tick(TimeData time)
        {
            var result = fn(time);
            //Debug.LogFormat("{0} returning {1}", name, result);
            return result;
        }
    }
}
