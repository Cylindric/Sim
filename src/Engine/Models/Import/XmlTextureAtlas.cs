using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    [XmlRoot("TextureAtlas")]
    public class XmlTextureAtlas
    {
        [XmlAttribute] public string imagePath;
        [XmlAttribute] public int width;
        [XmlAttribute] public int height;

        [XmlElement("sprite")]
        public List<XmlSprite> Sprites = new List<XmlSprite>();
    }
}
