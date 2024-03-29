using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Results
{
    public class UserCreatedResultShould
    {
        private IUserCreatedResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new UserCreatedResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.User, Is.InstanceOf<IUserDTO>());
            Assert.That(sut.Token, Is.InstanceOf<string>());
            Assert.That(sut.TokenExpirationDate, Is.InstanceOf<DateTime>());
            Assert.That(sut.EmailConfirmationSent, Is.InstanceOf<bool>());
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new UserCreatedResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<UserCreatedResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsParams()
        {
            // Arrange
            var userDTO = new UserDTO();
            var token = TestObjects.GetLicense();

            // Act
            sut = new UserCreatedResult(
                userDTO, 
                token, 
                new DateTime(),
                true);

            // Assert
            Assert.That(sut, Is.InstanceOf<UserCreatedResult>());
        }
    }
}