using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    public class Rect
    {
        [XmlAttribute]
        public int x;

        [XmlAttribute]
        public int y;

        [XmlAttribute]
        public int width;

        [XmlAttribute]
        public int height;

        public UnityEngine.Rect ToRect()
        {
            return new UnityEngine.Rect(x, y, width, height);
        }
    }
}
