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
    /// <remarks>
    /// Custom middleware constructor.
    /// </remarks>
    /// <param name="next"></param>
    public class ExpiredTokenMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

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
                    _serializerOptions);

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
