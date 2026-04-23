using Xunit;
using Api4;

namespace Api4.Tests;

public class TransformationTests
{
    [Fact]
    public void StripPhoneExtension_ShouldRemoveExtension()
    {
        Assert.Equal("1-770-736-8031", UserTransformer.StripPhoneExtension("1-770-736-8031 x56442"));
        Assert.Equal("010-692-6593",   UserTransformer.StripPhoneExtension("010-692-6593 x09125"));
    }

    [Fact]
    public void StripPhoneExtension_NoExtension_ShouldReturnUnchanged()
    {
        Assert.Equal("555-1234", UserTransformer.StripPhoneExtension("555-1234"));
    }

    [Theory]
    [InlineData(1,  "https://myapp.com/users/1")]
    [InlineData(10, "https://myapp.com/users/10")]
    public void BuildProfileUrl_ShouldBeCorrectlyFormatted(int userId, string expected)
    {
        Assert.Equal(expected, UserTransformer.BuildProfileUrl(userId));
    }
}
