using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class EnrolmentsTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public EnrolmentsTests(RaceDayFactory factory) => _factory = factory;

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
        throw new InvalidOperationException($"Property '{propertyName}' not found in JSON. " +
            $"Available: {string.Join(", ", root.EnumerateObject().Select(p => p.Name))}");
    }

    [Fact]
    public async Task Participant_Enrols_Returns201()
    {
        // 1) Organiser creates event
        var org = await LoginAs("Organiser");
        var evResp = await org.PostAsJsonAsync("/api/events", new
        {
            name = "Enrol Test",
            description = "X",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Durban",
            distance = 10m,
            eventType = "Run"
        });

        var evJson = JsonDocument.Parse(await evResp.Content.ReadAsStringAsync());
        var eventId = ExtractIntProperty(evJson.RootElement, "eventID");

        // 2) Organiser creates category under that event
        var catResp = await org.PostAsJsonAsync($"/api/events/{eventId}/categories", new
        {
            name = "Open",
            description = "Open",
            minAge = 18,
            maxAge = 99,
            distance = 10m,
            entryFee = 100m
        });

        var catJson = JsonDocument.Parse(await catResp.Content.ReadAsStringAsync());
        var catId = ExtractIntProperty(catJson.RootElement, "categoryID");

        // 3) Participant enrols
        var part = await LoginAs("Participant");
        var enrolResp = await part.PostAsJsonAsync($"/api/events/{eventId}/enrol", new { categoryId = catId });

        Assert.Equal(HttpStatusCode.Created, enrolResp.StatusCode);
    }
}