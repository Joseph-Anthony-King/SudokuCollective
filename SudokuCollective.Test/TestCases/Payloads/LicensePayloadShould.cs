using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class LicensePayloadShould
    {
        private ILicensePayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new LicensePayload();
        }

        [Test, Category("Requests")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Name, Is.InstanceOf<string>());
            Assert.That(sut.OwnerId, Is.InstanceOf<int>());
            Assert.That(sut.LocalUrl, Is.InstanceOf<string>());
            Assert.That(sut.TestUrl, Is.InstanceOf<string>());
            Assert.That(sut.StagingUrl, Is.InstanceOf<string>());
            Assert.That(sut.ProdUrl, Is.InstanceOf<string>());
        }

        [Test, Category("Requests")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new LicensePayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<LicensePayload>());
        }
    }
}
