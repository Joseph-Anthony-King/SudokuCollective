namespace SudokuCollective.Api.Models
{
    /// <summary>
    /// A class that manages email confirmations.
    /// </summary>
    public class ConfirmEmail
    {
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
        /// Indicates is this request is for a new
        /// user or if it is an update for an existing
        /// user.
        /// </summary>
        public bool IsUpdate { get; set; }
        /// <summary>
        /// Indicates if the new email address is
        /// confirmed.
        /// </summary>
        public bool NewEmailAddressConfirmed { get; set; }
        /// <summary>
        /// Captures the result from the user service
        /// indicating if the email was confirmed.
        /// </summary>
        public bool IsSuccess { get; set; }
    }
}
