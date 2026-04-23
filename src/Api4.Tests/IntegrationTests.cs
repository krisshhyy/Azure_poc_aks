using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api4.Tests;

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
        Assert.Contains("Api4", body);
        Assert.Contains("healthy", body);
    }

    [Fact]
    public async Task GetUsers_ShouldReturn200()
    {
        var response = await _client.GetAsync("/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnList()
    {
        var response = await _client.GetAsync("/users");
        var body = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("[", body.Trim());
    }

    [Fact]
    public async Task GetUserById_ShouldReturn200ForValidId()
    {
        var response = await _client.GetAsync("/users/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_ShouldContainTransformedFields()
    {
        var response = await _client.GetAsync("/users/1");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("profileUrl", body);
        Assert.Contains("myapp.com/users/1", body);
    }
}
