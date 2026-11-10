using System;
using System.Linq;
using Microsoft.OpenApi;
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

            foreach (var path in swaggerDoc.Paths.Keys
                .Where(path => path.Equals(route, StringComparison.OrdinalIgnoreCase))
                .ToList())
            {
                swaggerDoc.Paths.Remove(path);
            }
        }
    }
}
