using System.ComponentModel;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
namespace WebApi.Controllers.V2; 

[ApiVersion("2.0")]
[Route("carshop/v{version:apiVersion}")]

[ApiController]
[Consumes("application/json")] //default
[Produces("application/json")] //default

public class CarsController(
   IPeopleRepository peopleRepository,
   ICarsRepository carsRepository,
   IDataContext dataContext
) : ControllerBase {

   // get all cars http://localhost:5200/carshop/v2/cars
   [HttpGet("cars")]
   [EndpointSummary("Get all cars")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<CarDto>?> GetAll() {
      // get all cars 
      var cars = carsRepository.SelectAll();
      return Ok(cars?.Select(c => c.ToCarDto()));
   }

   // get car by id http://localhost:5200/carshop/v2/cars/{id}
   [HttpGet("cars/{id:guid}")]
   [EndpointSummary("Get car by id")]
   public ActionResult<CarDto?> GetById(
      [Description("Unique id of the car to be search for")] 
      [FromRoute] Guid id
   ) {
      return carsRepository.FindById(id) switch {
         Car car => Ok(car.ToCarDto()),
         null => NotFound("Car with given id not found")
      };
   }
   
   // create a new car for a given person http://localhost:5200/carshop/people/{personId}/cars
   [HttpPost("people/{personId:guid}/cars")]
   [EndpointSummary("Create a new car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public ActionResult<CarDto?> Create(
      [Description("Unique id of the given person")] 
      [FromRoute] Guid personId,
      [Description("CarDto of the new car's data")]
      [FromBody]  CarDto carDto
   ) {
      if(carsRepository.FindById(carDto.Id) != null)
         return BadRequest("Car with given Id already exists");
      
      // find person in the repository
      var person = peopleRepository.FindById(personId);
      if (person == null)
         return BadRequest("PersonId doesn't exist");
      
      // map Dto to entity
      var car = carDto.ToCar();
      // add car to person in the domain model
      person.AddCar(car);
      
      // add car to repository and save changes
      carsRepository.Add(car); 
      dataContext.SaveAllChanges();
      
      // return created car as Dto
      var requestPath = Request?.Path.ToString() ?? @"http://localhost:5200/carshop/v2/cars";
      var uri = new Uri($"{requestPath}/{car.Id}", UriKind.Absolute);
      return Created(uri, car.ToCarDto()); 
   }

   // update a car for a given person http://localhost:5200/carshop/v2/people/{personId}/cars/{id}
   [HttpPut("people/{personId:guid}/cars/{id:guid}")] 
   [EndpointSummary("Update a car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<CarDto?> Update(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId,
      [Description("Unique id for the car to be updated")] 
      [FromRoute] Guid id,
      [Description("CarDto of the updated car's data")]
      [FromBody]  CarDto updCarDto
   ) {

      // check if Id in the route and body match
      if(id != updCarDto.Id) 
         return BadRequest("Update Car: Id in the route and body do not match");
      // check if person with given Id exists
      var car = carsRepository.FindById(id);
      if (car == null) 
         return NotFound("Update Car: Car with given id not found");

      // map dto to entity
      var updCar = updCarDto.ToCar();
      // update car in the domain model
      car.Update(updCar.Maker, updCar.Model, updCar.Year, updCar.Price);
      
      // save to repository and write changes 
      carsRepository.Update(car);
      dataContext.SaveAllChanges();
      
      return Ok(car.ToCarDto());
   }
   
   // delete a car for a given person http://localhost:5200/carshop/people/{personId}/cars/{id}
   [HttpDelete("people/{personId:guid}/cars/{id:guid}")]
   [EndpointSummary("Delete a car for a given person")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<Car?> Delete(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId,
      [Description("Unique id for the given car")] 
      [FromRoute] Guid id
   ) {
      // find person in the repository
      var person = peopleRepository.FindById(personId);
      if(person == null) 
         return NotFound("Delete Car: Person not found.");
      // find car in the repository
      var car = carsRepository.FindById(id); 
      if(car == null) 
         return NotFound("Delete Car: Car not found.");
      
      // remove car from person in the doimain model
      person.RemoveCar(car);
      
      // save to repository and write changes 
      carsRepository.Remove(car);
      dataContext.SaveAllChanges();

      // return no content
      return NoContent(); 
   }
   
   // filter cars by attributes http://localhost:5200/carshop/v2/cars/attributes
   // filter criteria are passed in the header
   [HttpGet("cars/attributes")]
   [EndpointSummary("Get cars by attributes")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<CarDto>?> GetCarsByAttrubutes(
      [Description("maker of the car to be search for (can be null)")]
      [FromHeader] string? maker,
      [Description("model of the car to be search for (can be null)")]
      [FromHeader] string? model,
      [Description("year >= yearMin of the car to be search for (can be null)")]
      [FromHeader] int? yearMin,
      [Description("year <= yearMax of the car to be search for (can be null)")]
      [FromHeader] int? yearMax,
      [Description("price >= priceMin of the car to be search for (can be null)")]
      [FromHeader] decimal? priceMin,
      [Description("price <= priceMax of the car to be search for (can be null)")]
      [FromHeader] decimal? priceMax
   ) {
      // get all cars by attributes
      var cars = carsRepository.SelectByAttributes(maker, model, yearMin, yearMax, 
         priceMin, priceMax);
      return Ok(cars?.Select(c => c.ToCarDto()));
   }
  
   // get all cars of a given person http://localhost:5200/carshop/v2/people/{personId}/cars
   [HttpGet("people/{personId:guid}/cars")]
   [EndpointSummary("Get all cars of a given person")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<IEnumerable<CarDto>?> GetCarsByPersonId(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId
   ) {
      // get all cars of a given person
      var cars = carsRepository.SelectCarsByPersonId(personId);
      return Ok(cars?.Select(c => c.ToCarDto()));
   }
   
   
}