using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class PasswordResetShould
    {
        private IPasswordReset sut;

        [SetUp]
        public void Setup()
        {
            sut = new PasswordReset();
        }

        [Test, Category("Models")]
        public void ImplementIDomainEntity()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut, Is.InstanceOf<IDomainEntity>());
        }

        [Test, Category("Models")]
        public void HaveAnID()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Id, Is.TypeOf<int>());
            Assert.That(sut.Id, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void HaveAllRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserId, Is.TypeOf<int>());
            Assert.That(sut.AppId, Is.TypeOf<int>());
            Assert.That(sut.Token, Is.TypeOf<string>());
            Assert.That(sut.DateCreated, Is.TypeOf<DateTime>());
        }

        [Test, Category("Models")]
        public void HasAConstructorThatAcceptsUserIdAndAppId()
        {
            // Arrange
            var userId = 1;
            var appId = 2;

            // Act
            sut = new PasswordReset(userId, appId);

            // Assert
            Assert.That(sut, Is.TypeOf<PasswordReset>());
            Assert.That(sut.UserId, Is.EqualTo(1));
            Assert.That(sut.AppId, Is.EqualTo(2));
        }

        [Test, Category("Models")]
        public void HasAJsonConstructor()
        {
            // Arrange
            var id = 1;
            var userId = 1;
            var appId = 2;
            var token = TestData.TestObjects.GetToken();
            var dateCreated = DateTime.UtcNow;

            // Act
            sut = new PasswordReset(
                id,
                userId, 
                appId,
                token,
                dateCreated);

            // Assert
            Assert.That(sut, Is.TypeOf<PasswordReset>());
        }
    }
}