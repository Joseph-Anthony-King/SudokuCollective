using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Results
{
    public class UserResultShould
    {
        private IUserResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new UserResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.User, Is.InstanceOf<User>());
            Assert.That(sut.ConfirmationEmailSuccessfullySent, Is.Null);
            Assert.That(sut.Token, Is.InstanceOf<string>());
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new UserResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<UserResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsParams()
        {
            // Arrange

            // Act
            sut = new UserResult(
                new User(),
                false,
                TestObjects.GetToken());

            // Assert
            Assert.That(sut, Is.InstanceOf<UserResult>());
        }
    }
}
