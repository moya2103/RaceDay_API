using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class AuthTests : IClassFixture<RaceDayFactory>
{
    private readonly HttpClient _client;

    public AuthTests(RaceDayFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidParticipant_Returns201()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"p_{Guid.NewGuid()}@test.co.za",
            password = "Pass123!",
            fullName = "Test Participant",
            role = "Participant",
            phoneNumber = "0710000000"
        });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid()}@test.co.za";
        var body = new { email, password = "Pass123!", fullName = "X", role = "Participant", phoneNumber = "0710000000" };

        await _client.PostAsJsonAsync("/api/auth/register", body);
        var resp = await _client.PostAsJsonAsync("/api/auth/register", body);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidRole_Returns400()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"x_{Guid.NewGuid()}@test.co.za",
            password = "Pass123!",
            fullName = "X",
            role = "Admin",
            phoneNumber = "0710000000"
        });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Login_CorrectCredentials_Returns200()
    {
        var email = $"login_{Guid.NewGuid()}@test.co.za";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Pass123!",
            fullName = "Log",
            role = "Participant",
            phoneNumber = "0710000000"
        });

        var resp = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Pass123!" });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"wp_{Guid.NewGuid()}@test.co.za";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Pass123!",
            fullName = "W",
            role = "Participant",
            phoneNumber = "0710000000"
        });

        var resp = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong!" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutSession_Returns401()
    {
        var resp = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}