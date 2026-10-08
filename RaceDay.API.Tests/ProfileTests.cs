using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class ProfileTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public ProfileTests(RaceDayFactory factory) => _factory = factory;

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
    public async Task GetProfile_AsParticipant_Returns200()
    {
        var client = await LoginAs("Participant");
        var resp = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetProfile_AsOrganiser_Returns200()
    {
        var client = await LoginAs("Organiser");
        var resp = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetProfile_Unauthenticated_Returns401()
    {
        var anon = _factory.CreateClient();
        var resp = await anon.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_AsParticipant_Returns200()
    {
        var client = await LoginAs("Participant");
        var resp = await client.PutAsJsonAsync("/api/profile", new
        {
            fullName = "Updated Name",
            phoneNumber = "0821111111",
            dateOfBirth = "1995-05-05",
            gender = "Male",
            emergencyContact = "Emergency - 0829999999",
            companyName = (string?)null,
            organisationPhoneNumber = (string?)null
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithCorrectCurrent_Returns200()
    {
        var client = await LoginAs("Participant");
        var resp = await client.PutAsJsonAsync("/api/profile/password", new
        {
            currentPassword = "Pass123!",
            newPassword = "NewPass456!"
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrent_Returns400()
    {
        var client = await LoginAs("Participant");
        var resp = await client.PutAsJsonAsync("/api/profile/password", new
        {
            currentPassword = "WrongPassword",
            newPassword = "NewPass456!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Unauthenticated_Returns401()
    {
        var anon = _factory.CreateClient();
        var resp = await anon.PutAsJsonAsync("/api/profile/password", new
        {
            currentPassword = "Pass123!",
            newPassword = "NewPass456!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}