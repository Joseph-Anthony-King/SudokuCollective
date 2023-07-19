using NUnit.Framework;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Test.TestCases.Attributes
{
    public class EmailValidatedAttributeShould
    {
        private EmailValidatedAttribute sut;

        [SetUp]
        public void Setup()
        {
            sut = new EmailValidatedAttribute();
        }

        [Test, Category("Attributes")]
        public void AcceptsProperlyFormattedEmails()
        {
            // Arrange
            var email = "TestEmail@example.com";

            // Act
            var result = sut.IsValid(email);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Attributes")]
        public void RejectsImproperlyFormattedEmails()
        {
            // Arrange
            var email = "TestEmail@example";

            // Act
            var result = sut.IsValid(email);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void RejectsEmailsWithoutAtSign()
        {
            // Arrange
            var email = "TestEmailexample";

            // Act
            var result = sut.IsValid(email);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
