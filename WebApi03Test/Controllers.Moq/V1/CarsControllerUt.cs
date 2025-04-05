using System;
using System.Linq;
using WebApi.Core.DomainModel.Entities;
using WebApiTest.Data.Repositories;
using Xunit;
namespace WebApiTest.Controllers.Moq.V1;

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
      Assert.NotNull(actionResult);
      THelper.IsEnumerableOk(actionResult, expectedCars);
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
      Assert.NotNull(actionResult);
      THelper.IsEnumerableOk(actionResult, expectedCars);
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
      Assert.NotNull(actionResult);
      THelper.IsOk(actionResult, expected);
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
      THelper.IsNotFound(actionResult);
   }
   
   
   /*
      [Fact]
      public void Create_Created() {
         // Arrange
         var car = _seed.Car1;
         Car? addedCar = null;

         // mock the repository's FindById method to return null
         _mockCarsRepository.Setup(r => r.FindById(car.Id))
            .Returns((Car?)null);
         // mock the repository's Add method
         _mockCarsRepository.Setup(r => r.Add(It.IsAny<Car>()))
            .Callback<Person>(person => addedCar = car);
         // mock the data context's SaveAllChanges method
         _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
            .Returns(true);

         // Act
         var actionResult = _carsController.Create(_seed.Person1.Id, car);

         // Assert
         THelper.IsCreated(actionResult, car);
         // Verify that the repository's Add method was called once
         _mockCarsRepository.Verify(r => r.Add(It.IsAny<Car>()), Times.Once);
         // Verify that the data context's SaveAllChanges method was called once
         _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Once);
      }

      [Fact]
      public void Create_BadRequest() {
         // Arrange
         var person = _seed.Person1;
         // mock the repository's FindById method to return an existing owner
         _mockPeopleRepository.Setup(r => r.FindById(person.Id))
            .Returns(person);

         // Act
         var actionResult =  _peopleController.Create(person);

         // Assert
         THelper.IsBadRequest<Person>(actionResult);
         // Verify that the repository's Add method was not called
         _mockPeopleRepository.Verify(r => r.Add(It.IsAny<Person>()), Times.Never);
         // Verify that the data context's SaveAllChanges method was not called
         _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);
      }


      [Fact]
      public void Update_Ok() {
         // Arrange
         var person = _seed.Person1;
         var updPerson = new Person(person.Id, "Erna","meier","0511/6543-2109","e.meier@icloud.com");

         // mock the repository's FindByIdAsync method to return an existing owner
         _mockPeopleRepository.Setup(r => r.FindById(person.Id))
            .Returns(person);
         // mock the repository's Update method
         _mockPeopleRepository.Setup(r => r.Update(updPerson))
            .Verifiable();
         // mock the data context's SaveAllChangesAsync method
         _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
            .Returns(true);

         // Act
         var actionResult = _peopleController.Update(person.Id, updPerson);

         // Assert
         THelper.IsOk(actionResult!, updPerson);
         // Verify that the repository's Update method was called once
         _mockPeopleRepository.Verify(r => r.Update(It.IsAny<Person>()), Times.Once);
         // Verify that the data context's SaveAllChangesAsync method was called once
         _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Once);
      }

      [Fact]
      public void Update_BadRequest() {
         // Arrange
         var routeId = Guid.NewGuid();
         // updPerson has an id different from routeId
         var updPerson = new Person(_seed.Person1.Id, "Erna", "Meier", "0511/6543-2109", "e.meier@icloud.com");

         // Act
         var actionResult = _peopleController.Update(routeId, updPerson);

         // Assert
         THelper.IsBadRequest(actionResult);
         // Verify that repository update is not called due to id mismatch
         _mockPeopleRepository.Verify(r => r.Update(It.IsAny<Person>()), Times.Never);
         // Verify that SaveAllChanges is not called
         _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);
      }

      [Fact]
      public void Update_NotFound() {
         // Arrange
         var person = _seed.Person1;
         // Setup the repository to return null for the specified id
         _mockPeopleRepository.Setup(r => r.FindById(person.Id))
            .Returns((Person)null);

         // Act
         var actionResult = _peopleController.Update(person.Id, person);

         // Assert
         THelper.IsNotFound(actionResult);
         // Verify that repository update is not called due to id mismatch
         _mockPeopleRepository.Verify(r => r.Update(It.IsAny<Person>()), Times.Never);
         // Verify that SaveAllChanges is not called
         _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);

      }

      [Fact]
      public void Delete_NoContent() {
         // Arrange
         var person = _seed.Person1;
         // Setup the repository to return null for the specified id
         _mockPeopleRepository.Setup(r => r.FindById(person.Id))
            .Returns(person);
         _mockPeopleRepository.Setup(r => r.Remove(person))
            .Verifiable();
         _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
            .Returns(true);

         // Act
         var result = _peopleController.Delete(person.Id);

         // Assert
         THelper.IsNoContent(result);
         _mockPeopleRepository.Verify(r => r.Remove(person), Times.Once);
         _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Exactly(1));
      }

      [Fact]
      public void Delete_NotFound() {
         // Arrange
         var nonExistentPersonId = Guid.NewGuid();
         _mockPeopleRepository.Setup(r => r.FindById(nonExistentPersonId))
            .Returns((Person)null);

         // Act
         var actionResult = _peopleController.Delete(nonExistentPersonId);

         // Assert
         THelper.IsNotFound(actionResult);
         // Verify that Remove is never called
         _mockPeopleRepository.Verify(r => r.Remove(It.IsAny<Person>()), Times.Never);
         // Verify that SaveAllChanges is never called
         _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);
      }
   */
}