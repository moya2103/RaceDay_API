namespace RaceDay.API.Helpers;

public static class SessionExtensions
{
    public static void SetUser(this ISession session, int userId, string role, string email)
    {
        session.SetInt32("UserId", userId);
        session.SetString("Role", role);
        session.SetString("Email", email);
    }

    public static int? GetUserId(this ISession session) => session.GetInt32("UserId");

    public static string? GetRole(this ISession session) => session.GetString("Role");

    public static bool IsAuthenticated(this ISession session) => session.GetInt32("UserId") != null;
}
