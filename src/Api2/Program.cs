var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api2", version = "1.1.0" }));

// Sample domain endpoint — Orders service
app.MapGet("/orders", () => Results.Ok(new[]
{
    new { id = 101, product = "Widget A", quantity = 2, status = "shipped",    total = 19.98 },
    new { id = 102, product = "Widget B", quantity = 1, status = "pending",    total = 19.99 },
    new { id = 103, product = "Widget Pro", quantity = 1, status = "delivered", total = 49.99 },
}));

app.MapGet("/orders/{id:int}", (int id) =>
    id > 0
        ? Results.Ok(new { id, product = "Widget A", quantity = 1, status = "processing", total = 9.99 })
        : Results.NotFound());

app.MapPost("/orders", (dynamic order) =>
    Results.Created("/orders/104", new { id = 104, status = "created" }));

app.MapGet("/orders/status", () => Results.Ok(new[] { "pending", "processing", "shipped", "delivered", "cancelled" }));

app.Run();
