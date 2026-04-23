// Api5 — Posts Service
// Fetches raw post data from JSONPlaceholder (https://jsonplaceholder.typicode.com/posts)
// Transforms it: truncates body, adds word count, adds read time, adds post URL.

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient("jsonplaceholder", c =>
{
    c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api5", version = "1.0.0" }));

// GET /posts — fetch from JSONPlaceholder and transform
app.MapGet("/posts", async (IHttpClientFactory factory, int? userId) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var url = userId.HasValue ? $"/posts?userId={userId}" : "/posts";
    var raw = await client.GetFromJsonAsync<JsonPlaceholderPost[]>(url);

    if (raw is null) return Results.Problem("Failed to fetch upstream data");

    var transformed = raw.Select(p => Transform(p));
    return Results.Ok(transformed);
});

// GET /posts/{id}
app.MapGet("/posts/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var p = await client.GetFromJsonAsync<JsonPlaceholderPost>($"/posts/{id}");

    if (p is null) return Results.NotFound();

    // Custom override for post 4
    var transformed = Transform(p);
    if (id == 4)
    {
        return Results.Ok(new
        {
            id          = p.Id,
            userId      = p.UserId,
            title       = "nandi custom change",
            summary     = p.Body.Length > 100 ? p.Body[..100] + "..." : p.Body,
            wordCount   = p.Body.Split(' ').Length,
            readTimeSec = (int)Math.Ceiling(p.Body.Split(' ').Length / 3.0),
            postUrl     = $"https://myapp.com/posts/{p.Id}"
        });
    }

    return Results.Ok(transformed);
});

// GET /posts/{id}/comments — fetch comments for a post
app.MapGet("/posts/{id:int}/comments", async (int id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var comments = await client.GetFromJsonAsync<JsonPlaceholderComment[]>($"/posts/{id}/comments");

    if (comments is null) return Results.NotFound();

    // Transform: mask email partially
    var transformed = comments.Select(c => new
    {
        id      = c.Id,
        name    = c.Name,
        email   = MaskEmail(c.Email),
        preview = c.Body.Length > 80 ? c.Body[..80] + "..." : c.Body
    });

    return Results.Ok(transformed);
});

app.Run();

// ── Transformation helpers ────────────────────────────────────────────────────
static object Transform(JsonPlaceholderPost p)
{
    var wordCount = p.Body.Split(' ').Length;
    return new
    {
        id          = p.Id,
        userId      = p.UserId,
        title       = ToTitleCase(p.Title),
        summary     = p.Body.Length > 100 ? p.Body[..100] + "..." : p.Body,
        wordCount   = wordCount,
        readTimeSec = (int)Math.Ceiling(wordCount / 3.0),   // ~180 wpm
        postUrl     = $"https://myapp.com/posts/{p.Id}"
    };
}

static string ToTitleCase(string s) =>
    string.Join(' ', s.Split(' ').Select(w => w.Length > 0
        ? char.ToUpper(w[0]) + w[1..]
        : w));

static string MaskEmail(string email)
{
    var parts = email.Split('@');
    if (parts.Length != 2) return email;
    var name = parts[0];
    var masked = name.Length > 2 ? name[..2] + new string('*', name.Length - 2) : name;
    return $"{masked}@{parts[1]}";
}

// ── Upstream models ───────────────────────────────────────────────────────────
record JsonPlaceholderPost(int UserId, int Id, string Title, string Body);
record JsonPlaceholderComment(int PostId, int Id, string Name, string Email, string Body);
