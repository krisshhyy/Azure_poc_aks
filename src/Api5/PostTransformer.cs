using System.Diagnostics.CodeAnalysis;

namespace Api5;

public static class PostTransformer
{
    public static object Transform(JsonPlaceholderPost p) => new
    {
        id          = p.Id,
        userId      = p.UserId,
        title       = ToTitleCase(p.Title),
        summary     = Truncate(p.Body, 100),
        wordCount   = CountWords(p.Body),
        readTimeSec = CalcReadTime(p.Body),
        postUrl     = BuildPostUrl(p.Id)
    };

    public static object TransformComment(JsonPlaceholderComment c) => new
    {
        id      = c.Id,
        name    = c.Name,
        email   = MaskEmail(c.Email),
        preview = Truncate(c.Body, 80)
    };

    public static string ToTitleCase(string s) =>
        string.Join(' ', s.Split(' ').Select(w =>
            w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));

    public static string Truncate(string s, int max) =>
        s.Length > max ? s[..max] + "..." : s;

    public static int CountWords(string s) =>
        s.Split(' ').Length;

    public static int CalcReadTime(string s) =>
        (int)Math.Ceiling(CountWords(s) / 3.0);

    public static string BuildPostUrl(int postId) =>
        $"https://myapp.com/posts/{postId}";

    public static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2) return email;
        var name = parts[0];
        var masked = name.Length > 2 ? name[..2] + new string('*', name.Length - 2) : name;
        return $"{masked}@{parts[1]}";
    }
}

[ExcludeFromCodeCoverage] public record JsonPlaceholderPost(int UserId, int Id, string Title, string Body);
[ExcludeFromCodeCoverage] public record JsonPlaceholderComment(int PostId, int Id, string Name, string Email, string Body);
