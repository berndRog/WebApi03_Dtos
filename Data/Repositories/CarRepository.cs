using WebApi.Core;
using WebApi.Core.DomainModel.Entities;
namespace WebApi.Data.Repositories;
public class CarRepository(
   IDataContext dataContext
): ICarRepository {
   
   public ICollection<Car> SelectAll() =>
      dataContext.Cars;
   
   public Car? FindById(Guid id) =>
      dataContext.Cars.FirstOrDefault(car => car.Id == id);

   public void Add(Car car) =>
      dataContext.Cars.Add(car);
   
   public void Update(Car car) =>
      dataContext.Cars.Remove(car);

   public void Remove(Car car) =>
      dataContext.Cars.Remove(car);
   
   public ICollection<Car> SelectByPersonId(Guid personId) =>
      dataContext.People
         .Where(person => person.Id == personId)
         .SelectMany(person =>  person.Cars)
         .ToList();

   public ICollection<Car> SelectByMaker(string maker) =>
      dataContext.Cars
         .Where(car => car.Maker == maker)
         .ToList();

   public ICollection<Car> SelectByAttributes(
      string? maker = null, 
      string? model = null,
      int? year = null,
      double? price = null
   ) {
      var query = dataContext.Cars.AsQueryable();
   
      if (!string.IsNullOrEmpty(maker))
         query = query.Where(car => car.Maker == maker);
      if (!string.IsNullOrEmpty(model))
         query = query.Where(car => car.Model == model);
      if (year.HasValue)
         query = query.Where(car => car.Year == year.Value);
      if (price.HasValue)
         query = query.Where(car => car.Price == price.Value);
      return query.ToList();
   }
}