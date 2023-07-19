using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Data.Models.Results
{
    public class LicenseResult : ILicenseResult
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }
        [JsonPropertyName("isFromCache")]
        public bool IsFromCache { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("license")]
        public string License { get; set; }
        [JsonPropertyName("payload")]
        public List<object> Payload { get; set; }

        public LicenseResult()
        {
            IsSuccess = false;
            IsFromCache = false;
            Message = string.Empty;
            License = string.Empty;
            Payload = new List<object>();
        }

        public LicenseResult(
            bool isSuccess,
            bool isFromCache,
            string message,
            string license,
            object[] dataPacket)
        {
            IsSuccess = isSuccess;
            IsFromCache = isFromCache;
            Message = message;
            License = license;
            Payload = dataPacket.ToList();
        }

        public LicenseResult(
            bool isSuccess,
            bool isFromCache,
            string message,
            string license,
            List<object> dataPacket)
        {
            IsSuccess = isSuccess;
            IsFromCache = isFromCache;
            Message = message;
            License = license;
            Payload = dataPacket;
        }
    }
}
