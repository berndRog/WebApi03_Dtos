using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DeepEqual;
using DeepEqual.Syntax;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
using WebApiTest.Controllers.Moq;
using WebApiTest.Data.Repositories;
using Xunit;
namespace WebApiTest.Controllers.Moq.V2;

[Collection(nameof(SystemTestCollectionDefinition))]
public class CarsControllerUt : BaseControllerUt {
   
   [Fact]
   public void GetCars_Ok() {
      // Arrange People with Cars
      var (_, expectedCars) = Seed.InitPeopleWithCars(_seed.People, _seed.Cars);
      
      _mockCarsRepository.Setup(r => r.SelectAll())
         .Returns(expectedCars);

      // Act
      var actionResult = _carsController.GetAll();

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedCars.Select(c => c.ToCarDto()));
   }
   
   [Fact]
   public void GetCarsByPersonId_Ok() {
      // Arrange People with Cars
      var (_, updCars) = Seed.InitPeopleWithCars(_seed.People, _seed.Cars);
      var expectedCars = updCars.Where(c => c.PersonId == _seed.Person1.Id);
      
      _mockCarsRepository.Setup(r => r.SelectCarsByPersonId(_seed.Person1.Id))
         .Returns(expectedCars);

      // Act
      var actionResult = _carsController.GetCarsByPersonId(_seed.Person1.Id);

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedCars.Select(c => c.ToCarDto()));
   }
   
   [Fact]
   public void GetById_Ok() {
      // Arrange
      var id = _seed.Car1.Id;
      var expected = _seed.Car1;
      // mock the result of the repository
      _mockCarsRepository.Setup(r => r.FindById(id))
         .Returns(expected);
      
      // Act
      var actionResult = _carsController.GetById(id);

      // Assert
      THelper.IsOk(actionResult, expected.ToCarDto());
   }

   [Fact]
   public void GetById_NotFound() {
      // Arrange
      var id = Guid.NewGuid();
      _mockCarsRepository.Setup(r => r.FindById(id))
         .Returns(null as Car);

      // Act
      var actionResult =  _carsController.GetById(id);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public void Create_Created() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      // mock the repository's FindById method to return null
      _mockCarsRepository.Setup(r => r.FindById(car.Id))
         .Returns((Car?)null);
      // mock the peopleRepository's FindById method 
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(person);
      // mock the repository's Add method
      _mockCarsRepository.Setup(r => r.Add(It.IsAny<Car>()))
         .Verifiable();
      // mock the data context's SaveAllChanges method
      _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
         .Returns(true);

      // Act
      var carDto = car.ToCarDto();
      var actionResult = _carsController.Create(_seed.Person1.Id, carDto);

      // Assert
      THelper.IsCreated(actionResult, carDto);
      // Verify that the repository's Add method was called once
      _mockCarsRepository.Verify(r => r.Add(It.IsAny<Car>()), Times.Once);
      // Verify that the data context's SaveAllChanges method was called once
      _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Once);
   }

   [Fact]
   public void Create_NotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      // mock the peopleRepository's FindById method 
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(null as Person);
      
      // Act
      var carDto = car.ToCarDto();
      var actionResult =  _carsController.Create(person.Id, carDto);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }

   [Fact]
   public void Create_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      // mock the peopleRepository's FindById method 
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(person);
      // mock the repository's FindById method to return null
      _mockCarsRepository.Setup(r => r.FindById(car.Id))
         .Returns(car);
      
      // Act
      var carDto = car.ToCarDto();
      var actionResult =  _carsController.Create(person.Id, carDto);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }

   [Fact]
   public void Update_Ok() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      var updCar = new Car(car.Id, car.Maker, car.Model, car.Year, 9999m, null, car.PersonId);
      
      // mock the peopleRepository's FindById method to return an existing person
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(person);
      // mock the carsRepository's FindById method to return an existing car
      _mockCarsRepository.Setup(r => r.FindById(car.Id))
         .Returns(car);
      // mock the repository's Update method
      _mockCarsRepository.Setup(r => r.Update(updCar))
         .Verifiable();
      // mock the data context's SaveAllChangesAsync method
      _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
         .Returns(true);

      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = _carsController.Update(person.Id, car.Id, updCarDto);

      // Assert
      THelper.IsOk(actionResult!, updCarDto);
      // Verify that the repository's Update method was called once
      _mockCarsRepository.Verify(r => r.Update(It.IsAny<Car>()), Times.Once);
      // Verify that the data context's SaveAllChangesAsync method was called once
      _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Once);
   }

   [Fact]
   public void Update_PersonIdNotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      var updCar = car;
      updCar.Update("XYZ", "abc" , 1999, 999m);
      var routeId = Guid.NewGuid();

      // mock the peopleRepository's FindById method to return an existing person
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(null as Person);
      
      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = _carsController.Update(person.Id, car.Id, updCarDto);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }

   [Fact]
   public void Update_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      var updCar = car;
      updCar.Update("XYZ", "abc" , 1999, 999m);
      
      var badId = Guid.NewGuid();

      // mock the peopleRepository's FindById method to return an existing person
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(person);
      
      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = _carsController.Update(person.Id, badId, updCarDto);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }
  
   [Fact]
   public void Update_CarNotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      var updCar = car;
      updCar.Update("XYZ", "abc" , 1999, 999m);
      
      // mock the peopleRepository's FindById method to return an existing person
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(person);
      // Setup the repository to return null for the specified id
      _mockCarsRepository.Setup(r => r.FindById(car.Id))
         .Returns(null as Car);

      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = _carsController.Update(person.Id, car.Id, updCarDto);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }

   [Fact]
   public void Delete_NoContent() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
 
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(person);
      _mockCarsRepository.Setup(r => r.FindById(car.Id))
         .Returns(car);
      _mockCarsRepository.Setup(r => r.Remove(car))
         .Verifiable();
      _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
         .Returns(true);

      // Act
      var actionResult = _carsController.Delete(person.Id, car.Id);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NoContentResult>(actionResult);
   }
   
   [Fact]
   public void Delete_PersonNotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      var nonExistentPersonId = Guid.NewGuid();
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns(null as Person);
      
      // Act
      var actionResult = _carsController.Delete(nonExistentPersonId, car.Id);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult);
   }
   
   [Fact]
   public void GetCarsByAttributes_Ok() {
      // Arrange People with Cars
      var (_, updCars) = Seed.InitPeopleWithCars(_seed.People, _seed.Cars);

      var yearNow = DateOnly.FromDateTime(DateTime.Now).Year;
      var expectedCars = updCars.Where(car => 
         car.Maker == "BMW" && 
         car.Model == "X5" &&
         car.Year >= 2020 && car.Year <= yearNow && 
         car.Price >= 45_000 && car.Price <= 50_000
      ).ToList();
      
      _mockCarsRepository.Setup(r => r.SelectByAttributes("BMW", "X5", 2020, yearNow, 45_000, 50_000))
         .Returns(expectedCars);

      // Act
      var actionResult = _carsController.GetCarsByAttributes(
         maker:"BMW",
         model:"X5",
         yearMin: 2020,
         yearMax: yearNow,
         priceMin: 45_000,
         priceMax: 50_000
      );
   

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedCars.Select(c => c.ToCarDto()));
       
   }
}
