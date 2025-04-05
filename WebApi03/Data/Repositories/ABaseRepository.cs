using Microsoft.EntityFrameworkCore;
using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories_refactored;

public abstract class ABaseRepository<T>(
   DataContext dataContext
) where T : AEntity {
   
   // fields
   protected readonly DbSet<T> _dbSet = dataContext.Set<T>();
   
   // virtual methods, can be overridden in derived classes
   public virtual T? FindById(Guid id) {
      var entity = _dbSet.Find(id);
      dataContext.LogChangeTracker($"{typeof(T).Name}: FindById");
      return entity;
   }

   public virtual IEnumerable<T> SelectAll() {
      var entities = _dbSet.ToList();
      dataContext.LogChangeTracker($"{typeof(T).Name}: SelectAll");
      return entities;
   }

   public virtual void Add(T entity) =>
      _dbSet.Add(entity);

   public virtual void AddRange(IEnumerable<T> entities) =>
      _dbSet.AddRange(entities);

   // Update 
   public virtual void Update(T entity) {
      var entry = dataContext.Entry(entity);
      if (entry == null)
         throw new ApplicationException($"Update failed, entity with given id not found");
      if (entry.State == EntityState.Detached) _dbSet.Attach(entity);
      entry.State = EntityState.Modified;
   }
   
   public void Remove(T entity) {
      var entry = dataContext.Entry(entity);
      if (entry == null) throw new Exception($"{typeof(T).Name} to be removed not found");
      if (entry.State == EntityState.Detached) _dbSet.Attach(entity);
      entry.State = EntityState.Deleted;
   }
}