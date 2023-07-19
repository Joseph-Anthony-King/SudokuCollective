using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using SudokuCollective.Core.Enums;
using SudokuCollective.Data.Models.Params;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Property)]
    internal sealed class PaginatorValidatedAttribute : ValidationAttribute
    {
        private const string defaultError = "{0} Sortby field is invalid.";

        internal PaginatorValidatedAttribute() : base(defaultError) { }

        public override bool IsValid(object value)
        {
            var paginator = value as Paginator;

            if (paginator == null)
            {
                return true;
            }
            else
            {
                if ((int)paginator.SortBy < 0 || (int)paginator.SortBy > (int)Enum.GetValues(typeof(SortValue)).Cast<SortValue>().Last())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(this.ErrorMessageString, name);
        }
    }
}
