using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Core.DomainModel.Entities;
using WebApiTest.Data.Repositories;
using Xunit;
namespace WebApiTest.Controllers.Moq.V1;

[Collection(nameof(SystemTestCollectionDefinition))]
public class PeopleControllerUt : BaseControllerUt {
   
   
   [Fact]
   public void GetAll_Ok() {
      // Arrange
      var expectedPeople = _seed.People;
      _mockPeopleRepository.Setup(r => r.SelectAll())
         .Returns(expectedPeople);

      // Act
      var actionResult = _peopleController.GetAll();

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsEnumerableOk(actionResult, expectedPeople);
   }
   
   [Fact]
   public void GetById_Ok() {
      // Arrange
      var id = _seed.Person1.Id;
      var expected = _seed.Person1;
      // mock the result of the repository
      _mockPeopleRepository.Setup(r => r.FindById(id))
         .Returns(expected);
      
      // Act
      var actionResult = _peopleController.GetById(id);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsOk<Person>(actionResult, expected);
   }

   [Fact]
   public void GetById_NotFound() {
      // Arrange
      var id = Guid.NewGuid();
      _mockPeopleRepository.Setup(r => r.FindById(id))
         .Returns(null as Person);

      // Act
      var actionResult =  _peopleController.GetById(id);

      // Assert

   }
   
   [Fact]
   public void Create_Created() {
      // Arrange
      var person = _seed.Person1;
      Person? addedPerson = null;
      
      // mock the repository's FindById method to return null
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns((Person?)null);
      // mock the repository's Add method
      _mockPeopleRepository.Setup(r => r.Add(It.IsAny<Person>()))
         .Callback<Person>(person => addedPerson = person);
      // mock the data context's SaveAllChanges method
      _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
         .Returns(true);

      // Act
      var actionResult = _peopleController.Create(person);

      // Assert
      THelper.IsCreated(actionResult, person);
      // Verify that the repository's Add method was called once
      _mockPeopleRepository.Verify(r => r.Add(It.IsAny<Person>()), Times.Once);
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
      Assert.NotNull(actionResult);
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
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
      Assert.NotNull(actionResult);
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public void Update_NotFound() {
      // Arrange
      var person = _seed.Person1;
      // Setup the repository to return null for the specified id
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns((Person?)null);

      // Act
      var actionResult = _peopleController.Update(person.Id, person);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);

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
      var actionResult = _peopleController.Delete(person.Id);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NoContentResult>(actionResult);
   }
   
   [Fact]
   public void Delete_NotFound() {
      // Arrange
      var nonExistentPersonId = Guid.NewGuid();
      _mockPeopleRepository.Setup(r => r.FindById(nonExistentPersonId))
         .Returns((Person?)null);

      // Act
      var actionResult = _peopleController.Delete(nonExistentPersonId);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundResult>(actionResult);
   }

   [Fact]
   public void GetByName_Ok() {
      // Arrange
      var name = "ValidName";
      var expectedPeople = new List<Person> { _seed.Person1 };
      _mockPeopleRepository.Setup(r => r.SelectByName(name))
         .Returns(expectedPeople);

      // Act
      var actionResult = _peopleController.GetByName(name);

      // Assert
      THelper.IsEnumerableOk(actionResult, expectedPeople);
   }
   
   [Fact]
   public void GetByName_Ok_EmptyList() {
      // Arrange
      var expectedPeople = new List<Person>();
      var nonExistentName = "NonExistentName";
      _mockPeopleRepository.Setup(r => r.SelectByName(nonExistentName))
         .Returns(new Collection<Person>());

      // Act
      var actionResult = _peopleController.GetByName(nonExistentName);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<OkObjectResult>(actionResult.Result);
      Assert.Equal(expectedPeople.Count, ((actionResult.Result as OkObjectResult)?.Value as Collection<Person>)?.Count);
   }

}