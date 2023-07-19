using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Utilities;

[assembly:InternalsVisibleTo("SudokuCollective.Data")]
[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Core.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Property)]
    sealed internal class SudokuCellsValidatedAttribute : ValidationAttribute
    {
        internal SudokuCellsValidatedAttribute() : base() { }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }

            // Reject if value cannot be cast to ISudokuCell list
            if (value is not List<SudokuCell> sudokuCells || sudokuCells.Count > 81)
            {
                return false;
            }

            foreach (var cell in sudokuCells)
            {
                if (!ModelValidator.Validate((SudokuCell)cell, out ICollection<ValidationResult> _results))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
