using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class PlugindFromSubdomainAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);

        var subdomain = filterContext.HttpContext.Request.Host.Host.Split('.')[0];
        var parseresult = Guid.TryParse(subdomain, out Guid subDomainAsGuid);
        if (parseresult)
        {
            filterContext.ActionArguments.Add("pluginId", subDomainAsGuid);
            return;
        }
        if (string.Equals("portal", subdomain, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        filterContext.Result = new BadRequestObjectResult("Invalid subdomain.");

        //if (string.Equals("www", subdomain, StringComparison.OrdinalIgnoreCase))
        //{
        //this should never happens as the dns should send users of www.genesi.ai and genesi.ai to the SPA
        //}
    }
}
