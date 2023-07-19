using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Core.Utilities
{
    internal class CoreUtilities
    {
        internal static T SetField<T>(
            T value, 
            ValidationAttribute validator = null, 
            string errorMessage = null)
        {
            var fieldType = typeof(T);

            if (validator != null && !string.IsNullOrEmpty(errorMessage))
            {
                if (fieldType == typeof(string))
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(value)) && validator.IsValid(value))
                    {
                        return value;
                    }
                    else
                    {
                        throw new ArgumentException(errorMessage);
                    }
                }
                else
                {
                    if (value != null && validator.IsValid(value))
                    {
                        return value;
                    }
                    else
                    {
                        throw new ArgumentException(errorMessage);
                    }
                }
            }
            else
            {
                if (fieldType == typeof(string))
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(value)))
                    {
                        return value;
                    }
                    else
                    {
                        throw new NullReferenceException();
                    }
                }
                else
                {
                    if (value != null)
                    {
                        return value;
                    }
                    else
                    {
                        throw new NullReferenceException();
                    }
                }
            }
        }

        internal static T SetNullableField<T>(
            T value, 
            ValidationAttribute validator = null, 
            string errorMessage = null)
        {
            var fieldType = typeof(T);

            if (validator != null && !string.IsNullOrEmpty(errorMessage))
            {
                if (fieldType == typeof(string))
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(value)) && validator.IsValid(value))
                    {
                        return value;
                    }
                    else if (string.IsNullOrEmpty(Convert.ToString(value)))
                    {
                        return default(T);
                    }
                    else
                    {
                        throw new ArgumentException(errorMessage);
                    }
                }
                else
                {
                    if (value != null && validator.IsValid(value))
                    {
                        return value;
                    }
                    else if (value == null)
                    {
                        return default(T);
                    }
                    else
                    {
                        throw new ArgumentException(errorMessage);
                    }
                }
            }
            else
            {
                if (fieldType == typeof(string))
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(value)))
                    {
                        return value;
                    }
                    else
                    {
                        throw new NullReferenceException();
                    }
                }
                else
                {
                    if (value != null)
                    {
                        return value;
                    }
                    else
                    {
                        throw new NullReferenceException();
                    }
                }
            }
        }
    }
}
