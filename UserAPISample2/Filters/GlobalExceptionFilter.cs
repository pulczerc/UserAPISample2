using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace UserAPISample2.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var exceptionType = context.Exception.GetType();
            _logger.LogError(
                "Exception occurred. Message : {message} InnerException : {innerException} Stacktrace : {stackTrace}.",
                context.Exception.Message, context.Exception.InnerException, context.Exception.StackTrace);

            context.ExceptionHandled = true;
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Result = new JsonResult(new { ErrorMessage = $"Exception occurred. Type: {exceptionType.Name} - Message: {context.Exception.Message}" });
        }
    }
}
