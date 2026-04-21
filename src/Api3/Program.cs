var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api3" }));

// Sample domain endpoint — Notifications service
app.MapGet("/notifications", () => Results.Ok(new[]
{
    new { id = 1, type = "email", recipient = "user@example.com", status = "sent" },
    new { id = 2, type = "sms",   recipient = "+1234567890",       status = "pending" },
}));

app.MapPost("/notifications/send", (dynamic payload) =>
    Results.Accepted("/notifications/3", new { id = 3, status = "queued" }));

app.Run();
