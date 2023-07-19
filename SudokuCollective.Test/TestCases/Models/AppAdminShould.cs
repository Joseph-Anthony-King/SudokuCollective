using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class AppAdminShould
    {
        private IAppAdmin sut;

        [SetUp]
        public void Setup()
        {
            sut = new AppAdmin();
        }

        [Test, Category("Models")]
        public void ImplementIDomainEntity()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut, Is.InstanceOf<IDomainEntity>());
        }

        [Test, Category("Models")]
        public void HaveAnID()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Id, Is.TypeOf<int>());
            Assert.That(sut.Id, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.AppId, Is.TypeOf<int>());
            Assert.That(sut.UserId, Is.TypeOf<int>());
            Assert.That(sut.IsActive, Is.TypeOf<bool>());
        }

        [Test, Category("Models")]
        public void DefaultToIsActiveTrueStatus()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.IsActive, Is.True);
        }
    }
}
