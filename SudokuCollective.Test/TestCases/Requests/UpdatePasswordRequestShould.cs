using System;
using NUnit.Framework;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Requests
{
    public class UpdatePasswordRequestShould
    {
        private UpdatePasswordRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new UpdatePasswordRequest();
        }

        [Test, Category("Requests")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserId, Is.InstanceOf<int>());
            Assert.That(sut.NewPassword, Is.InstanceOf<string>());
            Assert.That(sut.License, Is.InstanceOf<string>());
        }

        [Test, Category("Requests")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new UpdatePasswordRequest();

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdatePasswordRequest>());
        }

        [Test, Category("Requests")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new UpdatePasswordRequest(TestObjects.GetLicense(), 1, "T3stPass0rd?1");

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdatePasswordRequest>());
        }

        [Test, Category("Requests")]
        public void ThrowExceptionForInvalidLicenses()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.License = "InvalidLicense");
        }

        [Test, Category("Requests")]
        public void ThrowExceptionForInvalidPasswords()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.NewPassword = "invalidpassword");
        }
    }
}
