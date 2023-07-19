using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Test.TestData;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;

namespace SudokuCollective.Test.Services
{
    public class MockedUsersService
    {
        private MockedUsersRepository MockedUsersRepository { get; set; }
        private MockedPasswordResetsRepository MockedPasswordResetsRepository { get; set; }
        private MockedEmailConfirmationsRepository MockedEmailConfirmationsRepository { get; set; }

        internal Mock<IUsersService> SuccessfulRequest { get; set; }
        internal Mock<IUsersService> FailedRequest { get; set; }
        internal Mock<IUsersService> FailedResetPasswordRequest { get; set; }

        public MockedUsersService(DatabaseContext context)
        {
            MockedUsersRepository = new MockedUsersRepository(context);
            MockedPasswordResetsRepository = new MockedPasswordResetsRepository(context);
            MockedEmailConfirmationsRepository = new MockedEmailConfirmationsRepository(context);

            SuccessfulRequest = new Mock<IUsersService>();
            FailedRequest = new Mock<IUsersService>();
            FailedResetPasswordRequest = new Mock<IUsersService>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(service =>
                service.CreateAsync(It.IsAny<ISignupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .AddAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserCreatedMessage,
                        Payload = new List<object>()
                            {
                                new EmailConfirmationSentResult 
                                { 
                                    EmailConfirmationSent = true 
                                }
                            }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Request>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserFoundMessage,
                        Payload = new List<object>()
                            {
                                MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<User>())
                                .Result
                                .Object
                            }
                } as IResult));

            SuccessfulRequest.Setup(service =>
                service.GetUsersAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UsersFoundMessage,
                        Payload = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(u => (object)u)
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserUpdatedMessage,
                        Payload = new List<object>()
                            {
                                MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<User>())
                                .Result
                                .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.RequestPasswordResetAsync(
                    It.IsAny<IRequestPasswordResetRequest>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.ProcessedPasswordResetRequestMessage
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.ResendPasswordResetAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.PasswordResetEmailResentMessage
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.GetUserByPasswordTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.UserFoundMessage,
                        Payload = new List<object>()
                            {
                                MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<User>())
                                .Result
                                .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.GetAppLicenseByPasswordTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new LicenseResult()
                    {
                        IsSuccess = true,
                        Message = AppsMessages.AppsFoundMessage,
                        License = TestObjects.GetLicense()
                    } as ILicenseResult));

            SuccessfulRequest.Setup(service =>
                service.UpdatePasswordAsync(
                    It.IsAny<IUpdatePasswordRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.PasswordResetMessage
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.DeleteAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserDeletedMessage
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.AddUserRolesAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.RolesAddedMessage
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.RemoveUserRolesAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.RolesRemovedMessage
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.ActivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .ActivateAsync(It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.UserActivatedMessage
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.DeactivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .DeactivateAsync(It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.UserDeactivatedMessage
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.ConfirmEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .ConfirmEmailAsync(It.IsAny<IEmailConfirmation>())
                        .Result
                        .IsSuccess,
                    Message = UsersMessages.EmailConfirmedMessage,
                    Payload = new List<object>()
                    {
                        TestObjects.GetConfirmEmailResult()
                    }
                } as IResult));

            SuccessfulRequest.Setup(service =>
                service.InitiatePasswordResetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedPasswordResetsRepository
                            .SuccessfulRequest
                            .Object
                            .CreateAsync(It.IsAny<PasswordReset>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserFoundMessage,
                        Payload = new List<object>()
                        {
                            TestObjects.GetInitiatePasswordResetResult()
                        }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.ResendEmailConfirmationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedEmailConfirmationsRepository
                            .SuccessfulRequest
                            .Object
                            .HasOutstandingEmailConfirmationAsync(It.IsAny<int>(), It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.EmailConfirmationEmailResentMessage,
                        Payload = new List<object>()
                        {
                            TestObjects.GetUserResult()
                        }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.CancelEmailConfirmationRequestAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedEmailConfirmationsRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<EmailConfirmation>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.EmailConfirmationRequestCancelledMessage,
                        Payload = new List<object>()
                            {
                                TestObjects.GetUserResult()
                            }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.CancelPasswordResetRequestAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedPasswordResetsRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<PasswordReset>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.PasswordResetRequestCancelledMessage,
                        Payload = new List<object>()
                            {
                                TestObjects.GetUserResult()
                            }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.CancelAllEmailRequestsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = string.Format("{0} and {1}",
                            UsersMessages.EmailConfirmationRequestCancelledMessage,
                            UsersMessages.PasswordResetRequestCancelledMessage),
                        Payload = new List<object>()
                            {
                                TestObjects.GetUserResult()
                            }
                } as IResult));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(service =>
                service.CreateAsync(It.IsAny<ISignupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .AddAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserNotCreatedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Request>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.GetUsersAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UsersNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserNotUpdatedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.RequestPasswordResetAsync(
                    It.IsAny<IRequestPasswordResetRequest>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = false,
                        Message = UsersMessages.UnableToProcessPasswordResetRequesMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.ResendPasswordResetAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = false,
                        Message = UsersMessages.UserNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.GetUserByPasswordTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.UserFoundMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.GetAppLicenseByPasswordTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new LicenseResult()
                    {
                        IsSuccess = false,
                        Message = AppsMessages.AppNotFoundMessage,
                        License = string.Empty
                    } as ILicenseResult));

            FailedRequest.Setup(service =>
                service.UpdatePasswordAsync(
                    It.IsAny<IUpdatePasswordRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.PasswordNotResetMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.DeleteAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .DeleteAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserNotDeletedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.AddUserRolesAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.RolesNotAddedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.RemoveUserRolesAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.RolesNotRemovedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.ActivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .ActivateAsync(It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.UserNotActivatedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.DeactivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .DeactivateAsync(It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.UserNotDeactivatedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.ConfirmEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .ConfirmEmailAsync(It.IsAny<IEmailConfirmation>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.EmailNotConfirmedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.InitiatePasswordResetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedPasswordResetsRepository
                        .FailedRequest
                        .Object
                        .CreateAsync(It.IsAny<PasswordReset>())
                        .Result
                        .IsSuccess,
                    Message = UsersMessages.UserNotFoundMessage
                } as IResult));

            FailedRequest.Setup(service =>
                service.ResendEmailConfirmationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedEmailConfirmationsRepository
                            .FailedRequest
                            .Object
                            .HasOutstandingEmailConfirmationAsync(It.IsAny<int>(), It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.EmailConfirmationEmailNotResentMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.CancelEmailConfirmationRequestAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedEmailConfirmationsRepository
                            .FailedRequest
                            .Object
                            .DeleteAsync(It.IsAny<EmailConfirmation>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.EmailConfirmationRequestNotCancelledMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.CancelPasswordResetRequestAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedPasswordResetsRepository
                            .FailedRequest
                            .Object
                            .DeleteAsync(It.IsAny<PasswordReset>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.PasswordResetRequestNotCancelledMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.CancelAllEmailRequestsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = false,
                        Message = UsersMessages.EmailRequestsNotFoundMessage
                    } as IResult));
            #endregion

            #region FailedResetPasswordRequest
            FailedResetPasswordRequest.Setup(service =>
                service.CreateAsync(It.IsAny<ISignupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .AddAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserCreatedMessage,
                        Payload = new List<object>()
                            {
                                new EmailConfirmationSentResult 
                                { 
                                    EmailConfirmationSent = true 
                                }
                            }
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Request>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserFoundMessage,
                        Payload = new List<object>()
                            {
                                MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<User>())
                                .Result
                                .Object
                            }
                } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.GetUsersAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UsersFoundMessage,
                        Payload = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(u => (object)u)
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserUpdatedMessage,
                        Payload = new List<object>()
                            {
                                MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<User>())
                                .Result
                                .Object
                            }
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.RequestPasswordResetAsync(
                    It.IsAny<IRequestPasswordResetRequest>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.ProcessedPasswordResetRequestMessage
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.ResendPasswordResetAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.PasswordResetEmailResentMessage
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.GetUserByPasswordTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.UserFoundMessage,
                        Payload = new List<object>()
                            {
                                MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<User>())
                                .Result
                                .Object
                            }
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.GetAppLicenseByPasswordTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new LicenseResult()
                    {
                        IsSuccess = true,
                        Message = AppsMessages.AppsFoundMessage,
                        License = TestObjects.GetLicense()
                    } as ILicenseResult));

            FailedResetPasswordRequest.Setup(service =>
                service.UpdatePasswordAsync(
                    It.IsAny<IUpdatePasswordRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.PasswordNotResetMessage
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.DeleteAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<User>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserDeletedMessage
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.AddUserRolesAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.RolesAddedMessage
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.RemoveUserRolesAsync(
                    It.IsAny<int>(),
                    It.IsAny<IRequest>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.RolesRemovedMessage
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.ActivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .ActivateAsync(It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.UserActivatedMessage
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.DeactivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .DeactivateAsync(It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.UserDeactivatedMessage
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.ConfirmEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .ConfirmEmailAsync(It.IsAny<IEmailConfirmation>())
                        .Result
                        .IsSuccess,
                    Message = UsersMessages.EmailConfirmedMessage,
                    Payload = new List<object>()
                    {
                        TestObjects.GetConfirmEmailResult()
                    }
                } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.InitiatePasswordResetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedPasswordResetsRepository
                            .SuccessfulRequest
                            .Object
                            .CreateAsync(It.IsAny<PasswordReset>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserFoundMessage,
                        Payload = new List<object>()
                        {
                            TestObjects.GetInitiatePasswordResetResult()
                        }
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.ResendEmailConfirmationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedEmailConfirmationsRepository
                            .SuccessfulRequest
                            .Object
                            .HasOutstandingEmailConfirmationAsync(It.IsAny<int>(), It.IsAny<int>())
                            .Result,
                        Message = UsersMessages.EmailConfirmationEmailResentMessage,
                        Payload = new List<object>()
                        {
                            TestObjects.GetUserResult()
                        }
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.CancelEmailConfirmationRequestAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedEmailConfirmationsRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<EmailConfirmation>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.EmailConfirmationRequestCancelledMessage,
                        Payload = new List<object>()
                            {
                                TestObjects.GetUserResult()
                            }
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.CancelPasswordResetRequestAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedPasswordResetsRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<PasswordReset>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.PasswordResetRequestCancelledMessage,
                        Payload = new List<object>()
                            {
                                TestObjects.GetUserResult()
                            }
                    } as IResult));

            FailedResetPasswordRequest.Setup(service =>
                service.CancelAllEmailRequestsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = string.Format("{0} and {1}",
                            UsersMessages.EmailConfirmationRequestCancelledMessage,
                            UsersMessages.PasswordResetRequestCancelledMessage),
                        Payload = new List<object>()
                            {
                                TestObjects.GetUserResult()
                            }
                } as IResult));
            #endregion
        }
    }
}
