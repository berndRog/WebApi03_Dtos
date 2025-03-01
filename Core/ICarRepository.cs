using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface ICarRepository {
   ICollection<Car> SelectAll();
   ICollection<Car> SelectByAttributes(
      string? maker, string? model, int? yearMin, int? yearMax, 
      double? priceMin, double? priceMax);
   ICollection<Car> SelectByPersonId(Guid personId);
   
   Car? FindById(Guid id);
   void Add(Car car);
   void Update(Car car);
   void Remove(Car car);
}