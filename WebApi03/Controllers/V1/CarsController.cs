using Microsoft.AspNetCore.Mvc;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Controllers; 

[Route("carshop")]

[ApiController]
[Consumes("application/json")] //default
[Produces("application/json")] //default

public class CarsController(
   IPeopleRepository peopleRepository,
   ICarsRepository carsRepository,
   IDataContext dataContext
) : ControllerBase {
   
   // get all cars http://localhost:5200/carshop/cars
   [HttpGet("cars")]
   public ActionResult<IEnumerable<Car>?> GetAll() {
      // get all cars 
      var cars = carsRepository.SelectAll();
      return Ok(cars);
   }
   
   // get car by id http://localhost:5200/carshop/cars/{id}
   [HttpGet("cars/{id:guid}")]
   public ActionResult<Car?> GetById(
      [FromRoute] Guid id
   ) {
      return carsRepository.FindById(id) switch {
         Car car => Ok(car),
         null => NotFound("Car with given id not found")
      };
   }
   
   // create a new car for a given person http://localhost:5200/carshop/people/{personId}/cars
   [HttpPost("people/{personId:guid}/cars")]
   public ActionResult<Car?> Create(
      [FromRoute] Guid personId,
      [FromBody]  Car car
   ) {
      if(carsRepository.FindById(car.Id) != null)
         return BadRequest("Car with given Id already exists");
      
      // find person in the repository
      var person = peopleRepository.FindById(personId);
      if (person == null)
         return BadRequest("personId doesn't exist");
      
      // add car to person in the domain model
      person.AddCar(car);
      
      // add car to repository and save changes
      carsRepository.Add(car); 
      dataContext.SaveAllChanges();
      
      // return created car as Dto
      var requestPath = Request?.Path ?? $"http://localhost:5200/carshop/cars/{car.Id}";
      var uri = new Uri($"{requestPath}/{car.Id}", UriKind.Relative);
      return Created(uri, car); 
   }

   // update a car for a given person http://localhost:5200/carshop/people/{personId}/cars/{id}
   [HttpPut("people/{personId:guid}/cars/{id:guid}")] 
   public ActionResult<Car?> Update(
      [FromRoute] Guid personId,
      [FromRoute] Guid id,
      [FromBody]  Car updCar
   ) {
      // check if Id in the route and body match
      if(personId != updCar.Id) return BadRequest("Update Car: Id in the route and body do not match");
      // check if person with given Id exists
      var car = carsRepository.FindById(id);
      if (car == null) return NotFound("Update Car: Car with given id not found");

      // update car in the domain model
      car.Update(updCar.Maker, updCar.Model, updCar.Year, updCar.Price);
      
      // save to repository and write changes 
      carsRepository.Update(car);
      dataContext.SaveAllChanges();
      
      return Ok(car);
   }
   
   // delete a car for a given person http://localhost:5200/carshop/people/{personId}/cars/{id}
   [HttpDelete("people/{personId:guid}/cars/{id:guid}")]
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
   
   // filter cars by attributes http://localhost:5200/carshop/cars/attributes
   // filter criteria are passed in the header
   [HttpGet("cars/attributes")]
   public ActionResult<IEnumerable<Car>?> GetCarsByAttributes(
      [FromHeader] string? maker,
      [FromHeader] string? model,
      [FromHeader] int? yearMin,
      [FromHeader] int? yearMax,
      [FromHeader] decimal? priceMin,
      [FromHeader] decimal? priceMax
   ) {
      // get all cars by attributes
      var cars = carsRepository.SelectByAttributes(maker, model, yearMin, yearMax, 
         priceMin, priceMax);
      return Ok(cars);
   }
   
      
   // get all cars of a given person http://localhost:5200/carshop/people/{personId}/cars
   [HttpGet("people/{personId:guid}/cars")]
   public ActionResult<IEnumerable<Car>?> GetCarsByPersonId(
      [FromRoute] Guid personId
   ) {
      // get all cars of a given person
      var cars = carsRepository.SelectCarsByPersonId(personId);
      return Ok(cars);
   }
}