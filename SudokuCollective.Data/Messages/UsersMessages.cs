using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Messages
{
    internal class UsersMessages
    {
        internal const string UserFoundMessage = "User found";
        internal const string UsersFoundMessage = "Users found";
        internal const string UserNotFoundMessage = "User not found";
        internal const string UsersNotFoundMessage = "Users not found";
        internal const string UserCreatedMessage = "User created";
        internal const string UserNotCreatedMessage = "User not created";
        internal const string UserUpdatedMessage = "User updated";
        internal const string UserNotUpdatedMessage = "User not updated";
        internal const string UserExistsMessage = "User exists";
        internal const string UserDeletedMessage = "User deleted";
        internal const string UserNotDeletedMessage = "User not deleted";
        internal const string UserActivatedMessage = "User activated";
        internal const string UserNotActivatedMessage = "User not activated";
        internal const string UserDeactivatedMessage = "User deactivated";
        internal const string UserNotDeactivatedMessage = "User not deactivated";
        internal const string UserDoesNotExistMessage = "User does not exist";
        internal const string UserNameUniqueMessage = "User name not unique";
        internal const string UserNameRequiredMessage = "User name required";
        internal const string UserNameInvalidMessage = "User name accepts alphanumeric and special characters except double and single quotes";
        internal const string UserNameConfirmedMessage = "User name confirmed";
        internal const string EmailUniqueMessage = "Email not unique";
        internal const string EmailRequiredMessage = "Email required";
        internal const string NoUserIsUsingThisEmailMessage = "No user is using this email";
        internal const string RolesAddedMessage = "Roles added";
        internal const string RolesNotAddedMessage = "Roles not added";
        internal const string RolesRemovedMessage = "Roles removed";
        internal const string RolesNotRemovedMessage = "Roles not removed";
        internal const string RolesInvalidMessage = "Roles invalid";
        internal const string PasswordResetMessage = "Password reset";
        internal const string PasswordNotResetMessage = "Password not reset";
        internal const string EmailConfirmedMessage = "Email confirmed";
        internal const string EmailConfirmationTokenNotFound = "Email confirmation token not found";
        internal const string EmailConfirmationTokenExpired = "Email confirmation token expired";
        internal const string OldEmailConfirmedMessage = "Old email confirmed";
        internal const string OldEmailNotConfirmedMessage = "Old email not confirmed";
        internal const string ProcessedPasswordResetRequestMessage = "Processed password reset request, please check your email";
        internal const string ResentPasswordResetRequestMessage = "Resent password reset request, please check your email";
        internal const string UnableToProcessPasswordResetRequesMessage = "Unable to process password reset request";
        internal const string UserEmailNotConfirmedMessage = "User email not confirmed";
        internal const string PasswordResetTokenNotFound = "Password reset token not found";
        internal const string PasswordResetTokenExpired = "Password reset token expired";
        internal const string PasswordResetRequestNotFoundMessage = "Password reset request not found";
        internal const string NoOutstandingRequestToResetPassworMessage = "No outstanding request to reset password";
        internal const string EmailConfirmationEmailResentMessage = "Email confirmation email resent";
        internal const string EmailConfirmationEmailNotResentMessage = "Email confirmation email not resent";
        internal const string EmailConfirmationRequestNotFoundMessage = "No outstanding email confirmation request found";
        internal const string EmailConfirmationRequestCancelledMessage = "Email confirmation request cancelled";
        internal const string EmailConfirmationRequestNotCancelledMessage = "Email confirmation request not cancelled";
        internal const string PasswordResetEmailResentMessage = "Password reset email resent";
        internal const string PasswordResetEmailNotResentMessage = "Password reset email not resent";
        internal const string PasswordResetRequestCancelledMessage = "Password reset request cancelled";
        internal const string PasswordResetRequestNotCancelledMessage = "Password reset request not cancelled";
        internal const string EmailRequestsNotFoundMessage = "Email requests not found";
        internal const string UserIsAlreadyAnAdminMessage = "User is already an admin";
        internal const string UserHasBeenPromotedToAdminMessage = "User has been promoted to admin";
        internal const string UserHasNotBeenPromotedToAdminMessage = "User has not been promoted to admin";
        internal const string UserDoesNotHaveAdminPrivilegesMessage = "User does not have admin privileges";
        internal const string SuperUserCannotBePromotedMessage = "Super User cannot be promoted";
        internal const string SuperUserCannotBeDeletedMessage = "Super User cannot be deleted";
    }
}
