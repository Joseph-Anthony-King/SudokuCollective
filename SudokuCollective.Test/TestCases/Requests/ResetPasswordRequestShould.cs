using System;
using NUnit.Framework;
using SudokuCollective.Data.Models.Requests;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class ResetPasswordRequestShould
    {
        private ResetPasswordRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new ResetPasswordRequest();
        }

        [Test, Category("Requests")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.NewPassword, Is.InstanceOf<string>());
        }

        [Test, Category("Requests")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            Assert.That(sut, Is.InstanceOf<ResetPasswordRequest>());
        }

        [Test, Category("Requests")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            Assert.That(sut, Is.InstanceOf<ResetPasswordRequest>());
        }

        [Test, Category("Requests")]
        public void ThrowExceptionForInvalidEmails()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.NewPassword = "invalidpassword");
        }
    }
}
