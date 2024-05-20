using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Messages
{
    internal class UsersMessages
    {
        internal const string UserFoundMessage = "User was found.";
        internal const string UsersFoundMessage = "Users were found.";
        internal const string UserNotFoundMessage = "User was not found.";
        internal const string UsersNotFoundMessage = "Users were not found.";
        internal const string UserCreatedMessage = "User was created.";
        internal const string UserNotCreatedMessage = "User was not created.";
        internal const string UserUpdatedMessage = "User was updated.";
        internal const string UserNotUpdatedMessage = "User was not updated.";
        internal const string UserExistsMessage = "User exists.";
        internal const string UserDeletedMessage = "User was deleted.";
        internal const string UserNotDeletedMessage = "User was not deleted.";
        internal const string UserActivatedMessage = "User was activated.";
        internal const string UserNotActivatedMessage = "User was not activated.";
        internal const string UserDeactivatedMessage = "User was deactivated.";
        internal const string UserNotDeactivatedMessage = "User was not deactivated.";
        internal const string UserDoesNotExistMessage = "User does not exist.";
        internal const string UserNameUniqueMessage = "User name is not unique.";
        internal const string UserNameRequiredMessage = "User name is required.";
        internal const string UserNameInvalidMessage = "User name accepts alphanumeric and special characters except double and single quotes.";
        internal const string UserNameConfirmedMessage = "User name was confirmed.";
        internal const string EmailUniqueMessage = "Email is not unique.";
        internal const string EmailRequiredMessage = "Email is required.";
        internal const string NoUserIsUsingThisEmailMessage = "No user is using this email.";
        internal const string RolesAddedMessage = "Roles were added.";
        internal const string RolesNotAddedMessage = "Roles were not added.";
        internal const string RolesRemovedMessage = "Roles were removed.";
        internal const string RolesNotRemovedMessage = "Roles were not removed.";
        internal const string RolesInvalidMessage = "Roles are invalid.";
        internal const string PasswordResetMessage = "Password was reset.";
        internal const string PasswordNotResetMessage = "Password was not reset.";
        internal const string EmailConfirmedMessage = "Email was confirmed.";
        internal const string EmailConfirmationTokenNotFound = "Email confirmation token was not found.";
        internal const string EmailConfirmationTokenExpired = "Email confirmation token has expired.";
        internal const string OldEmailConfirmedMessage = "Old email was confirmed.";
        internal const string OldEmailNotConfirmedMessage = "Old email was not confirmed.";
        internal const string ProcessedPasswordResetRequestMessage = "Password reset request was processed, please check your email.";
        internal const string ResentPasswordResetRequestMessage = "RPassword reset request was resent, please check your email.";
        internal const string UnableToProcessPasswordResetRequesMessage = "Unable to process the password reset request.";
        internal const string UserEmailNotConfirmedMessage = "User email was not confirmed.";
        internal const string PasswordResetTokenNotFound = "Password reset token was not found.";
        internal const string PasswordResetTokenExpired = "Password reset token has expired.";
        internal const string PasswordResetRequestNotFoundMessage = "Password reset request was not found.";
        internal const string NoOutstandingRequestToResetPasswordMessage = "No outstanding request to reset password.";
        internal const string EmailConfirmationEmailResentMessage = "Email confirmation email was resent.";
        internal const string EmailConfirmationEmailNotResentMessage = "Email confirmation email was not resent.";
        internal const string EmailConfirmationRequestNotFoundMessage = "No outstanding email confirmation request was found.";
        internal const string EmailConfirmationRequestCancelledMessage = "Email confirmation request was cancelled.";
        internal const string EmailConfirmationRequestNotCancelledMessage = "Email confirmation request was not cancelled.";
        internal const string PasswordResetEmailResentMessage = "Password reset email was resent.";
        internal const string PasswordResetEmailNotResentMessage = "Password reset email was not resent.";
        internal const string PasswordResetRequestCancelledMessage = "Password reset request was cancelled.";
        internal const string PasswordResetRequestNotCancelledMessage = "Password reset request was not cancelled.";
        internal const string EmailRequestsNotFoundMessage = "Email requests were not found.";
        internal const string UserIsAlreadyAnAdminMessage = "User is already an admin.";
        internal const string UserHasBeenPromotedToAdminMessage = "User has been promoted to admin.";
        internal const string UserHasNotBeenPromotedToAdminMessage = "User has not been promoted to admin.";
        internal const string UserDoesNotHaveAdminPrivilegesMessage = "User does not have admin privileges.";
        internal const string SuperUserCannotBePromotedMessage = "Super User cannot be promoted.";
        internal const string SuperUserCannotBeDeletedMessage = "Super User cannot be deleted.";
    }
}
