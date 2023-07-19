using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Data.Validation.Attributes;

namespace SudokuCollective.Test.TestCases.Attributes
{
    public class RowValidatedAttributeShould
    {
        private RowValidatedAttribute sut;

        [SetUp]
        public void Setup()
        {
            sut = new RowValidatedAttribute();
        }

        [Test, Category("Attributes")]
        public void AcceptsValidSudokuRows()
        {
            // Arrange
            var row = new List<int> { 9, 1, 7, 5, 6, 8, 3, 2, 4 };

            // Act
            var result = sut.IsValid(row);

            // Assert
            Assert.That(row, Is.InstanceOf<List<int>>());
            Assert.That(result, Is.True);
        }

        [Test, Category("Attributes")]
        public void RejectSudokuRowsWithDuplicateValues()
        {
            // Arrange
            var row = new List<int> { 9, 1, 7, 5, 6, 8, 3, 2, 9 };

            // Act
            var result = sut.IsValid(row);

            // Assert
            Assert.That(row, Is.InstanceOf<List<int>>());
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void RejectsSudokuRowsWithCountsLessThanNine()
        {
            // Arrange
            var row = new List<int> { 9, 1, 7, 5, 6, 8, 3, 2 };

            // Act
            var result = sut.IsValid(row);

            // Assert
            Assert.That(row, Is.InstanceOf<List<int>>());
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void RejectsSudokuRowsWithCountsMoreThanNine()
        {
            // Arrange
            var row = new List<int> { 9, 1, 7, 5, 6, 8, 3, 2, 4, 0 };

            // Act
            var result = sut.IsValid(row);

            // Assert
            Assert.That(row, Is.InstanceOf<List<int>>());
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void RejectsSudokuRowsWithNonIntValues()
        {
            // Arrange
            var row = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i' };

            // Act
            var result = sut.IsValid(row);

            // Assert
            Assert.That(row, Is.Not.InstanceOf<List<int>>());
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void RejectsNullValues()
        {
            // Arrange
            List<int> row = null;

            // Act
            var result = sut.IsValid(row);

            // Assert
            Assert.That(row, Is.Null);
            Assert.That(result, Is.False);
        }
    }
}
