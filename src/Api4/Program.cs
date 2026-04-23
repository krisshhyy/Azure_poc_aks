// Api4 — Users Service
// Fetches raw user data from JSONPlaceholder (https://jsonplaceholder.typicode.com/users)
// Transforms it into a simplified, domain-specific shape before returning.

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient("jsonplaceholder", c =>
{
    c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api4", version = "1.0.0" }));

// GET /users — fetch from JSONPlaceholder and transform
app.MapGet("/users", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var raw = await client.GetFromJsonAsync<JsonPlaceholderUser[]>("/users");

    if (raw is null) return Results.Problem("Failed to fetch upstream data");

    // Transform: flatten address, drop unused fields, add computed field
    var transformed = raw.Select(u => new
    {
        id          = u.Id,
        name        = u.Name,
        email       = u.Email.ToLower(),
        phone       = u.Phone.Split(' ')[0],   // strip extensions
        city        = u.Address.City,
        company     = u.Company.Name,
        profileUrl  = $"https://myapp.com/users/{u.Id}"
    });

    return Results.Ok(transformed);
});

// GET /users/{id} — single user transformed
app.MapGet("/users/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("jsonplaceholder");
    var u = await client.GetFromJsonAsync<JsonPlaceholderUser>($"/users/{id}");

    if (u is null) return Results.NotFound();

    return Results.Ok(new
    {
        id         = u.Id,
        name       = u.Name,
        email      = u.Email.ToLower(),
        phone      = u.Phone.Split(' ')[0],
        city       = u.Address.City,
        zipCode    = u.Address.Zipcode,
        company    = u.Company.Name,
        website    = u.Website,
        profileUrl = $"https://myapp.com/users/{u.Id}"
    });
});

app.Run();

// ── Upstream model (JSONPlaceholder shape) ────────────────────────────────────
record JsonPlaceholderUser(
    int    Id,
    string Name,
    string Username,
    string Email,
    string Phone,
    string Website,
    Address Address,
    Company Company
);
record Address(string Street, string Suite, string City, string Zipcode);
record Company(string Name, string CatchPhrase, string Bs);
