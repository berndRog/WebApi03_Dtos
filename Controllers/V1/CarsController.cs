using System.ComponentModel;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
using WebApi.Core.Dtos;
using WebApi.Core.Mapping;
namespace WebApi.Controllers.V1; 

[ApiVersion("1.0")]
[Route("carshop/v{version:apiVersion}")]

[ApiController]
[Consumes("application/json")] //default
[Produces("application/json")] //default

public class CarsController(
   IPeopleRepository peopleRepository,
   ICarsRepository carsRepository,
   IDataContext dataContext
) : ControllerBase {

   /// <summary>
   /// Get all cars
   /// </summary>
   [HttpGet("cars")]
   [EndpointSummary("Get all cars")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<CarDto>> GetCars() {
      // get all cars 
      var cars = carsRepository.SelectAll();
      return Ok(cars.Select(c => c.ToCarDto()));
   }

   /// <summary>
   /// Get cars by attributes
   /// </summary>
   /// <param name="maker">maker to be search for (can be null)</param>
   /// <param name="model">model to be search for (can be null)</param>
   /// <param name="yearMin">year >= yearMin of the car to be search for (can be null)."</param>
   /// <param name="yearMax">year <= yearMax of the car to be search for (can be null)."</param>
   /// <param name="priceMin">price >= priceMin of the car to be search for (can be null)."</param>
   /// <param name="priceMax">price >= priceMin of the car to be search for (can be null)."</param>
   [HttpGet("cars/attributes")]
   [EndpointSummary("Get cars by attributes")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<CarDto>> GetCarsByAttrubutes(
      [Description("maker of the car to be search for (can be null)")]
      [FromHeader] string? maker,
      [Description("model of the car to be search for (can be null)")]
      [FromHeader] string? model,
      [Description("year >= yearMin of the car to be search for (can be null)")]
      [FromHeader] int? yearMin,
      [Description("year <= yearMax of the car to be search for (can be null)")]
      [FromHeader] int? yearMax,
      [Description("price >= priceMin of the car to be search for (can be null)")]
      [FromHeader] double? priceMin,
      [Description("price <= priceMax of the car to be search for (can be null)")]
      [FromHeader] double? priceMax
   ) {
      // get all cars by attributes
      var cars = carsRepository.SelectByAttributes(maker, model, yearMin, yearMax, 
         priceMin, priceMax);
      return Ok(cars.Select(c => c.ToCarDto()));
   }
  
   /// <summary>
   /// Get all cars of a given person
   /// </summary>
   /// <param name="personId">Unique id for the given person</param>
   [HttpGet("people/{personId:guid}/cars")]
   [EndpointSummary("Get all cars of a given person")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<IEnumerable<CarDto>> GetCarsByPerson(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId
   ) {
      // get all cars of a given person
      var cars = carsRepository.SelectByPersonId(personId);

      return cars.Any() switch {
         true => Ok(cars.Select(c => c.ToCarDto())),
         false => NotFound("No cars found for given personId")
      };
   }
   
   /// <summary>
   /// <param name="id">Unique id of the car to be search for</param>
   [HttpGet("cars/{id:guid}")]
   [EndpointSummary("Get car by id")]
   public ActionResult<CarDto> GetById(
      [Description("Unique id of the car to be search for")] 
      [FromRoute] Guid id
   ) {
      return carsRepository.FindById(id) switch {
         Car car => Ok(car.ToCarDto()),
         null => NotFound("Car with given id not found")
      };
   }
   
   /// <summary>
   /// Create a new car for a given person
   /// </summary>
   /// <param name="personId">Unique id of the given person</param>
   /// <param name="carDto">CarDto of the new car's data</param>
   [HttpPost("people/{personId:guid}/cars")]
   [EndpointSummary("Create a new car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public ActionResult<CarDto> Create(
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
         return BadRequest("personId doesn't exist");
      
      // map Dto to entity
      var car = carDto.ToCar();
      // add car to person in the domain model
      person.AddCar(car);
      
      // add car to repository and save changes
      carsRepository.Add(car); 
      dataContext.SaveAllChanges();
      
      // return created car as Dto
      var requestPath = Request?.Path ?? $"http://localhost:5200/carshop/cars/{car.Id}";
      var uri = new Uri($"{requestPath}/{car.Id}", UriKind.Relative);
      return Created(uri, car.ToCarDto()); 
   }

   /// <summary>
   /// Update a car for a given person
   /// </summary>
   /// <param name="personId">uid for the given person</param>
   /// <param name="id">Guid for the car to be updated</param>
   /// <param name="updCarDto">CarDto of the updated car's data</param>
   /// <returns></returns>
   [HttpPut("people/{personId:guid}/cars/{id:guid}")] 
   [EndpointSummary("Update a car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<CarDto> Update(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId,
      [Description("Unique id for the car to be updated")] 
      [FromRoute] Guid id,
      [Description("CarDto of the updated car's data")]
      [FromBody]  CarDto updCarDto
   ) {

      // check if Id in the route and body match
      if(personId != updCarDto.Id) return BadRequest("Update Car: Id in the route and body do not match");
      // check if person with given Id exists
      var car = carsRepository.FindById(id);
      if (car == null) return NotFound("Update Car: Car with given id not found");

      // map dto to entity
      var updCar = updCarDto.ToCar();
      // update car in the domain model
      car.Update(updCar);
      
      // save to repository and write changes 
      carsRepository.Update(car);
      dataContext.SaveAllChanges();
      
      return Ok(car.ToCarDto());
   }
   
   /// <summary>
   /// Delete a car for a given person
   /// </summary>
   /// <param name="personId">Unique id for the given person</param>
   /// <param name="id">Unique id for the given car"</param>
   [HttpDelete("people/{personId:guid}/cars/{id:guid}")]
   [EndpointSummary("Delete a car for a given person")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public IActionResult Delete(
      [Description("Unique id for the given person")] 
      [FromRoute] Guid personId,
      [Description("Unique id for the given car")] 
      [FromRoute] Guid id
   ) {
      // find person in the repository
      var person = peopleRepository.FindById(personId);
      if(person == null) return NotFound("Delete Car: Person not found.");
      // find car in the repository
      var car = carsRepository.FindById(id); 
      if(car == null) return NotFound("Delete Car: Car not found.");
      
      // remove car from person in the doimain model
      person.RemoveCar(car);
      
      // save to repository and write changes 
      carsRepository.Remove(car);
      dataContext.SaveAllChanges();

      // return no content
      return NoContent(); 
   }
}
