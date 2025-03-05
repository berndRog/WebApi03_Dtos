using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
namespace WebApi.Data;

public class DataContext: IDataContext {
   // fake storage with JSON file
   private readonly string _filePath = string.Empty;
   private readonly ILogger<DataContext> _logger;

   public ICollection<Person> People { get; } = [];
   public ICollection<Car> Cars { get; } = [];
   
   private class CombinedCollections {
      public ICollection<PersonDto> PersonDtos { get; init; } = [];
      public ICollection<CarDto> CarDtos { get; init; } = [];
   }

   public DataContext(
      ILoggerFactory logFactory
   ) {
      _logger = logFactory.CreateLogger<DataContext>();
      
      try {
         // Create the directory if it does not exist /Users/rogallab/Webtech/WebApps/WebApp02
         var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
         var directory = Path.Combine(homeDirectory, "Webtech/WebApis/WebApi03");
         if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
         
         // Create the file path
         _filePath = Path.Combine(directory, "people&cars.json");
         _logger.LogDebug("File path: {_filePath}", _filePath);
         
         // Create an Empty JSON file
         if (!File.Exists(_filePath)) {
            var emptyCollections = new {
               PersonDtos = new Collection<PersonDto>(),
               CarDtos = new Collection<CarDto>()
            };
            var emptyJson = JsonSerializer.Serialize(
               emptyCollections,
               GetJsonSerializerOptions()
            );
            File.WriteAllText(_filePath, emptyJson, Encoding.UTF8);
         }
         var json = File.ReadAllText(_filePath, Encoding.UTF8);
         
         // Prettify the JSON for logging
         var jsonObj = JsonSerializer.Deserialize<JsonDocument>(json);
         var prettyJson = JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { 
            WriteIndented = true 
         });
         _logger.LogDebug("Read JSON: {1}", prettyJson);
         
         var combinedCollections = JsonSerializer.Deserialize<CombinedCollections>(
            json,
            GetJsonSerializerOptions()
         ) ?? throw new ApplicationException("Deserialization failed");
         People = combinedCollections.PersonDtos.Select(dto => dto.ToPerson()).ToList();
         Cars = combinedCollections.CarDtos.Select(dto => dto.ToCar()).ToList();
      }
      catch (Exception e) {
         _logger.LogError("Error reading JSON file: {1}", e.Message);
      }
   }
   
   private JsonSerializerOptions GetJsonSerializerOptions() {
      return new JsonSerializerOptions {
         PropertyNameCaseInsensitive = true,
         //ReferenceHandler = ReferenceHandler.Preserve,
         ReferenceHandler = ReferenceHandler.IgnoreCycles,
         IncludeFields = true,
         WriteIndented = true
      };
   }

   public void SaveAllChanges() {
      try {
         var combinedCollections = new {
            PersonDtos = People.Select(person => person.ToPersonDto()).ToList(), 
            CarDtos = Cars.Select(car => car.ToCarDto()).ToList()
         };
         var json = JsonSerializer.Serialize(
            combinedCollections,
            GetJsonSerializerOptions()
         );
         // Prettify the JSON for logging
         var jsonObj = JsonSerializer.Deserialize<JsonDocument>(json);
         var prettyJson = JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { 
            WriteIndented = true 
         });
         _logger.LogDebug("Write JSON: {1}", prettyJson);
         
         File.WriteAllText(_filePath, json, Encoding.UTF8);
      }
      catch (Exception e) {
         Console.WriteLine(e.Message);
         throw; // Re-throw the exception
      }
   }
}