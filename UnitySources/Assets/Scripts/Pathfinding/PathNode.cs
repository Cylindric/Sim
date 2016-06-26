namespace Assets.Scripts.Pathfinding
{
    /// <summary>
    /// Represents a single Node in the map graph.
    /// </summary>
    /// <typeparam name="T">The type of object represented by this node.</typeparam>
    public class PathNode<T>
    {
        /// <summary>
        /// Keep a copy of the object this Node represents.
        /// </summary>
        public T Data;

        /// <summary>
        /// Nodes leading out from this node.
        /// </summary>
        public PathEdge<T>[] Edges;
    }
}
