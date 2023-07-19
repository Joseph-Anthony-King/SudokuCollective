using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using SudokuCollective.Cache;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Services;
using SudokuCollective.Test.Cache;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Services
{
    public class AppsServiceShould
    {
        private DatabaseContext context;
        private MockedAppsRepository mockedAppsRepository;
        private MockedUsersRepository mockedUsersRepository;
        private MockedAppAdminsRepository mockedAppAdminsRepository;
        private MockedRolesRepository mockedRolesRepository;
        private MockedRequestService mockedRequestService;
        private MockedCacheService mockedCacheService;
        private MemoryDistributedCache memoryCache;
        private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
        private Mock<ILogger<AppsService>> mockedLogger;
        private IAppsService sut;
        private IAppsService sutAppRepoFailure;
        private IAppsService sutUserRepoFailure;
        private IAppsService sutPromoteUser;
        private IAppsService sutPermitSuperUser;
        private DateTime dateCreated;
        private Request request;
        private string license;
        private Paginator paginator;
        private int userId;
        private int appId;
        private User user;
        private App app;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();

            mockedAppsRepository = new MockedAppsRepository(context);
            mockedUsersRepository = new MockedUsersRepository(context);
            mockedAppAdminsRepository = new MockedAppAdminsRepository(context);
            mockedRolesRepository = new MockedRolesRepository(context);
            mockedRequestService = new MockedRequestService();
            mockedCacheService = new MockedCacheService(context);
            memoryCache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()));
            mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockedLogger = new Mock<ILogger<AppsService>>();

            dateCreated = DateTime.UtcNow;
            request = TestObjects.GetRequest();
            license = TestObjects.GetLicense();
            paginator = TestObjects.GetPaginator();
            userId = 1;
            appId = 1;
            user = context.Users.FirstOrDefault(u => u.Id == userId);
            app = context.Apps.FirstOrDefault(a => a.Id == appId);

            mockedHttpContextAccessor = TestObjects.GetHttpContextAccessor(user, app);

            sut = new AppsService(
                mockedAppsRepository.SuccessfulRequest.Object,
                mockedUsersRepository.SuccessfulRequest.Object,
                mockedAppAdminsRepository.SuccessfulRequest.Object,
                mockedRolesRepository.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.SuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);

            sutAppRepoFailure = new AppsService(
                mockedAppsRepository.FailedRequest.Object,
                mockedUsersRepository.SuccessfulRequest.Object,
                mockedAppAdminsRepository.FailedRequest.Object,
                mockedRolesRepository.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.FailedRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);

            sutUserRepoFailure = new AppsService(
                mockedAppsRepository.SuccessfulRequest.Object,
                mockedUsersRepository.FailedRequest.Object,
                mockedAppAdminsRepository.FailedRequest.Object,
                mockedRolesRepository.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.FailedRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);

            sutPromoteUser = new AppsService(
                mockedAppsRepository.SuccessfulRequest.Object,
                mockedUsersRepository.InitiatePasswordSuccessfulRequest.Object,
                mockedAppAdminsRepository.PromoteUserRequest.Object,
                mockedRolesRepository.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.SuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);

            sutPermitSuperUser = new AppsService(
                mockedAppsRepository.PermitSuperUserRequest.Object,
                mockedUsersRepository.SuccessfulRequest.Object,
                mockedAppAdminsRepository.SuccessfulRequest.Object,
                mockedRolesRepository.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.PermitSuperUserSuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
        }

        [Test, Category("Services")]
        public async Task GetAppByID()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(1, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("App found"));
            Assert.That(result.Payload[0], Is.TypeOf<App>());
        }

        [Test, Category("Services")]
        public async Task GetAppByIDReturnsFalseIfNotFound()
        {
            // Arrange

            // Act
            var result = await sutAppRepoFailure.GetAsync(4, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("App not found"));
            Assert.That(result.Payload.Count, Is.EqualTo(0));
        }

        [Test, Category("Services")]
        public async Task GetApps()
        {
            // Arrange

            // Act
            var result = await sut.GetAppsAsync(TestObjects.GetRequest());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Apps found"));
            Assert.That(result.Payload.Count, Is.EqualTo(3));
        }

        [Test, Category("Services")]
        public async Task CreateApps()
        {
            // Arrange
            request.Payload = new LicensePayload()
            {

                Name = "Test App 4",
                OwnerId = 1,
                LocalUrl = "https://localhost:8081",
                StagingUrl = "https://testapp3-dev.com",
                ProdUrl = "https://testapp3.com"
            };


            // Act
            var result = await sut.CreateAync(request);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("App created"));
            Assert.That(((App)result.Payload[0]).IsActive, Is.True);
        }

        [Test, Category("Services")]
        public async Task NotCreateAppsIfOwnerDoesNotExist()
        {
            // Arrange
            request.Payload = new LicensePayload()
            {

                Name = "Test App 4",
                OwnerId = 4,
                LocalUrl = "https://localhost:8081",
                StagingUrl = "https://testapp3-dev.com",
                ProdUrl = "https://testapp3.com"
            };

            // Act
            var result = await sutUserRepoFailure.CreateAync(request);

            var apps = context.Apps.ToList();

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("User does not exist"));
            Assert.That(apps.Count, Is.EqualTo(3));
        }

        [Test, Category("Services")]
        public async Task GetAppByLicense()
        {
            // Arrange

            // Act
            var result = await sut.GetByLicenseAsync(license, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("App found"));
            Assert.That(((App)result.Payload[0]).Id, Is.EqualTo(1));
            Assert.That(result.Payload[0], Is.TypeOf<App>());
        }

        [Test, Category("Services")]
        public async Task NotGetAppByLicenseIfInvalid()
        {
            // Arrange
            var invalidLicense = "5CDBFC8F-F304-4703-831B-750A7B7F8531";

            // Act
            var result = await sutAppRepoFailure.GetByLicenseAsync(invalidLicense, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("App not found"));
        }

        [Test, Category("Services")]
        public async Task RetrieveLicense()
        {
            // Arrange

            // Act
            var result = await sutPermitSuperUser.GetLicenseAsync(3, 2);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("App found"));
            Assert.That(result.License, Is.EqualTo(TestObjects.GetThirdLicense()));
        }

        [Test, Category("Services")]
        public async Task NotRetrieveLicenseIfAppDoesNotExist()
        {
            // Arrange

            // Act
            var result = await sutAppRepoFailure.GetLicenseAsync(5, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("App not found"));
            Assert.That(result.License, Is.Not.EqualTo(license));
        }

        [Test, Category("Services")]
        public async Task GetUsersByApp()
        {
            // Arrange

            // Act
            var result = await sut.GetAppUsersAsync(1, 1, paginator);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Users found"));
            Assert.That(result.Payload.Count, Is.EqualTo(2));
        }

        [Test, Category("Services")]
        public async Task UpdateApps()
        {
            // Arrange

            var payload = new AppPayload()
                {
                    Name = "Test App 1... UPDATED!",
                    LocalUrl = "https://localhost:4200",
                    StagingUrl = "https://testapp-dev.com",
                    ProdUrl = "https://testapp.com",
                    IsActive = true,
                    Environment = ReleaseEnvironment.LOCAL,
                    PermitSuperUserAccess = true,
                    PermitCollectiveLogins = false,
                    DisableCustomUrls = false,
                    CustomEmailConfirmationAction = "ConfirmEmail",
                    CustomPasswordResetAction = "ResetPassword",
                    TimeFrame = TimeFrame.DAYS,
                    AccessDuration = 1
                };
            var request = new Request { Payload = payload };

            // Act
            var result = await sut.UpdateAsync(1, request);

            var name = ((App)result.Payload[0]).Name;
            var apps = context.Apps.ToList();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("App updated"));
            Assert.That(name, Is.EqualTo("Test App 1... UPDATED!"));
        }

        [Test, Category("Services")]
        public async Task AddUsersToApp()
        {
            // Arrange

            // Act
            var result = await sut.AddAppUserAsync(1, 3);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("User added to app"));
        }

        [Test, Category("Services")]
        public async Task RemoveUsersFromApps()
        {
            // Arrange

            // Act
            var result = await sut.RemoveAppUserAsync(1, 2);
            var user = (User)result.Payload[0];

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(user, Is.InstanceOf<User>());
            Assert.That(result.Message, Is.EqualTo("User removed from app"));
        }

        [Test, Category("Services")]
        public async Task DeleteApps()
        {
            // Arrange

            // Act
            var result = await sut.DeleteOrResetAsync(2);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("App deleted"));
        }

        [Test, Category("Services")]
        public async Task ActivateApps()
        {
            // Arrange

            // Act
            var result = await sut.ActivateAsync(1);
            var app = (App)result.Payload[0];

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(app, Is.InstanceOf<App>());
            Assert.That(result.Message, Is.EqualTo("App activated"));
        }

        [Test, Category("Services")]
        public async Task DeactivateApps()
        {
            // Arrange

            // Act
            var result = await sut.DeactivateAsync(1);
            var app = context.Apps.FirstOrDefault(a => a.Id == 1);
            var returnedApp = (App)result.Payload[0];

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(returnedApp, Is.InstanceOf<App>());
            Assert.That(result.Message, Is.EqualTo("App deactivated"));
        }

        [Test, Category("Services")]
        public async Task PermitValidRequests()
        {
            // Arrange

            // Act
            var result = await sut.IsRequestValidOnThisTokenAsync(
                mockedHttpContextAccessor.Object, 
                license, 
                appId, 
                userId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Services")]
        public async Task DenyInvalidRequests()
        {
            // Arrange
            var invalidLicense = "5CDBFC8F-F304-4703-831B-750A7B7F8531";

            // Act
            var result = await sutAppRepoFailure.IsRequestValidOnThisTokenAsync(
                mockedHttpContextAccessor.Object, 
                invalidLicense, 
                appId, 
                userId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Services")]
        public async Task DenyRequestWhereUserIsNotRegisteredToApp()
        {
            // Arrange
            var invalidLicense = "5CDBFC8F-F304-4703-831B-750A7B7F8531";

            appId = 1;

            var user = new User()
            {
                Id = 4,
                UserName = "TestUser3",
                FirstName = "John",
                LastName = "Doe",
                NickName = "Johnny Boy",
                Email = "testuser3@example.com",
                Password = "password1",
                DateCreated = dateCreated,
                DateUpdated = dateCreated
            };

            var invalidmockedHttpContextAccessor = TestObjects.GetInvalidHttpContextAccessor(user);

            // Act
            var result = await sutUserRepoFailure.IsRequestValidOnThisTokenAsync(
                invalidmockedHttpContextAccessor.Object, 
                invalidLicense, 
                appId, 
                user.Id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Services")]
        public async Task PermitSuperUserSystemWideAccess()
        {
            // Arrange
            var app = context.Apps.FirstOrDefault(a => a.Id == 3);
            var licenseResult =await sutPermitSuperUser.GetLicenseAsync(app.Id, 2);
            var superUser = context.Users.Where(user => user.Id == 1).FirstOrDefault();

            // Act
            var superUserIsInApp = app.Users
                .Any(ua => ua.UserId == superUser.Id);

            var mockSuperUserHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var claim = new List<Claim> {

                new Claim(ClaimTypes.Name, "TestSuperUser"),
                new Claim(ClaimTypes.Name, superUser.Id.ToString()),
                new Claim(ClaimTypes.Name, app.Id.ToString()),
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("865542af-e02f-446d-ad34-b121554f37be"));
            
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationLimit = DateTime.UtcNow.AddDays(1);

            var jwtInvalidToken = new JwtSecurityToken(
                    "test",
                    "test",
                    claim.ToArray(),
                    notBefore: DateTime.UtcNow,
                    expires: expirationLimit,
                    signingCredentials: credentials
                );

            mockSuperUserHttpContextAccessor.Setup(
                mock => mock.HttpContext.Request.Headers["Authorization"])
                .Returns(string.Format("bearer {0}", new JwtSecurityTokenHandler().WriteToken(jwtInvalidToken)));

            var result = await sutPermitSuperUser.IsRequestValidOnThisTokenAsync(
                mockSuperUserHttpContextAccessor.Object, 
                licenseResult.License, 
                app.Id, 
                superUser.Id);

            // Assert
            Assert.That(superUser.IsSuperUser, Is.True);
            Assert.That(result, Is.True);
        }

        [Test, Category("Services")]
        public async Task PermitOwnerRequests()
        {
            // Arrange

            // Act
            var result = await sut.IsUserOwnerOThisfAppAsync(
                mockedHttpContextAccessor.Object, 
                license,
                userId,
                license,
                appId, 
                userId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Services")]
        public async Task DenyInvalidOwnerRequests()
        {
            // Arrange
            var invalidLicense = "5CDBFC8F-F304-4703-831B-750A7B7F8531";

            // Act
            var result = await sutUserRepoFailure.IsUserOwnerOThisfAppAsync(
                mockedHttpContextAccessor.Object, 
                invalidLicense,
                userId,
                license,
                appId, 
                userId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Services")]
        public async Task PromoteUsersToAdmin()
        {
            // Arrange

            // Act
            var result = await sutPromoteUser.ActivateAdminPrivilegesAsync(1, 3);
            var user = (User)result.Payload[0];

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(user, Is.InstanceOf<User>());
            Assert.That(result.Message, Is.EqualTo("User has been promoted to admin"));
        }

        [Test, Category("Services")]
        public async Task ReturnFalseIfPromoteUsersToAdminFails()
        {
            // Arrange

            // Act
            var result = await sutAppRepoFailure.ActivateAdminPrivilegesAsync(1, 3);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("App not found"));
        }

        [Test, Category("Services")]
        public async Task DeactivateUserAdminPrivileges()
        {
            // Arrange

            // Act
            var result = await sut
                .DeactivateAdminPrivilegesAsync(1, 3);

            // Assert
            Assert.That(result, Is.InstanceOf<Core.Interfaces.Models.DomainObjects.Params.IResult>());
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Admin privileges deactivated"));
            Assert.That(result.Payload[0], Is.TypeOf<User>());

        }

        [Test, Category("Services")]
        public async Task ReturnFalseIfDeactivateUserAdminPrivilegesFails()
        {
            // Arrange

            // Act
            var result = await sutAppRepoFailure
                .DeactivateAdminPrivilegesAsync(1, 3);

            // Assert
            Assert.That(result, Is.InstanceOf<Core.Interfaces.Models.DomainObjects.Params.IResult>());
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("App not found"));
        }

        [Test, Category("Services")]
        public async Task GetMyApps()
        {
            // Arrange

            // Act
            var result = await sut.GetMyAppsAsync(1, new Paginator());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Apps found"));
            Assert.That(result.Payload.Count, Is.EqualTo(2));
        }

        [Test, Category("Services")]
        public async Task ReturnFalseIfGetMyAppsFails()
        {
            // Arrange

            // Act
            var result = await sutAppRepoFailure.GetMyAppsAsync(1, new Paginator());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Apps not found"));
        }

        [Test, Category("Services")]
        public async Task GetRegisteredApps()
        {
            // Arrange

            // Act
            var result = await sut.GetMyRegisteredAppsAsync(2, new Paginator());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Apps found"));
            Assert.That(result.Payload.Count, Is.EqualTo(2));
        }

        [Test, Category("Services")]
        public async Task ReturnFalseIfGetRegisteredAppsFails()
        {
            // Arrange

            // Act
            var result = await sutAppRepoFailure.GetMyRegisteredAppsAsync(1, new Paginator());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Apps not found"));
        }

        [Test, Category("Repository")]
        public async Task ReturnAllGalleryApps()
        {
            // Arrange and Act
            var result = await sut.GetGalleryAppsAsync(paginator);
            var isSuccess = result.IsSuccess;
            var apps = result.Payload.ConvertAll(a => (GalleryApp)a);

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(isSuccess, Is.True);
            Assert.That(apps, Is.InstanceOf<List<GalleryApp>>());
            Assert.That(result.Message, Is.EqualTo("Apps found"));
        }

        [Test, Category("Repository")]
        public async Task ReturnsFalseIfAllGalleryAppsFails()
        {
            // Arrange and Act
            var result = await sutAppRepoFailure.GetGalleryAppsAsync(paginator);
            var isSuccess = result.IsSuccess;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(isSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Apps not found"));
        }
    }
}
