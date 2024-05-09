using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SudokuCollective.Cache")]
namespace SudokuCollective.HerokuIntegration.Models.Configuration
{
    internal class FailedResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Url { get; set; }

        internal FailedResponse()
        {
            Id = string.Empty;
            Message = string.Empty;
            Url = string.Empty;
        }

        [JsonConstructor]
        internal FailedResponse(string id, string message, string url)
        {
            Id = id;
            Message = message;
            Url = url;
        }
    }
}
