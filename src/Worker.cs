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

namespace FakeDataEngine
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private Config global = new Config();
    private IList<Table> _tables = new List<Table>();

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

          foreach(var table in _tables)
          {
            var columnNames = table.Fields.Select(f => f.Name);
            var paramsNames = table.Fields.Select(f => "@" + f.Name);
            // Generate values
            var sql = GenerateInsertStatement(table.Scheme, table.Name, columnNames, paramsNames);
            
            _logger.LogInformation(sql);

            var dictionary = new Dictionary<string, object>();
            foreach(var field in table.Fields)
            {
              object val = null;
              if (field.Format.Equals("raw"))
              {
                val = ParseValue(field.Value, faker);
              }
              else if (field.Format.Equals("json"))
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

      var yaml = new YamlStream();
      yaml.Load(System.IO.File.OpenText(pathToConfig));
      
      var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
      YamlNode node;
      if(mapping.Children.TryGetValue("connection.string", out node))
      {
        global.ConnectionString = ((YamlScalarNode)node).Value;
      }

      if (mapping.Children.TryGetValue("throttle.ms", out node))
      {
        global.ThrottleMs = int.Parse(((YamlScalarNode)node).Value);
      }

      YamlNode tables;      
      if (mapping.Children.TryGetValue("tables", out tables))
      {
        foreach(YamlMappingNode table in ((YamlSequenceNode)tables).Children)
        {
          var t = new Table {
            Name = table.Children["name"].ToString(),
            Scheme = table.Children["schema"].ToString(),
          };

          var fields = (YamlSequenceNode)table.Children["columns"];
          foreach(YamlMappingNode f in fields.Children)
          {
            var field = new Field
            {
              Name = f["name"].ToString()
            };
            YamlNode format;
            if(f.Children.TryGetValue("format", out format))
            {
              field.Format = ((YamlScalarNode)format).Value;
            }
            YamlNode value;
            if (f.Children.TryGetValue("value", out value))
            {
              field.Value = ((YamlScalarNode)value).Value;
            }
            YamlNode obj;
            if (field.Format.Equals("json") && f.Children.TryGetValue("object", out obj))
            {
              foreach(var p in ((YamlMappingNode)obj).Children)
              {
                field.Object.Add(p.Key.ToString(), p.Value.ToString());
              }
            }
            t.Fields.Add(field);
          }

          _tables.Add(t);
        }
      }
    }

    protected async Task FailFastDbConnection(CancellationToken stoppingToken)
    {
      using (SqlConnection connection = new SqlConnection(global.ConnectionString))
      {
        _logger.LogInformation("Testing db connection....");
        await connection.OpenAsync(stoppingToken);
        await connection.CloseAsync();  
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
