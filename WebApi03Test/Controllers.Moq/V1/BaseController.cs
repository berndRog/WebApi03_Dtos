using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using WebApi.Controllers;
using WebApi.Core;
namespace WebApiTest.Controllers.Moq;

public class BaseControllerUt {
   protected readonly Seed _seed;
   protected readonly Mock<IPeopleRepository> _mockPeopleRepository;
   protected readonly Mock<ICarsRepository> _mockCarsRepository;

   protected readonly Mock<IDataContext> _mockDataContext;
     
   protected readonly PeopleController _peopleController;
   protected readonly CarsController _carsController;
   
   protected BaseControllerUt() {
      var serviceCollection = new ServiceCollection();
      
      var serviceProvider = serviceCollection.BuildServiceProvider()
         ?? throw new Exception("Failed to build Serviceprovider");

      // var loggerFactory = serviceProvider.GetService<ILoggerFactory>()
      //    ?? throw new Exception("Failed to build ILoggerFactory");
      
      _seed = new Seed();
      
      // Mocking the repository and the data context
      _mockPeopleRepository = new Mock<IPeopleRepository>();
      _mockCarsRepository = new Mock<ICarsRepository>();
      _mockDataContext = new Mock<IDataContext>();
      
      // Mocking the controller
      _peopleController = new PeopleController(
         _mockPeopleRepository.Object,
         _mockDataContext.Object
         //       loggerFactory.CreateLogger<PeopleController>()
      );

      _carsController = new CarsController(
         _mockPeopleRepository.Object,
         _mockCarsRepository.Object,
         _mockDataContext.Object
      );

   }
}