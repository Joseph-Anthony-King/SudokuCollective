using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Core.Utilities
{
    internal static class ModelValidator
    {
        internal static bool Validate<T>(T obj, out ICollection<ValidationResult> results)
        {
            results = new List<ValidationResult>();

            return Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);
        }
    }
}
