using System;

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

        public ActionNode(Func<TimeData, BehaviourTreeStatus> fn) : this("action", fn)
        {
        }

        public BehaviourTreeStatus Tick(TimeData time)
        {
            return fn(time);
        }
    }
}
