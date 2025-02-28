using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories;

public class PersonRepository(
   IDataContext dataContext   
): IPersonRepository {

   public Person? FindById(Guid id) =>
      dataContext.People.FirstOrDefault(person => person.Id == id);

   public ICollection<Person> SelectAll() => dataContext.People;
   
   public Person? FindByEmail(string email) =>
      dataContext.People.FirstOrDefault(person => person.Email == email);
   
   public Person? FindByName(string name) {
      var tokens = name.Trim().Split(" ");
      var firstName = string.Join(" ", tokens.Take(tokens.Length - 1));
      var lastName = tokens.Last();
      return dataContext.People.FirstOrDefault(person =>
         person.FirstName == firstName && person.LastName == lastName);
   }

   public void Add(Person person) =>
      dataContext.People.Add(person);

   public void Update(Person updPerson) {
      var person = dataContext.People.FirstOrDefault(person => 
         person.Id == updPerson.Id);
      if (person == null) return;
      
      person.Update(updPerson);
   }

   public void Remove(Person person) =>
      dataContext.People.Remove(person);
}