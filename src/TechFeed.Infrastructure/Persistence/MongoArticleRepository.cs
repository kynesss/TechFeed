using MongoDB.Bson;
using MongoDB.Driver;
using TechFeed.Core;

namespace TechFeed.Infrastructure.Persistence;

public class MongoArticleRepository : IArticleRepository
{
    private const string CollectionName = "articles";

    private readonly IMongoCollection<ArticleDocument> _collection;

    public MongoArticleRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ArticleDocument>(CollectionName);
        EnsureIndexes();
    }

    public async Task<Article?> GetByIdAsync(string id)
    {
        // Guard against ids that are not valid ObjectIds — the BSON
        // serializer would otherwise throw while building the filter.
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        var filter = Builders<ArticleDocument>.Filter.Eq(x => x.Id, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        return document?.ToDomain();
    }

    public async Task<List<Article>> GetAllAsync(string? tag, string? source, int limit)
    {
        var documents = await _collection
            .Find(BuildFilter(tag, source))
            .SortByDescending(x => x.PublishedAt)
            .Limit(limit)
            .ToListAsync();

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<int> CountAsync(string? tag, string? source)
    {
        var count = await _collection.CountDocumentsAsync(BuildFilter(tag, source));

        // CountDocuments returns a long; article counts comfortably fit an int.
        return (int)count;
    }

    private static FilterDefinition<ArticleDocument> BuildFilter(string? tag, string? source)
    {
        var builder = Builders<ArticleDocument>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(tag))
        {
            filter &= builder.AnyEq(x => x.Tags, tag);
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            filter &= builder.Eq(x => x.Source, source);
        }

        return filter;
    }

    public async Task UpsertAsync(Article article)
    {
        var document = article.ToDocument();

        // Match on the natural key (ExternalId + Source). Let Mongo own the
        // _id: clear it so an insert generates a fresh ObjectId and a replace
        // keeps the existing (immutable) _id.
        document.Id = null;

        var filter = Builders<ArticleDocument>.Filter.And(
            Builders<ArticleDocument>.Filter.Eq(x => x.ExternalId, article.ExternalId),
            Builders<ArticleDocument>.Filter.Eq(x => x.Source, article.Source));

        await _collection.ReplaceOneAsync(
            filter,
            document,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<bool> ExistsAsync(string externalId, string source)
    {
        var filter = Builders<ArticleDocument>.Filter.And(
            Builders<ArticleDocument>.Filter.Eq(x => x.ExternalId, externalId),
            Builders<ArticleDocument>.Filter.Eq(x => x.Source, source));

        return await _collection.Find(filter).AnyAsync();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<ArticleDocument>.IndexKeys
            .Ascending(x => x.ExternalId)
            .Ascending(x => x.Source);

        var indexModel = new CreateIndexModel<ArticleDocument>(
            indexKeys,
            new CreateIndexOptions { Unique = true, Name = "ux_externalId_source" });

        _collection.Indexes.CreateOne(indexModel);
    }
}
