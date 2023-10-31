using SudokuCollective.Core.Enums;

namespace SudokuCollective.Api.Models
{
    /// <summary>
    /// A class that manages email confirmations.
    /// </summary>
    public class ConfirmEmail
    {
        /// <summary>
        /// Captures the result from the user service
        /// indicating if the email was confirmed.
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// The type of email confirmation response 
        /// that is being handled.
        /// </summary>
        public EmailConfirmationType ConfirmationType { get; set; }
        /// <summary>
        /// The user name for the relevant user.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The app title for the relevant app.
        /// </summary>
        public string AppTitle { get; set; }
        /// <summary>
        /// The return url for the relevant app.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// The email for the relevant request.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Indicates if the new email address is
        /// confirmed.
        /// </summary>
        public bool NewEmailAddressConfirmed { get; set; }
    }
}
