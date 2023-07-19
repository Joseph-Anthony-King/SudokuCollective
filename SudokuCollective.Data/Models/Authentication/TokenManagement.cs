using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SudokuCollective.Core.Interfaces.ServiceModels;

namespace SudokuCollective.Data.Models.Authentication
{
    public class TokenManagement : ITokenManagement
    {
        [Required, JsonPropertyName("secret")]
        public string Secret { get; set; }
        [Required, JsonPropertyName("issuer")]
        public string Issuer { get; set; }
        [Required, JsonPropertyName("audience")]
        public string Audience { get; set; }
        [Required, JsonPropertyName("accessExpiration")]
        public int AccessExpiration { get; set; }
        [Required, JsonPropertyName("refreshExpiration")]
        public int RefreshExpiration { get; set; }

        public TokenManagement()
        {
            Secret = string.Empty;
            Issuer = string.Empty;
            Audience = string.Empty;
            AccessExpiration = 0;
            RefreshExpiration = 0;
        }
    }
}