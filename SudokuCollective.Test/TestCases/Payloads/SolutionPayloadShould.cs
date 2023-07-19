using System;
using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class SolutionPayloadShould
    {
        private ISolutionPayload sut;

        [SetUp]
        public void Setup()
        {
            sut = TestObjects.GetValidSolutionPayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.FirstRow, Is.InstanceOf<List<int>>());
            Assert.That(sut.SecondRow, Is.InstanceOf<List<int>>());
            Assert.That(sut.ThirdRow, Is.InstanceOf<List<int>>());
            Assert.That(sut.FourthRow, Is.InstanceOf<List<int>>());
            Assert.That(sut.FifthRow, Is.InstanceOf<List<int>>());
            Assert.That(sut.SixthRow, Is.InstanceOf<List<int>>());
            Assert.That(sut.SeventhRow, Is.InstanceOf<List<int>>());
            Assert.That(sut.EighthRow, Is.InstanceOf<List<int>>());
            Assert.That(sut.NinthRow, Is.InstanceOf<List<int>>());
        }

        [Test, Category("Payloads")]
        public void OnlyAcceptsValidLists()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.FirstRow.Count, Is.EqualTo(9));
            Assert.That(sut.SecondRow.Count, Is.EqualTo(9));
            Assert.That(sut.ThirdRow.Count, Is.EqualTo(9));
            Assert.That(sut.FourthRow.Count, Is.EqualTo(9));
            Assert.That(sut.FifthRow.Count, Is.EqualTo(9));
            Assert.That(sut.SixthRow.Count, Is.EqualTo(9));
            Assert.That(sut.SeventhRow.Count, Is.EqualTo(9));
            Assert.That(sut.EighthRow.Count, Is.EqualTo(9));
            Assert.That(sut.NinthRow.Count, Is.EqualTo(9));
        }

        [Test, Category("Payloads")]
        public void ThrowsExceptionForInvalidRows()
        {
            // Arrange and Act

            // Assert and Act
            Assert.Throws<ArgumentException>(() => TestObjects.GetInvalidSolutionPayload());
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new SolutionPayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<SolutionPayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsIntArrays()
        {
            // Arrange and Act
            sut = new SolutionPayload(
                new int[9],
                new int[9],
                new int[9],
                new int[9],
                new int[9],
                new int[9],
                new int[9],
                new int[9],
                new int[9]);

            // Assert
            Assert.That(sut, Is.InstanceOf<SolutionPayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsIntLists()
        {
            // Arrange and Act
            sut = new SolutionPayload(
                new List<int> { 7, 8, 5, 4, 1, 3, 2, 9, 6 },
                new List<int> { 1, 4, 2, 8, 6, 9, 5, 7, 3 },
                new List<int> { 6, 9, 3, 2, 7, 5, 4, 1, 8 },
                new List<int> { 5, 1, 4, 3, 8, 2, 7, 6, 9 },
                new List<int> { 2, 6, 7, 9, 4, 1, 3, 8, 5 },
                new List<int> { 8, 3, 9, 7, 5, 6, 1, 2, 4 },
                new List<int> { 4, 2, 1, 6, 3, 8, 9, 5, 7 },
                new List<int> { 3, 5, 8, 1, 9, 7, 6, 4, 2 },
                new List<int> { 9, 7, 6, 5, 2, 4, 8, 3, 1 });

            // Assert
            Assert.That(sut, Is.InstanceOf<SolutionPayload>());
        }
    }
}
