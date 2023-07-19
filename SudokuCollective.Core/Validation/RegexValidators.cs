using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Core.Validation
{
    internal static class RegexValidators
    {
        // Email must be in a valid format
        internal const string EmailRegexPattern = @"(^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w{2,3})+$)";
        // Guid must be in the pattern of d36ddcfd-5161-4c20-80aa-b312ef161433 with hexadecimal characters
        internal const string GuidRegexPattern = @"(^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$)";
        /* Password must be from 4 and up through 20 characters with at least 1 upper case letter, 1 lower case letter, 1 numeric character, 
         * and 1 special character of ! @ # $ % ^ & * + = ? - _ . , */
        internal const string PasswordRegexPattern = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*+=?\-_.,]).{3,21}$";
        /* User name must be at least 4 characters and can contain alphanumeric characters and special characters of
         * [! @ # $ % ^ & * + = ? - _ . ,] */
        internal const string UserNameRegexPattern = @"^[a-zA-Z0-9!@#$%^&*+=<>?-_.,].{3,}$";
        // Must be a valid url with an http or https protocol
        internal const string UrlRegexPattern = @"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$";
    }
}
