using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;

namespace SudokuCollective.Data.Models.Payloads
{
    public class CreateGamePayload : ICreateGamePayload
    {
        [Required, JsonPropertyName("difficultyId")]
        public int DifficultyId { get; set; }

        public CreateGamePayload()
        {
            DifficultyId = 0;
        }

        public CreateGamePayload(int difficultyId)
        {
            DifficultyId = difficultyId;
        }

        public static implicit operator JsonElement(CreateGamePayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
