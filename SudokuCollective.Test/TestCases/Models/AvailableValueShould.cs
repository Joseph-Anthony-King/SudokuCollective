using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class AvailableValueShould
    {
        private AvailableValue sut;

        [SetUp]
        public void Setup()
        {
            sut = new AvailableValue();
        }

        [Test, Category("Models")]
        public void DoesNotImplementIDomainEntity()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut, Is.Not.InstanceOf<IDomainEntity>());
        }

        [Test, Category("Models")]
        public void TracksIntegerAndBooleanFields()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Value, Is.InstanceOf<int>());
            Assert.That(sut.Available, Is.InstanceOf<bool>());
        }
    }
}
