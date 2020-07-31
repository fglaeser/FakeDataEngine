using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeDataEngine
{
  public static class Helper
  {
    public static string AppendStrings(this IEnumerable<string> list, string seperator = ", ")
    {
      return list.Aggregate(
          new StringBuilder(),
          (sb, s) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(s),
          sb => sb.ToString());
    }
  }
}
