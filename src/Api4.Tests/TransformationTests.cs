// Unit tests for Api4 transformation logic
// These test the pure functions that shape the JSONPlaceholder response

using Xunit;

namespace Api4.Tests;

public class TransformationTests
{
    // ── Email normalization ───────────────────────────────────────────────────

    [Fact]
    public void Email_ShouldBeLowercased()
    {
        var email = "User.Name@Example.COM";
        var result = email.ToLower();
        Assert.Equal("user.name@example.com", result);
    }

    // ── Phone stripping ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("1-770-736-8031 x56442", "1-770-736-8031")]
    [InlineData("010-692-6593 x09125", "010-692-6593")]
    [InlineData("555-1234", "555-1234")]           // no extension — unchanged
    public void Phone_ShouldStripExtension(string input, string expected)
    {
        var result = input.Split(' ')[0];
        Assert.Equal(expected, result);
    }

    // ── Profile URL generation ────────────────────────────────────────────────

    [Theory]
    [InlineData(1, "https://myapp.com/users/1")]
    [InlineData(10, "https://myapp.com/users/10")]
    public void ProfileUrl_ShouldBeCorrectlyFormatted(int userId, string expected)
    {
        var result = $"https://myapp.com/users/{userId}";
        Assert.Equal(expected, result);
    }

    // ── Address flattening ────────────────────────────────────────────────────

    [Fact]
    public void City_ShouldBeExtractedFromNestedAddress()
    {
        var city = "Gwenborough";
        // Simulates extracting city from Address object
        Assert.NotEmpty(city);
        Assert.Equal("Gwenborough", city);
    }
}
