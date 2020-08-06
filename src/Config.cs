using FakeDataEngine.Model;
using FakeDataEngine.Sql;
using YamlDotNet.Serialization;

namespace FakeDataEngine
{
  public class Config
  {
    [YamlMember(Alias = "connection.string", ApplyNamingConventions = false)]
    public string ConnectionString { get; set; }

    [YamlMember(Alias = "database.provider", ApplyNamingConventions = false)]
    public DatabaseProvider Provider { get; set; }

    [YamlMember(Alias = "throttle.ms", ApplyNamingConventions = false)]
    public int ThrottleMs { get; set; } = 1000;

    [YamlMember(Alias = "tables", ApplyNamingConventions = false)]
    public Table[] Tables { get; set; }
  }
}
