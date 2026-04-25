using Api5;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient("jsonplaceholder", c =>
{
    c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api5", version = "1.3.0" }));

app.MapGet("/posts", async (IHttpClientFactory factory, int? userId) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var url = userId.HasValue ? $"/posts?userId={userId}" : "/posts";
    var raw = await client.GetFromJsonAsync<JsonPlaceholderPost[]>(url);
    if (raw is null) return Results.Problem("Failed to fetch upstream data");
    return Results.Ok(raw.Select(p => PostTransformer.Transform(p)));
});

app.MapGet("/posts/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var p = await client.GetFromJsonAsync<JsonPlaceholderPost>($"/posts/{id}");
    if (p is null) return Results.NotFound();

    if (id == 4)
        return Results.Ok(new
        {
            id          = p.Id,
            userId      = p.UserId,
            title       = "nandi custom change",
            summary     = PostTransformer.Truncate(p.Body, 100),
            wordCount   = PostTransformer.CountWords(p.Body),
            readTimeSec = PostTransformer.CalcReadTime(p.Body),
            postUrl     = PostTransformer.BuildPostUrl(p.Id)
        });

    return Results.Ok(PostTransformer.Transform(p));
});

app.MapGet("/posts/{id:int}/comments", async (int id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var comments = await client.GetFromJsonAsync<JsonPlaceholderComment[]>($"/posts/{id}/comments");
    if (comments is null) return Results.NotFound();
    return Results.Ok(comments.Select(c => PostTransformer.TransformComment(c)));
});

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
