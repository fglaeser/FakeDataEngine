using Bogus;
using Dapper;
using FakeDataEngine.Model;
using FakeDataEngine.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FakeDataEngine
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private IDatabase _database;
    private Config global = new Config();

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
      _logger = logger;
      LoadConfiguration(configuration);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var faker = new Faker();
      _database = DatabaseFactory.CreateDatabase(global.Provider, global.ConnectionString);

      try
      {
        _logger.LogInformation("Testing db connection....");
        await _database.TestConnectionAsync();
        _logger.LogInformation("Testing db connection....[OK]");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Testing db connection....[NO_OK]");
        throw;
      }


      BuildInsertStatements(global.Tables);

      while (!stoppingToken.IsCancellationRequested)
      {
        await Task.Delay(global.ThrottleMs, stoppingToken);

        using(var connection = _database.CreateConnection())
        {
          await connection.OpenAsync(stoppingToken);

          foreach(var table in global.Tables)
          {
            _logger.LogInformation(table.InsertSqlStatement);

            var dictionary = new Dictionary<string, object>();
            foreach(var column in table.Columns)
            {
              object val = null;
              switch(column.Format)
              {
                case Format.raw:
                  val = ParseValue(column.Value, faker);
                  break;
                case Format.json:
                  val = GenerateJson(column.Object, faker);
                  break;
                case Format.array:
                  val = faker.PickRandom(column.Items);
                  break;
              }

              dictionary.Add(_database.ParameterPrefix + column.Name, val);
              _logger.LogInformation($"{_database.ParameterPrefix}{column.Name}, {val}");
            }
            try
            {
              var parameters = new DynamicParameters(dictionary);
              await connection.ExecuteAsync(table.InsertSqlStatement, parameters);
            }
            catch (Exception ex)
            {
              _logger.LogError(ex, "Exception trying to insert fake data.");
            }
          }
        }
      }
    }

    private void BuildInsertStatements(Table[] tables)
    {
      foreach (var table in tables)
      {
        var columnNames = table.Columns.Select(f => f.Name);
        var paramsNames = table.Columns.Select(f => _database.ParameterPrefix + f.Name);
        // Generate once
        table.SetInsertSqlStatement(GenerateInsertStatement(table.Schema, table.Name, columnNames, paramsNames));
      }
    }

    protected void LoadConfiguration(IConfiguration configuration)
    {
      var pathToConfig = configuration.GetSection("FAKER_CONFIG_PATH").Value ?? "/opt/config.yml";

      if (File.Exists(pathToConfig) == false)
      {
        throw new FileNotFoundException("Config file not found.", pathToConfig);
      }

      var serializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

      global = serializer.Deserialize<Config>(File.ReadAllText(pathToConfig));
      
    }

    protected string GenerateInsertStatement(string scheme, 
      string tableName, 
      IEnumerable<string> columnNames,
      IEnumerable<string> parameters)
    {
      string sql = string.Format("INSERT INTO {0}.{1} ({2}) VALUES ({3})",
                           scheme, tableName,
                           columnNames.AppendStrings(),
                           parameters.AppendStrings());
      return sql;

    }

    protected object ParseValue(string value, Faker faker)
    {
      if (value.StartsWith("{{"))
        return faker.Parse(value);

      return value;
    }
    protected string GenerateJson(IDictionary<string, string> members, Faker faker)
    {
      var obj = new Dictionary<string, object>();
      foreach(var member in members)
      {
        obj.Add(member.Key, ParseValue(member.Value, faker));
      }
      return JsonSerializer.Serialize(obj);
    }
  }
}
