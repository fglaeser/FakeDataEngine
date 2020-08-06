namespace FakeDataEngine.Sql
{
  public class OracleDatabase : AbstractDatabase
  {
    public OracleDatabase(string providerName, string connectionString) : base(providerName, connectionString)
    {
    }
    public override char ParameterPrefix
    {
      get { return ':'; }
    }
  }
}
