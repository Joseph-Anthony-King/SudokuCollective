using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class UserRoleShould
    {
        private IUserRole sut;

        [SetUp]
        public void Setup()
        {
            sut = new UserRole();
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
        public void UserIdAndRoleIdAreInts()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserId, Is.TypeOf<int>());
            Assert.That(sut.RoleId, Is.TypeOf<int>());
        }

        [Test, Category("Models")]
        public void AcceptsReferenceToUserObjects()
        {
            // Arrange
            var user = new User();

            // Act
            sut.User = user;

            // Assert
            Assert.That(user, Is.InstanceOf<User>());
            Assert.That(((UserRole)sut).User, Is.InstanceOf<User>());
        }

        [Test, Category("Models")]
        public void AcceptsReferenceToAppObjects()
        {
            // Arrange
            var role = new Role();

            // Act
            sut.Role = role;

            // Assert
            Assert.That(role, Is.InstanceOf<Role>());
            Assert.That(((UserRole)sut).Role, Is.InstanceOf<Role>());
        }

        [Test, Category("Models")]
        public void AcceptsUserIdAndAppIdInConstructor()
        {
            // Arrange
            var userId = 2;
            var roleId = 3;

            // Act
            sut = new UserRole(userId, roleId);

            // Assert
            Assert.That(sut, Is.InstanceOf<UserRole>());
            Assert.That(sut.UserId, Is.EqualTo(userId));
            Assert.That(sut.RoleId, Is.EqualTo(roleId));
        }

        [Test, Category("Models")]
        public void HasAJsonConstructor()
        {
            // Arrange
            var id = 1;
            var userId = 2;
            var roleId = 3;

            // Act
            sut = new UserRole(id, userId, roleId);

            // Assert
            Assert.That(sut, Is.InstanceOf<UserRole>());
            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.UserId, Is.EqualTo(userId));
            Assert.That(sut.RoleId, Is.EqualTo(roleId));
        }
    }
}
