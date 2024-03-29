﻿using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Results
{
    public class AuthenticationResultShould
    {
        private IAuthenticationResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new AuthenticationResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.User, Is.InstanceOf<IUserDTO>());
            Assert.That(sut.Token, Is.InstanceOf<string>());
            Assert.That(sut.TokenExpirationDate, Is.InstanceOf<DateTime>());
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new AuthenticationResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<AuthenticationResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsParams()
        {
            // Arrange
            var userDTO = new UserDTO();
            var token = TestObjects.GetLicense();
            var expirationDate = new DateTime();

            // Act
            sut = new AuthenticationResult(userDTO, token, expirationDate);

            // Assert
            Assert.That(sut, Is.InstanceOf<AuthenticationResult>());
        }
    }
}
