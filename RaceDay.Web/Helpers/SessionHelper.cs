namespace RaceDay.Web.Helpers;

public static class SessionHelper
{
    public static void SetUser(this ISession session, int userId, string role, string fullName)
    {
        session.SetInt32("MvcUserId", userId);
        session.SetString("MvcUserRole", role);
        session.SetString("MvcUserFullName", fullName);
    }

    public static int? GetUserId(this ISession session) => session.GetInt32("MvcUserId");

    public static string? GetRole(this ISession session) => session.GetString("MvcUserRole");

    public static string? GetFullName(this ISession session) => session.GetString("MvcUserFullName");

    public static bool IsAuthenticated(this ISession session) => session.GetInt32("MvcUserId") != null;
}
