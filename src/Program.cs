using Figgle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace FakeDataEngine
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var v = Assembly.GetExecutingAssembly().GetName().Version;
      Console.WriteLine(FiggleFonts.Slant.Render(string.Format("FakeDataEngine {0}.{1}.{2}",v.Major, v.Minor, v.Revision)));
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
              services.AddHostedService<Worker>();
            });
  }
}
