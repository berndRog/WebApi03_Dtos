using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Controllers;

[Route("carshop")]

[ApiController]
//[Consumes("application/json","appliation/xml")] 
//[Produces("application/json", "appliation/xml")]
[Consumes("application/json")] //default
[Produces("application/json")] //default
public class PeopleController(
   IPeopleRepository peopleRepository,
   IDataContext dataContext
   //ILogger<PersonController> logger
) : ControllerBase {
   
   // get all people http://localhost:5200/carshop/people
   [HttpGet("people")]  
   public ActionResult<IEnumerable<Person>?> GetAll() {
      var people = peopleRepository.SelectAll();
      return Ok(people);
   }
   
   
   // get by id http://localhost:5200/carshop/people/{id}
   [HttpGet("people/{id:guid}")]
   public ActionResult<Person?> GetById(
      [FromRoute] Guid id
   ) {
      // switch(personRepository.FindById(id)) {
      //    case Person person:
      //       return Ok(person);
      //    case null:
      //       return NotFound("Owner with given Id not found");
      // };
      return peopleRepository.FindById(id) switch {
         Person person => Ok(person),
         null => NotFound("Person with given id not found")
      };
   }
   
   // get person by lastname http://localhost:5200/carshop/people/name?name={name}
   // using sql like operation, i.e. name must be a part of the lastname
   [HttpGet("people/name")]
   public ActionResult<IEnumerable<Person>?> GetByName(
      [Description("Name to be search for")]
      [FromQuery] string name
   ) {
      return peopleRepository.SelectByName(name) switch {
         IEnumerable<Person> people => Ok(people),
         null => NotFound("People with given name not found")
      };
   }

   // create a new person http://localhost:5200/carshop/people
   [HttpPost("people")]
   public ActionResult<Person?> Create(
      [FromBody] Person person
   ) {
      if(peopleRepository.FindById(person.Id) != null) 
         return BadRequest("Person with given id already exists"); 
      
      // add person to repository and save changes
      peopleRepository.Add(person);
      dataContext.SaveAllChanges();
      
      return Created($"/people/{person.Id}", person);
   }
   
   // update a person http://localhost:5200/carshop/people/{id}
   [HttpPut("people/{id}")]
   public ActionResult<Person?> Update(
      [FromRoute] Guid id,
      [FromBody] Person updPerson
   ) {
      // check if the id of the person in the body is the same as the id in the url
      if (updPerson.Id != id) 
         return BadRequest("Id  do not match");
      
      // find person in the repository
      var person = peopleRepository.FindById(id);
      if (person == null) return NotFound("Person with given id not found");
      
      // update person in the domain model
      person.Update(updPerson.FirstName, updPerson.LastName, 
         updPerson.Email, updPerson.Phone);
      
      // update person in the repository and save changes
      peopleRepository.Update(person);
      dataContext.SaveAllChanges();
      
      return Ok(person);
   }

   // delete a person http://localhost:5200/carshop/people/{id}
   [HttpDelete("people/{id}")]
   public IActionResult Delete(
      [FromRoute] Guid id
   ) {
      // find person in the repository
      var person = peopleRepository.FindById(id);
      if (person == null) return NotFound();
     
      // remove person from the repository and save changes
      peopleRepository.Remove(person);
      dataContext.SaveAllChanges();
      
      return NoContent();
   }
}