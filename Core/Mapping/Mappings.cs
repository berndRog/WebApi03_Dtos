using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
namespace WebApi.Core.Mapping;

public static class Mappings {
   
   // Entity Person -> DTO PersonDto
   public static PersonDto ToPersonDto(this Person p) =>
      new PersonDto(p.Id, p.FirstName, p.LastName, p.Email, p.Phone);
   
   // DTO PersonDto -> Entity Person
   public static Person ToPerson(this PersonDto dto) =>
      new Person(dto.Id, dto.FirstName, dto.LastName, dto.Email, dto.Phone);
   
   // Entity Car -> DTO CarDto
   public static CarDto ToCarDto(this Car car) =>
      new CarDto(car.Id, car.Maker, car.Model, car.Year, car.Price, car.ImageUrl, car.PersonId);
   
   // DTO CarDto -> Entity Car
   public static Car ToCar(this CarDto carDto) =>
      new Car(carDto.Id, carDto.Maker, carDto.Model, carDto.Year, carDto.Price, carDto.ImageUrl, carDto.PersonId);
   
}