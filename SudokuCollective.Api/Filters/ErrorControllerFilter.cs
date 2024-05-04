using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SudokuCollective.Api.Filters
{
    /// <summary>
    /// A Swashbuckler filter to filter out the error controller.
    /// </summary>
    public class ErrorControllerFilter : IDocumentFilter
    {
        /// <summary>
        /// A method which applies the filter.
        /// </summary>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var route = "/error";
            swaggerDoc.Paths.Remove(route);
        }
    }
}
