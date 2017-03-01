namespace Assets.Scripts.Pathfinding
{
    public class Path_Edge<T> {

        public float cost;	// Cost to traverse this edge (i.e. cost to ENTER the Tile)

        public Path_Node<T> node;
    }
}
