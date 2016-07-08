using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    public class Pivot
    {
        [XmlAttribute]
        public float x;

        [XmlAttribute]
        public float y;


        public UnityEngine.Vector2 ToVector2()
        {
            return new UnityEngine.Vector2(x, y);
        }
    }
}
