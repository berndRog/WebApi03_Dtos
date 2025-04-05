using System.Linq;
using WebApi.Core.DomainModel.Entities;
using Xunit;
namespace WebApiTest.Core.DomainModel.Entities;
public class PersonUt {

   private readonly Seed _seed = new();
   
   [Fact]
   public void CtorStdUt() {
      // Arrange
      Person actual = new Person();
      // Assert
      Assert.NotNull(actual);
      Assert.IsType<Person>(actual);
   }

   [Fact]
   public void CtorUt() {
      // Arrange
      Person actual = new Person(
         id: _seed.Person1.Id,
         firstName: _seed.Person1.FirstName,
         lastName: _seed.Person1.LastName,
         email:  _seed.Person1.Email,
         phone: _seed.Person1.Phone
      );
      // Assert
      Assert.NotNull(actual);
      Assert.IsType<Person>(actual);
      
      // Assert.Equal(_seed.Person1.Id, actual.Id);
      // Assert.Equal(_seed.Person1.FirstName, actual.FirstName);
      // Assert.Equal(_seed.Person1.LastName, actual.LastName);
      // Assert.Equal(_seed.Person1.Email, actual.Email);
      // Assert.Equal(_seed.Person1.Phone,actual.Phone);
      Assert.Equivalent(_seed.Person1, actual);
   }
   
   [Fact]
   public void GetterUt() {
      // Arrange
      var actual = _seed.Person1;
      // Act
      var actualId = actual.Id;
      var actualFirstName = actual.FirstName;
      var actualLastName = actual.LastName;
      var actualEmail = actual.Email;
      var actualPhone = actual.Phone;
      // Assert
      // Assert.Equivalent(_seed.Person1.Id, actualId);
      // Assert.Equal(_seed.Person1.FirstName, actualFirstName);
      // Assert.Equal(_seed.Person1.LastName, actualLastName);
      // Assert.Equal(_seed.Person1.Email, actualEmail);
      // Assert.Equal(_seed.Person1.Phone,actualPhone);
      Assert.Equivalent(_seed.Person1, actual);
   }
   
   [Fact]
   public void SetUt() {
      // Arrange
      Person actual = new Person(
         id: _seed.Person1.Id,
         firstName: _seed.Person1.FirstName,
         lastName: _seed.Person1.LastName,
         email:  null,
         phone: null
      );
      // Act
      actual.Set(email: _seed.Person1.Email, phone: _seed.Person1.Phone);
      // Assert
      // Assert.Equivalent(_seed.Person1.Id, actual.Id);
      // Assert.Equal(_seed.Person1.FirstName, actual.FirstName);
      // Assert.Equal(_seed.Person1.LastName, actual.LastName);
      // Assert.Equal(_seed.Person1.Email, actual.Email);
      // Assert.Equal(_seed.Person1.Phone,actual.Phone);
      Assert.Equivalent(_seed.Person1, actual);
   }
   
   [Fact]
   public void UpdateUt() {
      // Arrange
      var firstName = _seed.Person2.FirstName;
      var lastName = _seed.Person2.LastName;
      var email = _seed.Person2.Email;
      var phone = _seed.Person2.Phone;
      // Act
      _seed.Person1.Update(firstName, lastName, email, phone);
      // Assert
      Assert.Equal(_seed.Person2.FirstName, _seed.Person1.FirstName);
      Assert.Equal(_seed.Person2.LastName, _seed.Person1.LastName);
      Assert.Equal(_seed.Person2.Email, _seed.Person1.Email);
      Assert.Equal(_seed.Person2.Phone, _seed.Person1.Phone);
   }
   
   [Fact]
   public void PersonAddCarUt() {
      // Arrange
      // Act
      _seed.Person1.AddCar(_seed.Car1);
      _seed.Person1.AddCar(_seed.Car2);
      // Assert
      var car1 = _seed.Person1.Cars.ToList()[0];
      Assert.Equivalent(car1,_seed.Car1);
   }
   
   [Fact]
   public void PersonRemoveCarUt() {
      // Arrange
      _seed.Person1.AddCar(_seed.Car1);
      _seed.Person1.AddCar(_seed.Car2);
      // Act
      _seed.Person1.RemoveCar(_seed.Car1);
      // Assert
      Assert.Single(_seed.Person1.Cars);
   }
}