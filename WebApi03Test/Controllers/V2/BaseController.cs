using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Controllers.V2;
using WebApi.Core;
using WebApi.Data;
using WebApiTest.Di;
using WebApiTest.Data.Repositories;
using Xunit;

namespace WebApiTest.Controllers;
[Collection(nameof(SystemTestCollectionDefinition))]
public abstract class BaseController {
   
   protected readonly PeopleController _peopleController;
   protected readonly CarsController _carsController;
   
   protected readonly IPeopleRepository _peopleRepository;
   protected readonly ICarsRepository _carsRepository;
   protected readonly IDataContext _dataContext;
   protected readonly Seed _seed;

   protected BaseController() {
      
      // Test DI-Container
      IServiceCollection services = new ServiceCollection();
      // Add Repositories, Test Databases, ...
      var (useDatabase, dataSource) = services.AddDataTest();
      // Build ServiceProvider,
      // and use Dependency Injection or Service Locator Pattern
      var serviceProvider = services.BuildServiceProvider()
         ?? throw new Exception("Failed to create an instance of ServiceProvider");

      //-- Service Locator 
      var dbContext = serviceProvider.GetRequiredService<DataContext>()
         ?? throw new Exception("Failed to create DbContext");

      // In-Memory
      if (useDatabase == "SqliteInMemory") {
         dbContext.Database.OpenConnection();
      } else if (useDatabase == "Sqlite") {
         dbContext.Database.EnsureDeleted();
         // Workaround  SQLite I/O Errors
         SqliteConnection connection = new SqliteConnection(dataSource);
         connection.Open();
         using var command = connection.CreateCommand();
         // Switch the journal mode from WAL to DELETE.
         command.CommandText = "PRAGMA journal_mode = DELETE;";
         var result = command.ExecuteScalar();
         Console.WriteLine("Current journal mode: " + result);
         connection.Close();
      } else {
         dbContext.Database.EnsureDeleted();
      }
      dbContext.Database.EnsureCreated();
      
      _dataContext = serviceProvider.GetRequiredService<IDataContext>()
         ?? throw new Exception("Failed to create an instance of IDataContext");

      _peopleRepository = serviceProvider.GetRequiredService<IPeopleRepository>()
         ?? throw new Exception("Failed create an instance of IPeopleRepository");
      _carsRepository = serviceProvider.GetRequiredService<ICarsRepository>()
         ?? throw new Exception("Failed create an instance of ICarsRepository");
      
      _peopleController = new PeopleController(_peopleRepository, _dataContext);
      _carsController = new CarsController(_peopleRepository,_carsRepository,_dataContext);
      
      _seed = new Seed();
   }
   
   public static string ToPrettyJson(
      string text, 
      object obj
   ) {
      return 
         text + "\n" +
         JsonSerializer.Serialize(obj, new JsonSerializerOptions {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
            //  MaxDepth = 2
         });
   }
}