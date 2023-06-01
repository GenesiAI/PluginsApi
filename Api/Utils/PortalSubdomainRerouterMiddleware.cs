public class PortalSubdomainRerouterMiddleware
{
    private readonly RequestDelegate _next;

    public PortalSubdomainRerouterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var subdomain = context.Request.Host.Host.Split('.')[0];

        if (subdomain == "portal")
        {
            context.Request.Path = "/api" + context.Request.Path;
        }

        await _next(context);
    }
}