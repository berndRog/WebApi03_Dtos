using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Core.DomainModel.Entities;

namespace WebApiTest;

public class Seed {

   #region properties
   public Person Person1{ get; }
   public Person Person2{ get; }
   public Person Person3{ get; }
   public Person Person4{ get; }
   
   public Car Car1{ get; }
   public Car Car2{ get; }
   public Car Car3{ get; }
   public Car Car4{ get; }
   public Car Car5{ get; }
   public Car Car6{ get; }
   public Car Car7{ get; }
   public Car Car8{ get; }
   
   public List<Person> People{ get; private set; }
   public List<Car> Cars{ get; private set; } 
   #endregion

   public Seed(){
      
      #region People
      Person1 = new Person(
         id: new Guid("10000000-0000-0000-0000-000000000000"),
         firstName: "Erika",
         lastName: "Mustermann",
         email: "erika.mustermann@t-online.de",
         phone: "05826 1234 5678"
      );
      Person2 = new Person (
         id: new Guid("20000000-0000-0000-0000-000000000000"),
         firstName: "Max",
         lastName: "Mustermann",
         email: "max.mustermann@gmail.com",
         phone: "05826 1234 5678"
      );
      Person3 = new Person (
         id: new Guid("30000000-0000-0000-0000-000000000000"),
         firstName: "Arno",
         lastName: "Arndt",
         email: "a.arndt@t-online.de",
         phone: "04131 9876 5432"
      );
      Person4 = new Person(
         id: new Guid("40000000-0000-0000-0000-000000000000"),
         firstName: "Benno",
         lastName: "Bauer",
         email: "b.bauer@gmail.com",
         phone: "05141 4321 9876"
      );
      #endregion

      #region Cars
      Car1 = new Car(
         id: new Guid("00100000-0000-0000-0000-000000000000"),
         maker: "VW",
         model: "Golf",
         year: 2018,
         price: 15_000
      );
      Car2 = new Car(
         id: new Guid("00200000-0000-0000-0000-000000000000"),
         maker: "BMW",
         model: "520",
         year: 2020,
         price: 29_000
      );
      Car3 = new Car(
         id: new Guid("00300000-0000-0000-0000-000000000000"),
         maker: "Opel",
         model: "Mokka",
         year: 2022,
         price: 21_000
      );
      Car4 = new Car(
         id: new Guid("00400000-0000-0000-0000-000000000000"),
         maker: "VW",
         model: "Golf",
         year: 2015,
         price: 13_000
      );
      Car5 = new Car(
         id: new Guid("00500000-0000-0000-0000-000000000000"),
         maker: "BMW",
         model: "X5",
         year: 2021,
         price: 49_500
      );
      Car6 = new Car(
         id: new Guid("00600000-0000-0000-0000-000000000000"),
         maker: "Hyuandai",
         model: "Tucson",
         year: 2021,
         price: 24500
      );
      Car7 = new Car(
         id: new Guid("00700000-0000-0000-0000-000000000000"),
         maker: "VW",
         model: "Golf",
         year: 2010,
         price: 9500
      );
      Car8 = new Car(
         id: new Guid("00800000-0000-0000-0000-000000000000"),
         maker: "VW",
         model: "Golf",
         year: 2012,
         price: 10500
      );
      #endregion
      
      People = [Person1, Person2, Person3, Person4];
      Cars = [Car1, Car2, Car3, Car4, Car5, Car6, Car7, Car8];
   }
   
   
   // Setup Relations between Users and Cars
   // public Seed InitCars(){
   //    // Users
   //    Person1.AddCar(Car1); 
   //    Person1.AddCar(Car2);
   //    Person2.AddCar(Car3); 
   //    Person2.AddCar(Car4);
   //    Person3.AddCar(Car5); 
   //    Person3.AddCar(Car6);
   //    Person3.AddCar(Car7);
   //    Person4.AddCar(Car8); 
   //    return this;
   // }
   
   public static (IEnumerable<Person>, IEnumerable<Car>) InitPeopleWithCars(
      IEnumerable<Person> people,
      IEnumerable<Car> cars
   ) {
      var arrayPeople = people.ToArray();
      var arrayCars = cars.ToArray();
      if(arrayPeople.Count() != 4 || arrayCars.Count() != 8) 
         throw new ArgumentException("Invalid number of people or cars");
      arrayPeople[0].AddCar(arrayCars[0]);
      arrayPeople[0].AddCar(arrayCars[1]);
      arrayPeople[1].AddCar(arrayCars[2]);
      arrayPeople[1].AddCar(arrayCars[3]);
      arrayPeople[2].AddCar(arrayCars[4]);
      arrayPeople[2].AddCar(arrayCars[5]);
      arrayPeople[2].AddCar(arrayCars[6]);
      arrayPeople[3].AddCar(arrayCars[7]);
      return (arrayPeople, arrayCars);
   }
}