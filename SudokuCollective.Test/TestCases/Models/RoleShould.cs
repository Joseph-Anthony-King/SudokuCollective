using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class RoleShould
    {
        private IRole sut;

        [SetUp]
        public void Setup()
        {
            sut = new Role();
        }

        [Test, Category("Models")]
        public void ImplementIDomainEntity()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut, Is.InstanceOf<IDomainEntity>());
        }

        [Test, Category("Models")]
        public void HaveAnID()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Id, Is.TypeOf<int>());
            Assert.That(sut.Id, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void HasANameValue()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Name, Is.InstanceOf<string>());
        }

        [Test, Category("Models")]
        public void HasARoleLevel()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.RoleLevel, Is.InstanceOf<RoleLevel>());
        }

        [Test, Category("Models")]
        public void HasAListOfUsers()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut
                .Users
                .ToList()
                .ConvertAll(ur => (UserRole)ur), Is.InstanceOf<List<UserRole>>());
        }

        [Test, Category("Models")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new Role();

            // Assert
            Assert.That(sut, Is.InstanceOf<Role>());
        }

        [Test, Category("Models")]
        public void HasAJsonConstructor()
        {
            // Arrange and Act
            sut = new Role(0, string.Empty, RoleLevel.NULL);

            // Assert
            Assert.That(sut, Is.InstanceOf<Role>());
        }
    }
}
