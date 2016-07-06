using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;

namespace Assets.Scripts.Model
{
    [MoonSharpUserData]
    public class Inventory : IXmlSerializable
    {

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        public string objectType = "";

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

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this.objectType = reader.GetAttribute("objectType");

            int stackSize = int.Parse(reader.GetAttribute("stackSize"));
            int maxStackSize = int.Parse(reader.GetAttribute("maxStackSize"));

            this.stackSize = stackSize;
            this.maxStackSize = maxStackSize;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Inventory");
            writer.WriteAttributeString("objectType", this.objectType);
            writer.WriteAttributeString("stackSize", this.stackSize.ToString());
            writer.WriteAttributeString("maxStackSize", this.maxStackSize.ToString());
            writer.WriteEndElement();
        }
    }
}
