using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Repos")]
namespace SudokuCollective.Data.Messages
{
    internal static class RolesMessages
    {
        internal const string RoleFoundMessage = "Role was found.";
        internal const string RoleNotFoundMessage = "Role was not found.";
        internal const string RolesFoundMessage = "Roles were found.";
        internal const string RolesNotFoundMessage = "Roles were not found.";
        internal const string RoleCreatedMessage = "Role was created.";
        internal const string RoleNotCreatedMessage = "Role was not created.";
        internal const string RoleUpdatedMessage = "Role was updated.";
        internal const string RoleNotUpdatedMessage = "Role was not updated.";
        internal const string RoleDeletedMessage = "Role was deleted.";
        internal const string RoleNotDeletedMessage = "Role was not deleted.";
        internal const string RoleAlreadyExistsMessage = "Role already exists.";
        internal const string RoleDoesNotExistMessage = "Role does not exist.";
        internal const string RolesCannotBeAddedUsingThisEndpoint = "Roles cannot be added using this endpoint.";
        internal const string RolesCannotBeRemovedUsingThisEndpoint = "Roles cannot be removed using this endpoint.";
    }
}
