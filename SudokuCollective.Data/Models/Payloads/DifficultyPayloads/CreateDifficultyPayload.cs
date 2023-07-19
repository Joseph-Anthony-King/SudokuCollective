using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;

namespace SudokuCollective.Data.Models.Payloads
{
    public class CreateDifficultyPayload : ICreateDifficultyPayload
    {
        [Required, JsonPropertyName("name")]
        public string Name { get; set; }
        [Required, JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
        [Required, JsonPropertyName("difficultyLevel")]
        public DifficultyLevel DifficultyLevel { get; set; }

        public CreateDifficultyPayload()
        {
            Name = string.Empty;
            DisplayName = string.Empty;
            DifficultyLevel = DifficultyLevel.NULL;
        }

        public CreateDifficultyPayload(
            string name, 
            string displayName, 
            int difficultyLevel)
        {
            Name = name;
            DisplayName = displayName;
            DifficultyLevel = (DifficultyLevel)difficultyLevel;
        }

        public CreateDifficultyPayload(
            string name, 
            string displayName, 
            DifficultyLevel difficultyLevel)
        {
            Name = name;
            DisplayName = displayName;
            DifficultyLevel = difficultyLevel;
        }

        public static implicit operator JsonElement(CreateDifficultyPayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
