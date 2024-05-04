using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SudokuCollective.Api.Filters
{
    /// <summary>
    /// A custom document filter to include models in the swagger documentation.
    /// </summary>
    public class CustomModelDocumentFilter<T> : IDocumentFilter where T : class
    {
        /// <summary>
        /// A method to apply the filter
        /// </summary>
        public void Apply(OpenApiDocument openapiDoc, DocumentFilterContext context)
        {
            context.SchemaGenerator.GenerateSchema(typeof(T), context.SchemaRepository);
        }
    }
}
