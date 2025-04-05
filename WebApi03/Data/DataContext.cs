using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data; 

public class DataContext(
   DbContextOptions<DataContext> options
) : DbContext(options), IDataContext {
   
   private ILogger<DataContext>? _logger;
   
   public DbSet<Person> People => Set<Person>(); // call to a method, not a field 
   public DbSet<Car> Cars => Set<Car>(); 
   
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      // https://learn.microsoft.com/de-de/ef/core/logging-events-diagnostics/simple-logging
      var loggerFactory = LoggerFactory.Create(builder => {
         builder
            .SetMinimumLevel(LogLevel.Information)
            .AddDebug()
            .AddConsole();
      });
      _logger = loggerFactory.CreateLogger<DataContext>();
      
      // Configure logging
      optionsBuilder
         .UseLoggerFactory(loggerFactory)
         .LogTo(Console.WriteLine, LogLevel.Information)
         .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
         .EnableSensitiveDataLogging(true)
         .EnableDetailedErrors();
   }
   
   public bool SaveAllChanges(string? text = null) {
      
      // log repositories before transfer to the database
      var view = ChangeTracker.DebugView.LongView;
      Console.WriteLine($"{text}\n{view}");
      Debug.WriteLine($"{text}\n{view}");
      _logger?.LogInformation("\n{view}",view);
      
      // save all changes to the database, returns the number of rows affected
      var result = SaveChanges();
      
      // log repositories after transfer to the database
      _logger?.LogInformation("SaveChanges {result}",result);

      _logger?.LogInformation("\n{view}",ChangeTracker.DebugView.LongView);
      return result>0;
   }
   
   public void ClearChangeTracker() =>
      ChangeTracker.Clear();

   public void LogChangeTracker(string text) =>
      _logger?.LogInformation("{text}\n{change}", text, ChangeTracker.DebugView.LongView);

   
   public static (string useDatabase, string dataSource) EvalDatabaseConfiguration(
      IConfiguration configuration
   ) {
      // appsettings      
      //"LocalFolder": "Webtech/Orms",
      //"UseDatabase": "Sqlite",
      // "ConnectionStrings": {
      //   "Sqlite": "Orm01"
      //},
      var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      
      var localFolder = configuration.GetSection("LocalFolder").Value ??
         throw new Exception("LocalFolder is not available");
      localFolder.Split('/').ToList().ForEach(folder => {
         path = Path.Combine(path, folder);
      });
      if (!Directory.Exists(path)) Directory.CreateDirectory(path);

      // read active database configuration from appsettings.json
      var useDatabase = configuration.GetSection("UseDatabase").Value ??
         throw new Exception("UseDatabase is not available");

      // read connection string from appsettings.json
      var connectionString = configuration.GetSection("ConnectionStrings")[useDatabase]
         ?? throw new Exception("ConnectionStrings is not available"); 
      
      
      switch (useDatabase) {
         case "Sqlite":
            var dataSourceSqlite =
               "Data Source=" + Path.Combine(path, connectionString) + ".db";
            Console.WriteLine($"....: EvalDatabaseConfiguration: Sqlite {dataSourceSqlite}");
            return (useDatabase, dataSourceSqlite);
         case "SqliteInMemory":
            var dataSourceSqliteInMemory = "Data Source=:memory:";
            Console.WriteLine($"....: EvalDatabaseConfiguration: SqliteInMemory {dataSourceSqliteInMemory}");
            return (useDatabase, dataSourceSqliteInMemory);
         default:
            throw new Exception("appsettings.json Problems with database configuration");
      }   
   }
}