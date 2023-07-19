using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Results
{
    public class InitiatePasswordResetResultShould
    {
        private IInitiatePasswordResetResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new InitiatePasswordResetResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.App, Is.InstanceOf<IApp>());
            Assert.That(sut.User, Is.InstanceOf<IUser>());
            Assert.That(sut.ConfirmationEmailSuccessfullySent, Is.Null);
            Assert.That(sut.Token, Is.InstanceOf<string>());
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new InitiatePasswordResetResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<InitiatePasswordResetResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsParams()
        {
            // Arrange

            // Act
            sut = new InitiatePasswordResetResult(
                new App(),
                new User(),
                false,
                TestObjects.GetToken());

            // Assert
            Assert.That(sut, Is.InstanceOf<InitiatePasswordResetResult>());
        }
    }
}
