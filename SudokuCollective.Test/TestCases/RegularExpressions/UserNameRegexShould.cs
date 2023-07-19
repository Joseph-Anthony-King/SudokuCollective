using System.Text.RegularExpressions;
using NUnit.Framework;
using SudokuCollective.Core.Validation;

namespace SudokuCollective.Test.TestCases.RegularExpressions
{
    public class UserNameRegexShould
    {
        private Regex sut;

        [SetUp]
        public void Setup()
        {
            sut = new Regex(RegexValidators.UserNameRegexPattern);
        }

        [Test, Category("Regex")]
        public void HaveAtLeast4Characters()
        {
            // Arrange
            var userName = "JoeK";

            // Act
            var result = sut.IsMatch(userName);

            // Assert
            Assert.That(userName.Length, Is.EqualTo(4));
            Assert.That(result, Is.True);
        }

        [Test, Category("Regex")]
        public void RejectStringsOfLessThen3Characters()
        {
            // Arrange
            var userName = "Joe";

            // Act
            var result = sut.IsMatch(userName);

            // Assert
            Assert.That(userName.Length, Is.EqualTo(3));
            Assert.That(result, Is.False);
        }

        [Test, Category("Regex")]
        public void AcceptsAlphaNumericAndSpecialCharacters()
        {
            // Arrange
            var userName = "Joe!@#$%^&*(),.?<>+-";

            // Act
            var result = sut.IsMatch(userName);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
