using System.Collections.Generic;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Data.Models.Params
{
    public class Result : IResult
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }
        [JsonPropertyName("isFromCache")]
        public bool IsFromCache { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("payload")]
        public List<object> Payload { get; set; }

        public Result()
        {
            IsSuccess = false;
            IsFromCache = false;
            Message = string.Empty;
            Payload = new List<object>();
        }
    }
}
