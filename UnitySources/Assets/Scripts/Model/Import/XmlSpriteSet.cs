using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    [XmlRoot("SpriteSet")]
    public class XmlSpriteSet
    {
        [XmlAttribute]
        public string name;

        [XmlElement("Sprite")]
        public List<XmlSprite> Sprites = new List<XmlSprite>();
    }
}
