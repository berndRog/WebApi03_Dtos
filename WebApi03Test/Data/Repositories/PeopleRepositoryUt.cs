using System;
using System.Collections.Generic;
using DeepEqual;
using DeepEqual.Syntax;
using Microsoft.EntityFrameworkCore;
using WebApi.Core.DomainModel.Entities;
using Xunit;
namespace WebApiTest.Data.Repositories;

[Collection(nameof(SystemTestCollectionDefinition))]
public class PeopleRepositoryUt : BaseRepository {

   #region PersonOnly
   [Fact]
   public void FindByIdUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      // Act
      var actual = _peopleRepository.FindById(_seed.Person1.Id);
      // Assert
      Assert.Equivalent(_seed.Person1, actual);
   }
   
   [Fact]
   public void AddUt() {
      // Arrange
      var person = _seed.Person1;
      // Act
      _peopleRepository.Add(person);
      _dataContext.SaveAllChanges();
      // Assert
      var actual = _peopleRepository.FindById(person.Id);
      Assert.Equal(person, actual);
   }

   [Fact]
   public void AddRangeUt() {
      // Arrange
      var expected = _seed.People;
      // Act
      _peopleRepository.AddRange(_seed.People);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      // Assert
      var actual = _peopleRepository.SelectAll();
      Assert.Equivalent(expected, actual);
   }


   [Fact]
   public void UpdateUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
     
      // Act
      // retrieve person from database to track it
      var actualPerson = _peopleRepository.FindById(_seed.Person1.Id);
      Assert.NotNull(actualPerson);
      // domain model
      actualPerson.Update("Erika","Meier", _seed.Person1.Email, _seed.Person1.Phone);
      // update person in repository
      _peopleRepository.Update(actualPerson);
      _dataContext.SaveAllChanges();
     
      // Assert
      var actual = _peopleRepository.FindById(_seed.Person1.Id);
      Assert.Equivalent(actualPerson, actual);
   }
   
   [Fact]
   public void Update_EntityNotFoundUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      // Act
      // retrieve person from database to track it
      var actualPerson = _peopleRepository.FindById(_seed.Person1.Id);
      Assert.NotNull(actualPerson);
      
      // Remove the person to simulate entity not found
      _peopleRepository.Remove(actualPerson);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      
      // domain model
      actualPerson.Update("Erika","Meier", _seed.Person1.Email, _seed.Person1.Phone);
      
      var exception = Assert.Throws<ApplicationException>(() => _peopleRepository.Update(actualPerson));
      Assert.Equal("Update failed, entity with given id not found", exception.Message);
   }
   
   [Fact]
   public void RemoveUt() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      // Act
      _peopleRepository.Remove(_seed.Person1);
      _dataContext.SaveAllChanges();
      // Assert
      var actual = _peopleRepository.FindById(_seed.Person1.Id);
      Assert.Null(actual);
   }
   
   [Fact]
   public void SelectByNameUt() {
      // Arrange
      _peopleRepository.AddRange(_seed.People);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      var expected = new List<Person> { _seed.Person1, _seed.Person2 };
      
      // Act
      var actual = _peopleRepository.SelectByName("Muster"); 
      
      // Assert
      Assert.Equivalent(expected, actual);
   }
   #endregion

   #region PersonJoinCars
   [Fact]
   public void FindByIdJoinCarsUt() {
      // Arrange
      // add person to repository first
      _peopleRepository.Add(_seed.Person1);  
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      
      // Act
      var person = _peopleRepository.FindById(_seed.Person1.Id);
      Assert.NotNull(person);
      // Domain model
      var car1 = _seed.Car1;
      var car2 = _seed.Car2;
      person.AddCar(car1);
      person.AddCar(car2);
      _dataContext.LogChangeTracker("AddJoinCarsUt");
      // add the cars to the repository
      _carsRepository.Add(car1);
      _carsRepository.Add(car2);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      
      // Assert 
      var actual = _peopleRepository.FindByIdJoinCars(person.Id);      
      Assert.NotNull(actual);
      
      Console.WriteLine(ToPrettyJson("person", person));
     
      Console.WriteLine(ToPrettyJson("actual", actual));
      
      
      // Assert
      var comparison = new ComparisonBuilder()
         .IgnoreCircularReferences()
         .Create();
      Assert.True(person.IsDeepEqual(actual, comparison));
   }
   
   [Fact]
   public void DeleteWithCarsCascadingUt() {
      // Arrange
      _peopleRepository.Add(_seed.Person1);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();
      
      var person = _peopleRepository.FindById(_seed.Person1.Id);
      Assert.NotNull(person);
      person.AddCar(_seed.Car1);
      person.AddCar(_seed.Car2);
      _carsRepository.Add(_seed.Car1);
      _carsRepository.Add(_seed.Car2);
      _dataContext.SaveAllChanges();
      _dataContext.ClearChangeTracker();

      // Act
      _peopleRepository.Remove(_seed.Person1); 
      _dataContext.SaveAllChanges();

      // Assert
      var actualPerson= _peopleRepository.FindById(_seed.Person1.Id);
      var actualCar1 = _carsRepository.FindById(_seed.Car1.Id);
      var actualCar2 = _carsRepository.FindById(_seed.Car2.Id);

      Assert.Null(actualPerson);
      Assert.Null(actualCar1);
      Assert.Null(actualCar2);

   }
   #endregion
   
   
}