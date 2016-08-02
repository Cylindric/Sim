using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    [XmlRoot("XmlSprite")]
    public class XmlSprite
    {
        [XmlAttribute] public int x;
        [XmlAttribute] public int y;
    }
}
