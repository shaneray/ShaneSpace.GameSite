using System.Data.Entity.Core;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace ShaneSpace.GameSite.WebApi.App_Start
{
    public class GlobalExceptionHandler : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is EntityCommandExecutionException)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, context.Exception.InnerException);
            }
        }
    }
}