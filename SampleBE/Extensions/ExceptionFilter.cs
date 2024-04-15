using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Data.AppException;

namespace SampleBE.Extensions
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            string message;
            if (context.Exception is AppException)
            {
                message = context.Exception.Message;
            }
            else
            {
                message = context.Exception.Message + "\n" + (context.Exception.InnerException != null ? context.Exception.InnerException.Message : "") + "\n ***Trace*** \n" + context.Exception.StackTrace;
            }
            context.Result = new BadRequestObjectResult(message) { };
        }
    }
}
