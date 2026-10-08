using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class ResultsTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public ResultsTests(RaceDayFactory factory) => _factory = factory;

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

    private async Task<(int eventId, int categoryId)> SetupEventWithCategory(HttpClient org)
    {
        var evResp = await org.PostAsJsonAsync("/api/events", new
        {
            name = "Result Test Event " + Guid.NewGuid(),
            description = "X",
            eventDate = DateTime.UtcNow.AddDays(-1), // past event
            location = "Durban",
            distance = 10m,
            eventType = "Run"
        });
        var evJson = JsonDocument.Parse(await evResp.Content.ReadAsStringAsync());
        var eventId = ExtractIntProperty(evJson.RootElement, "eventID");

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

        return (eventId, catId);
    }

    private async Task<int> EnrolParticipant(HttpClient participant, int eventId, int categoryId)
    {
        var resp = await participant.PostAsJsonAsync($"/api/events/{eventId}/enrol", new { categoryId });
        var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return ExtractIntProperty(json.RootElement, "enrolmentID");
    }

    [Fact]
    public async Task RecordResult_AsOrganiser_Returns201()
    {
        var org = await LoginAs("Organiser");
        var (eventId, catId) = await SetupEventWithCategory(org);

        var part = await LoginAs("Participant");
        var enrolmentId = await EnrolParticipant(part, eventId, catId);

        var resp = await org.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new
        {
            finishTime = "00:45:30",
            position = 5,
            isCompleted = true,
            notes = "Good run"
        });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task RecordResult_AsParticipant_Returns403()
    {
        var org = await LoginAs("Organiser");
        var (eventId, catId) = await SetupEventWithCategory(org);

        var part = await LoginAs("Participant");
        var enrolmentId = await EnrolParticipant(part, eventId, catId);

        var resp = await part.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new
        {
            finishTime = "00:45:30",
            position = 5,
            isCompleted = true,
            notes = "Cheating"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task RecordResult_NotOwnerOrganiser_Returns403()
    {
        var owner = await LoginAs("Organiser");
        var (eventId, catId) = await SetupEventWithCategory(owner);

        var part = await LoginAs("Participant");
        var enrolmentId = await EnrolParticipant(part, eventId, catId);

        var otherOrg = await LoginAs("Organiser");
        var resp = await otherOrg.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new
        {
            finishTime = "00:45:30",
            position = 5,
            isCompleted = true,
            notes = "X"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task GetMyResults_AsParticipant_Returns200()
    {
        var org = await LoginAs("Organiser");
        var (eventId, catId) = await SetupEventWithCategory(org);

        var part = await LoginAs("Participant");
        var enrolmentId = await EnrolParticipant(part, eventId, catId);

        await org.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new
        {
            finishTime = "00:45:30",
            position = 5,
            isCompleted = true,
            notes = "Good"
        });

        var resp = await part.GetAsync("/api/results/my");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetMyResults_Unauthenticated_Returns401()
    {
        var anon = _factory.CreateClient();
        var resp = await anon.GetAsync("/api/results/my");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetResultForEnrolment_AsOwnerParticipant_Returns200()
    {
        var org = await LoginAs("Organiser");
        var (eventId, catId) = await SetupEventWithCategory(org);

        var part = await LoginAs("Participant");
        var enrolmentId = await EnrolParticipant(part, eventId, catId);

        await org.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new
        {
            finishTime = "00:45:30",
            position = 5,
            isCompleted = true,
            notes = "Good"
        });

        var resp = await part.GetAsync($"/api/enrolments/{enrolmentId}/result");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetResultForEnrolment_AsOtherParticipant_Returns403()
    {
        var org = await LoginAs("Organiser");
        var (eventId, catId) = await SetupEventWithCategory(org);

        var part = await LoginAs("Participant");
        var enrolmentId = await EnrolParticipant(part, eventId, catId);

        await org.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new
        {
            finishTime = "00:45:30",
            position = 5,
            isCompleted = true,
            notes = "Good"
        });

        var otherPart = await LoginAs("Participant");
        var resp = await otherPart.GetAsync($"/api/enrolments/{enrolmentId}/result");

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }
}