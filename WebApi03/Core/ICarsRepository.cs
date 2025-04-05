using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface ICarsRepository: IBaseRepository<Car> {
   //Car? FindById(Guid id);
   //IEnumerable<Car>? SelectAll();
   //void Add(Car car);
   //void AddRange(IEnumerable<Car> cars);
   //void Update(Car updCar);
   //void Remove(Car car);
   
   IEnumerable<Car>? SelectByAttributes(
      string? maker, string? model, int? yearMin, int? yearMax, 
      decimal? priceMin, decimal? priceMax);
   
   IEnumerable<Car>? SelectCarsByPersonId(Guid personId);
   
}