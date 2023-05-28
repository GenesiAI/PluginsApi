using Google.Apis.Auth;
using System.Security.Claims;

namespace AiPlugin.Api.Middlewares;

public class FirebaseAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public FirebaseAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string idToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);

        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string>() { "genesi-ai" },
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            context.Items["TokenPayload"] = payload;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, payload.Subject),
                new Claim(ClaimTypes.Email, payload.Email),
                new Claim(ClaimTypes.Name, payload.Name),
                new Claim(ClaimTypes.GivenName, payload.GivenName),
                new Claim(ClaimTypes.Surname, payload.FamilyName),
                new Claim("locale", payload.Locale),
                new Claim("picture", payload.Picture),
                new Claim("iss", payload.Issuer),
            };
            var identity = new ClaimsIdentity(claims, "Firebase");
            var principal = new ClaimsPrincipal(identity);
            context.User = principal;
        }
        catch (Exception)
        {
            // Handle invalid token
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context);
    }
}