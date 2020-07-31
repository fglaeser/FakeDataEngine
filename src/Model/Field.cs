using System.Collections.Generic;

namespace FakeDataEngine.Model
{
  public class Field
  {
    public string Name { get; set; }
    public string Value { get; set; }
    public string Format { get; set; } = "raw";
    public IDictionary<string, string> Object { get; set; } = new Dictionary<string, string>();
  }
}
