using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class SessionFlowTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public SessionFlowTests(RaceDayFactory factory) => _factory = factory;

    [Fact]
    public async Task FullFlow_Register_Login_Session_Logout()
    {
        var client = _factory.CreateClient();
        var email = $"flow_{Guid.NewGuid()}@test.co.za";

        // 1) Register
        var reg = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Pass123!",
            fullName = "Flow User",
            role = "Organiser",
            phoneNumber = "0710000000"
        });
        Assert.Equal(HttpStatusCode.Created, reg.StatusCode);

        // 2) Me before login → 401
        var meBefore = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meBefore.StatusCode);

        // 3) Login
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Pass123!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        // 4) Me after login → 200 (session persisted via cookie handler)
        var meAfter = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meAfter.StatusCode);

        // 5) Access protected endpoint as Organiser → 201
        var create = await client.PostAsJsonAsync("/api/events", new
        {
            name = "Session Flow Event",
            description = "X",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Durban",
            distance = 10m,
            eventType = "Run"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        // 6) Logout
        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        // 7) Me after logout → 401
        var meAfterLogout = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);

        // 8) Protected endpoint after logout → 401
        var createAfter = await client.PostAsJsonAsync("/api/events", new
        {
            name = "Should Fail",
            description = "X",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Durban",
            distance = 10m,
            eventType = "Run"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, createAfter.StatusCode);
    }

    [Fact]
    public async Task SessionIsIsolatedBetweenUsers()
    {
        var organiser = _factory.CreateClient();
        var participant = _factory.CreateClient();

        var orgEmail = $"org_{Guid.NewGuid()}@test.co.za";
        var partEmail = $"part_{Guid.NewGuid()}@test.co.za";

        await organiser.PostAsJsonAsync("/api/auth/register", new
        {
            email = orgEmail,
            password = "Pass123!",
            fullName = "Org",
            role = "Organiser",
            phoneNumber = "0710000000"
        });
        await participant.PostAsJsonAsync("/api/auth/register", new
        {
            email = partEmail,
            password = "Pass123!",
            fullName = "Part",
            role = "Participant",
            phoneNumber = "0710000000"
        });

        await organiser.PostAsJsonAsync("/api/auth/login", new { email = orgEmail, password = "Pass123!" });
        await participant.PostAsJsonAsync("/api/auth/login", new { email = partEmail, password = "Pass123!" });

        // Organiser can create events
        var orgCreate = await organiser.PostAsJsonAsync("/api/events", new
        {
            name = "Org Event",
            description = "X",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Durban",
            distance = 10m,
            eventType = "Run"
        });
        Assert.Equal(HttpStatusCode.Created, orgCreate.StatusCode);

        // Participant cannot create events
        var partCreate = await participant.PostAsJsonAsync("/api/events", new
        {
            name = "Part Event",
            description = "X",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Durban",
            distance = 10m,
            eventType = "Run"
        });
        Assert.Equal(HttpStatusCode.Forbidden, partCreate.StatusCode);
    }

    [Fact]
    public async Task Login_WithInactiveNonexistentUser_Returns401()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nonexistent@test.co.za",
            password = "Anything123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
