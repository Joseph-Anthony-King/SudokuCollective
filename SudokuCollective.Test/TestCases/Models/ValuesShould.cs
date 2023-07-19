using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;
using SudokuCollective.Data.Models.Values;

namespace SudokuCollective.Test.TestCases.Models
{
    public class ValuesServiceShould
    {
        private IValues sut;

        [SetUp]
        public void Setup()
        {
            sut = new Values();
        }

        [Test, Category("Models")]
        public void HasAListOfDifficulties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Difficulties, Is.TypeOf<List<IDifficulty>>());
        }

        [Test, Category("Models")]
        public void HasAListOfReleaseEnvironments()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.ReleaseEnvironments, Is.TypeOf<List<IEnumListItem>>());
        }

        [Test, Category("Models")]
        public void HasAListOfSortValues()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.SortValues, Is.TypeOf<List<IEnumListItem>>());
        }

        [Test, Category("Models")]
        public void HasAListOfTimeFrames()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.TimeFrames, Is.TypeOf<List<IEnumListItem>>());
        }
    }
}
