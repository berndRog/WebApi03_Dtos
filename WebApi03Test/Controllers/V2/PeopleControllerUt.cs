using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
using WebApiTest.Controllers.Moq;
using WebApiTest.Data.Repositories;
using Xunit;
namespace WebApiTest.Controllers;

[Collection(nameof(SystemTestCollectionDefinition))]
public class PeopleControllerUt : BaseController {
   
   [Fact]
   public void GetAll_Ok() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      _dataContext.SaveAllChanges("Add people");
      _dataContext.ClearChangeTracker();
      var expectedDtos = _seed.People.Select(p => p.ToPersonDto()); 
      
      // Act
      var actionResult = _peopleController.GetAll();

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsEnumerableOk(actionResult!, expectedDtos);
   }
   
   [Fact]
   public void GetById_Ok() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      _dataContext.SaveAllChanges("Add people");
      _dataContext.ClearChangeTracker();
      var person = _seed.Person1;
      var expectedDto = person.ToPersonDto(); 

      // Act
      var actionResult = _peopleController.GetById(person.Id);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsOk<PersonDto>(actionResult!, expectedDto);
   }
   
   [Fact]
   public void GetById_NotFound() {
      // Arrange
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      _dataContext.SaveAllChanges("Add people");
      _dataContext.ClearChangeTracker();
      
      // Act
      var actionResult =  _peopleController.GetById(Guid.NewGuid());

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);
   }
   
   [Fact]
   public void GetByName_Ok() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      _dataContext.SaveAllChanges("Add people");
      _dataContext.ClearChangeTracker();
      var name = "Muster";
      var expectedPeople = _seed.People
         .Where(p => p.LastName.Contains(name))
         .Select(p => p.ToPersonDto())
         .ToList();

      // Act
      var actionResult = _peopleController.GetByName(name);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsEnumerableOk(actionResult, expectedPeople);
   }

   [Fact]
   public void GetByName_NotFound() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      _dataContext.SaveAllChanges("Add people");
      _dataContext.ClearChangeTracker();
      var name = "NonExistentName";
      
      // Act
      var actionResult = _peopleController.GetByName(name);

      // Assert emptyList as result
      THelper.IsEnumerableOk(actionResult, new List<PersonDto>());

   }

   [Fact]
   public void Create_Created() {
      // Arrange
      var person = _seed.Person1;
      
      // Act
      var personDto = person.ToPersonDto();
      var actionResult = _peopleController.Create(personDto);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsCreated(actionResult, personDto);
   }

   [Fact]
   public void Create_BadRequest() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      _dataContext.SaveAllChanges("Add person");
      _dataContext.ClearChangeTracker();
      
      // Act
      var personDto = person.ToPersonDto();
      var actionResult =  _peopleController.Create(personDto);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);

   }
   
   [Fact]
   public void Update_Ok() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      _dataContext.SaveAllChanges("Add person");
      _dataContext.ClearChangeTracker();
      var updPerson = new Person(person.Id, "Erna","meier","0511/6543-2109","e.meier@icloud.com");
      
      // Act
      var updPersonDto = updPerson.ToPersonDto();
      var actionResult = _peopleController.Update(person.Id, updPersonDto);

      // Assert
      Assert.NotNull(actionResult);
      THelper.IsOk(actionResult!, updPersonDto);
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
      Assert.IsType<BadRequestObjectResult>(actionResult.Result);

   }

   [Fact]
   public void Update_NotFound() {
      // Arrange
      var person = _seed.Person2;
      _peopleRepository.Add(person);
      _dataContext.SaveAllChanges("Add person");
      _dataContext.ClearChangeTracker();
      var updPerson = new Person(_seed.Person1.Id, "Erna","meier","0511/6543-2109","e.meier@icloud.com");
      
      // Act
      var updPersonDto = updPerson.ToPersonDto();
      var actionResult = _peopleController.Update(updPerson.Id, updPersonDto);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundObjectResult>(actionResult.Result);

   }
   
   [Fact]
   public void Delete_NoContent() {
      // Arrange
      var person = _seed.Person1;
      _peopleRepository.Add(person);
      _dataContext.SaveAllChanges("Add person");
      _dataContext.ClearChangeTracker();

      // Act
      var actionResult = _peopleController.Delete(person.Id);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NoContentResult>(actionResult);
      
   }
   
   [Fact]
   public void Delete_NotFound() {
      // Arrange
      var person = _seed.Person2;
      _peopleRepository.Add(person);
      _dataContext.SaveAllChanges("Add person");
      _dataContext.ClearChangeTracker();
      var nonExistentPersonId = Guid.NewGuid();
      
      // Act
      var actionResult = _peopleController.Delete(nonExistentPersonId);

      // Assert
      Assert.NotNull(actionResult);
      Assert.IsType<NotFoundResult>(actionResult);

   }

}