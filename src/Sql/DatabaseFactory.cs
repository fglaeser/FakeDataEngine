using System.Data.Common;
using System.Data.SqlClient;

namespace FakeDataEngine.Sql
{
  public enum DatabaseProvider
  {
    sqlserver,
    oracle
    //mysql,
    //postgre
  }
  public static class DatabaseFactory
  {
    private const string MsSqlProviderName = "System.Data.SqlClient";
    private const string OracleProviderName = "Oracle.ManagedDataAccess.Client";
    private const string MySqlProviderName = "MySql.Data.MySqlClient";
    private const string PostgreSqlProviderName = "Npgsql";
    public static IDatabase CreateDatabase(DatabaseProvider provider, string connectionString)
    {
      IDatabase db = null;
      // create database
      switch (provider)
      {
        case DatabaseProvider.sqlserver:
            DbProviderFactories.RegisterFactory(MsSqlProviderName, SqlClientFactory.Instance);
            db = new SqlServerDatabase(MsSqlProviderName, connectionString);
            break;
        case DatabaseProvider.oracle:
            DbProviderFactories.RegisterFactory(OracleProviderName, "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess");
            db = new OracleDatabase(OracleProviderName, connectionString);
            break;
        //case DatabaseProvider.mysql:
        //    DbProviderFactories.RegisterFactory(MySqlProviderName, "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data");
        //    db = new MySqlDatabase(MySqlProviderName, connectionString);
        //    break;
      }
      return db;
    }
  }
}
