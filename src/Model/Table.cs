using System.Collections.Generic;

namespace FakeDataEngine.Model
{
  public class Table
  {
    public string Name { get; set; }
    public string Scheme { get; set; }
    public IList<Field> Fields { get; set; } = new List<Field>();
  }
}
