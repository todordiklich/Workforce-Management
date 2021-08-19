using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WFM.Common.CustomExceptions;

namespace WFM.WEB.CustomExceptionMiddleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            if (exception.GetType().Name == nameof(CustomApplicationException))
            {
                var message = exception.Message;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await CreateError(context, message);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                string message = "Internal server error";
                await CreateError(context, message);
            }            
        }

        private static async Task CreateError(HttpContext context, string message)
        {
            await context.Response.WriteAsync(new ErrorDetails()
            {
                Message = message
            }.ToString());
        }
    }
}

