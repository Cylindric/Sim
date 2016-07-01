namespace Assets.Scripts.Model
{
    public class Inventory {

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        public string objectType = "Steel Plate";

        public int stackSize = 1;

        public int maxStackSize = 50;

        public Tile tile;

        public Character character;

        public Inventory()
        {
            
        }

        private Inventory(Inventory other)
        {
            objectType = other.objectType;
            maxStackSize = other.maxStackSize;
            stackSize = other.stackSize;
        }

        public virtual Inventory Clone()
        {
            return new Inventory(this);
        }
    }
}
