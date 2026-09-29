using FunBooksAndVideos.Application.Exceptions;

namespace FunBooksAndVideos.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            var errorAsString = ex.ToString();
            logger.LogInformation(errorAsString);
            await context.Response.WriteAsJsonAsync(new
            {
                error = errorAsString
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Ooops, something went wrong"
            });
        }
    }
}
