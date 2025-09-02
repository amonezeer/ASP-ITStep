using ASP_ITStep.Data;
using ASP_ITStep.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_ITStep.Middleware.Auth
{
    public class AuthTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DataContext dataContext)
        {
            var authHeader = context
                .Request
                .Headers
                .Authorization
                .FirstOrDefault(h => h?.StartsWith("Bearer ") ?? false);

            if (authHeader is not null)
            {
                var jti = authHeader[7..];

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[AuthMiddleware] Отримано токен: {jti}");
                Console.ResetColor();

                var token = await dataContext.AccessTokens
                    .AsNoTracking()
                    .Include(at => at.userAccess)
                        .ThenInclude(ua => ua.UserData)
                    .FirstOrDefaultAsync(at => at.Jti == jti);

                if (token == null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"[AuthMiddleware] Токен з JTI {jti} не знайдено в БД.");
                }
                else
                {
                    if (ParseUnixTime(token.Exp) is DateTime exp)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"[AuthMiddleware] Exp (UTC): {exp:O}");

                        if (exp > DateTime.UtcNow)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"[AuthMiddleware] Токен дійсний ");

                            var user = token.userAccess.UserData;
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine($"[AuthMiddleware] Користувач: {user.Name} ({user.Email})");

                            context.User = new ClaimsPrincipal(
                                new ClaimsIdentity(
                                    new[]
                                    {
                                        new Claim(ClaimTypes.Name, user.Name),
                                        new Claim(ClaimTypes.Email, user.Email),
                                    },
                                    nameof(AuthTokenMiddleware)
                                )
                            );
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[AuthMiddleware] ❌ Токен протермінований (Exp: {exp:O})");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"[AuthMiddleware] ❌ Неможливо розпарсити Exp: '{token.Exp}'");
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[AuthMiddleware] Заголовок Authorization відсутній або неправильний.");
            }

            await _next(context);
        }

        private static DateTime? ParseUnixTime(string? unixTimeStr)
        {
            if (long.TryParse(unixTimeStr, out var timestamp))
            {
                timestamp /= 1000;
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
            }
            return null;
        }

        
    }

public static class AuthTokenMiddlewareExtension
        {
            public static IApplicationBuilder UseAuthToken(this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<AuthTokenMiddleware>();
            }
        }
}


//using ASP_ITStep.Data;
//using ASP_ITStep.Data.Entities;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using System.Text.Json;

//namespace ASP_ITStep.Middleware.Auth
//{
//    public class AuthTokenMiddleware
//    {
//        private readonly RequestDelegate _next;
//        public AuthTokenMiddleware(RequestDelegate next)
//        {
//            _next = next;
//        }
//        public async Task InvokeAsync(HttpContext context, DataContext dataContext)
//        {
//           if (context
//                .Request
//                .Headers
//                .Authorization
//                .FirstOrDefault(h => h?.StartsWith("Bearer ") ?? false)
//                is String authHeader)
//            {
//                String jti = authHeader[7..];
//                if (dataContext
//                    .AccessTokens
//                    .AsNoTracking()
//                    .Include(at => at.userAccess)
//                        .ThenInclude(ua => ua.UserData)
//                    .FirstOrDefault(at => at.Jti == jti)
//                    is AccessToken accesstoken)
//                {
//                    context.User = new ClaimsPrincipal(
//                        new ClaimsIdentity(
//                            new Claim[]
//                            {
//                            new(ClaimTypes.Name, accesstoken.userAccess.UserData.Name),
//                            new(ClaimTypes.Email, accesstoken.userAccess.UserData.Email)
//                            },
//                            nameof(AuthTokenMiddleware)
//                        )
//                    );
//                }
//            }
//            await _next(context);
//        }
//    }
//    public static class AuthTokenMiddlewareExtension
//    {
//        public static IApplicationBuilder UseAuthToken
//        (this IApplicationBuilder builder)
//        {
//            return builder.UseMiddleware<AuthTokenMiddleware>();
//        }
//    }
//}
