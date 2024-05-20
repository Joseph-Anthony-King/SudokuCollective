using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SudokuCollective.Cache;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Services;
using SudokuCollective.Test.Cache;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Services
{
	internal class UsersServiceShould
	{
		private DatabaseContext context;
		private MockedEmailService mockedEmailService;
		private MockedUsersRepository mockedUsersRepository;
		private MockedAppsRepository mockedAppsRepository;
		private MockedRolesRepository mockedRolesRepository;
		private MockedAppAdminsRepository mockedAppAdminsRepository;
		private MockedEmailConfirmationsRepository mockedEmailConfirmationsRepository;
		private MockedPasswordResetsRepository mockPasswordResetRepository;
		private MockedRequestService mockedRequestService;
		private MockedCacheService mockedCacheService;
		private MemoryDistributedCache memoryCache;
		private Mock<ILogger<UsersService>> mockedLogger;
		private IUsersService sut;
		private IUsersService sutFailure;
		private IUsersService sutEmailFailure;
		private IUsersService sutResetPassword;
		private IUsersService sutResendEmailConfirmation;
		private IUsersService sutRequestPasswordReset;
		private Request request;

		[SetUp]
		public async Task Setup()
		{
			context = await TestDatabase.GetDatabaseContext();

			mockedEmailService = new MockedEmailService();
			mockedUsersRepository = new MockedUsersRepository(context);
			mockedAppsRepository = new MockedAppsRepository(context);
			mockedRolesRepository = new MockedRolesRepository(context);
			mockedAppAdminsRepository = new MockedAppAdminsRepository(context);
			mockedEmailConfirmationsRepository = new MockedEmailConfirmationsRepository(context);
			mockPasswordResetRepository = new MockedPasswordResetsRepository(context);
			mockedRequestService = new MockedRequestService();
			mockedCacheService = new MockedCacheService(context);
			memoryCache = new MemoryDistributedCache(
					Options.Create(new MemoryDistributedCacheOptions()));
			mockedLogger = new Mock<ILogger<UsersService>>();

			sut = new UsersService(
					mockedUsersRepository.SuccessfulRequest.Object,
					mockedAppsRepository.SuccessfulRequest.Object,
					mockedRolesRepository.SuccessfulRequest.Object,
					mockedAppAdminsRepository.SuccessfulRequest.Object,
					mockedEmailConfirmationsRepository.SuccessfulRequest.Object,
					mockPasswordResetRepository.SuccessfulRequest.Object,
					mockedEmailService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					memoryCache,
					mockedCacheService.SuccessfulRequest.Object,
					new CacheKeys(),
					new CachingStrategy(),
					mockedLogger.Object);

			sutFailure = new UsersService(
					mockedUsersRepository.FailedRequest.Object,
					mockedAppsRepository.SuccessfulRequest.Object,
					mockedRolesRepository.SuccessfulRequest.Object,
					mockedAppAdminsRepository.FailedRequest.Object,
					mockedEmailConfirmationsRepository.FailedRequest.Object,
					mockPasswordResetRepository.FailedRequest.Object,
					mockedEmailService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					memoryCache,
					mockedCacheService.FailedRequest.Object,
					new CacheKeys(),
					new CachingStrategy(),
					mockedLogger.Object);

			sutEmailFailure = new UsersService(
					mockedUsersRepository.EmailFailedRequest.Object,
					mockedAppsRepository.SuccessfulRequest.Object,
					mockedRolesRepository.SuccessfulRequest.Object,
					mockedAppAdminsRepository.FailedRequest.Object,
					mockedEmailConfirmationsRepository.FailedRequest.Object,
					mockPasswordResetRepository.SuccessfulRequest.Object,
					mockedEmailService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					memoryCache,
					mockedCacheService.FailedRequest.Object,
					new CacheKeys(),
					new CachingStrategy(),
					mockedLogger.Object);

			sutResetPassword = new UsersService(
					mockedUsersRepository.InitiatePasswordSuccessfulRequest.Object,
					mockedAppsRepository.InitiatePasswordSuccessfulRequest.Object,
					mockedRolesRepository.SuccessfulRequest.Object,
					mockedAppAdminsRepository.SuccessfulRequest.Object,
					mockedEmailConfirmationsRepository.SuccessfulRequest.Object,
					mockPasswordResetRepository.SuccessfulRequest.Object,
					mockedEmailService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					memoryCache,
					mockedCacheService.SuccessfulRequest.Object,
					new CacheKeys(),
					new CachingStrategy(),
					mockedLogger.Object);

			sutResendEmailConfirmation = new UsersService(
					mockedUsersRepository.ResendEmailConfirmationSuccessfulRequest.Object,
					mockedAppsRepository.SuccessfulRequest.Object,
					mockedRolesRepository.SuccessfulRequest.Object,
					mockedAppAdminsRepository.SuccessfulRequest.Object,
					mockedEmailConfirmationsRepository.SuccessfulRequest.Object,
					mockPasswordResetRepository.SuccessfulRequest.Object,
					mockedEmailService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					memoryCache,
					mockedCacheService.SuccessfulRequest.Object,
					new CacheKeys(),
					new CachingStrategy(),
					mockedLogger.Object);

			sutRequestPasswordReset = new UsersService(
					mockedUsersRepository.SuccessfulRequest.Object,
					mockedAppsRepository.SuccessfulRequest.Object,
					mockedRolesRepository.SuccessfulRequest.Object,
					mockedAppAdminsRepository.SuccessfulRequest.Object,
					mockedEmailConfirmationsRepository.SuccessfulRequest.Object,
					mockPasswordResetRepository.SuccessfullyCreatedRequest.Object,
					mockedEmailService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					memoryCache,
					mockedCacheService.SuccessfulRequest.Object,
					new CacheKeys(),
					new CachingStrategy(),
					mockedLogger.Object);

			request = TestObjects.GetRequest();
		}

		[Test, Category("Services")]
		public async Task GetUser()
		{
			// Arrange
			var userId = 1;
			var license = TestObjects.GetLicense();

			// Act
			var result = await sut.GetAsync(
					userId,
					license,
					request);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("User was found."));
			Assert.That((UserDTO)result.Payload[0], Is.TypeOf<UserDTO>());
		}

		[Test, Category("Services")]
		public async Task ReturnMessageIfUserNotFound()
		{
			// Arrange
			var userId = 5;
			var license = TestObjects.GetLicense();

			// Act
			var result = await sutFailure.GetAsync(
					userId,
					license,
					request);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("User was not found."));
		}

		[Test, Category("Services")]
		public async Task GetUsers()
		{
			// Arrange
			var license = TestObjects.GetLicense();

			// Act
			var result = await sut.GetUsersAsync(
					request.RequestorId,
					license,
					request.Paginator);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Users were found."));
			Assert.That(result.Payload.ConvertAll(u => (IUserDTO)u), Is.TypeOf<List<IUserDTO>>());
		}

		[Test, Category("Services")]
		public async Task CreateUser()
		{
			// Arrange
			var request = new SignupRequest()
			{
				UserName = "NewUser",
				FirstName = "New",
				LastName = "User",
				NickName = "New Guy",
				Email = "newuser@example.com",
				Password = "T3stP@ssw0rd",
				License = TestObjects.GetLicense()
			};

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/create-email-inlined.html";

			// Act
			var result = await sut.CreateAsync(request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("User was created."));
		}

		[Test, Category("Services")]
		public async Task ConfirmUserEmail()
		{
			// Arrange
			var emailConfirmation = context.EmailConfirmations.FirstOrDefault();

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sut.ConfirmEmailAsync(emailConfirmation.Token, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Old email was confirmed."));
		}

		[Test, Category("Services")]
		public async Task NotifyIfConfirmUserEmailFails()
		{
			// Arrange
			var emailConfirmation = TestObjects.GetNewEmailConfirmation();

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sutEmailFailure.ConfirmEmailAsync(emailConfirmation.Token, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("Email confirmation token was not found."));
		}

		[Test, Category("Services")]
		public async Task RequireUserNameUnique()
		{
			// Arrange
			var request = new SignupRequest()
			{
				UserName = "TestUser",
				FirstName = "New",
				LastName = "User",
				NickName = "New Guy",
				Email = "newuser@example.com",
				Password = "T3stP@ssw0rd",
				License = TestObjects.GetLicense()
			};

			var baseUrl = "https://example.com";

			var html = "c:/path/to/html";

			// Act
			var result = await sutFailure.CreateAsync(request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("User name is not unique."));
		}

		[Test, Category("Services")]
		public async Task RequireUserName()
		{
			// Arrange
			var request = new SignupRequest()
			{
				FirstName = "New",
				LastName = "User",
				NickName = "New Guy",
				Email = "newuser@example.com",
				Password = "T3stP@ssw0rd",
				License = TestObjects.GetLicense()
			};

			var baseUrl = "https://example.com";

			var html = "c:/path/to/html";

			// Act
			var result = await sutEmailFailure.CreateAsync(request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("User name is required."));
		}

		[Test, Category("Services")]
		public async Task RequireUniqueEmail()
		{
			// Arrange
			var request = new SignupRequest()
			{
				UserName = "NewUser",
				FirstName = "New",
				LastName = "User",
				NickName = "New Guy",
				Email = "TestUser@example.com",
				Password = "T3stP@ssw0rd1",
				License = TestObjects.GetLicense()
			};

			var baseUrl = "https://example.com";

			var emailMetaData = new EmailMetaData();

			var html = "c:/path/to/html";

			// Act
			var result = await sutEmailFailure.CreateAsync(request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("Email is not unique."));
		}

		[Test, Category("Services")]
		public async Task RequireEmail()
		{
			// Arrange
			var request = new SignupRequest()
			{
				UserName = "NewUser",
				FirstName = "New",
				LastName = "User",
				NickName = "New Guy",
				Password = "T3stP@ssw0rd",
				License = TestObjects.GetLicense()
			};

			var baseUrl = "https://example.com";

			var html = "c:/path/to/html";

			// Act
			var result = await sut.CreateAsync(request, baseUrl, html);

			// Act and Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("Email is required."));
		}

		[Test, Category("Services")]
		public async Task UpdateUser()
		{
			// Arrange
			var userId = 2;

			var payload = new UpdateUserPayload()
			{
				UserName = "TestUserUPDATED",
				FirstName = "Test",
				LastName = "User",
				NickName = "Test User UPDATED",
				Email = "TestUser@example.com"
			};

			request.Payload = payload;

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/create-email-inlined.html";

			// Act
			var result = await sut.UpdateAsync(userId, request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("User was updated."));
			Assert.That(((UserResult)result.Payload[0]).User.UserName, Is.EqualTo("TestUserUPDATED"));
		}

		[Test, Category("Services")]
		public async Task RequestPasswordReset()
		{
			// Arrange
			var requestReset = new RequestPasswordResetRequest
			{
				License = context.Apps.Select(a => a.License).FirstOrDefault(),
				Email = "bademai@example.com"
			};

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sutRequestPasswordReset.RequestPasswordResetAsync(
								requestReset,
								baseUrl,
								html);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Password reset request was processed, please check your email."));
		}

		[Test, Category("Services")]
		public async Task ReturnsFalseIfRequestPasswordResetFails()
		{
			// Arrange
			var requestReset = new RequestPasswordResetRequest
			{
				License = context.Apps.Select(a => a.License).FirstOrDefault(),
				Email = "bademai@example.com"
			};

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sutFailure.RequestPasswordResetAsync(
					requestReset,
					baseUrl,
					html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
		}

		[Test, Category("Services")]
		public async Task RequireUniqueUserNameForUpdates()
		{
			// Arrange
			var userId = 1;

			var payload = new UpdateUserPayload()
			{
				UserName = "TestUser",
				FirstName = "Test Super",
				LastName = "User",
				NickName = "Test Super User",
				Email = "TestSuperUser@example.com"
			};

			request.Payload = payload;

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sutFailure.UpdateAsync(userId, request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("User name is not unique."));
		}

		[Test, Category("Services")]
		public async Task RequireUserNameForUpdates()
		{
			// Arrange
			var userId = 1;

			var payload = new UpdateUserPayload()
			{
				FirstName = "Test Super",
				LastName = "User",
				NickName = "Test Super User",
				Email = "TestSuperUser@example.com"
			};

			request.Payload = payload;

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sut.UpdateAsync(userId, request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("User name is required."));
		}

		[Test, Category("Services")]
		public async Task RequireUniqueEmailWithUpdates()
		{
			// Arrange
			var userId = 1;

			var payload = new UpdateUserPayload()
			{
				UserName = "TestSuperUserUPDATED",
				FirstName = "Test Super",
				LastName = "User",
				NickName = "Test Super User",
				Email = "TestUser@example.com"
			};

			request.Payload = payload;

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sutEmailFailure.UpdateAsync(userId, request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("Email is not unique."));
		}

		[Test, Category("Services")]
		public async Task RequireEmailWithUpdates()
		{
			// Arrange
			var userId = 1;

			var payload = new UpdateUserPayload()
			{
				UserName = "TestSuperUserUPDATED",
				FirstName = "Test Super",
				LastName = "User",
				NickName = "Test Super User"
			};

			request.Payload = payload;

			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sut.UpdateAsync(userId, request, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("Email is required."));
		}

		[Test, Category("Services")]
		public async Task UpdateUserPassword()
		{
			// Arrange
			var user = context.Users.FirstOrDefault(u => u.Id == 2);
			user.ReceivedRequestToUpdatePassword = true;
			context.SaveChanges();

			var updatePasswordRequest = new UpdatePasswordRequest()
			{
				UserId = user.Id,
				NewPassword = "T3stP@ssw0rd",
				License = TestObjects.GetLicense()
			};

			// Act
			var result = await sut.UpdatePasswordAsync(updatePasswordRequest);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Password was reset."));
		}

		[Test, Category("Services")]
		public async Task DeleteUsers()
		{
			// Arrange
			var userId = 2;
			var license = TestObjects.GetLicense();

			// Act
			var result = await sut.DeleteAsync(userId, license);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("User was deleted."));
        }

		[Test, Category("Services")]
		public async Task ReturnErrorMessageIfUserNotFoundForDeletion()
		{
			// Arrange
			var userId = 4;
			var license = TestObjects.GetLicense();

			// Act
			var result = await sutFailure.DeleteAsync(userId, license);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("User was not found."));
		}

		[Test, Category("Services")]
		public async Task AddRolesToUsers()
		{
			// Arrange
			var userId = 2;

			var user = context.Users
					.Include(u => u.Roles)
					.FirstOrDefault(u => u.Id == userId);
			var request = TestObjects.GetRequest();
			request.Payload = new UpdateUserRolePayload()
			{
				RoleIds = [3]
			};
			var license = TestObjects.GetLicense();

			// Act
			var result = await sut.AddUserRolesAsync(
					userId,
					request,
					license);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Roles were added."));
		}

		[Test, Category("Services")]
		public async Task RemoveRolesFromUsers()
		{
			// Arrange
			var userId = 1;

			var user = context.Users
					.Include(u => u.Roles)
					.FirstOrDefault(u => u.Id == userId);
			var request = TestObjects.GetRequest();
			request.Payload = new UpdateUserRolePayload()
			{
				RoleIds = [3]
			};
			var license = TestObjects.GetLicense();

			// Act
			var result = await sut.RemoveUserRolesAsync(
					userId,
					request,
					license);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Roles were removed."));
		}

		[Test, Category("Services")]
		public async Task ActivateUsers()
		{
			// Arrange
			var userId = 1;

			// Act
			var result = await sut.ActivateAsync(userId);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("User was activated."));
		}

		[Test, Category("Services")]
		public async Task DeactivateUsers()
		{
			// Arrange
			var userId = 1;

			// Act
			var result = await sut.DeactivateAsync(userId);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("User was deactivated."));
		}

		[Test, Category("Services")]
		public async Task InitiatePasswordReset()
		{
			// Arrange
			var passwordReset = context.PasswordResets.FirstOrDefault();

			// Act
			var result = await sutResetPassword.InitiatePasswordResetAsync(
					passwordReset.Token,
					TestObjects.GetLicense());

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("User was found."));
		}

		[Test, Category("Services")]
		public async Task ReturnsFalseIfInitiatePasswordResetFails()
		{
			// Arrange
			var passwordReset = context.PasswordResets.FirstOrDefault();

			// Act
			var result = await sutFailure.InitiatePasswordResetAsync(
					passwordReset.Token,
					TestObjects.GetLicense());

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("Password reset request was not found."));
		}

		[Test, Category("Services")]
		public async Task ResendEmailConfirmations()
		{
			// Arrange
			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			var license = TestObjects.GetLicense();

			// Act
			var result = await sutResendEmailConfirmation.ResendEmailConfirmationAsync(
					3,
					1,
					baseUrl,
					html,
					license);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Email confirmation email was resent."));
		}

		[Test, Category("Services")]
		public async Task ReturnsFalseForResendEmailConfirmationsIfUserEmailConfirmed()
		{
			// Arrange
			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			var license = TestObjects.GetLicense();

			// Act
			var result = await sutFailure.ResendEmailConfirmationAsync(
					3,
					1,
					baseUrl,
					html,
					license);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
		}

		[Test, Category("Services")]
		public async Task CancelEmailConfirmationRequests()
		{
			// Arrange
			var user = context.Users.FirstOrDefault(u => u.Id == 1);
			var app = context.Apps.FirstOrDefault(a => a.Id == 1);

			// Act
			var result = await sut.CancelEmailConfirmationRequestAsync(user.Id, app.Id);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Email confirmation request was cancelled."));
			Assert.That((UserResult)result.Payload[0], Is.TypeOf<UserResult>());
		}

		[Test, Category("Services")]
		public async Task ReturnsFalseIfCancelEmailConfirmationRequestsFails()
		{
			// Arrange
			var user = context.Users.FirstOrDefault(u => u.Id == 1);
			var app = context.Apps.FirstOrDefault(a => a.Id == 1);

			// Act
			var result = await sutFailure.CancelEmailConfirmationRequestAsync(user.Id, app.Id);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("User was not found."));
		}

		[Test, Category("Services")]
		public async Task ResendPasswordResetEmail()
		{
			// Arrange
			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sutResetPassword.ResendPasswordResetAsync(3, 1, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Password reset email was resent."));
		}

		[Test, Category("Services")]
		public async Task ReturnsFalseIfResendPasswordResetEmailFails()
		{
			// Arrange
			var baseUrl = "https://example.com";

			var html = "../../../../SudokuCollective.Api/Content/EmailTemplates/confirm-old-email-inlined.html";

			// Act
			var result = await sutFailure.ResendPasswordResetAsync(1, 1, baseUrl, html);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
		}

		[Test, Category("Services")]
		public async Task CancelPasswordResetRequests()
		{
			// Arrange
			var user = context.Users.FirstOrDefault(u => u.Id == 1);
			var app = context.Apps.FirstOrDefault(a => a.Id == 1);

			// Act
			var result = await sut.CancelPasswordResetRequestAsync(user.Id, app.Id);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Password reset request was cancelled."));
			Assert.That((UserResult)result.Payload[0], Is.TypeOf<UserResult>());
		}

		[Test, Category("Services")]
		public async Task ReturnsFalseIfCancelPasswordResetRequestFails()
		{
			// Arrange
			var user = context.Users.FirstOrDefault(u => u.Id == 1);
			var app = context.Apps.FirstOrDefault(a => a.Id == 1);

			// Act
			var result = await sutFailure.CancelPasswordResetRequestAsync(user.Id, app.Id);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("User was not found."));
		}

		[Test, Category("Services")]
		public async Task CancelAllEmailRequests()
		{
			// Arrange
			var user = context.Users.FirstOrDefault(u => u.Id == 1);
			var app = context.Apps.FirstOrDefault(a => a.Id == 1);

			// Act
			var result = await sut.CancelAllEmailRequestsAsync(user.Id, app.Id);

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("Email confirmation request was cancelled and password reset request was cancelled."));
			Assert.That((UserResult)result.Payload[0], Is.TypeOf<UserResult>());
		}

		[Test, Category("Services")]
		public async Task ReturnFalseIfCancelAllEmailRequestsFails()
		{
			// Arrange
			var user = context.Users.FirstOrDefault(u => u.Id == 1);
			var app = context.Apps.FirstOrDefault(a => a.Id == 1);

			// Act
			var result = await sutFailure.CancelAllEmailRequestsAsync(user.Id, app.Id);

			// Assert
			Assert.That(result.IsSuccess, Is.False);
		}

		[Test, Category("Services")]
		public async Task SuccessfullyGetUserByPasswordToken()
		{
			// Arrange

			// Act
			var result = await sut.GetUserByPasswordTokenAsync(Guid.NewGuid().ToString());

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("User was found."));
			Assert.That((User)result.Payload[0], Is.TypeOf<User>());
		}

		[Test, Category("Services")]
		public async Task ReturnFalseIfGetUserByPasswordTokenFails()
		{
			// Arrange

			// Act
			var result = await sutFailure.GetUserByPasswordTokenAsync(Guid.NewGuid().ToString());

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("Password reset token was not found."));
		}

		[Test, Category("Services")]
		public async Task SuccessfullyGetLicenseByPasswordToken()
		{
			// Arrange

			// Act
			var result = await sut.GetAppLicenseByPasswordTokenAsync(Guid.NewGuid().ToString());

			// Assert
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Message, Is.EqualTo("App was found."));
			Assert.That(result.License, Is.TypeOf<string>());
		}

		[Test, Category("Services")]
		public async Task ReturnFalseIfGetLicenseByPasswordTokenFails()
		{
			// Arrange

			// Act
			var result = await sutFailure.GetAppLicenseByPasswordTokenAsync(Guid.NewGuid().ToString());

			// Assert
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.Message, Is.EqualTo("No outstanding request to reset password."));
		}
	}
}
