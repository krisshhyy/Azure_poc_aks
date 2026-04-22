var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Api3", version = "1.1.0" }));

// Sample domain endpoint — Notifications service
app.MapGet("/notifications", () => Results.Ok(new[]
{
    new { id = 1, type = "email", recipient = "user@example.com",  status = "sent",    sentAt = "2026-04-22T10:00:00Z" },
    new { id = 2, type = "sms",   recipient = "+1234567890",        status = "pending", sentAt = (string?)null },
    new { id = 3, type = "push",  recipient = "device-token-xyz",   status = "failed",  sentAt = (string?)null },
}));

app.MapPost("/notifications/send", (dynamic payload) =>
    Results.Accepted("/notifications/4", new { id = 4, status = "queued" }));

app.MapGet("/notifications/types", () => Results.Ok(new[] { "email", "sms", "push" }));

app.Run();
