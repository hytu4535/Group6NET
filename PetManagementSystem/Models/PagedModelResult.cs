namespace PetManagementSystem.Models;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems);