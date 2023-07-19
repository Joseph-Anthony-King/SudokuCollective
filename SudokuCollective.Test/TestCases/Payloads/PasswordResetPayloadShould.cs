using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class PasswordResetPayloadShould
    {
        private IPasswordResetPayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new PasswordResetPayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserId, Is.InstanceOf<int>());
            Assert.That(sut.NewPassword, Is.InstanceOf<string>());
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new PasswordResetPayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<PasswordResetPayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new PasswordResetPayload(1, "T3stPass0rd?1");

            // Assert
            Assert.That(sut, Is.InstanceOf<PasswordResetPayload>());
        }

        [Test, Category("Payloads")]
        public void ThrowsExceptionForInvalidPasswords()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.NewPassword = "invalidpassword");
        }
    }
}
