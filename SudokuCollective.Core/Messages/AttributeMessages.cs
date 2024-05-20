using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Data")]
namespace SudokuCollective.Core.Messages
{
    internal static class AttributeMessages
    {
        internal const string InvalidUserName = "User name must be at least 4 characters and can contain alphanumeric characters and special characters of [! @ # $ % ^ & * + = ? - _ . ,].";
        internal const string InvalidEmail = "Email must be in a valid format.";
        internal const string InvalidLicense = "License must be in the pattern of d36ddcfd-5161-4c20-80aa-b312ef161433 with hexadecimal characters.";
        internal const string InvalidUrl = "Url is invalid.";
        internal const string InvalidToken = "Token must be in the pattern of d36ddcfd-5161-4c20-80aa-b312ef161433 with hexadecimal characters.";
        internal const string InvalidOldEmail = "Old Email must be in a valid format.";
        internal const string InvalidNewEmail = "New Email must be in a valid format.";
        internal const string InvalidIndex = "Sudoku Cell index must be between 1 and 81.";
        internal const string InvalidColumn = "Sudoku cell column must be between 1 and 9.";
        internal const string InvalidRegion = "Sudoku cell region must be between 1 and 9.";
        internal const string InvalidRow = "Sudoku cell row must be between 1 and 9.";
        internal const string InvalidValue = "Sudoku cell value must be between 1 and 9.";
        internal const string InvalidDisplayedValue = "Sudoku cell displayed value must be between 0 and 9.";
        internal const string InvalidSudokuCells = "The list of sudoku values is not valid.";
        internal const string InvalidPassword = "Password must be from 4 and up through 20 characters with at least 1 upper case letter, 1 lower case letter, 1 numeric character, and 1 special character of [! @ # $ % ^ & * + = ? - _ . ,].";
        internal const string InvalidFirstRow = "First row is invalid.";
        internal const string InvalidSecondRow = "Second row is invalid.";
        internal const string InvalidThirdRow = "Third row is invalid.";
        internal const string InvalidFourthRow = "Fourth row is invalid.";
        internal const string InvalidFifthRow = "Fifth row is invalid.";
        internal const string InvalidSixthRow = "Sixth row is invalid.";
        internal const string InvalidSeventhRow = "Seventh row is invalid.";
        internal const string InvalidEighthRow = "Eighth row is invalid.";
        internal const string InvalidNinthRow = "Ninth row is invalid.";
    }
}
