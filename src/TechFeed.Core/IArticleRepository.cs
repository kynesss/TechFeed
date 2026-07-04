namespace TechFeed.Core;

public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(string id);

    Task<List<Article>> GetAllAsync(string? tag, string? source, int limit);

    Task UpsertAsync(Article article);

    Task<bool> ExistsAsync(string externalId, string source);
}
