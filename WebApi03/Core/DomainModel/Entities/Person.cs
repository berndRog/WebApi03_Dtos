using System.Text.Json.Serialization;
namespace WebApi.Core.DomainModel.Entities; 

public class Person: AEntity {
   
   public override Guid Id { get; init; } = Guid.NewGuid();
   public string FirstName { get; private set; } = string.Empty;
   public string LastName { get; private set; } = string.Empty;
   public string? Email { get; private set; }
   public string? Phone { get; private set; } 
   // 1:n navigation collection Person <-> Cars (0,1):(0,n)
   public ICollection<Car> Cars { get; private set; } = [];
   
   // ctor EF Core.
   // EF Coreuses this ctor and reflexion to construct new Person object,
   // while ignoring private set in the properties
   public Person() { } 
   
   // ctor Domain Model
   [JsonConstructor]
   public Person(Guid id, string firstName, string lastName, string? email = null,
      string? phone = null) {
      Id = id;
      FirstName= firstName;
      LastName = lastName;
      Email = email;
      Phone = phone;
   }   
   
   // methods
   public void Set(string? email = null, string? phone = null) {
      if(email != null) Email = email;
      if(phone != null) Phone = phone;
   } 
   
   public void Update(string firstName, string lastName, string? email = null, string? phone = null) {
      FirstName = firstName;
      LastName = lastName;
      if(email != null) Email = email;
      if(phone != null) Phone = phone;
   }
   
   // 1:n Person <-> Cars
   public void AddCar(Car car) {
      car.Set(this);
      Cars.Add(car);
   }
   public void RemoveCar(Car car) {
      car.Set(null);
      Cars.Remove(car);
   }
}