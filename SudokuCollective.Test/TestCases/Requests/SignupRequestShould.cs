using System;
using NUnit.Framework;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Requests
{
    public class SignupRequestShould
    {
        private SignupRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new SignupRequest();
        }

        [Test, Category("Requests")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.License, Is.InstanceOf<string>());
            Assert.That(sut.UserName, Is.InstanceOf<string>());
            Assert.That(sut.FirstName, Is.InstanceOf<string>());
            Assert.That(sut.LastName, Is.InstanceOf<string>());
            Assert.That(sut.NickName, Is.InstanceOf<string>());
            Assert.That(sut.Email, Is.InstanceOf<string>());
            Assert.That(sut.Password, Is.InstanceOf<string>());
        }

        [Test, Category("Requests")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new SignupRequest();

            // Assert
            Assert.That(sut, Is.InstanceOf<SignupRequest>());
        }

        [Test, Category("Requests")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new SignupRequest(
                TestObjects.GetLicense(),
                "UserName",
                "FirstName",
                "LastName",
                "Nickname",
                "email@example.com",
                "T3stPass0rd?1");

            // Assert
            Assert.That(sut, Is.InstanceOf<SignupRequest>());
        }

        [Test, Category("Requests")]
        public void ThrowExceptionForInvalidLicenses()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.License = "InvalidLicense");
        }

        [Test, Category("Requests")]
        public void ThrowExceptionForInvalidEmails()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.Email = "invalidpassword");
        }
    }
}
