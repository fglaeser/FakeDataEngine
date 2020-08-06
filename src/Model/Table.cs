using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FakeDataEngine.Model
{
  public class Table
  {
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string Name { get; set; }

    [YamlMember(Alias = "schema", ApplyNamingConventions = false)]
    public string Schema { get; set; }

    [YamlMember(Alias = "columns", ApplyNamingConventions = false)]
    public IList<Column> Columns { get; set; } = new List<Column>();
  }
}
