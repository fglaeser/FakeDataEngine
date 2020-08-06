using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FakeDataEngine.Model
{
  public enum Format
  {
    raw,
    json,
    array
  }
  public class Column
  {
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string Name { get; set; }

    [YamlMember(Alias = "value", ApplyNamingConventions = false)]
    public string Value { get; set; }

    [YamlMember(Alias = "format", ApplyNamingConventions = false)]
    public Format Format { get; set; }

    [YamlMember(Alias = "object", ApplyNamingConventions = false)]
    public IDictionary<string, string> Object { get; set; } = new Dictionary<string, string>();

    [YamlMember(Alias = "items", ApplyNamingConventions = false)]
    public IList<string> Items { get; set; } = new List<string>();
  }
}
