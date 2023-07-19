using System.Text.RegularExpressions;
using NUnit.Framework;
using SudokuCollective.Core.Validation;

namespace SudokuCollective.Test.TestCases.RegularExpressions
{
    public class PasswordRegexShould
    {
        private Regex sut;

        [SetUp]
        public void Setup()
        {
            sut = new Regex(RegexValidators.PasswordRegexPattern);
        }

        [Test, Category("Regex")]
        public void HaveAtLeast4CharactersWithOneCapitalOneLowerOneNumericAndOneSpecialCharacter()
        {
            // Arrange
            var password = "P@s1";

            // Act
            var result = sut.IsMatch(password);

            // Assert
            Assert.That(password.Length, Is.EqualTo(4));
            Assert.That(result, Is.True);
        }

        [Test, Category("Regex")]
        public void RejectPasswordsOfLessThen3Characters()
        {
            // Arrange
            var password = "P@s";

            // Act
            var result = sut.IsMatch(password);

            // Assert
            Assert.That(password.Length, Is.EqualTo(3));
            Assert.That(result, Is.False);
        }

        [Test, Category("Regex")]
        public void HaveAPasswordLimitOf20Characters()
        {
            // Arrange
            var password = "P@s1abcdefghijklmnop";

            // Act
            var result = sut.IsMatch(password);

            // Assert
            Assert.That(password.Length, Is.EqualTo(20));
            Assert.That(result, Is.True);
        }

        [Test, Category("Regex")]
        public void RejectPasswordsOver20Characters()
        {
            // Arrange
            var password = "P@s1abcdefghijklmnop2";

            // Act
            var result = sut.IsMatch(password);

            // Assert
            Assert.That(password.Length, Is.EqualTo(21));
            Assert.That(result, Is.False);
        }

        [Test, Category("Regex")]
        public void AcceptsSpecialCharacters()
        {
            // Arrange
            var password = "P@s1!@#$%^&*+=?-_.,";

            // Act
            var result = sut.IsMatch(password);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
