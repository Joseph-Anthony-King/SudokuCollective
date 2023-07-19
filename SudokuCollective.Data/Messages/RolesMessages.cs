using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Repos")]
namespace SudokuCollective.Data.Messages
{
    internal static class RolesMessages
    {
        internal const string RoleFoundMessage = "Role found";
        internal const string RoleNotFoundMessage = "Role not found";
        internal const string RolesFoundMessage = "Roles found";
        internal const string RolesNotFoundMessage = "Roles not found";
        internal const string RoleCreatedMessage = "Role created";
        internal const string RoleNotCreatedMessage = "Role not created";
        internal const string RoleUpdatedMessage = "Role updated";
        internal const string RoleNotUpdatedMessage = "Role not updated";
        internal const string RoleDeletedMessage = "Role deleted";
        internal const string RoleNotDeletedMessage = "Role not deleted";
        internal const string RoleAlreadyExistsMessage = "Role already exists";
        internal const string RoleDoesNotExistMessage = "Role does not exist";
        internal const string RolesCannotBeAddedUsingThisEndpoint = "Roles cannot be added using this endpoint";
        internal const string RolesCannotBeRemovedUsingThisEndpoint = "RolesCannotBeRemovedUsingThisEndpoint";
    }
}
