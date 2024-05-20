using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Api")]
[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Messages
{
    internal static class AppsMessages
    {
        internal const string AppFoundMessage = "App was found.";
        internal const string AppsFoundMessage = "Apps were found.";
        internal const string AppNotFoundMessage = "App was not found.";
        internal const string AppsNotFoundMessage = "Apps were not found.";
        internal const string AppCreatedMessage = "App was created.";
        internal const string AppNotCreatedMessage = "App was not created.";
        internal const string AppUpdatedMessage = "App was updated.";
        internal const string AppNotUpdatedMessage = "App was not updated.";
        internal const string AppResetMessage = "App was reset.";
        internal const string AppNotResetMessage = "App was not reset.";
        internal const string AppDeletedMessage = "App was deleted.";
        internal const string AppNotDeletedMessage = "App was not deleted.";
        internal const string UserAddedToAppMessage = "User was added to the app.";
        internal const string UserNotAddedToAppMessage = "User was not added to app.";
        internal const string UserRemovedFromAppMessage = "User was removed from the app.";
        internal const string UserNotRemovedFromAppMessage = "User was not removed from the app.";
        internal const string AppActivatedMessage = "App was activated.";
        internal const string AppNotActivatedMessage = "App was not activated.";
        internal const string AppDeactivatedMessage = "App was deactivated.";
        internal const string AppNotDeactivatedMessage = "App was not deactivated.";
        internal const string UserNotSignedUpToAppMessage = "User is not signed up to this app.";
        internal const string UserIsNotARegisteredUserOfThisAppMessage = "User is not a registered user of this app.";
        internal const string UserIsNotAnAssignedAdminMessage = "User is not an assigned admin.";
        internal const string AdminPrivilegesActivatedMessage = "Admin privileges were activated.";
        internal const string ActivationOfAdminPrivilegesFailedMessage = "Activation of admin privileges failed.";
        internal const string AdminPrivilegesDeactivatedMessage = "Admin privileges were deactivated.";
        internal const string DeactivationOfAdminPrivilegesFailedMessage = "Deactivation of admin privileges failed.";
        internal const string AdminAppCannotBeDeletedMessage = "Admin app cannot be deleted.";
        internal const string UserIsTheAppOwnerMessage = "User is the owner of this app.";
        internal const string UserIsNotTheAppOwnerMessage = "User is not the owner of this app.";
    }
}
