using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class EventsTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public EventsTests(RaceDayFactory factory) => _factory = factory;

    private async Task<HttpClient> LoginAsOrganiser()
    {
        var client = _factory.CreateClient();
        var email = $"org_{Guid.NewGuid()}@test.co.za";
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Pass123!",
            fullName = "Org",
            role = "Organiser",
            phoneNumber = "0710000000"
        });
        await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Pass123!" });
        return client;
    }

    private static int ExtractIntProperty(JsonElement root, string propertyName)
    {
        foreach (var prop in root.EnumerateObject())
        {
            if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                return prop.Value.GetInt32();
        }
        throw new InvalidOperationException($"Property '{propertyName}' not found in JSON.");
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_Returns201()
    {
        var client = await LoginAsOrganiser();

        var resp = await client.PostAsJsonAsync("/api/events", new
        {
            name = "Comrades Test",
            description = "Test event",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Pietermaritzburg",
            distance = 89.0m,
            eventType = "Run"
        });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task GetEvents_Anonymous_Returns200()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_NotOwner_Returns403()
    {
        var owner = await LoginAsOrganiser();
        var create = await owner.PostAsJsonAsync("/api/events", new
        {
            name = "Owned",
            description = "X",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Cape Town",
            distance = 21m,
            eventType = "Run"
        });

        var json = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var eventId = ExtractIntProperty(json.RootElement, "eventID");

        var other = await LoginAsOrganiser();
        var resp = await other.PutAsJsonAsync($"/api/events/{eventId}", new
        {
            name = "Hacked",
            description = "X",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Joburg",
            distance = 21m,
            eventType = "Run"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }
}