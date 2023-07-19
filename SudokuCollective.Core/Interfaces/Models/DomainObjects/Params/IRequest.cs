using System.Text.Json;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Params
{
    public interface IRequest
    {
        string License { get; set; }
        int RequestorId { get; set; }
        int AppId { get; set; }
        IPaginator Paginator { get; set; }
        JsonElement Payload { get; set; }
    }
}
