using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    public class Sprites
    {
        [XmlElement(ElementName = "Sprite")]
        public List<Sprite> SpriteList = new List<Sprite>();
    }
}
