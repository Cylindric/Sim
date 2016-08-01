using System.Collections.Generic;
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

        public List<Rect> Rects = new List<Rect>();

        public Pivot Pivot;
    }
}
