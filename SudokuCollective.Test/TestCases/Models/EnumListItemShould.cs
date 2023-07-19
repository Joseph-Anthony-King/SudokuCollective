using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;
using SudokuCollective.Data.Models.Values;

namespace SudokuCollective.Test.TestCases.Models
{
    public class EnumListItemShould
    {
        private IEnumListItem sut;

        [SetUp]
        public void Setup()
        {
            sut = new EnumListItem();
        }

        [Test, Category("Models")]
        public void HaveALabel()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Label, Is.TypeOf<string>());
        }

        [Test, Category("Models")]
        public void HaveAValue()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Value, Is.InstanceOf<int>());
        }

        [Test, Category("Models")]
        public void HaveAListOfApplications()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.AppliesTo, Is.InstanceOf<IEnumerable<string>>());
        }
    }
}
