namespace WebApi.Core.Dtos;

public record CarDto(
   Guid Id,
   string Maker,
   string Model,
   int Year,
   double Price,
   string? ImageUrl,
   Guid? PersonId
);