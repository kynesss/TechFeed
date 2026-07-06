namespace TechFeed.Shared;

public record PagedResponse<T>(List<T> Items, int TotalCount, int Limit);
