using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Data")]
[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Core.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property)]
    internal sealed class EmailValidatedAttribute : RegularExpressionAttribute
    {
        internal EmailValidatedAttribute() : base(RegexValidators.EmailRegexPattern)
        {
        }
    }
}
