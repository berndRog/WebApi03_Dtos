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
[Consumes("application/json","appliation/xml")] //default
[Produces("application/json", "appliation/xml")] //default
public class PeopleController(
   IPeopleRepository peopleRepository,
   IDataContext dataContext
   //ILogger<PersonController> logger
) : ControllerBase {
   
   // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-9.0&tabs=controller

   /// <summary>
   /// Get all people
   /// </summary>
   [HttpGet("people")]  
   [EndpointSummary("Get all people")] 
   [ProducesResponseType(StatusCodes.Status200OK)]
   public ActionResult<IEnumerable<PersonDto>> GetAll() {
      var people = peopleRepository.SelectAll();
      return Ok(people.Select(p => p.ToPersonDto()));
   }
 
   /// <summary>
   /// Get person by id
   /// </summary>
   /// <param name="id">Unique id of the person to be found</param>
   /// <returns></returns>
   [HttpGet("people/{id:guid}")]
   [EndpointSummary("Get person by id")]
   [ProducesResponseType<PersonDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<PersonDto> GetById(
      [Description("Unique id of the person to be found")]
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
   
   /// <summary>
   /// Get person by name
   /// </summary>
   /// <param name="name">Name to be search for</param>
   [HttpGet("people/name")]
   [EndpointSummary("Get person by name")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<PersonDto> GetByName(
      [Description("Name to be search for")]
      [FromQuery] string name
   ) {
      return peopleRepository.FindByName(name) switch {
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Person with given name not found")
      };
   }
 
   /// <summary>
   /// Get person by email 
   /// </summary>
   /// <param name="email">Email to be search for</param>
   [HttpGet("people/email")]
   [EndpointSummary("Get person by email")]
   public ActionResult<PersonDto> GetByEmail( 
      [Description("Email to be search for")]
      [FromQuery] string email
   ) {
      return peopleRepository.FindByEmail(email) switch {
         Person person => Ok(person.ToPersonDto()),
         null => NotFound("Person with given email not found")
      };
   }

   /// <summary>
   /// Create a new person
   /// </summary>
   /// <param name="personDto">PersonDto with the new person's data</param> 
   [HttpPost("people")]
   [EndpointSummary("Create a new person")]
   [ProducesResponseType(StatusCodes.Status201Created)]
   public ActionResult<PersonDto> Create(
      [Description("PersonDto with the new person's data")]
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
 
   /// <summary>
   /// Update a person
   /// </summary>
   /// <param name="id">Unique id of the person to be updated</param>
   /// <param name="updPersonDto">PersonDto with the updated person's data</param>
   [HttpPut("people/{id}")]
   [EndpointSummary("Update a person")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   public ActionResult<PersonDto> Update(
      [Description("Unique id of the existing person")]
      [FromRoute] Guid id,
      [Description("PersonDto with the updated person's data")]
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

   
   /// <summary>
   /// Delete a given person
   /// </summary>
   /// <param name="id">Unique id of the person to delete</param>
   [HttpDelete("people/{id}")]
   [EndpointSummary("Delete a person")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public IActionResult Delete(
      [Description("Unique id of the existing person")]
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