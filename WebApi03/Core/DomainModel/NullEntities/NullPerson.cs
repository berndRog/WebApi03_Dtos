using WebApi.Core.DomainModel.Entities;
namespace WebApi.Core.DomainModel.NullEntities;
// https://jonskeet.uk/csharp/singleton.html

public sealed class NullPerson: Person {  
   // Singleton Skeet Version 4
   public static NullPerson Instance { get; } = new ();
   static NullPerson() { }
   private NullPerson() { }
}