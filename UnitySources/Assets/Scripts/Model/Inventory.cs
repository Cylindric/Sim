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

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public Inventory()
        {
            
        }

        public Inventory(string objectType, int maxStackSize, int stackSize)
        {
            this.objectType = objectType;
            this.maxStackSize = maxStackSize;
            this.stackSize = stackSize;
        }

        private Inventory(Inventory other)
        {
            this.objectType = other.objectType;
            this.maxStackSize = other.maxStackSize;
            this.stackSize = other.stackSize;
        }

        public virtual Inventory Clone()
        {
            return new Inventory(this);
        }
    }
}
