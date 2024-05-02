using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SudokuCollective.Api.Filters
{
    /// <summary>
    /// A Swashbuckler filter which displays api paths in lower case.
    /// </summary>
    public class PathLowercaseDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// A method which applies the filter.
        /// </summary>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var dictionaryPath = swaggerDoc.Paths.ToDictionary(x => ToLowercase(x.Key), x => x.Value);
            var newPaths = new OpenApiPaths();
            foreach (var path in dictionaryPath)
            {
                newPaths.Add(path.Key, path.Value);
            }
            swaggerDoc.Paths = newPaths;
        }

        private static string ToLowercase(string key)
        {
            var parts = key.Split('/').Select(part => part.Contains('}') ? part : part.ToLowerInvariant());
            return string.Join('/', parts);
        }
    }
}
