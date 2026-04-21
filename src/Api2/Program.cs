var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api2" }));

// Sample domain endpoint — Orders service
app.MapGet("/orders", () => Results.Ok(new[]
{
    new { id = 101, product = "Widget A", quantity = 2, status = "shipped" },
    new { id = 102, product = "Widget B", quantity = 1, status = "pending" },
}));

app.MapGet("/orders/{id:int}", (int id) =>
    id > 0
        ? Results.Ok(new { id, product = "Widget A", quantity = 1, status = "processing" })
        : Results.NotFound());

app.MapPost("/orders", (dynamic order) =>
    Results.Created("/orders/103", new { id = 103, status = "created" }));

app.Run();
