using Figgle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace FakeDataEngine
{
  public class Program
  {
    public static void Main(string[] args)
    {
      Console.WriteLine(FiggleFonts.Slant.Render("FakeDataEngine"));
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
