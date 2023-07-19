using System;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;

namespace SudokuCollective.Data.Models.Results
{
    public class ConfirmEmailResult : IConfirmEmailResult
    {
        [JsonPropertyName("userName")]
        public string UserName { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("dateUpdated")]
        public DateTime DateUpdated { get; set; }
        [JsonPropertyName("appTitle")]
        public string AppTitle { get; set; }
        [JsonPropertyName("appUrl")]
        public string AppUrl { get; set; }
        [JsonPropertyName("isUpdate")]
        public bool? IsUpdate { get; set; }
        [JsonPropertyName("newEmailAddressConfirmed")]
        public bool? NewEmailAddressConfirmed { get; set; }
        [JsonPropertyName("confirmationEmailSuccessfullySent")]
        public bool? ConfirmationEmailSuccessfullySent { get; set; }

        public ConfirmEmailResult()
        {
            UserName = string.Empty;
            Email = string.Empty;
            DateUpdated = DateTime.MinValue;
            AppTitle = string.Empty;
            AppUrl = string.Empty;
            IsUpdate = null;
            NewEmailAddressConfirmed = null;
            ConfirmationEmailSuccessfullySent = null;
        }

        public ConfirmEmailResult(
            string userName,
            string email,
            DateTime dateUpdated,
            string appTitle, 
            string url, 
            bool? isUpdate, 
            bool? newEmailAddressConfirmed, 
            bool? confirmationEmailSuccessfullySent)
        {
            UserName = userName;
            Email = email;
            DateUpdated = dateUpdated;
            AppTitle = appTitle;
            AppUrl = url;
            IsUpdate = isUpdate;
            NewEmailAddressConfirmed = newEmailAddressConfirmed;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
        }
    }
}
