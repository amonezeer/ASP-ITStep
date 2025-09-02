using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASP_ITStep.Filters
{
    public class AutorizationFilter : ActionFilterAttribute
    {

        override public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                base.OnActionExecuting(context);
            }
            else
            {
                context.Result = new JsonResult(new
                {
                    status = 401,
                    message = "UnAuthorized"
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }
    }
}
