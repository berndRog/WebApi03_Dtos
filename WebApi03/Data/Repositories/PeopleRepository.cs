using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories_refactored;

public class PeopleRepository(
   DataContext dataContext
) : ABaseRepository<Person>(dataContext), IPeopleRepository {
   private readonly DataContext _dataContext = dataContext;

   // inherited from ABaseRepository<T>
   // protected readonly DbSet<T> _dbSet
   // public virtual T? FindById(Guid id)
   // public virtual IEnumerable<T>? SelectAll()
   // public virtual void Add(T entity) 
   // public virtual void AddRange(IEnumerable<T> entities) 
   // public virtual void Update(T entity) 
   // public virtual void Remove(T entity) 
   
   public IEnumerable<Person>? SelectByName(string namePattern) {
      if (string.IsNullOrWhiteSpace(namePattern))
         return null;
      var people = _dbSet
         .Where(person => EF.Functions.Like(person.LastName, $"%{namePattern.Trim()}%"))
         .ToList();
      _dataContext.LogChangeTracker("Person: FindByNamePattern");
      return people;
   }
   
   public Person? FindByIdJoinCars(Guid id) {
      var person = _dbSet
         .Where(person => person.Id == id)
         .Include(person => person.Cars)
         .FirstOrDefault();
      _dataContext.LogChangeTracker("Person: FindByIdJoinCars");
      return person;
   }
   
}