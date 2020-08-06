using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace FakeDataEngine.Sql
{
  public class AbstractDatabase : IDatabase
  {
    private readonly string _connectionString = string.Empty;
    private DbProviderFactory _provider;

    protected AbstractDatabase(string providerName, string connectionString)
    {
      // check if the arguments are not null
      if (providerName == null) throw new ArgumentNullException("providerName");
      _connectionString = connectionString ?? throw new ArgumentNullException("connectionString");

      if(!DbProviderFactories.TryGetFactory(providerName, out _provider))
      {
        string errorMessage = "There is no DbProviderFactory for " + providerName + ".";
        throw new ArgumentException(errorMessage);
      }
    }
    public DbConnection CreateConnection()
    {
      var result = _provider.CreateConnection();
      result.ConnectionString = _connectionString;
      return result;
    }

    /// <summary>
    /// Dispose method of AbstractDatabase which closes the databse connection.
    /// </summary>
    /// <param name="disposing">
    /// If disposing equals true, the method has been called directly or indirectly by a user's code 
    /// and managed and unmanaged resources can be disposed. 
    /// If disposing equals false, the method has been called by the runtime from inside the finalizer and 
    /// only unmanaged resources can be disposed.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        _provider = null;
      }
    }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="AbstractDatabase"/> is reclaimed by garbage collection.
    /// </summary>
    ~AbstractDatabase()
    {
      Dispose(false);
    }

    /// <summary>
    /// Tests that this db instance can connect to the database
    /// </summary>
    public async Task TestConnectionAsync()
    {
      using DbConnection conn = CreateConnection();
      await conn.OpenAsync();
      conn.Close();
    }

    public virtual char ParameterPrefix
    {
      get
      {
        return '@';
      }
    }
  }
}
