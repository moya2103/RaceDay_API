using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class RoleAccessTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public RoleAccessTests(RaceDayFactory factory) => _factory = factory;

    private async Task<HttpClient> LoginAs(string role)
    {
        var client = _factory.CreateClient();
        var email = $"{role}_{Guid.NewGuid()}@test.co.za";
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Pass123!",
            fullName = role,
            role,
            phoneNumber = "0710000000"
        });
        await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Pass123!" });
        return client;
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_Returns403()
    {
        var client = await LoginAs("Participant");

        var resp = await client.PostAsJsonAsync("/api/events", new
        {
            name = "X",
            description = "Y",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Durban",
            distance = 10m,
            eventType = "Run"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Enrol_AsOrganiser_Returns403()
    {
        var client = await LoginAs("Organiser");
        var resp = await client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithoutSession_Returns401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}