using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class CreateDifficultyPayloadShould
    {
        private ICreateDifficultyPayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new CreateDifficultyPayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.Name, Is.InstanceOf<string>());
            Assert.That(sut.DisplayName, Is.InstanceOf<string>());
            Assert.That(sut.DifficultyLevel, Is.InstanceOf<DifficultyLevel>());
        }

        [Test, Category("Payloads")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new CreateDifficultyPayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<CreateDifficultyPayload>());
        }

        [Test, Category("Payloads")]
        public void HaveAConstructorThatAcceptsIntsForEnums()
        {
            // Arrange and Act
            sut = new CreateDifficultyPayload(
                "name",
                "displayName",
                2);

            // Assert
            Assert.That(sut, Is.InstanceOf<CreateDifficultyPayload>());
        }

        [Test, Category("Payloads")]
        public void HaveAConstructorThatAcceptsEnums()
        {
            // Arrange and Act
            sut = new CreateDifficultyPayload(
                "name",
                "displayName",
                DifficultyLevel.EASY);

            // Assert
            Assert.That(sut, Is.InstanceOf<CreateDifficultyPayload>());
        }
    }
}
