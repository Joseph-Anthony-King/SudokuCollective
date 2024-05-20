using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.TestData;
using SudokuCollective.Test.Services;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Controllers
{
	public class UsersControllerShould
	{
		private DatabaseContext context;
		private UsersController sutSuccess;
		private UsersController sutFailure;
		private UsersController sutFailureResetPassword;
		private MockedUsersService mockedUsersService;
		private MockedAppsService mockedAppsService;
		private MockedRequestService mockedRequestService;
		private Request request;
		private UpdateUserPayload updateUserPayload;
		private RequestPasswordResetRequest requestPasswordResetRequest;
		private UpdateUserRolePayload updateUserRolePayload;
		private ResendRequestPasswordRequest resendRequestPasswordRequest;
		private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
		private Mock<ILogger<UsersController>> mockedLogger;
        private Mock<IWebHostEnvironment> mockWebHostEnvironment;

        [SetUp]
		public async Task Setup()
		{
			context = await TestDatabase.GetDatabaseContext();
			mockedUsersService = new MockedUsersService(context);
			mockedAppsService = new MockedAppsService(context);
			mockedRequestService = new MockedRequestService();
			mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
			mockedLogger = new Mock<ILogger<UsersController>>();
            mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            request = TestObjects.GetRequest();

			updateUserPayload = new UpdateUserPayload()
			{
				UserName = "Test Username",
				FirstName = "FirstName",
				LastName = "LastName",
				NickName = "MyNickname",
				Email = "testemail@example.com"
			};

			requestPasswordResetRequest = new RequestPasswordResetRequest()
			{
				License = TestObjects.GetLicense(),
				Email = "TestSuperUser@example.com"
			};

			updateUserRolePayload = new UpdateUserRolePayload()
			{
				RoleIds = [3]
			};

			resendRequestPasswordRequest = new ResendRequestPasswordRequest()
			{
				UserId = 1,
				AppId = 1
			};

			sutSuccess = new UsersController(
					mockedUsersService.SuccessfulRequest.Object,
					mockedAppsService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					mockedHttpContextAccessor.Object,
					mockedLogger.Object,
                    mockWebHostEnvironment.Object);

			sutFailure = new UsersController(
					mockedUsersService.FailedRequest.Object,
					mockedAppsService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					mockedHttpContextAccessor.Object,
					mockedLogger.Object,
                    mockWebHostEnvironment.Object);

			sutFailureResetPassword = new UsersController(
					mockedUsersService.FailedResetPasswordRequest.Object,
					mockedAppsService.SuccessfulRequest.Object,
					mockedRequestService.SuccessfulRequest.Object,
					mockedHttpContextAccessor.Object,
					mockedLogger.Object,
                    mockWebHostEnvironment.Object);
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyGetUser()
		{
			// Arrange
			var userId = 1;

			// Act
			var actionResult = await sutSuccess.GetAsync(userId, request);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var user = (User)result.Payload[0];
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: User was found."));
			Assert.That(statusCode, Is.EqualTo(200));
			Assert.That(user, Is.InstanceOf<User>());
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldGetUserFail()
		{
			// Arrange
			var userId = 1;

			// Act
			var actionResult = await sutFailure.GetAsync(userId, request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: User was not found."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyGetUsers()
		{
			// Arrange

			// Act
			var actionResult = await sutSuccess.GetUsersAsync(request);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var users = result.Payload.ConvertAll(u => (User)u);
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(users, Is.InstanceOf<IEnumerable<User>>());
			Assert.That(message, Is.EqualTo("Status Code 200: Users were found."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldGetUsersFail()
		{
			// Arrange

			// Act
			var actionResult = await sutFailure.GetUsersAsync(request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Users were not found."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyUpdateUsers()
		{
			// Arrange
			int userId = 1;
			request.Payload = updateUserPayload;

			// Act
			var actionResult = await sutSuccess.UpdateAsync(userId, request);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var user = (User)result.Payload[0];
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: User was updated."));
			Assert.That(user, Is.InstanceOf<User>());
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldUpdateUserFail()
		{
			// Arrange
			int userId = 1;
			request.Payload = updateUserPayload;

			// Act
			var actionResult = await sutFailure.UpdateAsync(userId, request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: User was not updated."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyUpdateUsersPasswords()
		{
			// Arrange and Act
			var actionResult = await sutSuccess.RequestPasswordResetAsync(requestPasswordResetRequest);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Password reset request was processed, please check your email."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldUpdateUsersPasswordsFail()
		{
			// Arrange and Act
			var actionResult = await sutFailure.RequestPasswordResetAsync(requestPasswordResetRequest);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Unable to process the password reset request."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyDeleteUsers()
		{
			// Arrange
			int userId = 1;

			// Act
			var actionResult = await sutSuccess.DeleteAsync(userId, request);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: User was deleted."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldDeleteUsersFail()
		{
			// Arrange
			int userId = 1;

			// Act
			var actionResult = await sutFailure.DeleteAsync(userId, request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: User was not deleted."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyAddUsersRole()
		{
			// Arrange
			int userId = 1;
			request.Payload = updateUserRolePayload;

			// Act
			var actionResult = await sutSuccess.AddRolesAsync(userId, request);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Roles were added."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldAddUsersRoleFail()
		{
			// Arrange
			int userId = 1;
			request.Payload = updateUserRolePayload;

			// Act
			var actionResult = await sutFailure.AddRolesAsync(userId, request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Roles were not added."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyRemoveUsersRoles()
		{
			// Arrange
			int userId = 1;
			request.Payload = updateUserRolePayload;

			// Act
			var actionResult = await sutSuccess.RemoveRolesAsync(userId, request);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Roles were removed."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldRemoveUsersRolesFail()
		{
			// Arrange
			int userId = 1;
			request.Payload = updateUserRolePayload;

			// Act
			var actionResult = await sutFailure.RemoveRolesAsync(userId, request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Roles were not removed."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyActivateUsers()
		{
			// Arrange
			int userId = 1;

			// Act
			var actionResult = await sutSuccess.ActivateAsync(userId);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: User was activated."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldActivateUsersFail()
		{
			// Arrange
			int userId = 1;

			// Act
			var actionResult = await sutFailure.ActivateAsync(userId);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: User was not activated."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyDeactivateUsers()
		{
			// Arrange
			int userId = 1;

			// Act
			var actionResult = await sutSuccess.DeactivateAsync(userId);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: User was deactivated."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldDeactivateUsersFail()
		{
			// Arrange
			int userId = 1;

			// Act
			var actionResult = await sutFailure.DeactivateAsync(userId);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: User was not deactivated."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyResendPasswordResetEmails()
		{
			// Arrange

			// Act
			var actionResult = await sutSuccess.ResendPasswordResetAsync(resendRequestPasswordRequest);
			var result = (Result)((ObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Password reset email was resent."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldSuccessfullyResendEmailConfirmationFail()
		{
			// Arrange

			// Act
			var actionResult = await sutFailure.ResendPasswordResetAsync(resendRequestPasswordRequest);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: User was not found."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyCancelEmailConfirmation()
		{
			// Arrange

			// Act
			var actionResult = await sutSuccess.CancelEmailConfirmationAsync(request);
			var result = (Result)((ObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Email confirmation request was cancelled."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldSuccessfullyCancelEmailConfirmationFail()
		{
			// Arrange

			// Act
			var actionResult = await sutFailure.CancelEmailConfirmationAsync(request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Email confirmation request was not cancelled."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyCancelPasswordRequest()
		{
			// Arrange

			// Act
			var actionResult = await sutSuccess.CancelPasswordResetAsync(request);
			var result = (Result)((ObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Password reset request was cancelled."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldSuccessfullyCancelPasswordRequestFail()
		{
			// Arrange

			// Act
			var actionResult = await sutFailure.CancelPasswordResetAsync(request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Password reset request was not cancelled."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyCancelAllEmailRequests()
		{
			// Arrange

			// Act
			var actionResult = await sutSuccess.CancelAllEmailRequestsAsync(request);
			var result = (Result)((ObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Email confirmation request was cancelled and password reset request was cancelled."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorAndMessageShouldSuccessfullyCancelAllEmailRequestsFail()
		{
			// Arrange

			// Act
			var actionResult = await sutFailure.CancelAllEmailRequestsAsync(request);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Email requests were not found."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyConfirmUserEmail()
		{
			// Arrange
			var emailConfirmationToken = Guid.NewGuid().ToString();

			// Act
			var actionResult = await sutSuccess.ConfirmEmailAsync(emailConfirmationToken);
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Email was confirmed."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorMessageShouldConfirmUserEmailFails()
		{
			// Arrange
			var emailConfirmationToken = Guid.NewGuid().ToString();

			// Act
			var actionResult = await sutFailure.ConfirmEmailAsync(emailConfirmationToken);
			var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Email confirmation token was not found."));
			Assert.That(statusCode, Is.EqualTo(404));
		}

		[Test, Category("Controllers")]
		public async Task SuccessfullyResetPassword()
		{
			// Arrange
			var request = new ResetPasswordRequest
            {
				NewPassword = "P@ssword2"
			};

			// Act
			var actionResult = await sutSuccess.ResetPasswordAsync(request, Guid.NewGuid().ToString());
			var result = (Result)((OkObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 200: Password was reset."));
			Assert.That(statusCode, Is.EqualTo(200));
		}

		[Test, Category("Controllers")]
		public async Task IssueErrorMessageShouldResetPasswordFails()
		{
			// Arrange
			var request = new ResetPasswordRequest
			{
				NewPassword = "P@ssword2"
			};

			// Act
			var actionResult = await sutFailureResetPassword.ResetPasswordAsync(request, Guid.NewGuid().ToString());
			var result = (Result)((ObjectResult)actionResult.Result).Value;
			var message = result.Message;
			var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

			// Assert
			Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
			Assert.That(result, Is.InstanceOf<Result>());
			Assert.That(message, Is.EqualTo("Status Code 404: Password was not reset."));
			Assert.That(statusCode, Is.EqualTo(404));
		}
	}
}
