using System.Text.Json.Serialization;

namespace SudokuCollective.Heroku.Models
{
    internal class FailedResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }

        internal FailedResponse()
        {
            Id = string.Empty;
            Message = string.Empty;
        }

        [JsonConstructor]
        internal FailedResponse(string id, string message)
        {
            Id = id;
            Message = message;
        }
    }
}
