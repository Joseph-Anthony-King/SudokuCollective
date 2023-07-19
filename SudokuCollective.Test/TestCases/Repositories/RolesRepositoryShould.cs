using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Repositories
{
    public class RolesRepositoryShould
    {
        private DatabaseContext context;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<RolesRepository<Role>>> mockedLogger;
        private IRolesRepository<Role> sut;
        private Role newRole;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<RolesRepository<Role>>>();

            sut = new RolesRepository<Role>(
                context,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);

            newRole = new Role()
            {
                Name = "New Role",
                RoleLevel = RoleLevel.NULL
            };
        }

        [Test, Category("Repository")]
        public async Task CreateRoles()
        {
            // Arrange
            var testRole = context.Roles.FirstOrDefault(r => r.RoleLevel == RoleLevel.NULL);
            context.Roles.Remove(testRole);
            context.SaveChanges();

            // Act
            var result = await sut.AddAsync(newRole);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Role)result.Object, Is.InstanceOf<Role>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfCreateRolesFails()
        {
            // Arrange

            // Act
            var result = await sut.AddAsync(newRole);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetRolesById()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Role)result.Object, Is.InstanceOf<Role>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetByIdFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(5);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAllRoles()
        {
            // Arrange

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(r => (Role)r), Is.InstanceOf<List<Role>>());
        }

        [Test, Category("Repository")]
        public async Task UpdateRoles()
        {
            // Arrange
            var role = context.Roles.FirstOrDefault(r => r.Id == 1);
            role.Name = string.Format("{0} UPDATED!", role.Name);

            // Act
            var result = await sut.UpdateAsync(role);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Object, Is.InstanceOf<Role>());
            Assert.That(((Role)result.Object).Name, Is.EqualTo(role.Name));
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateRolesFails()
        {
            // Arrange

            // Act
            var result = await sut.UpdateAsync(newRole);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task DeleteRoles()
        {
            // Arrange
            var role = context.Roles.FirstOrDefault(r => r.Id == 1);

            // Act
            var result = await sut.DeleteAsync(role);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteRolesFails()
        {
            // Arrange

            // Act
            var result = await sut.DeleteAsync(newRole);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasARole()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmItHasARoleFails()
        {
            // Arrange
            var id = context
                .Roles
                .ToList()
                .OrderBy(r => r.Id)
                .Last<Role>()
                .Id + 1;

            // Act
            var result = await sut.HasEntityAsync(id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasARoleLevel()
        {
            // Arrange

            // Act
            var result = await sut.HasRoleLevelAsync(RoleLevel.NULL);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmItHasARoleLevelFails()
        {
            // Arrange
            var testRole = context.Roles.FirstOrDefault(r => r.RoleLevel == RoleLevel.NULL);
            context.Roles.Remove(testRole);
            context.SaveChanges();

            // Act
            var result = await sut.HasRoleLevelAsync(RoleLevel.NULL);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmIIfIdListIsValid()
        {
            // Arrange
            var ids = new List<int>() { 1, 2, 3, 4 };

            // Act
            var result = await sut.IsListValidAsync(ids);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmIIfIdListIsValidFails()
        {
            // Arrange
            var ids = new List<int>() { 1, 2, 3, 4, 5 };

            // Act
            var result = await sut.IsListValidAsync(ids);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
