using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
using SudokuCollective.Data.Models.Requests;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class AnnonymousGameRequestShould
    {
        private IAnnonymousGameRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new AnnonymousGameRequest();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.DifficultyLevel, Is.InstanceOf<DifficultyLevel>());
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new AnnonymousGameRequest();

            // Assert
            Assert.That(sut, Is.InstanceOf<AnnonymousGameRequest>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsInts()
        {
            // Arrange and Act
            sut = new AnnonymousGameRequest(1);

            // Assert
            Assert.That(sut, Is.InstanceOf<AnnonymousGameRequest>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsDifficultyLevels()
        {
            // Arrange and Act
            sut = new AnnonymousGameRequest(DifficultyLevel.EASY);

            // Assert
            Assert.That(sut, Is.InstanceOf<AnnonymousGameRequest>());
        }
    }
}
