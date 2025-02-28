using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core;

public interface ICarRepository {
   ICollection<Car> SelectAll();
   Car? FindById(Guid id);
   void Add(Car car);
   void Update(Car car);
   void Remove(Car car);

   ICollection<Car> SelectByPersonId(Guid personId);
   ICollection<Car> SelectByMaker(string maker);
   ICollection<Car> SelectByAttributes(
      string? maker, string? model, int? year, double? price);

}