using System.Xml.Serialization;

namespace Assets.Scripts.Model.Import
{
    [XmlRoot("Pivot")]
    public class XmlPivot
    {
        [XmlAttribute]
        public float x;

        [XmlAttribute]
        public float y;
    }
}
