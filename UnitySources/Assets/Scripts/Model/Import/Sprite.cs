using System.Xml.Schema;
using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    public class Sprite
    {
        [XmlAttribute]
        public string name;

        [XmlAttribute]
        public int pixelsPerUnit;

        public Rect Rect;
        public Pivot Pivot;
    }
}
