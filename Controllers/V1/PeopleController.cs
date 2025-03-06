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
[Consumes("application/json","appliation/xml")] //default
[Produces("application/json", "appliation/xml")] //default
public class PeopleController(
   IPeopleRepository peopleRepository,
   IDataContext dataContext
   //ILogger<PersonController> logger
) : ControllerBase {
   
   [HttpGet("people")]  
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<PersonDto>> GetAll() {
      var people = peopleRepository.SelectAll();
      return Ok(people.Select(p => p.ToPersonDto()));
   }
   
   [HttpGet("people/{id:guid}")]
   [ProducesResponseType<PersonDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<PersonDto> GetById(
      [FromRoute] Guid id
   ) {
      // switch(personRepository.FindById(id)) {
      //    case Person person:
      //       return Ok(person);
      //    case null:
      //       return NotFound("Owner with given Id not found");
      // };
      return peopleRepository.FindById(id) switch {
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Person with given id not found")
      };
   }
   
   [HttpGet("people/name")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<PersonDto> GetByName(
      [FromQuery] string name
   ) {
      return peopleRepository.FindByName(name) switch {
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Person with given name not found")
      };
   }
 
   [HttpGet("people/email")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<PersonDto> GetByEmail( 
      [FromQuery] string email
   ) {
      return peopleRepository.FindByEmail(email) switch {
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Person with given email not found")
      };
   }

   [HttpPost("people")]
   [ProducesResponseType(StatusCodes.Status201Created)]
   public ActionResult<PersonDto> Create(
      [FromBody] PersonDto personDto
   ) {
      if(peopleRepository.FindById(personDto.Id) != null) 
         BadRequest("Person with given id already exists"); 
      
      // map dto to entity
      var person = personDto.ToPerson();
      
      // add person to repository and save changes
      peopleRepository.Add(person);
      dataContext.SaveAllChanges();
      
      return Created($"/people/{person.Id}", person.ToPersonDto());
   }
   
   [HttpPut("people/{id}")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   public ActionResult<PersonDto> Update(
      [FromRoute] Guid id,
      [FromBody] PersonDto updPersonDto
   ) {
      // find person in the repository
      var person = peopleRepository.FindById(id);
      if (person == null) return NotFound("Person with given id not found");
      
      // map dto to entity
      var updPerson = updPersonDto.ToPerson();
      // update person in the domain model
      person.Update(updPerson);
      
      // update person in the repository and save changes
      peopleRepository.Update(person);
      dataContext.SaveAllChanges();
      
      return Ok(person.ToPersonDto());
   }

   
   [HttpDelete("people/{id}")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
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