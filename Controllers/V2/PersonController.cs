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
public class PersonController(
   ControllerHelper helper,
   IPersonRepository personRepository,
   IDataContext dataContext
   //ILogger<PersonController> logger
) : ControllerBase {
   
   // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-9.0&tabs=controllers
   
   [HttpGet("people")]  
   [EndpointSummary("Get all people")] 
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<PersonDto>> GetAll() {
      var people = personRepository.SelectAll();
      return Ok(people.Select(p => p.ToPersonDto()));
   }
   
   [HttpGet("people/{id:guid}")]
   [EndpointSummary("Get person by id")]
   [ProducesResponseType<PersonDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<PersonDto> GetById(
      [Description("id of the person to be found")]
      [FromRoute] Guid id
   ) {
      // switch(personRepository.FindById(id)) {
      //    case Person person:
      //       return Ok(person);
      //    case null:
      //       return NotFound("Owner with given Id not found");
      // };
      return personRepository.FindById(id) switch {
         Person person => Ok(person.ToPersonDto()),
         null => helper.DetailsNotFound<PersonDto>("Person with given id not found")
      };
   }
   
   // Get person by name http://localhost:5200/carshop/people/name?name={name}
   /// <summary>
   /// Get person by name
   /// </summary>
   /// <param name="name">name to be search for</param>
   /// <returns>found person</returns>
   [HttpGet("people/name")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   public ActionResult<PersonDto> GetByName(
      [FromQuery] string name
   ) {
      // switch(personRepository.FindById(id)) {
      //    case Person person:
      //       return Ok(person);
      //    case null:
      //       return NotFound("Owner with given Id not found");
      // };
      return personRepository.FindByName(name) switch {
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Owner with given name not found")
      };
   }
   
   // Get person by email http://localhost:5200/carshop/people/email?email={email}
   /// <summary>
   /// Get person by email 
   /// </summary>
   /// <param name="email">email address to be search for</param>
   /// <returns>found person</returns>
   [HttpGet("people/email")]
   public ActionResult<PersonDto> GetByEmail(
      [FromQuery] string email
   ) {
      return personRepository.FindByEmail(email) switch {
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Owner with given EMail not found")
      };
   }
   
   /// <summary>
   /// Create a new person
   /// </summary>
   /// <param name="personDto">dto with the new person's data</param>
   /// <returns>new created dto</returns>
   [HttpPost("people")]  
   [ProducesResponseType(StatusCodes.Status201Created)]
   public ActionResult<PersonDto> Create(
      [FromBody] PersonDto personDto
   ) {
      var person = personDto.ToPerson();
      personRepository.Add(person);
      dataContext.SaveChanges();
      return Created($"/people/{person.Id}", person.ToPersonDto());
   }
   
   // Update a person   http://localhost:5100/people/{id}
   /// <summary>
   /// Update a person
   /// </summary>
   /// <param name="id">id of the person to be updated</param>
   /// <param name="updPersonDto">dto with the updated person's data</param>
   /// <returns>dto of the updated person</returns>
   [HttpPut("people/{id}")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   public ActionResult<PersonDto> Update(
      Guid id,
      [FromBody] PersonDto updPersonDto
   ) {
      var person = personRepository.FindById(id);
      if (person == null) {
         return NotFound();
      }
      var updPerson = updPersonDto.ToPerson();
      person.Update(updPerson);
      personRepository.Update(person);
      dataContext.SaveChanges();
      return Ok(person.ToPersonDto());
   }

   // Delete a person   http://localhost:5100/people/{id}
   /// <summary>
   /// 
   /// </summary>
   /// <param name="id"></param>
   /// <returns></returns>
   [HttpDelete("people/{id}")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public IActionResult Delete(Guid id) {
      var person = personRepository.FindById(id);
      if (person == null) {
         return NotFound();
      }
      personRepository.Remove(person);
      dataContext.SaveChanges();
      return NoContent();
   }
}