using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.TestCases.Results
{
    public class AuthenticatedUserNameResultShould
    {
        private IAuthenticatedUserNameResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new AuthenticatedUserNameResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserName, Is.InstanceOf<string>());
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new AuthenticatedUserNameResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<AuthenticatedUserNameResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsAUserName()
        {
            // Arrange
            var userName = "userName";

            // Act
            sut = new AuthenticatedUserNameResult(userName);

            // Assert
            Assert.That(sut, Is.InstanceOf<AuthenticatedUserNameResult>());
        }
    }
}
