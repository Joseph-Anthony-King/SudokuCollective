using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.TestCases.Results
{
    public class LicenseResultShould
    {
        private ILicenseResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new LicenseResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.IsSuccess, Is.InstanceOf<bool>());
            Assert.That(sut.IsFromCache, Is.InstanceOf<bool>());
            Assert.That(sut.Message, Is.InstanceOf<string>());
            Assert.That(sut.License, Is.InstanceOf<string>());
            Assert.That(sut.Payload, Is.InstanceOf<List<object>>());
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new LicenseResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<LicenseResult>());
            Assert.That(sut, Is.InstanceOf<IResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsParams()
        {
            // Arrange

            // Act
            sut = new LicenseResult(
                false,
                false,
                "message",
                "license",
                new List<object>());

            // Assert
            Assert.That(sut, Is.InstanceOf<LicenseResult>());
        }
    }
}
