using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Logs.Models
{
    public class Request : IRequest
    {
        private IPaginator paginator = null;
        private JsonElement? payload = null;
        public string License { get; set; }
        public int RequestorId { get; set; }
        public int AppId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IPaginator Paginator
        {
            get => paginator;
            set => paginator = value;
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        JsonElement IRequest.Payload
        {
            get => (JsonElement)Payload;
            set => Payload = (JsonElement)value;
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonElement? Payload
        {
            get => payload;
            set => payload = value;
        }

        public Request()
        {
            License = string.Empty;
            RequestorId = 0;
            AppId = 0;
        }
    }
}
