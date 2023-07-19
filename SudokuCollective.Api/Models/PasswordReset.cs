using System.ComponentModel.DataAnnotations;

namespace SudokuCollective.Api.Models
{
    /// <summary>
    /// A class that manages password resets.
    /// </summary>
    public class PasswordReset
    {
        /// <summary>
        /// An indicator documenting if the password reset was successful.
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// The user id for the relevant user.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// The user name for the relevant user.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The new password.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your new password")]
        [Compare("ConfirmNewPassword", ErrorMessage = "Must match the value of confirm new password")]
        public string NewPassword { get; set; }
        /// <summary>
        /// The string to confirm the new password, these two values must match.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please confirm your new password")]
        public string ConfirmNewPassword { get; set; }
        /// <summary>
        /// The app title for the relevant app.
        /// </summary>
        public string AppTitle { get; set; }
        /// <summary>
        /// The app id for the relevant app.
        /// </summary>
        public int AppId { get; set; }
        /// <summary>
        /// The app url for the relevant app.
        /// </summary>
        public string AppUrl { get; set; }
        /// <summary>
        /// Any error messages incurred during the process are passed through here.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
