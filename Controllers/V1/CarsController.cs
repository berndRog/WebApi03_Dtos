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
   
   [HttpGet("cars")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<CarDto>> GetCars() {
      // get all cars 
      var cars = carsRepository.SelectAll();
      return Ok(cars.Select(c => c.ToCarDto()));
   }

   [HttpGet("cars/attributes")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<CarDto>> GetCarsByAttrubutes(
      [FromHeader] string? maker,
      [FromHeader] string? model,
      [FromHeader] int? yearMin,
      [FromHeader] int? yearMax,
      [FromHeader] double? priceMin,
      [FromHeader] double? priceMax
   ) {
      // get all cars by attributes
      var cars = carsRepository.SelectByAttributes(maker, model, yearMin, yearMax, 
         priceMin, priceMax);
      return Ok(cars.Select(c => c.ToCarDto()));
   }
  
   [HttpGet("people/{personId:guid}/cars")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<IEnumerable<CarDto>> GetCarsByPerson(
      [FromRoute] Guid personId
   ) {
      // get all cars of a given person
      var cars = carsRepository.SelectByPersonId(personId);

      return cars.Any() switch {
         true => Ok(cars.Select(c => c.ToCarDto())),
         false => NotFound("No cars found for given personId")
      };
   }
   
   [HttpGet("cars/{id:guid}")]
   public ActionResult<CarDto> GetById(
      [FromRoute] Guid id
   ) {
      return carsRepository.FindById(id) switch {
         Car car => Ok(car.ToCarDto()),
         null => NotFound("Car with given id not found")
      };
   }
   
   [HttpPost("people/{personId:guid}/cars")]
   [ProducesResponseType<CarDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public ActionResult<CarDto> Create(
      [FromRoute] Guid personId,
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

   [HttpPut("people/{personId:guid}/cars/{id:guid}")] 
   [ProducesResponseType<CarDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<CarDto> Update(
      [FromRoute] Guid personId,
      [FromRoute] Guid id,
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
   
   [HttpDelete("people/{personId:guid}/cars/{id:guid}")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public IActionResult Delete(
      [FromRoute] Guid personId,
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
