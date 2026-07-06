using MongoDB.Driver;
using Refit;
using Scalar.AspNetCore;
using TechFeed.API.Endpoints;
using TechFeed.Core;
using TechFeed.Infrastructure.Configuration;
using TechFeed.Infrastructure.ExternalApis.DevTo;
using TechFeed.Infrastructure.ExternalApis.HackerNews;
using TechFeed.Infrastructure.Persistence;
using TechFeed.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// MongoDB configuration and data-access layer.
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(nameof(MongoDbSettings)));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<IArticleRepository, MongoArticleRepository>();

// External article providers (Refit typed clients).
builder.Services
    .AddRefitClient<IDevToClient>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://dev.to");
        // dev.to rejects requests without a User-Agent as "Forbidden Bots" (403).
        c.DefaultRequestHeaders.UserAgent.ParseAdd("TechFeed/1.0");
    });

builder.Services
    .AddRefitClient<IHackerNewsClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://hacker-news.firebaseio.com"));

// Register both source clients under the common contract so consumers can
// inject IEnumerable<IArticleSourceClient> and get every provider at once.
builder.Services.AddTransient<IArticleSourceClient, DevToSourceClient>();
builder.Services.AddTransient<IArticleSourceClient, HackerNewsSourceClient>();

// Feed refresh use case (combines the source clients with the repository).
builder.Services.AddScoped<IFeedRefreshService, FeedRefreshService>();

var app = builder.Build();

// Eagerly resolve the repository so its unique index on (ExternalId, Source)
// is created at startup rather than on first use.
app.Services.GetRequiredService<IArticleRepository>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapFeedEndpoints();

app.Run();
