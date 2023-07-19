using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Data")]
[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Core.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property)]
    sealed internal class PasswordValidatedAttribute : RegularExpressionAttribute
    {
        internal PasswordValidatedAttribute() : base(RegexValidators.PasswordRegexPattern)
        {
        }
    }
}
