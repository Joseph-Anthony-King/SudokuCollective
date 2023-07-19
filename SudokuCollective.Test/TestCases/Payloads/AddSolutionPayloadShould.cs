using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class AddSolutionPayloadShould
    {
        private IAddSolutionsPayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new AddSolutionsPayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Limit, Is.InstanceOf<int>());
        }

        [Test, Category("Payloads")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new AddSolutionsPayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<AddSolutionsPayload>());
        }

        [Test, Category("Payloads")]
        public void HaveAConstructorThatAcceptsIntsForEnums()
        {
            // Arrange and Act
            sut = new AddSolutionsPayload(100);

            // Assert
            Assert.That(sut, Is.InstanceOf<AddSolutionsPayload>());
        }
    }
}
