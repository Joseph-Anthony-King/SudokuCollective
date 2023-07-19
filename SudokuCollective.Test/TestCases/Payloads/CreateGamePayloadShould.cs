using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class CreateGamePayloadShould
    {
        private ICreateGamePayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new CreateGamePayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.DifficultyId, Is.InstanceOf<int>());
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new CreateGamePayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<CreateGamePayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new CreateGamePayload(2);

            // Assert
            Assert.That(sut, Is.InstanceOf<CreateGamePayload>());
        }
    }
}
