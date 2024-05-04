using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Heroku.Models
{
    internal class HerokuCredentials
    {
        [JsonPropertyName("client")]
        public string Client { get; set; }
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("updatedAt")]
        public string UpdatedAt { get; set; }

        internal HerokuCredentials()
        {
            Client = string.Empty;
            ID = string.Empty;
            Description = string.Empty;
            Scope = string.Empty;
            Token = string.Empty;
            UpdatedAt = string.Empty;
        }

        [JsonConstructor]
        internal HerokuCredentials(
            string client,
            string id,
            string description,
            string scope,
            string token,
            string updatedAt)
        {
            Client = client;
            ID = id;
            Description = description;
            Scope = scope;
            Token = token;
            UpdatedAt = updatedAt;
        }
    }
}
