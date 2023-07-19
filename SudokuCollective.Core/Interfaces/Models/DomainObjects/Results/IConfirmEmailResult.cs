using System;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    public interface IConfirmEmailResult
    {
        string UserName { get; set; }
        string Email { get; set; }
        DateTime DateUpdated { get; set; }
        string AppTitle { get; set; }
        string AppUrl { get; set; }
        bool? IsUpdate { get; set; }
        bool? NewEmailAddressConfirmed { get; set; }
        bool? ConfirmationEmailSuccessfullySent { get; set; }
    }
}
