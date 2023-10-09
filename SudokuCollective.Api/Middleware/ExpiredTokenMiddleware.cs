using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;

namespace SudokuCollective.Api.Middleware
{
    /// <summary>
    /// Custom middleware to handle the expired token response.
    /// </summary>
    public class ExpiredTokenMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Custom middleware constructor.
        /// </summary>
        /// <param name="next"></param>
        public ExpiredTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Custom response issued by middleware if the token has expired.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Response.Headers["Token-Expired"] == "true")
            {
                context.Response.StatusCode = 401;

                var result = new Result
                {
                    IsSuccess = false,
                    IsFromCache = false,
                    Message = ControllerMessages.ExpiredTokenMessage
                };

                var json = JsonSerializer.Serialize(
                    result,
                    new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    });

                var data = Encoding.UTF8.GetBytes(json);

                context.Response.ContentType = "application/json";
                context.Response.ContentLength = data.Length;

                await context.Response.Body.WriteAsync(data);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
