namespace Assets.Scripts.Pathfinding
{
    /// <summary>
    /// Represents a join between two Nodes.
    /// </summary>
    /// <typeparam name="T">The type of object represented by the nodes.</typeparam>
    public class PathEdge<T>
    {
        /// <summary>
        /// The cost to travel along this edge, to enter the Node.
        /// </summary>
        public float Cost;

        /// <summary>
        /// The Node that this Edge points to.
        /// </summary>
        public PathNode<T> Node;
    }
}
