using System;

namespace Assets.Scripts.Model
{
    public class Inventory {

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        public string objectType = "Steel Plate";

        private int _stackSize = 1;

        public int stackSize
        {
            get { return _stackSize; }
            set
            {
                if (_stackSize != value)
                {
                    _stackSize = value;
                    if (cbInventoryChanged != null)
                    {
                        cbInventoryChanged(this);
                    }
                }
            }
        }

        public void UnRegisterInventoryChangedCallback(Action<Inventory> callback)
        {
            cbInventoryChanged -= callback;
        }

        public void RegisterInventoryChangedCallback(Action<Inventory> callback)
        {
            cbInventoryChanged += callback;
        }


        public int maxStackSize = 50;

        public Tile tile;

        public Character character;

        private Action<Inventory> cbInventoryChanged;

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

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        public int Space
        {
            get
            {
                return this.maxStackSize - this.stackSize;
            }
        }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public virtual Inventory Clone()
        {
            return new Inventory(this);
        }
    }
}
