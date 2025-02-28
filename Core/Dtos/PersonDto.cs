namespace WebApi.Core.Dtos;

// Immutable Data Transfer Object (DTO) for Person
public record PersonDto(
   Guid Id,
   string FirstName,
   string LastName,
   string? Email,
   string? Phone
);