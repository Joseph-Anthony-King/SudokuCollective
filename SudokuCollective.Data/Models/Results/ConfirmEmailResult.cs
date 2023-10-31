using System;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;

namespace SudokuCollective.Data.Models.Results
{
    public class ConfirmEmailResult : IConfirmEmailResult
    {
        [JsonPropertyName("ConfirmationType")]
        public EmailConfirmationType ConfirmationType { get; set; }
        [JsonPropertyName("userName")]
        public string UserName { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
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
        [JsonPropertyName("dateUpdated")]
        public DateTime DateUpdated { get; set; }

        public ConfirmEmailResult()
        {
            ConfirmationType = EmailConfirmationType.NULL;
            UserName = string.Empty;
            Email = string.Empty;
            AppTitle = string.Empty;
            AppUrl = string.Empty;
            IsUpdate = false;
            NewEmailAddressConfirmed = null;
            ConfirmationEmailSuccessfullySent = null;
            DateUpdated = DateTime.MinValue;
        }

        public ConfirmEmailResult(
            EmailConfirmationType emailConfirmationType,
            string userName,
            string email,
            string appTitle, 
            string url, 
            bool? isUpdate,
            bool? newEmailAddressConfirmed, 
            bool? confirmationEmailSuccessfullySent,
            DateTime dateUpdated)
        {
            ConfirmationType = emailConfirmationType;
            UserName = userName;
            Email = email;
            AppTitle = appTitle;
            AppUrl = url;
            IsUpdate = isUpdate;
            NewEmailAddressConfirmed = newEmailAddressConfirmed;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            DateUpdated = dateUpdated;
        }
    }
}
