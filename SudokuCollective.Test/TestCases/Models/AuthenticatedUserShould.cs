using System;
using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class AuthenticatedUserShould
    {
        private IAuthenticatedUser sut;

        [SetUp]
        public void Setup()
        {
            sut = new AuthenticatedUser();
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
            Assert.That(sut.UserName, Is.TypeOf<string>());
            Assert.That(sut.FirstName, Is.TypeOf<string>());
            Assert.That(sut.LastName, Is.TypeOf<string>());
            Assert.That(sut.NickName, Is.TypeOf<string>());
            Assert.That(sut.FullName, Is.TypeOf<string>());
            Assert.That(sut.Email, Is.TypeOf<string>());
            Assert.That(sut.IsEmailConfirmed, Is.TypeOf<bool>());
            Assert.That(sut.ReceivedRequestToUpdateEmail, Is.TypeOf<bool>());
            Assert.That(sut.ReceivedRequestToUpdatePassword, Is.TypeOf<bool>());
            Assert.That(sut.IsActive, Is.TypeOf<bool>());
            Assert.That(sut.IsSuperUser, Is.TypeOf<bool>());
            Assert.That(sut.IsAdmin, Is.TypeOf<bool>());
            Assert.That(sut.DateCreated, Is.TypeOf<DateTime>());
            Assert.That(sut.DateUpdated, Is.TypeOf<DateTime>());
            Assert.That(((AuthenticatedUser)sut).Games, Is.TypeOf<List<Game>>());
        }
    }
}
