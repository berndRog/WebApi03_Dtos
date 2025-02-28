using WebApi.Core.DomainModel.NullEntities;
using WebApi.Core.Dtos;
namespace WebApi.Core.DomainModel.Entities;

public class Car: AEntity {
   
   // properties with getter only
   public override Guid Id { get; init; } = Guid.NewGuid();
   public string Maker {get; private set;} = string.Empty;
   public string Model {get; private set;} = string.Empty;
   public int Year {get; private set;} = 1900;
   public double Price {get; private set;} = 0.0;
   public string? ImageUrl { get; private set; } = null;
   // navigation property
   public Guid PersonId { get; private set; } = NullPerson.Instance.Id;
   public Person Person { get; private set; } = NullPerson.Instance;

   // EF Core requires a constructor
   internal Car() { }  // for subclasses only
   public Car(Guid id, string maker, string model, int year, double price, 
      string? imageUrl = null, Guid? personId = null) {
      Id = id;
      Maker = maker;
      Model = model;
      Year = year;
      Price = price;
      ImageUrl = imageUrl;
      if(personId != null) PersonId = personId.Value;
   }
   
   public void SetImageUrl(string imageUrl) =>
      ImageUrl = imageUrl;

   public void Set(Person? person) {
      if (person != null) {
         PersonId = person.Id;
         Person = person;
      }
      else {
         PersonId = NullPerson.Instance.Id;
         Person = NullPerson.Instance;
      }
   }
   
   public void Update(Car updCar) {
      Maker = updCar.Maker;  // can the car maker be updated?
      Model = updCar.Model;  // can the car model be updated?
      Year = updCar.Year;    // can the car year be updated?
      Price = updCar.Price;
   }
   
   public CarDto ToCarDto() =>
      new CarDto(Id, Maker, Model, Year, Price, ImageUrl, PersonId);

}