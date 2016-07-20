using System;
using System.Xml;
using System.Xml.Schema;
using MoonSharp.Interpreter;

namespace Assets.Scripts.Model
{
    [MoonSharpUserData]
    public class Inventory
    {
        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        private int _stackSize;

        public void UnRegisterInventoryChangedCallback(Action<Inventory> callback)
        {
            _cbInventoryChanged -= callback;
        }

        public void RegisterInventoryChangedCallback(Action<Inventory> callback)
        {
            _cbInventoryChanged += callback;
        }

        private Action<Inventory> _cbInventoryChanged;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        public Inventory()
        {
            this.StackSize = 1;
            this.MaxStackSize = 50;
        }

        public Inventory(string objectType, int maxStackSize, int stackSize) : this()
        {
            this.ObjectType = objectType;
            this.MaxStackSize = maxStackSize;
            this.StackSize = stackSize;
        }

        private Inventory(Inventory other) : this()
        {
            this.ObjectType = other.ObjectType;
            this.MaxStackSize = other.MaxStackSize;
            this.StackSize = other.StackSize;
        }

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        public string ObjectType { get; set; }

        public int StackSize
        {
            get { return _stackSize; }
            set
            {
                if (_stackSize != value)
                {
                    _stackSize = value;
                    if (_cbInventoryChanged != null) _cbInventoryChanged(this);
                }
            }
        }

        public int Space
        {
            get
            {
                return this.MaxStackSize - this.StackSize;
            }
        }

        public int MaxStackSize { get; set; }

        public Tile Tile { get; set; }

        public Character Character { get; set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        public virtual Inventory Clone()
        {
            return new Inventory(this);
        }

        public void ReadXml(XmlNode xml)
        {
            this.ObjectType = xml.Attributes["objectType"].Value;
            this.StackSize = int.Parse(xml.Attributes["stackSize"].Value);
            this.MaxStackSize = int.Parse(xml.Attributes["maxStackSize"].Value);
        }

        public XmlElement WriteXml(XmlDocument xml)
        {
            var element = xml.CreateElement("Inventory");
            element.SetAttribute("objectType", this.ObjectType);
            element.SetAttribute("stackSize", this.StackSize.ToString());
            element.SetAttribute("maxStackSize", this.MaxStackSize.ToString());
            return element;
        }
    }
}
