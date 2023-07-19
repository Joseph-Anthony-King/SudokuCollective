using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class CreateRolePayloadShould
    {
        private ICreateRolePayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new CreateRolePayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Name, Is.InstanceOf<string>());
            Assert.That(sut.RoleLevel, Is.InstanceOf<RoleLevel>());
        }

        [Test, Category("Payloads")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new CreateRolePayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<CreateRolePayload>());
        }

        [Test, Category("Payloads")]
        public void HaveAConstructorThatAcceptsIntsForEnums()
        {
            // Arrange and Act
            sut = new CreateRolePayload(
                "name",
                3);

            // Assert
            Assert.That(sut, Is.InstanceOf<CreateRolePayload>());
        }

        [Test, Category("Payloads")]
        public void HaveAConstructorThatAcceptsEnums()
        {
            // Arrange and Act
            sut = new CreateRolePayload(
                "name",
                RoleLevel.USER);

            // Assert
            Assert.That(sut, Is.InstanceOf<CreateRolePayload>());
        }
    }
}
