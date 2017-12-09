using System.Xml.Serialization;

namespace Engine.Model.Import
{
    [XmlRoot("sprite")]
    public class XmlSprite
    {
        [XmlAttribute("n")] public string name;
        [XmlAttribute("x")] public int x;
        [XmlAttribute("y")] public int y;
        [XmlAttribute("w")] public int width;
        [XmlAttribute("h")] public int height;
        [XmlAttribute("pX")] public float pivotX;
        [XmlAttribute("pY")] public float pivotY;
    }
}
