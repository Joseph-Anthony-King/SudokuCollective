using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Utilities;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Params
{
    public class Request : IRequest
    {
        private string _license = string.Empty;
        private readonly GuidValidatedAttribute _guidValidator = new();

        [Required, GuidValidated(ErrorMessage = AttributeMessages.InvalidLicense), JsonPropertyName("license")]
        public string License
        {
            get => _license;
            set => _license = CoreUtilities.SetField(value, _guidValidator, AttributeMessages.InvalidLicense);
        }
        [Required, JsonPropertyName("requestorId")]
        public int RequestorId { get; set; }
        [Required, JsonPropertyName("appId")]
        public int AppId { get; set; }
        [JsonIgnore]
        IPaginator IRequest.Paginator
        {
            get => Paginator;
            set => Paginator = (Paginator)value;
        }
        [Required, JsonPropertyName("paginator")]
        public Paginator Paginator { get; set; }
        [Required, JsonPropertyName("payload")]
        public JsonElement Payload { get; set; }

        public Request()
        {
            _license = string.Empty;
            RequestorId = 0;
            AppId = 0;
            Paginator = new Paginator();
            Payload = new JsonElement();
        }

        public static explicit operator SudokuCollective.Logs.Models.Request(Request request)
        {
            var result = new SudokuCollective.Logs.Models.Request();

            result.License = request.License;
            result.RequestorId = request.RequestorId;
            result.AppId = request.AppId;

            return result;
        }
    }
}
