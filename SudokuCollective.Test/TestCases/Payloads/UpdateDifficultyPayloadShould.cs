using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class UpdateDifficultyPayloadShould
    {
        private IUpdateDifficultyPayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new UpdateDifficultyPayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.Id, Is.InstanceOf<int>());
            Assert.That(sut.Name, Is.InstanceOf<string>());
            Assert.That(sut.DisplayName, Is.InstanceOf<string>());
        }

        [Test, Category("Payloads")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new UpdateDifficultyPayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdateDifficultyPayload>());
        }

        [Test, Category("Payloads")]
        public void HaveAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new UpdateDifficultyPayload(
                1,
                "name",
                "displayName");

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdateDifficultyPayload>());
        }
    }
}
