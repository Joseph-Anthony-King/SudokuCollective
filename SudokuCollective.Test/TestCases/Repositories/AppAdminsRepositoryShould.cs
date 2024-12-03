using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Repositories
{
    public class AppAdminsRepositoryShould
    {
        private DatabaseContext context;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<AppAdminsRepository<AppAdmin>>> mockedLogger;
        private IAppAdminsRepository<AppAdmin> sut;
        private AppAdmin newAppAdmin;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<AppAdminsRepository<AppAdmin>>>();

            sut = new AppAdminsRepository<AppAdmin>(
                context,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);

            newAppAdmin = new AppAdmin()
            {
                AppId = 2,
                UserId = 2
            };
        }

        [Test, Category("Repository")]
        public async Task CreateAppAdmins()
        {
            // Arrange and Act
            var result = await sut.AddAsync(newAppAdmin);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((AppAdmin)result.Object, Is.InstanceOf<AppAdmin>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfCreateDifficutliesFails()
        {
            // Arrange

            // EF is responsible for assigning ID numbers, a non-zero ID should cause failure
            newAppAdmin.Id = 1;

            // Act
            var result = await sut.AddAsync(newAppAdmin);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetAppAdminsById()
        {
            // Arrange and Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((AppAdmin)result.Object, Is.InstanceOf<AppAdmin>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetByIdFails()
        {
            // Arrange and Act
            var result = await sut.GetAsync(7);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAllAppAdmins()
        {
            // Arrange and Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(aa => (AppAdmin)aa), Is.InstanceOf<List<AppAdmin>>());
        }

        [Test, Category("Repository")]
        public async Task UpdateAppAdmins()
        {
            // Arrange
            var appAdmin = context.AppAdmins.FirstOrDefault(aa => aa.Id == 1);
            
            appAdmin.IsActive = false;

            // Act
            var result = await sut.UpdateAsync(appAdmin);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Object, Is.InstanceOf<AppAdmin>());
            Assert.That(((AppAdmin)result.Object).IsActive, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateAppAdminsFails()
        {
            // Arrange and Act
            var result = await sut.UpdateAsync(newAppAdmin);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task DeleteAppAdmins()
        {
            // Arrange
            var appAdmin = context
                .AppAdmins
                .FirstOrDefault(aa => aa.Id == 1);

            // Act
            var result = await sut.DeleteAsync(appAdmin);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteAppAdminsFails()
        {
            // Arrange and Act
            var result = await sut.DeleteAsync(newAppAdmin);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasAnAppAdmin()
        {
            // Arrange and Act
            var result = await sut.HasEntityAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmItHasAnAppAdminFails()
        {
            // Arrange

            // Add 1 to the last ID to ensure failure
            var id = context
                .AppAdmins
                .ToList()
                .OrderBy(aa => aa.Id)
                .Last<AppAdmin>()
                .Id + 1;

            // Act
            var result = await sut.HasEntityAsync(id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasAnAdminRecord()
        {
            // Arrange
            var userId = context
                .Users
                .Where(u => u.Id == 1)
                .Select(u => u.Id)
                .FirstOrDefault();

            var appId = context
                .Apps
                .Where(a => a.Id == 1)
                .Select(a => a.Id)
                .FirstOrDefault();

            // Act
            var result = await sut.HasAdminRecordAsync(appId, userId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmItHasAnAdminRecordFails()
        {
            // Arrange
            var userId = context
                .Users
                .Where(u => u.Id == 1)
                .Select(u => u.Id)
                .FirstOrDefault();

            var appId = context
                .Apps
                .Where(a => a.Id == 1)
                .Select(a => a.Id)
                .FirstOrDefault();

            // Act
            var result = await sut.HasAdminRecordAsync(appId, userId + 1);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetAppAdminsRecords()
        {
            // Arrange
            var userId = context
                .Users
                .Where(u => u.Id == 1)
                .Select(u => u.Id)
                .FirstOrDefault();

            var appId = context
                .Apps
                .Where(a => a.Id == 1)
                .Select(a => a.Id)
                .FirstOrDefault();

            // Act
            var result = await sut.GetAdminRecordAsync(appId, userId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((AppAdmin)result.Object, Is.InstanceOf<AppAdmin>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetRecordsFails()
        {
            // Arrange
            var userId = context
                .Users
                .Where(u => u.Id == 1)
                .Select(u => u.Id)
                .FirstOrDefault();

            var appId = context
                .Apps
                .Where(a => a.Id == 1)
                .Select(a => a.Id)
                .FirstOrDefault();

            // Act
            var result = await sut.GetAdminRecordAsync(appId, userId + 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }
    }
}
