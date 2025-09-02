using ASP_ITStep.Data.Entities;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_ITStep.Middleware.Auth
{
    public class AuthSessionMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.ContainsKey("logout"))
            {
                context.Session.Remove("userAccess");
                context.Response.Redirect(context.Request.Path);
                return;
            }
            else if (context.Session.Keys.Contains("userAccess"))
            {
               var ua = JsonSerializer
                    .Deserialize<UserAccess>(
                    context.Session.GetString("userAccess")!
                    )!;
                // context.Items["userAccess"] = ua;
                // Передача даних у вигляді Entities до HttpContext утворює сильне зчеплення , що може призвести до проблем
                // після створення міграцій або переходу до іншого постачальника даних

                // Рішення - викорсистання іншої моделі ( рівня HttpContext ) context.User
                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new Claim[]
                        {
                            new(ClaimTypes.Name, ua.UserData.Name),
                            new(ClaimTypes.Email, ua.UserData.Email),
                            new(ClaimTypes.Sid, ua.Login),
                        },
                        nameof(AuthSessionMiddleware)
                    )
                );
            }
            await _next(context);
        }
    }
}
