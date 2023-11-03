using System;
using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    public interface IConfirmEmailResult
    {
        EmailConfirmationType ConfirmationType { get; set; }
        string UserName { get; set; }
        string Email { get; set; }
        string AppTitle { get; set; }
        string AppUrl { get; set; }
        bool? NewEmailAddressConfirmed { get; set; }
        bool? ConfirmationEmailSuccessfullySent { get; set; }
    }
}
