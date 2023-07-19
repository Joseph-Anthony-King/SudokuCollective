using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Payloads
{
    internal class UpdateUserRolePayloadShould
    {
        private IUpdateUserRolePayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new UpdateUserRolePayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.RoleIds, Is.InstanceOf<List<int>>());
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new UpdateUserRolePayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdateUserRolePayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsIntArray()
        {
            // Arrange
            var intArray = new int[1];

            // Act
            sut = new UpdateUserRolePayload(intArray);

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdateUserRolePayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsIntList()
        {
            // Arrange
            var intList = new List<int> { 1 };

            // Act
            sut = new UpdateUserRolePayload(intList);

            // Assert
            Assert.That(sut, Is.InstanceOf<UpdateUserRolePayload>());
        }
    }
}
