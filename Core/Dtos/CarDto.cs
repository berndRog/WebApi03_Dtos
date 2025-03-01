using System.ComponentModel;
namespace WebApi.Core.Dtos;

public record CarDto(
   [Description("unique identifier of the car")]
   Guid Id,
   [Description("maker of the car")]
   string Maker,
   [Description("model of the car")]
   string Model,
   [Description("first registration of the car")]
   int Year,
   [Description("price of the car")]
   double Price,
   [Description("image URL of the car")]
   string? ImageUrl,
   [Description("unique identifier of the car's owner")]
   Guid? PersonId
);