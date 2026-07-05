using HSCrm.BussinessLogic.PublicMethod;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HSCrm.Dashboard.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedException)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    new { area = "" }
                );
                context.ExceptionHandled = true;
            }
            else if (context.Exception is AccessDeniedException)
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied",
                    "Account",
                    new { area = "" }
                );
                context.ExceptionHandled = true;
            }
        }
    }
}
