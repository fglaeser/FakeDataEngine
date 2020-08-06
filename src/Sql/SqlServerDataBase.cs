namespace FakeDataEngine.Sql
{
  public class SqlServerDatabase : AbstractDatabase
  {
    public SqlServerDatabase(string providerName, string connectionString) : base(providerName, connectionString)
    {
    }
  }
}
