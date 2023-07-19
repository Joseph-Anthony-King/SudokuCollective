using System.Collections.Generic;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IUpdateUserRolePayload : IPayload
    {
        List<int> RoleIds { get; set; }
    }
}
