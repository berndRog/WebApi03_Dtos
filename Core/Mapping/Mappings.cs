using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
namespace WebApi.Core.Mapping;

public static class Mappings {
   
   // Entity Person -> DTO PersonDto
   public static PersonDto ToPersonDto(this Person person) =>
      new PersonDto(person.Id, person.FirstName, person.LastName, person.Email, person.Phone);
   
   // DTO PersonDto -> Entity Person
   public static Person ToPerson(this PersonDto personDto) =>
      new Person(personDto.Id, personDto.FirstName, personDto.LastName, personDto.Email, personDto.Phone);
   
   // Entity Car -> DTO CarDto
   public static CarDto ToCarDto(this Car car) =>
      new CarDto(car.Id, car.Maker, car.Model, car.Year, car.Price, car.ImageUrl, car.PersonId);
   
   // DTO CarDto -> Entity Car
   public static Car ToCar(this CarDto carDto) =>
      new Car(carDto.Id, carDto.Maker, carDto.Model, carDto.Year, carDto.Price, carDto.ImageUrl, carDto.PersonId);
   
}