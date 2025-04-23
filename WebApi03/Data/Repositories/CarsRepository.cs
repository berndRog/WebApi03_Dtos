using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories_refactored;

public class CarsRepository(
   DataContext dataContext
) : ABaseRepository<Car>(dataContext), ICarsRepository {
   private readonly DataContext _dataContext = dataContext;

   // inherited from ABaseRepository<T>
   // protected readonly DbSet<T> _dbSet
   // public virtual T? FindById(Guid id)
   // public virtual IEnumerable<T>? SelectAll()
   // public virtual void Add(T entity) 
   // public virtual void AddRange(IEnumerable<T> entities) 
   // public virtual void Update(T entity) 
   // public virtual void Remove(T entity) 
   
   public  IEnumerable<Car> SelectByAttributes(
      string? maker = null, 
      string? model = null,
      int? yearMin = null,
      int? yearMax = null,
      decimal? priceMin = null,
      decimal? priceMax = null
   ) {
      var query = _dbSet.AsQueryable();
   
      if (!string.IsNullOrEmpty(maker))
         query = query.Where(car => car.Maker == maker);
      if (!string.IsNullOrEmpty(model))
         query = query.Where(car => car.Model == model);
      if (yearMin.HasValue)
         query = query.Where(car => car.Year >= yearMin.Value);
      if (yearMax.HasValue)
         query = query.Where(car => car.Year <= yearMax.Value);
      if (priceMin.HasValue)
         query = query.Where(car => car.Price >= priceMin.Value);
      if (priceMax.HasValue)
         query = query.Where(car => car.Price <= priceMax.Value);

      var cars = query.ToList();
      _dataContext.LogChangeTracker("Car: SelectByAttributes");
      return cars;
   }


   public IEnumerable<Car> SelectByPersonId(Guid personId) {
      var cars = _dbSet
         .Where(car => car.PersonId == personId)
         .ToList();
      _dataContext.LogChangeTracker("Car: SelectCarsByPersonId");
      return cars;
   }
}