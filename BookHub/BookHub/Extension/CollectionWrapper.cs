using System.Xml.Serialization;

namespace BookHub.Extension;

[XmlRoot("Items")]
public class CollectionWrapper<T>
{
    [XmlElement("Item")]
    public List<T> Items { get; set; } = new();
}