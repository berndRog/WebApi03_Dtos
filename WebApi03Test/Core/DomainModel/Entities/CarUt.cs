using System;

using WebApi.Core.DomainModel.Entities;
using Microsoft.VisualBasic;
using WebApi.Core.DomainModel.NullEntities;
using Xunit;

namespace WebApiTest.Core.DomainModel.Entities;
public class CarUt {

   private readonly Seed _seed;
   
   public CarUt() {
      _seed = new Seed();
   }

   [Fact]
   public void CtorStdUt() {
      // Arrange
      // Act
      var actual = new Car();
      // Assert
      Assert.NotNull(actual);
      Assert.IsType<Car>(actual);
   }

   [Fact]
   public void CtorUt() {
      // Arrange
      // Act
      var actual = new Car(
         id: _seed.Car1.Id,
         maker: _seed.Car1.Maker,
         model: _seed.Car1.Model,
         year: _seed.Car1.Year,
         price: _seed.Car1.Price
      );
      // Assert
      Assert.NotNull(actual);
      Assert.IsType<Car>(actual);
      Assert.Equivalent(_seed.Car1, actual);
      
   }

   [Fact]
   public void GetterUt() {
      // Act
      var actual = _seed.Car1;
      var id = actual.Id;
      var maker = actual.Maker;
      var model = actual.Model;
      var year = actual.Year;
      var price = actual.Price;
      // Assert
      Assert.Equal(_seed.Car1.Id, id);
      Assert.Equal(_seed.Car1.Maker, maker);
      Assert.Equal(_seed.Car1.Model, model); 
      Assert.Equal(_seed.Car1.Year, year);
      Assert.Equal(_seed.Car1.Price, price);
   }
   
   
   [Fact]
   public void SetImageUrlUt() {
      // Arrange
      var actual = _seed.Car1;
      var imageUrl = "user/photos/image1.jpg";
      // Act
      actual.SetImageUrl(imageUrl);
      // Assert
      Assert.Equal(imageUrl, actual.ImageUrl);
   }
   
   [Fact]
   public void SetPersonUt() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      // Act
      person.AddCar(car); // setPerson is called by AddCar
      // Assert
      Assert.Equivalent(person, car.Person);
      Assert.Equal(person.Id, car.PersonId);
   }
   
   [Fact]
   public void SetPersonToNullPersonUt() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car); // setPerson is called by AddCar
      // Act
      person.RemoveCar(car);
      // Assert
      Assert.Equivalent(NullPerson.Instance, car.Person);
      Assert.Equal(NullPerson.Instance.Id, car.PersonId);
   }
   
   [Fact]
   public void UpdateUt() {
      // Arrange
      var person = _seed.Person1;
      var car = _seed.Car1;
      person.AddCar(car); 
      // New values for update
      var updatedMaker = "UpdatedMaker";
      var updatedModel = "UpdatedModel";
      var updatedYear = car.Year + 1;
      var updatedPrice = car.Price + 1000m;
      // Act
      car.Update(updatedMaker, updatedModel, updatedYear, updatedPrice);
      // Assert: verify values are updated as expected
      Assert.Equal(updatedMaker, car.Maker);
      Assert.Equal(updatedModel, car.Model);
      Assert.Equal(updatedYear, car.Year);
      Assert.Equal(updatedPrice, car.Price);
   }
}