using NUnit.Framework;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Test.TestCases.Attributes
{
    public class PasswordValidatedAttributeShould
    {
        private PasswordValidatedAttribute sut;

        [SetUp]
        public void Setup()
        {
            sut = new PasswordValidatedAttribute();
        }

        [Test, Category("Attributes")]
        public void HaveAtLeast4CharactersWithOneCapitalOneLowerOneNumericAndOneSpecialCharacter()
        {
            // Arrange
            var password = "P@s1";

            // Act
            var result = sut.IsValid(password);

            // Assert
            Assert.That(password.Length, Is.EqualTo(4));
            Assert.That(result, Is.True);
        }

        [Test, Category("Attributes")]
        public void RejectPasswordsOfLessThen3Characters()
        {
            // Arrange
            var password = "P@s";

            // Act
            var result = sut.IsValid(password);

            // Assert
            Assert.That(password.Length, Is.EqualTo(3));
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void HaveAPasswordLimitOf20Characters()
        {
            // Arrange
            var password = "P@s1abcdefghijklmnop";

            // Act
            var result = sut.IsValid(password);

            // Assert
            Assert.That(password.Length, Is.EqualTo(20));
            Assert.That(result, Is.True);
        }

        [Test, Category("Attributes")]
        public void RejectPasswordsOver20Characters()
        {
            // Arrange
            var password = "P@s1abcdefghijklmnop2";

            // Act
            var result = sut.IsValid(password);

            // Assert
            Assert.That(password.Length, Is.EqualTo(21));
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void AcceptsSpecialCharacters()
        {
            // Arrange
            var password = "P@s1!@#$%^&*+=?-_.,";

            // Act
            var result = sut.IsValid(password);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
