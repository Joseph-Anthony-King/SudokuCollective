using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;

namespace SudokuCollective.Data.Models.Requests
{
    public class ResendRequestPasswordRequest : IResendRequestPasswordRequest
    {
        [Required, JsonPropertyName("userId")]
        public int UserId { get; set; }
        [Required, JsonPropertyName("appId")]
        public int AppId { get; set; }

        public ResendRequestPasswordRequest()
        {
            UserId = 0;
            AppId = 0;
        }

        public ResendRequestPasswordRequest(int userId, int appId)
        {
            UserId = userId;
            AppId = appId;
        }

        public static implicit operator JsonElement(ResendRequestPasswordRequest v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
