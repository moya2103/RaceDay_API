using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class CategoriesTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public CategoriesTests(RaceDayFactory factory) => _factory = factory;

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

    private static int ExtractIntProperty(JsonElement root, string propertyName)
    {
        foreach (var prop in root.EnumerateObject())
        {
            if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                return prop.Value.GetInt32();
        }
        throw new InvalidOperationException($"Property '{propertyName}' not found in JSON.");
    }

    private async Task<int> CreateOwnedEvent(HttpClient organiser)
    {
        var resp = await organiser.PostAsJsonAsync("/api/events", new
        {
            name = "Event " + Guid.NewGuid(),
            description = "Test event",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Durban",
            distance = 10m,
            eventType = "Run"
        });
        var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return ExtractIntProperty(json.RootElement, "eventID");
    }

    [Fact]
    public async Task GetCategories_ForEvent_Returns200()
    {
        var org = await LoginAs("Organiser");
        var eventId = await CreateOwnedEvent(org);

        var anon = _factory.CreateClient();
        var resp = await anon.GetAsync($"/api/events/{eventId}/categories");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_AsOwner_Returns201()
    {
        var org = await LoginAs("Organiser");
        var eventId = await CreateOwnedEvent(org);

        var resp = await org.PostAsJsonAsync($"/api/events/{eventId}/categories", new
        {
            name = "Senior Men",
            description = "Open male category",
            minAge = 20,
            maxAge = 39,
            distance = 10m,
            entryFee = 100m
        });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_NotOwner_Returns403()
    {
        var owner = await LoginAs("Organiser");
        var eventId = await CreateOwnedEvent(owner);

        var other = await LoginAs("Organiser");
        var resp = await other.PostAsJsonAsync($"/api/events/{eventId}/categories", new
        {
            name = "Hacker Category",
            description = "X",
            minAge = 18,
            maxAge = 99,
            distance = 10m,
            entryFee = 50m
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_AsParticipant_Returns403()
    {
        var org = await LoginAs("Organiser");
        var eventId = await CreateOwnedEvent(org);

        var participant = await LoginAs("Participant");
        var resp = await participant.PostAsJsonAsync($"/api/events/{eventId}/categories", new
        {
            name = "X",
            description = "X",
            minAge = 18,
            maxAge = 99,
            distance = 10m,
            entryFee = 50m
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_AsOwner_Returns200()
    {
        var org = await LoginAs("Organiser");
        var eventId = await CreateOwnedEvent(org);

        var create = await org.PostAsJsonAsync($"/api/events/{eventId}/categories", new
        {
            name = "Old Name",
            description = "Old",
            minAge = 18,
            maxAge = 99,
            distance = 10m,
            entryFee = 50m
        });
        var json = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var catId = ExtractIntProperty(json.RootElement, "categoryID");

        var resp = await org.PutAsJsonAsync($"/api/events/{eventId}/categories/{catId}", new
        {
            name = "New Name",
            description = "Updated",
            minAge = 20,
            maxAge = 39,
            distance = 15m,
            entryFee = 75m
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_NotOwner_Returns403()
    {
        var owner = await LoginAs("Organiser");
        var eventId = await CreateOwnedEvent(owner);

        var create = await owner.PostAsJsonAsync($"/api/events/{eventId}/categories", new
        {
            name = "ToDelete",
            description = "X",
            minAge = 18,
            maxAge = 99,
            distance = 10m,
            entryFee = 50m
        });
        var json = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var catId = ExtractIntProperty(json.RootElement, "categoryID");

        var other = await LoginAs("Organiser");
        var resp = await other.DeleteAsync($"/api/events/{eventId}/categories/{catId}");

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }
}