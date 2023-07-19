using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;

namespace SudokuCollective.Data.Models.Payloads
{
    public class UpdateDifficultyPayload : IUpdateDifficultyPayload
    {
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [Required, JsonPropertyName("name")]
        public string Name { get; set; }
        [Required, JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        public UpdateDifficultyPayload()
        {
            Id = 0;
            Name = string.Empty;
            DisplayName = string.Empty;
        }

        public UpdateDifficultyPayload(int id, string name, string displayName)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
        }

        public static implicit operator JsonElement(UpdateDifficultyPayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
