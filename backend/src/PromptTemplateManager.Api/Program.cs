using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PromptTemplateManager.Api.Middleware;
using PromptTemplateManager.Application.Services;
using PromptTemplateManager.Application.Validators;
using PromptTemplateManager.Core.Interfaces;
using PromptTemplateManager.Infrastructure.Data;
using PromptTemplateManager.Infrastructure.DevinIntegration;
using PromptTemplateManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Register repositories
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IFolderRepository, FolderRepository>();

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

app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
