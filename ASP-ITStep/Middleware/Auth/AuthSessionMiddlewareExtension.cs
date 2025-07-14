namespace ASP_ITStep.Middleware.Auth
{
    public static class AuthSessionMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthSession
        (this IApplicationBuilder builder)
        { 
            return builder.UseMiddleware<AuthSessionMiddleware>(); 
        }
    }
}
