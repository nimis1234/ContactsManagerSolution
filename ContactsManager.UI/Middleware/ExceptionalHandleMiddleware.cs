using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CURDExample.Middleware
{
    public class ExceptionalHandleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionalHandleMiddleware> _logger;

        // all the exception controll comes here and log it and display the message in web page as response
        public ExceptionalHandleMiddleware(RequestDelegate next, ILogger<ExceptionalHandleMiddleware> logger)
        {

            _next = next;
            _logger = logger;
        }

      public async Task InvokeAsync(HttpContext context)
         {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var response="";
                if(ex.InnerException!=null)
                {

                    _logger.LogError("{ExceptionType} {ExceptionMessage}", ex.InnerException.GetType().ToString(), ex.InnerException.Message);
                    response= ex.InnerException.Message;
                }
                else
                {
                    _logger.LogError("{ExceptionType} {ExceptionMessage}", ex.GetType().ToString(), ex.Message);
                    response= ex.Message;
                }
                context.Response.StatusCode = 500;
                string responseMessage = ex.InnerException != null
                ? ex.InnerException.Message
                : ex.Message;
                context.Items["ErrorMessage"] = responseMessage;

                //await context.Response.WriteAsync(response); //if we are using error page then instead of this use, throw error here
                // context.Response.Redirect("/Home/Errors");
                throw;
            }
        }

        // ADD this method as extension method in Program.cs
        
    }

    // ADD this method as extension method of IApplicationBuilder  USED FOR exception in Program.cs
    public static class ExceptionalHandleMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionalHandleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionalHandleMiddleware>();
        }
    }
}
