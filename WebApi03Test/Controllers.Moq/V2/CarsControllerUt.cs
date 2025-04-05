using System;
using System.Linq;
using System.Threading;
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
      THelper.IsNotFound(actionResult);
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
   public void Create_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      
      // mock the repository's FindById method to return an existing owner
      _mockCarsRepository.Setup(r => r.FindById(car.Id))
         .Returns(car);
      
      // Act
      var carDto = car.ToCarDto();
      var actionResult =  _carsController.Create(person.Id, carDto);

      // Assert
      THelper.IsBadRequest(actionResult);
      // Verify that the repository's Add method was not called
      _mockCarsRepository.Verify(r => r.Add(It.IsAny<Car>()), Times.Never);
      // Verify that the data context's SaveAllChanges method was not called
      _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);
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
   public void Update_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      var updCar = new Car(car.Id, car.Maker, car.Model, car.Year, 9999m, null, car.PersonId);
      var routeId = Guid.NewGuid();

      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = _carsController.Update(person.Id, routeId, updCarDto);

      // Assert
      THelper.IsBadRequest(actionResult);
      // Verify that repository update is not called due to id mismatch
      _mockCarsRepository.Verify(r => r.Update(It.IsAny<Car>()), Times.Never);
      // Verify that SaveAllChanges is not called
      _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);
   }
  
   [Fact]
   public void Update_NotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      var updCar = new Car(car.Id, car.Maker, car.Model, car.Year, 9999m, null, car.PersonId);
      var routeId = Guid.NewGuid();

      // Setup the repository to return null for the specified id
      _mockCarsRepository.Setup(r => r.FindById(car.Id))
         .Returns((Car)null);

      // Act
      var updCarDto = updCar.ToCarDto();
      var actionResult = _carsController.Update(person.Id, car.Id, updCarDto);

      // Assert
      THelper.IsNotFound(actionResult);
      // Verify that repository update is not called due to id mismatch
      _mockCarsRepository.Verify(r => r.Update(It.IsAny<Car>()), Times.Never);
      // Verify that SaveAllChanges is not called
      _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);

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
      THelper.IsNoContent(actionResult);
      _mockCarsRepository.Verify(r => r.Remove(car), Times.Once);
      _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Exactly(1));
   }
   
   [Fact]
   public void Delete_PersonNotFound() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car);
      var nonExistentPersonId = Guid.NewGuid();
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns((Person) null);
      _mockCarsRepository.Setup(r => r.FindById(car.Id))
         .Returns(car);
      _mockCarsRepository.Setup(r => r.Remove(car))
         .Verifiable();
      _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
         .Returns(true);
      
      // Act
      var actionResult = _carsController.Delete(nonExistentPersonId, car.Id);

      // Assert
      THelper.IsNotFound(actionResult);
      // Verify that Remove is never called
      _mockCarsRepository.Verify(r => r.Remove(It.IsAny<Car>()), Times.Never);
      // Verify that SaveAllChanges is never called
      _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);
   }
}