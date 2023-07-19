using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Api")]
[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Messages
{
    internal static class AppsMessages
    {
        internal const string AppFoundMessage = "App found";
        internal const string AppsFoundMessage = "Apps found";
        internal const string AppNotFoundMessage = "App not found";
        internal const string AppsNotFoundMessage = "Apps not found";
        internal const string AppCreatedMessage = "App created";
        internal const string AppNotCreatedMessage = "App not created";
        internal const string AppUpdatedMessage = "App updated";
        internal const string AppNotUpdatedMessage = "App not updated";
        internal const string AppResetMessage = "App reset";
        internal const string AppNotResetMessage = "App not reset";
        internal const string AppDeletedMessage = "App deleted";
        internal const string AppNotDeletedMessage = "App not deleted";
        internal const string UserAddedToAppMessage = "User added to app";
        internal const string UserNotAddedToAppMessage = "User not added to app";
        internal const string UserRemovedFromAppMessage = "User removed from app";
        internal const string UserNotRemovedFromAppMessage = "User not removed from app";
        internal const string AppActivatedMessage = "App activated";
        internal const string AppNotActivatedMessage = "App not activated";
        internal const string AppDeactivatedMessage = "App deactivated";
        internal const string AppNotDeactivatedMessage = "App not deactivated";
        internal const string UserNotSignedUpToAppMessage = "User is not signed up to this app";
        internal const string UserIsNotARegisteredUserOfThisAppMessage = "User is not a registered user of this app";
        internal const string UserIsNotAnAssignedAdminMessage = "User is not an assigned admin";
        internal const string AdminPrivilegesActivatedMessage = "Admin privileges activated";
        internal const string ActivationOfAdminPrivilegesFailedMessage = "Activation of admin privileges failed";
        internal const string AdminPrivilegesDeactivatedMessage = "Admin privileges deactivated";
        internal const string DeactivationOfAdminPrivilegesFailedMessage = "Deactivation of admin privileges failed";
        internal const string AdminAppCannotBeDeletedMessage = "Admin app cannot be deleted";
        internal const string UserIsTheAppOwnerMessage = "User is the app owner";
        internal const string UserIsNotTheAppOwnerMessage = "User is not the app owner";
    }
}
