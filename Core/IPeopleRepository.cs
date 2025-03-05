using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface IPeopleRepository {
   Person? FindById(Guid id);
   Person? FindByEmail(string email);
   Person? FindByName(string name);
   ICollection<Person> SelectAll();
   
   void Add(Person person);
   void Update(Person updPerson);
   void Remove(Person person);
}