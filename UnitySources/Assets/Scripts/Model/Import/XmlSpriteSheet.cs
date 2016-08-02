using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    [XmlRoot("SpriteSheet")]
    public class XmlSpriteSheet
    {
        [XmlAttribute] public string name;
        [XmlAttribute] public int pixelsPerUnit;
        [XmlAttribute] public int width;
        [XmlAttribute] public int height;

        [XmlElement("Pivot")]
        public XmlPivot Pivot;

        [XmlElement("SpriteSet")]
        public List<XmlSpriteSet> SpriteSets = new List<XmlSpriteSet>();
    }
}
