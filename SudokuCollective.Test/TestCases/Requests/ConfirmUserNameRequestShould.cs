using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Requests
{
    public class ConfirmUserNameRequestShould
    {
        private IConfirmUserNameRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new ConfirmUserNameRequest();
        }

        [Test, Category("Requests")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Email, Is.InstanceOf<string>());
            Assert.That(sut.License, Is.InstanceOf<string>());
        }

        [Test, Category("Requests")]
        public void ThrowsAnExceptionIfEmailIsInvalid()
        {
            // Arrange

            // Assert and Assert
            Assert.Throws<ArgumentException>(() => sut.Email = "invalidEmail@");
        }

        [Test, Category("Requests")]
        public void ThrowsAnExceptionIfLicenseIsInvalid()
        {
            // Arrange and Act

            // Assert and Assert
            Assert.Throws<ArgumentException>(() => sut.License = "invalidLicense");
        }

        [Test, Category("Requests")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new ConfirmUserNameRequest();

            // Assert
            Assert.That(sut, Is.InstanceOf<ConfirmUserNameRequest>());
        }

        [Test, Category("Requests")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new ConfirmUserNameRequest(TestObjects.GetLicense(), "testEmail@example.com");

            // Assert
            Assert.That(sut, Is.InstanceOf<ConfirmUserNameRequest>());
        }
    }
}
