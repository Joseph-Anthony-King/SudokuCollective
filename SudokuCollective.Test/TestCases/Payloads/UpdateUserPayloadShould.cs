using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class UpdateUserPayloadShould
    {
        private IUpdateUserPayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new UpdateUserPayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserName, Is.InstanceOf<string>());
            Assert.That(sut.FirstName, Is.InstanceOf<string>());
            Assert.That(sut.LastName, Is.InstanceOf<string>());
            Assert.That(sut.NickName, Is.InstanceOf<string>());
            Assert.That(sut.Email, Is.InstanceOf<string>());
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new UpdateUserPayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdateUserPayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new UpdateUserPayload(
                "UserName",
                "FirstName",
                "LastName",
                "NickName",
                "testEmail@example.com");

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdateUserPayload>());
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
