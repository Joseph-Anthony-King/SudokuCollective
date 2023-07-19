using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;

namespace SudokuCollective.Data.Models.Payloads
{
    public class AddSolutionsPayload : IAddSolutionsPayload
    {
        [Required, JsonPropertyName("limit")]
        public int Limit { get; set; }

        public AddSolutionsPayload()
        {
            Limit = 0;
        }

        public AddSolutionsPayload(int limit)
        {
            Limit = limit;
        }

        public static implicit operator JsonElement(AddSolutionsPayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
