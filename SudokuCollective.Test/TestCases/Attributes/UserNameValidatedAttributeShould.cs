using NUnit.Framework;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Test.TestCases.Attributes
{
    public class UserNameValidatedAttributeShould
    {
        private UserNameValidatedAttribute sut;

        [SetUp]
        public void Setup()
        {
            sut = new UserNameValidatedAttribute();
        }

        [Test, Category("Attributes")]
        public void HaveAtLeast4Characters()
        {
            // Arrange
            var userName = "JoeK";

            // Act
            var result = sut.IsValid(userName);

            // Assert
            Assert.That(userName.Length, Is.EqualTo(4));
            Assert.That(result, Is.True);
        }

        [Test, Category("Attributes")]
        public void RejectStringsOfLessThen3Characters()
        {
            // Arrange
            var userName = "Joe";

            // Act
            var result = sut.IsValid(userName);

            // Assert
            Assert.That(userName.Length, Is.EqualTo(3));
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void AcceptsAlphaNumericAndSpecialCharacters()
        {
            // Arrange
            var userName = "Joe!@#$%^&*(),.?<>+-";

            // Act
            var result = sut.IsValid(userName);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
