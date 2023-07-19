using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Dev")]
[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property)]
    internal sealed class RowValidatedAttribute : ValidationAttribute
    {
        private const string defaultError = "{0} is invalid.";

        internal RowValidatedAttribute() : base(defaultError) { }

        public override bool IsValid(object value)
        {
            // Reject if value is null
            if (value == null)
            {
                return false;
            }

            var instanceArray = value as List<int>;

            // Reject if value cannot be cast to int list
            if (instanceArray == null)
            {
                return false;
            }

            var possibleIntList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Reject if less than or more than nine elements in the list
            if (instanceArray != null && instanceArray.Count != possibleIntList.Count)
            {
                return false;
            }

            var containsDuplicates = false;

            foreach (var i in instanceArray)
            {
                if (i != 0)
                {
                    if (possibleIntList.Contains(i))
                    {
                        possibleIntList.Remove(i);
                    }
                    else
                    {
                        containsDuplicates = true;
                    }
                }
            }

            // Reject if there are duplicates, otherwise accept
            if (!containsDuplicates && instanceArray.Count == 9)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            name = name.Replace("Row", " Row");
            return string.Format(this.ErrorMessageString, name);
        }
    }
}
