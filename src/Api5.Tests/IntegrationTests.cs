using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api5.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_ShouldReturnCorrectServiceName()
    {
        var response = await _client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Api5", body);
        Assert.Contains("healthy", body);
    }

    [Fact]
    public async Task GetPosts_ShouldReturn200()
    {
        var response = await _client.GetAsync("/posts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnTransformedList()
    {
        var response = await _client.GetAsync("/posts");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("postUrl", body);
        Assert.Contains("wordCount", body);
        Assert.Contains("readTimeSec", body);
    }

    [Fact]
    public async Task GetPostById_ShouldReturn200ForValidId()
    {
        var response = await _client.GetAsync("/posts/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPostById_CustomOverride_ShouldReturnNandiTitle()
    {
        var response = await _client.GetAsync("/posts/4");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("nandi custom change", body);
    }

    [Fact]
    public async Task GetPostComments_ShouldReturn200()
    {
        var response = await _client.GetAsync("/posts/1/comments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ShouldReturnMaskedEmails()
    {
        var response = await _client.GetAsync("/posts/1/comments");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("***", body);  // masked email
    }
}
