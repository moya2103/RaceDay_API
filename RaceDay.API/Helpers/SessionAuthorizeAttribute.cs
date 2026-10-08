using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Helpers;

public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public SessionAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.IsAuthenticated())
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Not logged in" });
            return;
        }

        if (_roles.Length > 0)
        {
            var role = session.GetRole();
            if (role == null || !_roles.Contains(role))
            {
                context.Result = new ObjectResult(new { message = "Forbidden: insufficient role" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}