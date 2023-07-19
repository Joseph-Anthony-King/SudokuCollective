using System;
using System.ComponentModel.DataAnnotations;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Utilities;

namespace SudokuCollective.Test.TestCases.Utilities
{
    public class CoreUtilitiesShould
    {
        Mock<ValidationAttribute> mockedValidator;

        [SetUp]
        public void SetUp()
        {
            mockedValidator = new Mock<ValidationAttribute>();

            mockedValidator.Setup(validator =>
                validator.IsValid(It.IsAny<object>()))
                .Returns(true);
        }

        [Test, Category("Utilities")]
        public void SetStringFields()
        {
            // Arrange
            var field = string.Empty;
            var value = "value";

            // Act
            field = CoreUtilities.SetField(value, mockedValidator.Object, "Error Message");

            // Assert
            Assert.That(string.IsNullOrEmpty(field), Is.False);
            Assert.That(string.Equals(field, value), Is.True);
        }

        [Test, Category("Utilities")]
        public void SetNonStringFields()
        {
            // Arrange
            var field = 0;
            var value = 100;

            // Act
            field = CoreUtilities.SetField(value, mockedValidator.Object, "Error Message");

            // Assert
            Assert.That(field, Is.EqualTo(100));
        }

        [Test, Category("Utilities")]
        public void SetFieldsWithoutValidation()
        {
            // Arrange
            var field = string.Empty;
            var value = "value";

            // Act
            field = CoreUtilities.SetField(value);

            // Assert
            Assert.That(string.IsNullOrEmpty(field), Is.False);
            Assert.That(string.Equals(field, value), Is.True);
        }

        [Test, Category("Utilities")]
        public void DoNothingWithNullFields()
        {
            // Arrange
            var field = "field";
            string value = null;

            // Act
            Assert.Throws<ArgumentException>(
                () => field = CoreUtilities.SetField(
                    value, 
                    mockedValidator.Object, 
                    "Error Message"));
        }

        [Test, Category("Utilities")]
        public void DoNothingWithNullableFieldsWithoutValidation()
        {
            // Arrange
            var field = "field";
            string value = null;

            // Act
            Assert.Throws<NullReferenceException>(
                () => field = CoreUtilities.SetField(value));
        }
    }
}
