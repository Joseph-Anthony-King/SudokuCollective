using System.Text.RegularExpressions;
using NUnit.Framework;
using SudokuCollective.Core.Validation;

namespace SudokuCollective.Test.TestCases.RegularExpressions
{
    internal class UrlRegexShould
    {
        private Regex sut;

        [SetUp]
        public void Setup()
        {
            sut = new Regex(RegexValidators.UrlRegexPattern);
        }

        [Test, Category("Regex")]
        public void PermitHttpUrls()
        {
            // Arrange
            var url = "http://www.example.com";

            // Act
            var result = sut.IsMatch(url);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Regex")]
        public void PermitHttpsUrls()
        {
            // Arrange
            var url = "https://www.example.com";

            // Act
            var result = sut.IsMatch(url);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Regex")]
        public void RejectInvalidUrls()
        {
            // Arrange
            var url = "//example.com";

            // Act
            var result = sut.IsMatch(url);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
