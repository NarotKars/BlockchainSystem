using BlockChainSystem.Models;
using System.Data.SqlClient;
using System.Net;
using System.Text.Json;

namespace BlockChainSystem
{
    public class Error
    {
        public required string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public class ExceptionHandlingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (SqlException e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonSerializer.Serialize(e.Message));
            }
            catch (RESTException e)
            {
                context.Response.StatusCode = (int)e.StatusCode;
                await context.Response.WriteAsync(JsonSerializer.Serialize(e.Message));
            }
            catch (AggregateException e)
            {
                foreach (var exception in e.InnerExceptions)
                {
                    if (exception.GetType().Name == nameof(ArgumentException))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await context.Response.WriteAsync(JsonSerializer.Serialize(e.Message));
                    }
                    else if (exception.GetType().Name == nameof(SqlException))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await context.Response.WriteAsync(JsonSerializer.Serialize(e.Message));
                    }
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonSerializer.Serialize(e.Message));
            }
        }
    }
}
