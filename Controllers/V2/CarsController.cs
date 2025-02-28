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
//[Route("carshop")]

[ApiController]
[Consumes("application/json")] //default
[Produces("application/json")] //default

public class CarsController(
   ControllerHelper helper,
   IPersonRepository personRepository,
   ICarRepository carRepository,
   IDataContext dataContext
) : ControllerBase {
   
   [HttpGet("people/{personId:guid}/cars")]
   [EndpointSummary("Get all cars of a given person")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<IEnumerable<CarDto>> GetCarsByPerson(
      [Description("personId for the given person.")] 
      [FromRoute] Guid personId
   ) {
      // get all cars of a given person
      var cars = carRepository.SelectByPersonId(personId);

      return cars.Any() switch {
         true => Ok(cars.Select(c => c.ToCarDto())),
         false => helper.DetailsNotFound<IEnumerable<CarDto>>("No cars found for given personId")
      };
   }
   
   [HttpGet("cars/{id:guid}")]
   [EndpointSummary("Get car by id")]
   public ActionResult<CarDto> GetById(
      [Description("id of the car to be searched for.")] 
      [FromRoute] Guid id
   ) {
      return carRepository.FindById(id) switch {
         Car car => Ok(car.ToCarDto()),
         null => NotFound("Car with given id not found")
      };
   }
  
   /*
   /// <summary>
   /// Create a new car for a given person 
   /// </summary>
   /// <param name="personId">id of the person</param>
   /// <param name="car">id of the car</param>
   /// <returns></returns>
   [HttpPost("people/{personId:guid}/carserror")]
   public ActionResult<Car> Create(
      [FromRoute] Guid personId,
      [FromBody]  Car car
   ) {
      // find person
      var person = personRepository.FindById(personId);
      if (person == null)
         return BadRequest("Bad request: personId doesn't exists.");

      // check if car with given Id already exists   
      if(carRepository.FindById(car.Id) != null) 
         return Conflict("Car with given Id already exists");
      
      // add car to person in the domain model
      person.AddCar(car);
      
      // add car to repository and save to datastore
      carRepository.Add(car); 
      dataContext.SaveChanges();
      
      // return created car as Dto
      var requestPath = Request?.Path ?? $"http://localhost:5200/carshop/cars/{car.Id}";
      var uri = new Uri($"{requestPath}/{car.Id}", UriKind.Relative);
      return Created(uri, car); 
   }
   */

   [HttpPost("people/{personId:guid}/cars")]
   [EndpointSummary("Create a new car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public ActionResult<CarDto> Create(
      [Description("id of the given person.")] 
      [FromRoute] Guid personId,
      [Description("data of the new car.")]
      [FromBody]  CarDto carDto
   ) {
      // find person
      var person = personRepository.FindById(personId);
      if (person == null)
         return helper.DetailsBadRequest<CarDto>("personId doesn't exist.");
      
      // check if car with given Id already exists   
      if(carRepository.FindById(carDto.Id) != null) 
         return helper.DetailsConflict<CarDto>("Car with given id already exists");

      // map Dto to domain model
      var car = carDto.ToCar();
      // add car to person in the domain model
      person.AddCar(car);
      
      // add car to repository and save to datastore
      carRepository.Add(car); 
      dataContext.SaveChanges();
      
      // return created car as Dto
      var requestPath = Request?.Path ?? $"http://localhost:5200/carshop/cars/{car.Id}";
      var uri = new Uri($"{requestPath}/{car.Id}", UriKind.Relative);
      return Created(uri, car.ToCarDto()); 
   }



   // Update car
   /// <summary>
   /// 
   /// </summary>
   /// <param name="personId"></param>
   /// <param name="id"></param>
   /// <param name="updCarDto"></param>
   /// <returns></returns>
   [HttpPut("people/{personId:guid}/cars/{id:guid}")] 
   [EndpointSummary("Update a car for a given person")]
   [ProducesResponseType<CarDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<CarDto> Update(
      [Description("personId for the given person.")] 
      [FromRoute] Guid personId,
      [Description("id for the car to be updated.")] 
      [FromRoute] Guid id,
      [FromBody]  CarDto updCarDto
   ) {

      // check if Id in the route and body match
      if(personId != updCarDto.Id) return helper.DetailsBadRequest<CarDto>(
            "Update Car: Id in the route and body do not match.");
      // check if person with given Id exists
      var car = carRepository.FindById(id);
      if (car == null) return helper.DetailsNotFound<CarDto>(
         "Update Car: Car with given id not found.");

      // Update car in the domain model
      var updCar = updCarDto.ToCar();
      car.Update(updCar);
      
      // save to repository and write to datastore 
      carRepository.Update(car);
      dataContext.SaveChanges();

      // return updated car as Dto
      return Ok(car.ToCarDto());
   }
   
   /// <summary>
   /// Delete a car for a given person
   /// </summary>
   /// <param name="personId">id of the person</param>
   /// <param name="id">id of the car</param>
   [HttpDelete("people/{personId:guid}/cars/{id:guid}")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public IActionResult Delete(
      [FromRoute] Guid personId,
      [FromRoute] Guid id
   ) {
      var car = carRepository.FindById(id); 
      if(car == null)
         return NotFound("Delete Car: Car not found.");
      
      var person = personRepository.FindById(personId);
      if(person == null)
         return NotFound("Delete Car: Person not found.");
      
      // remove car from person in the doimainmodel
      person.RemoveCar(car);
      
      // save to repository and write to database 
      carRepository.Remove(car);
      dataContext.SaveChanges();

      // return no content
      return NoContent(); 
   }
}