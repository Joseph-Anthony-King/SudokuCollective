using NUnit.Framework;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Test.TestCases.Attributes
{
    public class GuidValidatedAttributeShould
    {
        private GuidValidatedAttribute sut;

        [SetUp]
        public void Setup()
        {
            sut = new GuidValidatedAttribute();
        }

        [Test, Category("Attributes")]
        public void AcceptsProperlyFormattedGUIDs()
        {
            // Arrange
            var guidString = "d36ddcfd-5161-4c20-80aa-b312ef161433";

            // Act
            var result = sut.IsValid(guidString);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Attributes")]
        public void AcceptsLowerCaseCharsFormattedGUIDs()
        {
            // Arrange
            var guidString = "d36ddcfd-5161-4c20-80aa-b312ef161433";
            var allLowerCase = true;

            // Act
            var result = sut.IsValid(guidString);

            foreach (var c in guidString)
            {
                if (char.IsUpper(c) && !char.IsDigit(c) && c != '-')
                {
                    allLowerCase = false;
                }
            }

            // Assert
            Assert.That(result, Is.True);
            Assert.That(allLowerCase, Is.True);
        }

        [Test, Category("Attributes")]
        public void WorksRegardlessOfCapitalization()
        {
            // Arrange
            var guidString = "D36DDCFD-5161-4C20-80AA-B312EF161433";
            var allCaps = true;

            // Act
            var result = sut.IsValid(guidString);

            foreach (var c in guidString)
            {
                if (char.IsLower(c) && !char.IsDigit(c) && c != '-')
                {
                    allCaps = false;
                }
            }

            // Assert
            Assert.That(result, Is.True);
            Assert.That(allCaps, Is.True);
        }

        [Test, Category("Attributes")]
        public void RequiresHyphens()
        {
            // Arrange
            var guidString = "d36ddcfd51614c2080aab312ef161433";

            // Act
            var result = sut.IsValid(guidString);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void RejectsNonHexGUIDs()
        {
            // Arrange
            var guidString = "h36ddcfd-5161-4c20-80aa-b312ef161433";

            // Act
            var result = sut.IsValid(guidString);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
