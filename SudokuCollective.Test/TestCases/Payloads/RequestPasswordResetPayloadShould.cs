using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class RequestPasswordResetPayloadShould
    {
        private IRequestPasswordResetPayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new RequestPasswordResetPayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.License, Is.InstanceOf<string>());
            Assert.That(sut.Email, Is.InstanceOf<string>());
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new RequestPasswordResetPayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<RequestPasswordResetPayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new RequestPasswordResetPayload(TestObjects.GetLicense(), "testEmail@example.com");

            // Assert
            Assert.That(sut, Is.InstanceOf<RequestPasswordResetPayload>());
        }

        [Test, Category("Payloads")]
        public void ThrowsExceptionForInvalidLicenses()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.License = "InvalidLicense");
        }

        [Test, Category("Payloads")]
        public void ThrowsExceptionForInvalidEmails()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.Email = "InvalidEmail@");
        }
    }
}
