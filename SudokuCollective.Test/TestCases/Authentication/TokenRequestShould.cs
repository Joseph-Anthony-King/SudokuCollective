using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.LoginModels;
using SudokuCollective.Data.Models.Authentication;
using SudokuCollective.Test.TestData;
using System;

namespace SudokuCollective.Test.TestCases.Authentication
{
    public class TokenRequestShould
    {
        private ILoginRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new LoginRequest();
        }

        [Test, Category("Authentication")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserName, Is.InstanceOf<string>());
            Assert.That(sut.Password, Is.InstanceOf<string>());
            Assert.That(sut.License, Is.InstanceOf<string>());
        }

        [Test, Category("Authentication")]
        public void HasADefaultConstrutor()
        {
            // Arrange and Act
            sut = new LoginRequest();

            // Assert
            Assert.That(sut, Is.InstanceOf<LoginRequest>());
        }

        [Test, Category("Authentication")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new LoginRequest(
                TestObjects.GetToken(),
                "username", 
                "T3stP4ssw0rd$");

            // Assert
            Assert.That(sut, Is.InstanceOf<LoginRequest>());
        }

        [Test, Category("Authentication")]
        public void ThrowAnExceptionIfUserNameIsInvalid()
        {
            // Arrange
            
            // Act and Assert
            Assert.Throws<ArgumentException>(
                () => new LoginRequest( 
                    TestObjects.GetToken(),
                    "usn", 
                    "T3stP4ssw0rd$"));
        }

        [Test, Category("Authentication")]
        public void ThrowAnExceptionIfPasswordIsInvalid()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(
                () => new LoginRequest(
                    TestObjects.GetToken(),
                    "username",
                    "test"));
        }

        [Test, Category("Authentication")]
        public void ThrowAnExceptionIfLicenseIsInvalid()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(
                () => new LoginRequest(
                    "license",
                    "username",
                    "test"));
        }
    }
}
