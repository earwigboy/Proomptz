using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PromptTemplateManager.Api.Middleware;
using PromptTemplateManager.Application.Services;
using PromptTemplateManager.Application.Validators;
using PromptTemplateManager.Core.Interfaces;
using PromptTemplateManager.Infrastructure.Data;
using PromptTemplateManager.Infrastructure.DevinIntegration;
using PromptTemplateManager.Infrastructure.Repositories;
using PromptTemplateManager.Infrastructure.FileSystem;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel server ports
// Priority: ASPNETCORE_URLS env var > PORT env var > appsettings.json
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add memory cache for template caching
builder.Services.AddMemoryCache();

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add health checks
builder.Services.AddHealthChecks();

// Configure SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure filesystem storage
var templateStoragePath = builder.Configuration.GetValue<string>("TemplateStoragePath") ?? "/data/templates";
var searchIndexPath = Path.Combine(Path.GetDirectoryName(templateStoragePath) ?? "/data", "search-index");

// Register FileSystemSearchEngine as singleton (shared index writer)
builder.Services.AddSingleton<FileSystemSearchEngine>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FileSystemSearchEngine>>();
    return new FileSystemSearchEngine(searchIndexPath, logger);
});

// Register repositories
// FileSystemTemplateRepository is registered as singleton to ensure FileSystemWatcher is not duplicated
builder.Services.AddSingleton<ITemplateRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FileSystemTemplateRepository>>();
    var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
    var searchEngine = sp.GetRequiredService<FileSystemSearchEngine>();
    return new FileSystemTemplateRepository(templateStoragePath, logger, cache, searchEngine);
});

// FileSystemFolderRepository is registered as singleton to share folder cache
builder.Services.AddSingleton<IFolderRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FileSystemFolderRepository>>();
    return new FileSystemFolderRepository(templateStoragePath, logger);
});

// Register services
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IPlaceholderService, PlaceholderService>();
builder.Services.AddScoped<IDevinClient, DevinClient>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateTemplateRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseResponseCompression();

// In production (containerized), serve static files and SPA fallback
// In development, CORS is used instead (frontend runs on separate Vite server)
if (!app.Environment.IsDevelopment())
{
    // Serve static files from wwwroot (for containerized frontend)
    app.UseStaticFiles();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// In production, use SPA fallback routing to serve index.html for non-API routes
// This allows React Router to handle client-side routing in the containerized app
if (!app.Environment.IsDevelopment())
{
    app.MapFallbackToFile("index.html");
}

// Apply database migrations (disabled - now using filesystem storage)
// Note: Database is kept for potential migration from SQLite to filesystem
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     dbContext.Database.Migrate();
// }

// Initialize template storage on startup
// Scan filesystem and rebuild search index from .md files
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var templateRepository = scope.ServiceProvider.GetRequiredService<ITemplateRepository>();
    var searchEngine = scope.ServiceProvider.GetRequiredService<FileSystemSearchEngine>();

    try
    {
        logger.LogInformation("Scanning filesystem for templates and rebuilding search index...");

        // Get all templates from filesystem
        var templates = (await templateRepository.GetAllAsync(null, 1, int.MaxValue, CancellationToken.None)).ToList();
        var totalCount = await templateRepository.GetCountAsync(null, CancellationToken.None);

        // Rebuild search index from all templates
        await searchEngine.RebuildIndexAsync(templates);

        // Warm cache with frequently accessed templates (most recently updated)
        var recentlyUpdated = templates
            .OrderByDescending(t => t.UpdatedAt)
            .Take(100) // Cache top 100 most recently updated templates
            .ToList();

        foreach (var template in recentlyUpdated)
        {
            // Access each template to populate the cache
            await templateRepository.GetByIdAsync(template.Id, CancellationToken.None);
        }

        logger.LogInformation("Successfully initialized template storage: {TemplateCount} templates indexed, {CachedCount} templates cached", totalCount, recentlyUpdated.Count);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize template storage on startup");
        // Don't fail application startup - templates can still be accessed, search may be degraded
    }
}

app.Run();
