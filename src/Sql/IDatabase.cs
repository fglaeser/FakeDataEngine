using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace FakeDataEngine.Sql
{
  public interface IDatabase : IDisposable
  {
    DbConnection CreateConnection();

    /// <summary>
    /// Tests that this db instance can connect to the database
    /// </summary>
    Task TestConnectionAsync();

    char ParameterPrefix { get; }
  }
}
