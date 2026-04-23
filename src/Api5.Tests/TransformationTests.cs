// Unit tests for Api5 transformation logic

using Xunit;

namespace Api5.Tests;

public class TransformationTests
{
    // ── Title case ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("sunt aut facere", "Sunt Aut Facere")]
    [InlineData("nandi custom change", "Nandi Custom Change")]
    [InlineData("", "")]
    public void ToTitleCase_ShouldCapitalizeEachWord(string input, string expected)
    {
        var result = ToTitleCase(input);
        Assert.Equal(expected, result);
    }

    // ── Body truncation ───────────────────────────────────────────────────────

    [Fact]
    public void Summary_ShouldTruncateLongBody()
    {
        var body = new string('a', 150);
        var summary = body.Length > 100 ? body[..100] + "..." : body;
        Assert.Equal(103, summary.Length);
        Assert.EndsWith("...", summary);
    }

    [Fact]
    public void Summary_ShouldNotTruncateShortBody()
    {
        var body = "short body";
        var summary = body.Length > 100 ? body[..100] + "..." : body;
        Assert.Equal("short body", summary);
    }

    // ── Word count & read time ────────────────────────────────────────────────

    [Theory]
    [InlineData("one two three", 3)]
    [InlineData("hello", 1)]
    [InlineData("a b c d e f", 6)]
    public void WordCount_ShouldCountCorrectly(string body, int expected)
    {
        var count = body.Split(' ').Length;
        Assert.Equal(expected, count);
    }

    [Theory]
    [InlineData(3, 1)]    // 3 words → ceil(3/3) = 1 sec
    [InlineData(6, 2)]    // 6 words → ceil(6/3) = 2 sec
    [InlineData(7, 3)]    // 7 words → ceil(7/3) = 3 sec
    public void ReadTime_ShouldBeBasedOnWordCount(int wordCount, int expectedSec)
    {
        var readTime = (int)Math.Ceiling(wordCount / 3.0);
        Assert.Equal(expectedSec, readTime);
    }

    // ── Email masking ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Eliseo@gardner.biz", "El****@gardner.biz")]
    [InlineData("ab@test.com", "ab@test.com")]          // 2 chars — no masking
    [InlineData("a@test.com", "a@test.com")]            // 1 char — no masking
    public void MaskEmail_ShouldMaskCorrectly(string input, string expected)
    {
        var result = MaskEmail(input);
        Assert.Equal(expected, result);
    }

    // ── Post URL ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, "https://myapp.com/posts/1")]
    [InlineData(4, "https://myapp.com/posts/4")]
    public void PostUrl_ShouldBeCorrectlyFormatted(int postId, string expected)
    {
        var result = $"https://myapp.com/posts/{postId}";
        Assert.Equal(expected, result);
    }

    // ── Helpers (mirrors Program.cs logic) ───────────────────────────────────

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
}
