using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Requests
{
    public class ResendEmailConfirmationRequestShould
    {
        private IResendEmailConfirmationRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new ResendEmailConfirmationRequest();
        }

        [Test, Category("Requests")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.License, Is.InstanceOf<string>());
            Assert.That(sut.RequestorId, Is.InstanceOf<int>());
            Assert.That(sut.AppId, Is.InstanceOf<int>());
        }

        [Test, Category("Requests")]
        public void ThrowsAnExceptionIfLicenseIsInvalid()
        {
            // Arrange

            // Assert and Assert
            Assert.Throws<ArgumentException>(() => sut.License = "invalidLicense");
        }

        [Test, Category("Requests")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new ResendEmailConfirmationRequest();

            // Assert
            Assert.That(sut, Is.InstanceOf<ResendEmailConfirmationRequest>());
        }

        [Test, Category("Requests")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new ResendEmailConfirmationRequest(TestObjects.GetLicense(), 0, 0);

            // Assert
            Assert.That(sut, Is.InstanceOf<ResendEmailConfirmationRequest>());
        }
    }
}
