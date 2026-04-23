using Api4;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient("jsonplaceholder", c =>
{
    c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api4", version = "1.0.0" }));

app.MapGet("/users", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var raw = await client.GetFromJsonAsync<JsonPlaceholderUser[]>("/users");
    if (raw is null) return Results.Problem("Failed to fetch upstream data");
    return Results.Ok(raw.Select(u => UserTransformer.Transform(u)));
});

app.MapGet("/users/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var u = await client.GetFromJsonAsync<JsonPlaceholderUser>($"/users/{id}");
    if (u is null) return Results.NotFound();
    return Results.Ok(UserTransformer.TransformDetail(u));
});

app.Run();
