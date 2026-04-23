using Xunit;
using Api5;

namespace Api5.Tests;

public class TransformationTests
{
    [Theory]
    [InlineData("hello world",       "Hello World")]
    [InlineData("sunt aut facere",   "Sunt Aut Facere")]
    [InlineData("nandi custom change","Nandi Custom Change")]
    [InlineData("",                  "")]
    public void ToTitleCase_ShouldCapitalizeEachWord(string input, string expected)
    {
        Assert.Equal(expected, PostTransformer.ToTitleCase(input));
    }

    [Fact]
    public void Truncate_LongBody_ShouldAddEllipsis()
    {
        var body = new string('a', 150);
        var result = PostTransformer.Truncate(body, 100);
        Assert.Equal(103, result.Length);
        Assert.EndsWith("...", result);
    }

    [Fact]
    public void Truncate_ShortBody_ShouldReturnUnchanged()
    {
        Assert.Equal("short", PostTransformer.Truncate("short", 100));
    }

    [Theory]
    [InlineData("one two three", 3)]
    [InlineData("hello",         1)]
    [InlineData("a b c d e f",   6)]
    public void CountWords_ShouldCountCorrectly(string body, int expected)
    {
        Assert.Equal(expected, PostTransformer.CountWords(body));
    }

    [Theory]
    [InlineData(3, 1)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    public void CalcReadTime_ShouldBeBasedOnWordCount(int wordCount, int expectedSec)
    {
        // Build a string with exactly wordCount words
        var body = string.Join(' ', Enumerable.Repeat("word", wordCount));
        Assert.Equal(expectedSec, PostTransformer.CalcReadTime(body));
    }

    [Theory]
    [InlineData("Eliseo@gardner.biz", "El****@gardner.biz")]
    [InlineData("ab@test.com",        "ab@test.com")]
    [InlineData("a@test.com",         "a@test.com")]
    public void MaskEmail_ShouldMaskCorrectly(string input, string expected)
    {
        Assert.Equal(expected, PostTransformer.MaskEmail(input));
    }

    [Theory]
    [InlineData(1, "https://myapp.com/posts/1")]
    [InlineData(4, "https://myapp.com/posts/4")]
    public void BuildPostUrl_ShouldBeCorrectlyFormatted(int postId, string expected)
    {
        Assert.Equal(expected, PostTransformer.BuildPostUrl(postId));
    }
}
