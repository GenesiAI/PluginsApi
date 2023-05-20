using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class UserIdFromSubdomainAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);

        var subdomain = filterContext.HttpContext.Request.Host.Host.Split('.')[0];
        var parseresult = Guid.TryParse(subdomain, out Guid subDomainAsGuid);
        if (subdomain == "www" || !parseresult)
        {
            filterContext.Result = new BadRequestObjectResult("Invalid subdomain.");
        }

        filterContext.ActionArguments.Add("userId", subDomainAsGuid);
    }
}
