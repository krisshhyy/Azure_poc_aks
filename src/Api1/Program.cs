var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Health check endpoint — used by K8s readiness probe
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api1" }));

// Sample domain endpoint
app.MapGet("/products", () => Results.Ok(new[]
{
    new { id = 1, name = "Widget A", price = 9.99 },
    new { id = 2, name = "Widget B", price = 19.99 },
}));

app.MapGet("/products/{id:int}", (int id) =>
    id > 0
        ? Results.Ok(new { id, name = $"Widget {id}", price = id * 9.99 })
        : Results.NotFound());

app.Run();
