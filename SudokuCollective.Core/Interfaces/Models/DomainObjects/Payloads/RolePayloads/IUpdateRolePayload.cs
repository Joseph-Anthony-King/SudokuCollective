using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IUpdateRolePayload : IPayload
    {
        int Id { get; set; }
        string Name { get; set; }
    }
}
