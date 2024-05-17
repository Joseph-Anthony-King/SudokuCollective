using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Repositories
{
    public class AppsRepositoryShould
    {
        private DatabaseContext context;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<AppsRepository<App>>> mockedLogger;
        private IAppsRepository<App> sut;
        private App newApp;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<AppsRepository<App>>>();

            sut = new AppsRepository<App>(
                context,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);

            newApp = new App(
                "Test App 4",
                "6e32e987-13b9-43ab-aee5-9df659eeb6bd",
                1,
                "TestSuperUser",
                "https://localhost:5173",
                "",
                "",
                "https://testapp3.com",
                string.Empty);
        }

        [Test, Category("Repository")]
        public async Task CreateApps()
        {
            // Arrange and Act
            var result = await sut.AddAsync(newApp);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((App)result.Object, Is.InstanceOf<App>());
        }

        [Test, Category("Repository")]
        public async Task ThrowExceptionIfCreateAppsFails()
        {
            // Arrange

            // A new app requires a reference to an admin user, a 0 id will should throw a failure
            newApp.OwnerId = 0;

            // Act
            var result = await sut.AddAsync(newApp);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAppsById()
        {
            // Arrange and Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((App)result.Object, Is.InstanceOf<App>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetByIdFails()
        {
            // Arrange

            // Retrieve the last app from the test database
            var app = context.Apps.OrderBy(a => a.Id).LastOrDefault();

            IRepositoryResponse result;

            // Act

            // Add 1 to the last ID retrieved to cause a failure
            result = await sut.GetAsync(app.Id + 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAppsByLicense()
        {
            // Arrange
            var license = context.Apps.FirstOrDefault().License;

            // Act
            var result = await sut.GetByLicenseAsync(license);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((App)result.Object, Is.InstanceOf<App>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetAppsByLicenseFails()
        {
            // Arrange
            var license = "aabe5bb5-cf60-4d7e-8bf1-f3686e4a1c4c";

            // Act
            var result = await sut.GetByLicenseAsync(license);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAllApps()
        {
            // Arrange

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(a => (App)a), Is.InstanceOf<List<App>>());
        }

        [Test, Category("Repository")]
        public async Task GetUsersByApp()
        {
            // Arrange
            var id = context.Apps.FirstOrDefault(a => a.Id == 2).Id;

            // Act
            var result = await sut.GetAppUsersAsync(id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(a => (User)a), Is.InstanceOf<List<User>>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetUsersByAppFails()
        {
            // Arrange
            var id = context.Apps.OrderBy(a => a.Id).LastOrDefault().Id;

            // Act

            // Add 1 to the last ID retrieved to cause a failure
            var result = await sut.GetAppUsersAsync(id + 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetNonAppUsersByApp()
        {
            // Arrange
            var id = context.Apps.LastOrDefault().Id;

            // Act
            var result = await sut.GetNonAppUsersAsync(id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(a => (User)a), Is.InstanceOf<List<User>>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetNonAppUsersByAppFails()
        {
            // Arrange
            // Add 2 to the last ID retrieved to cause a failure
            var id = context.Apps.LastOrDefault(a => a.Id == 3).Id + 2;

            // Act
            var result = await sut.GetNonAppUsersAsync(id);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task UpdateApps()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault(a => a.Id == 1);
            app.Name = string.Format("{0} UPDATED!", app.Name);

            // Act
            var result = await sut.UpdateAsync(app);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Object, Is.InstanceOf<App>());
            Assert.That(((App)result.Object).Name, Is.EqualTo(app.Name));
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateAppsFails()
        {
            // Arrange and Act

            // New app is not saved in the DB so this should cause a failure
            var result = await sut.UpdateAsync(newApp);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task AddUsersToApps()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault();
            var license = context.Apps.FirstOrDefault(a => a.Id == app.Id).License;
            var user = context.Users.FirstOrDefault(u => u.Apps.Any(ua => ua.AppId != app.Id));

            // Act
            var result = await sut.AddAppUserAsync(user.Id, license);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfAddUsersToAppsFails()
        {
            // Arrange
            var license = "aabe5bb5-cf60-4d7e-8bf1-f3686e4a1c4c";
            var id = context.Apps.LastOrDefault().Id;

            // Act

            // Add 1 to the last ID retrieved to cause a failure
            var result = await sut.AddAppUserAsync(id + 1, license);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task RemoveUsersFromApps()
        {
            // Arrange
            var user = context
                .Users
                .Include(u => u.Roles)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .ToList()
                .FirstOrDefault(u => u.Apps.Any(ua => ua.AppId == 1) && u.IsSuperUser == false);

            var license = context
                .Apps
                .FirstOrDefault(a => a.Id == 1).License;

            // Act
            var result = await sut.RemoveAppUserAsync(user.Id, license);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfRemoveUsersFromAppsFails()
        {
            // Arrange
            var user = context.Users.FirstOrDefault();
            var license = "aabe5bb5-cf60-4d7e-8bf1-f3686e4a1c4c";

            // Act

            // The license is not in the DB so this should cause a failure
            var result = await sut.RemoveAppUserAsync(user.Id, license);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task DeleteApps()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault();

            // Act
            var result = await sut.DeleteAsync(app);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteAppsFails()
        {
            // Arrange and Act

            // newApp is not in the DB so this should cause a failure
            var result = await sut.DeleteAsync(newApp);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ResetApps()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault();

            // Act
            var result = await sut.ResetAsync(app);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfResetAppsFails()
        {
            // Arrange and Act

            // newApp is not in the DB so this should cause a failure
            var result = await sut.ResetAsync(newApp);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ActivateApps()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault();

            // Act
            var result = await sut.ActivateAsync(app.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfActivateAppsFails()
        {
            // Arrange
            var app = context.Apps.OrderBy(a => a.Id).LastOrDefault();

            // Act

            // Add 1 to the last ID retrieved to cause a failure
            var result = await sut.ActivateAsync(app.Id + 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task DeactivateApps()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault();

            // Act
            var result = await sut.DeactivateAsync(app.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeactivateAppsFails()
        {
            // Arrange
            var app = context.Apps.OrderBy(a => a.Id).LastOrDefault();

            // Act

            // Add 1 to the last ID retrieved to cause a failure
            var result = await sut.DeactivateAsync(app.Id + 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasAnApp()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault();

            // Act
            var result = await sut.HasEntityAsync(app.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmItHasAnAppFails()
        {
            // Arrange
            var id = context
                .Apps
                .ToList()
                .OrderBy(a => a.Id)
                .Last<App>()
                .Id + 1;

            // Act
            var result = await sut.HasEntityAsync(id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmAppLicenseIsValid()
        {
            // Arrange
            var license = context.Apps.FirstOrDefault(a => a.Id == 1).License;

            // Act
            var result = await sut.IsAppLicenseValidAsync(license);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmAppLicenseIsValidFails()
        {
            // Arrange and Act

            // The license is not in the DB so this should cause a failure
            var result = await sut.IsAppLicenseValidAsync("aabe5bb5-cf60-4d7e-8bf1-f3686e4a1c4c");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmIfUserIsRegisteredToApp()
        {
            // Arrange
            var license = context.Apps.FirstOrDefault(a => a.Id == 1).License;

            // Act
            var result = await sut.IsUserRegisteredToAppAsync(1, license, 1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmIfUserIsRegisteredToAppFails()
        {
            // Arrange
            var app = context.Apps.OrderBy(a => a.Id).FirstOrDefault();
            var user = context.Users.OrderBy(u => u.Id).LastOrDefault();

            // Act

            // Add 1 to the last user id to invoke an error
            var result = await sut.IsUserRegisteredToAppAsync(app.Id, app.License, user.Id + 1);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmIfUserIsOwnerToApp()
        {
            // Arrange
            var user = context.Users.OrderBy(u => u.Id).FirstOrDefault();
            var app = context.Apps.FirstOrDefault(a => a.OwnerId == user.Id);

            // Act
            var result = await sut.IsUserOwnerOThisfAppAsync(
                app.Id, 
                app.License, 
                user.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmIfUserIsOwnerToAppFails()
        {
            // Arrange
            var apps = context.Apps.ToList();
            var app = apps.FirstOrDefault();
            var users = context.Users.ToList();
            var user = new User();

            foreach (var u in users)
            {
                var userNotAnOwner = true;

                foreach (var a in apps)
                {
                    if (a.OwnerId == u.Id)
                    {
                        userNotAnOwner = false;
                    }
                }

                if (userNotAnOwner)
                {
                    user = u;
                }
            }

            // Act
            var result = await sut.IsUserOwnerOThisfAppAsync(app.Id, app.License, user.Id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetLicenses()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault();

            // Act
            var result = await sut.GetLicenseAsync(app.Id);

            // Assert
            Assert.That(result, Is.InstanceOf<string>());
            Assert.That(result, Is.EqualTo(app.License));
        }

        [Test, Category("Repository")]
        public async Task ReturnNullIfGetLicensesFails()
        {
            // Arrange
            var app = context.Apps.OrderBy(a => a.Id).LastOrDefault();

            // Act

            // Add 1 to the last ID retrieved to cause a failure
            var result = await sut.GetLicenseAsync(app.Id + 1);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetMyApps()
        {
            // Arrange
            var user = context.Users.OrderBy(u => u.Id).FirstOrDefault();

            // Act
            var result = await sut.GetMyAppsAsync(user.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(a => (App)a), Is.InstanceOf<List<App>>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIgGetMyAppsFails()
        {
            // Arrange
            var user = context.Users.LastOrDefault();

            // Act

            // Add 1 to the last ID retrieved to cause a failure
            var result = await sut.GetMyAppsAsync(user.Id + 2);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetMyRegisteredApps()
        {
            // Arrange
            var user = context.Users.OrderBy(u => u.Id).FirstOrDefault(u => u.Id != 1);

            // Act
            var result = await sut.GetMyRegisteredAppsAsync(user.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(a => (App)a), Is.InstanceOf<List<App>>());
            Assert.That(result.Objects.Count, Is.EqualTo(2));
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseifGetMyRegisteredAppsFails()
        {
            // Arrange

            // Act
            var result = await sut.GetMyRegisteredAppsAsync(4);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ReturnAllGalleryApps()
        {
            // Arrange and Act
            var result = await sut.GetGalleryAppsAsync();
            var isSuccess = result.IsSuccess;
            var apps = result.Objects.ConvertAll(a => (App)a);

            // Assert
            Assert.That(result, Is.InstanceOf<IRepositoryResponse>());
            Assert.That(isSuccess, Is.True);
            Assert.That(apps, Is.InstanceOf<List<App>>());
        }
    }
}
