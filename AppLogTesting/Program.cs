using AppLogTesting;
using Central.Logging.Abstractions;
using Central.Logging.Core.Extensions;
using Central.Logging.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add Extended logging
builder.Services.AddCentralLogging();

builder.Services.AddCentralHttpLogging();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Add HTTP logging middleware early in the pipeline
app.UseCentralHttpLogging();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Sample endpoints
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.MapPost(
    "/api/users",
    async ([FromBody] CreateUserRequest request, IBaseLogger logger) =>
    {
        logger.LogInformation("UserManagement", "Creating new user: {Email}", request.Email);

        try
        {
            // Simulate some business logic
            await Task.Delay(100);

            if (string.IsNullOrEmpty(request.Email))
            {
                logger.LogWarning("Validation", "User creation failed: Email is required");
                return Results.BadRequest(new { Error = "Email is required" });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
            };

            logger.LogInformation("UserManagement", "User created successfully: {UserId}", user.Id);
            return Results.Created($"/api/users/{user.Id}", user);
        }
        catch (Exception ex)
        {
            logger.LogError("UserManagement", ex, "Failed to create user: {Email}", request.Email);
            return Results.Problem("An error occurred while creating the user");
        }
    }
);

app.MapGet(
    "/api/users/{id:guid}",
    async (Guid id, IBaseLogger logger) =>
    {
        logger.LogInformation("UserManagement", "Retrieving user: {UserId}", id);

        try
        {
            // Simulate database lookup
            await Task.Delay(50);

            if (id == Guid.Empty)
            {
                logger.LogWarning("UserManagement", "User not found: {UserId}", id);
                return Results.NotFound(new { Error = "User not found" });
            }

            var user = new User
            {
                Id = id,
                Email = "user@example.com",
                Name = "John Doe",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
            };

            logger.LogInformation("UserManagement", "User retrieved successfully: {UserId}", id);
            return Results.Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError("UserManagement", ex, "Failed to retrieve user: {UserId}", id);
            return Results.Problem("An error occurred while retrieving the user");
        }
    }
);

app.MapPost(
    "/api/error",
    (IBaseLogger logger) =>
    {
        logger.LogWarning("Testing", "Intentionally throwing an exception for testing");
        throw new InvalidOperationException("This is a test exception");
    }
);

app.Run();
