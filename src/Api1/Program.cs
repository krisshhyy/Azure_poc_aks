var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Health check endpoint — used by K8s readiness probe
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api1", version = "1.3.0" }));

// Sample domain endpoint
app.MapGet("/products", () => Results.Ok(new[]
{
    new { id = 1, name = "Widget A", price = 9.99,  category = "basic" },
    new { id = 2, name = "Widget B", price = 19.99, category = "basic" },
    new { id = 3, name = "Widget Pro", price = 49.99, category = "premium" },
}));

app.MapGet("/products/{id:int}", (int id) =>
    id > 0
        ? Results.Ok(new { id, name = $"Widget {id}", price = id * 9.99, category = "basic" })
        : Results.NotFound());

app.MapGet("/products/categories", () => Results.Ok(new[] { "basic", "premium" }));

app.Run();
