using Bogus;
using Dapper;
using FakeDataEngine.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace FakeDataEngine
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private Config global = new Config();

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

      LoadConfiguration();
      var faker = new Faker();

      await FailFastDbConnection(stoppingToken);

      while (!stoppingToken.IsCancellationRequested)
      {
        await Task.Delay(global.ThrottleMs, stoppingToken);

        using (SqlConnection connection = new SqlConnection(global.ConnectionString))
        {
          
          await connection.OpenAsync(stoppingToken);

          foreach(var table in global.Tables)
          {
            var columnNames = table.Columns.Select(f => f.Name);
            var paramsNames = table.Columns.Select(f => "@" + f.Name);
            // Generate values
            var sql = GenerateInsertStatement(table.Schema, table.Name, columnNames, paramsNames);
            
            _logger.LogInformation(sql);

            var dictionary = new Dictionary<string, object>();
            foreach(var field in table.Columns)
            {
              object val = null;
              if (field.Format == Format.raw)
              {
                val = ParseValue(field.Value, faker);
              }
              else if (field.Format == Format.json)
              {
                val = GenerateJson(field.Object, faker);
              }
              else 
              {
                _logger.LogWarning($"The format {field.Format} is not supported.");
              }
              dictionary.Add("@" + field.Name, val);
              _logger.LogInformation($"@{field.Name}, {val}");
            }

            var parameters = new DynamicParameters(dictionary);
            await connection.ExecuteAsync(sql, parameters);
          }
        }
      }
    }

    protected void LoadConfiguration()
    {
      var pathToConfig = _configuration.GetSection("FAKER_CONFIG_PATH").Value;

      if(string.IsNullOrEmpty(pathToConfig))
      {
        throw new ArgumentException("Missing FAKER_CONFIG_PATH value.");
      }
      if (File.Exists(pathToConfig) == false)
      {
        throw new FileNotFoundException("Config file  not found.", pathToConfig);
      }

      var serializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

      global = serializer.Deserialize<Config>(File.ReadAllText(pathToConfig));
      
    }

    protected async Task FailFastDbConnection(CancellationToken stoppingToken)
    {
      using (SqlConnection connection = new SqlConnection(global.ConnectionString))
      {
        _logger.LogInformation("Testing db connection....");
        await connection.OpenAsync(stoppingToken);
        await connection.CloseAsync();
        _logger.LogInformation("Testing db connection....[OK]");
      }
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
