using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;

namespace SudokuCollective.Data.Models.Payloads
{
    public class CreateGamePayload : ICreateGamePayload
    {
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [Required, JsonPropertyName("difficultyLevel")]
        public DifficultyLevel DifficultyLevel { get; set; }

        public CreateGamePayload()
        {
            DifficultyLevel = DifficultyLevel.EASY;
        }

        public CreateGamePayload(DifficultyLevel difficultyLevel)
        {
            DifficultyLevel = difficultyLevel;
        }

        public static implicit operator JsonElement(CreateGamePayload v)
        {
            return JsonSerializer.SerializeToElement(v, _serializerOptions);
        }
    }
}
