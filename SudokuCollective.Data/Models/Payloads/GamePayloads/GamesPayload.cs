using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;

namespace SudokuCollective.Data.Models.Payloads
{
    public class GamesPayload : IGamesPayload
    {
        [Required, JsonPropertyName("userId")]
        public int UserId { get; set; }

        public GamesPayload()
        {
            UserId = 0;
        }

        public GamesPayload(int userId)
        {
            UserId = userId;
        }

        public static implicit operator JsonElement(GamesPayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
