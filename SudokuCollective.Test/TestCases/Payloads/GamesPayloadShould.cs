using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class GamesPayloadShould
    {
        private IGamesPayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new GamesPayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.UserId, Is.InstanceOf<int>());
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new GamesPayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<GamesPayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new GamesPayload(1);

            // Assert
            Assert.That(sut, Is.InstanceOf<GamesPayload>());
        }
    }
}
