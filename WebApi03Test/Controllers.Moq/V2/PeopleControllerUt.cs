using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
using WebApiTest.Data.Repositories;
using Xunit;
namespace WebApiTest.Controllers.Moq.V2;

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
      THelper.IsEnumerableOk(actionResult!, expectedPeople.Select(p => p.ToPersonDto()));
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
      THelper.IsOk<PersonDto>(actionResult, expected.ToPersonDto());
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
      Assert.NotNull(actionResult);
      THelper.IsNotFound(actionResult);
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
      Assert.NotNull(actionResult);
      THelper.IsEnumerableOk(actionResult, expectedPeople.Select(p => p.ToPersonDto()));
   }
   
   [Fact]
   public void GetByName_NotFound() {
      // Arrange
      var nonExistentName = "NonExistentName";
      _mockPeopleRepository.Setup(r => r.SelectByName(nonExistentName))
         .Returns((IEnumerable<Person>?)null);

      // Act
      var actionResult = _peopleController.GetByName(nonExistentName);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsNotFound(actionResult);
   }

   [Fact]
   public void Create_Created() {
      // Arrange
      var person = _seed.Person1;
      
      // mock the repository's FindById method to return null
      _mockPeopleRepository.Setup(r => r.FindById(person.Id))
         .Returns((Person?)null);
      // mock the repository's Add method
      _mockPeopleRepository.Setup(r => r.Add(It.IsAny<Person>()))
         .Verifiable();
      // mock the data context's SaveAllChanges method
      _mockDataContext.Setup(c => c.SaveAllChanges(It.IsAny<string>()))
         .Returns(true);

      // Act
      var personDto = person.ToPersonDto();
      var actionResult = _peopleController.Create(personDto);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsCreated(actionResult, personDto);
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
      var personDto = person.ToPersonDto();
      var actionResult =  _peopleController.Create(personDto);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsBadRequest<PersonDto>(actionResult);
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
      var updPersonDto = updPerson.ToPersonDto();
      var actionResult = _peopleController.Update(person.Id, updPersonDto);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsOk(actionResult!, updPersonDto);
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
      var updPersonDto = updPerson.ToPersonDto();
      var actionResult = _peopleController.Update(routeId, updPersonDto);

      // Assert
      Assert.NotNull(actionResult);
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
         .Returns((Person?)null);
      
      // Act
      var updPersonDto = person.ToPersonDto();
      var actionResult = _peopleController.Update(person.Id, updPersonDto);

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
         .Returns((Person?)null);

      // Act
      var actionResult = _peopleController.Delete(nonExistentPersonId);

      // Assert
      THelper.IsNotFound(actionResult);
      // Verify that Remove is never called
      _mockPeopleRepository.Verify(r => r.Remove(It.IsAny<Person>()), Times.Never);
      // Verify that SaveAllChanges is never called
      _mockDataContext.Verify(c => c.SaveAllChanges(It.IsAny<string>()), Times.Never);
   }

}