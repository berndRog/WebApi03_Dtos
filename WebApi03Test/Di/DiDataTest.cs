using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApi;
using WebApi.Data;
namespace WebApiTest.Di;

public static class DiTestData {

   public static (string, string) AddDataTest(
      this IServiceCollection services
   ) {

      // Configuration
      // Nuget:  Microsoft.Extensions.Configuration
      //       + Microsoft.Extensions.Configuration.Json
      var configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettingsTest.json", false)
         .Build();
      services.AddSingleton<IConfiguration>(configuration);
      
      // Logging
      // Nuget:  Microsoft.Extensions.Logging
      //       + Microsoft.Extensions.Logging.Configuration
      //       + Microsoft.Extensions.Logging.Debug
      var logging = configuration.GetSection("Logging");
      services.AddLogging(builder => {
         builder.ClearProviders();
         builder.AddConfiguration(logging);
         builder.AddDebug();
      });
      
      // Repository, Database ...
      services.AddData(configuration);
      
      return DataContext.EvalDatabaseConfiguration(configuration);

   }
}