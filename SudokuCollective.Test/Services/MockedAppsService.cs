using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Test.Repositories;

namespace SudokuCollective.Test.Services
{
    public class MockedAppsService
    {
        internal MockedAppsRepository MockedAppsRepository { get; set; }
        internal MockedUsersRepository MockedUsersRepository { get; set; }
        internal MockedAppAdminsRepository MockedAppAdminsRepository { get; set; }

        internal Mock<IAppsService> SuccessfulRequest { get; set; }
        internal Mock<IAppsService> FailedRequest { get; set; }
        internal Mock<IAppsService> InvalidRequest { get; set; }
        internal Mock<IAppsService> PromoteUserFailsRequest { get; set; }

        public MockedAppsService(DatabaseContext context)
        {
            MockedAppsRepository = new MockedAppsRepository(context);
            MockedUsersRepository = new MockedUsersRepository(context);
            MockedAppAdminsRepository = new MockedAppAdminsRepository(context);

            SuccessfulRequest = new Mock<IAppsService>();
            FailedRequest = new Mock<IAppsService>();
            InvalidRequest = new Mock<IAppsService>();
            PromoteUserFailsRequest = new Mock<IAppsService>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(service =>
                service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppFoundMessage,
                    Payload =
                        [
                            MockedAppsRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.GetByLicenseAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetByLicenseAsync(It.IsAny<string>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppFoundMessage,
                    Payload =
                        [
                            MockedAppsRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.GetAppsAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppsFoundMessage,
                    Payload = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .Objects
                        .ConvertAll(a => (object)a)
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.GetMyAppsAsync(
                    It.IsAny<int>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppsFoundMessage,
                    Payload = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .Objects
                        .ConvertAll(a => (object)a)
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.GetMyRegisteredAppsAsync(
                    It.IsAny<int>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppsFoundMessage,
                    Payload = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .Objects
                        .ConvertAll(a => (object)a)
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(appsService =>
                appsService.CreateAync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .AddAsync(It.IsAny<App>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppCreatedMessage,
                    Payload =
                        [
                            MockedAppsRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(service =>
                service.IsUserOwnerOThisfAppAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(service =>
                service.UpdateAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .UpdateAsync(It.IsAny<App>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppUpdatedMessage,
                    Payload =
                        [
                            MockedAppsRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.GetAppUsersAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Paginator>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAppUsersAsync(It.IsAny<int>())
                        .Result
                        .IsSuccess,
                    Message = UsersMessages.UsersFoundMessage,
                    Payload = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAppUsersAsync(It.IsAny<int>())
                        .Result
                        .Objects
                        .ConvertAll(u => (object)u)
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.GetLicenseAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new LicenseResult()
                {
                    IsSuccess = true,
                    Message = AppsMessages.AppFoundMessage,
                    License = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetLicenseAsync(It.IsAny<int>())
                        .Result
                } as ILicenseResult));

            SuccessfulRequest.Setup(service =>
                service.AddAppUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .AddAppUserAsync(It.IsAny<int>(), It.IsAny<string>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.UserAddedToAppMessage,
                    Payload =
                        [
                            MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<string>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.UserRemovedFromAppMessage,
                    Payload =
                        [
                            MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.ActivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .ActivateAsync(It.IsAny<int>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppActivatedMessage,
                    Payload =
                        [
                            MockedAppsRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.DeactivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .DeactivateAsync(It.IsAny<int>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppDeactivatedMessage,
                    Payload =
                        [
                            MockedAppsRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(service =>
                service.DeleteOrResetAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .DeleteAsync(It.IsAny<App>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppDeletedMessage
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(appsService =>
                appsService.ActivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppAdminsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .IsSuccess,
                    Message = UsersMessages.UserHasBeenPromotedToAdminMessage,
                    Payload =
                        [
                             MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .AddAsync(It.IsAny<User>())
                            .Result
                            .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(appsService =>
                appsService.DeactivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppAdminsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AdminPrivilegesDeactivatedMessage,
                    Payload =
                        [
                            MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<User>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(appsService =>
                appsService.GetGalleryAppsAsync(It.IsAny<IPaginator>()))
                .ReturnsAsync(new Result()
                {
                    IsSuccess = true,
                    Message = AppsMessages.AppsFoundMessage,
                    Payload = (new List<GalleryApp>()).ConvertAll(a => (object)a)
                });
            #endregion

            #region FailedRequest
            FailedRequest.Setup(service =>
                service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppNotFoundMessage,
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.GetByLicenseAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .GetByLicenseAsync(It.IsAny<string>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppNotFoundMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.GetAppsAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppsNotFoundMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.GetMyAppsAsync(
                    It.IsAny<int>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppsNotFoundMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.GetMyRegisteredAppsAsync(
                    It.IsAny<int>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppsNotFoundMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(appsService =>
                appsService.CreateAync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .AddAsync(It.IsAny<App>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppNotCreatedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            FailedRequest.Setup(service =>
                service.IsUserOwnerOThisfAppAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(false));

            FailedRequest.Setup(service =>
                service.UpdateAsync(It.IsAny<int>(), It.IsAny<Request>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<App>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppNotUpdatedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.GetAppUsersAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Paginator>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .GetAppUsersAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UsersNotFoundMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.GetLicenseAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new LicenseResult()
                    {
                        IsSuccess = false,
                        Message = AppsMessages.AppNotFoundMessage
                    } as ILicenseResult));

            FailedRequest.Setup(service =>
                service.AddAppUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .AddAppUserAsync(It.IsAny<int>(), It.IsAny<string>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.UserNotAddedToAppMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<string>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.UserNotRemovedFromAppMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.ActivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .ActivateAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppNotActivatedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.DeactivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .DeactivateAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppNotDeactivatedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(service =>
                service.DeleteOrResetAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .FailedRequest
                            .Object
                            .DeleteAsync(It.IsAny<App>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppNotDeletedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(appsService =>
                appsService.ActivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppAdminsRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserHasNotBeenPromotedToAdminMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(appsService =>
                appsService.DeactivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppAdminsRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.DeactivationOfAdminPrivilegesFailedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            FailedRequest.Setup(appsService =>
                appsService.GetGalleryAppsAsync(It.IsAny<IPaginator>()))
                .ReturnsAsync(new Result()
                {
                    IsSuccess = false,
                    Message = AppsMessages.AppsNotFoundMessage
                });
            #endregion

            #region InvalidRequest
            InvalidRequest.Setup(service =>
                service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppFoundMessage,
                        Payload =
                            [
                                MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.GetByLicenseAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetByLicenseAsync(It.IsAny<string>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppFoundMessage,
                    Payload =
                            [
                                MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.GetAppsAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppsFoundMessage,
                    Payload = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .Objects
                        .ConvertAll(a => (object)a)
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.GetMyAppsAsync(
                    It.IsAny<int>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppsFoundMessage,
                    Payload = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .Objects
                        .ConvertAll(a => (object)a)
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.GetMyRegisteredAppsAsync(
                    It.IsAny<int>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppsFoundMessage,
                    Payload = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAllAsync()
                        .Result
                        .Objects
                        .ConvertAll(a => (object)a)
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(appsService =>
                appsService.CreateAync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .AddAsync(It.IsAny<App>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppCreatedMessage,
                    Payload =
                        [
                            MockedAppsRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<App>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(false));

            InvalidRequest.Setup(service =>
                service.IsUserOwnerOThisfAppAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            InvalidRequest.Setup(service =>
                service.UpdateAsync(It.IsAny<int>(), It.IsAny<Request>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .UpdateAsync(It.IsAny<App>())
                        .Result
                        .IsSuccess,
                    Message = AppsMessages.AppUpdatedMessage,
                    Payload =
                        [
                            MockedAppsRepository
                                .SuccessfulRequest
                                .Object
                                .UpdateAsync(It.IsAny<App>())
                                .Result
                                .Object
                        ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.GetAppUsersAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Paginator>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAppUsersAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UsersFoundMessage,
                        Payload = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAppUsersAsync(It.IsAny<int>())
                            .Result
                            .Objects
                            .ConvertAll(u => (object)u)
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.GetLicenseAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new LicenseResult()
                    {
                        IsSuccess = true,
                        Message = AppsMessages.AppFoundMessage,
                        License = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetLicenseAsync(It.IsAny<int>())
                            .Result
                    } as ILicenseResult));

            InvalidRequest.Setup(service =>
                service.AddAppUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .AddAppUserAsync(It.IsAny<int>(), It.IsAny<string>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.UserAddedToAppMessage,
                        Payload =
                            [
                                MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<string>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.UserRemovedFromAppMessage,
                        Payload =
                            [
                                MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.ActivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .ActivateAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppActivatedMessage,
                        Payload =
                            [
                                MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.DeactivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .DeactivateAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppDeactivatedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(service =>
                service.DeleteOrResetAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<App>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppDeletedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(appsService =>
                appsService.ActivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppAdminsRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppNotFoundMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            InvalidRequest.Setup(appsService =>
                appsService.DeactivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppAdminsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AdminPrivilegesDeactivatedMessage,
                        Payload =
                            [
                                MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .UpdateAsync(It.IsAny<User>())
                                    .Result
                                    .Object
                            ]
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            SuccessfulRequest.Setup(appsService =>
                appsService.GetGalleryAppsAsync(It.IsAny<IPaginator>()))
                .ReturnsAsync(new Result()
                {
                    IsSuccess = true,
                    Message = AppsMessages.AppsFoundMessage,
                    Payload = (new List<GalleryApp>()).ConvertAll(a => (object)a)
                });
            #endregion

            #region PromoteUserFailsRequest
            PromoteUserFailsRequest.Setup(service =>
                service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppFoundMessage,
                        Payload =
                            [
                                MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.GetByLicenseAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetByLicenseAsync(It.IsAny<string>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppFoundMessage,
                        Payload =
                            [
                                MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.GetAppsAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppsFoundMessage,
                        Payload = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(a => (object)a)
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.GetMyAppsAsync(
                    It.IsAny<int>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppsFoundMessage,
                        Payload = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(a => (object)a)
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.GetMyRegisteredAppsAsync(
                    It.IsAny<int>(),
                    It.IsAny<Paginator>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppsFoundMessage,
                        Payload = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(a => (object)a)
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(appsService =>
                appsService.CreateAync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .AddAsync(It.IsAny<App>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppCreatedMessage,
                        Payload =
                            [
                                MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            PromoteUserFailsRequest.Setup(service =>
                service.IsUserOwnerOThisfAppAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            PromoteUserFailsRequest.Setup(service =>
                service.UpdateAsync(It.IsAny<int>(), It.IsAny<Request>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<App>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppUpdatedMessage,
                        Payload =
                            [
                                MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.GetAppUsersAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Paginator>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAppUsersAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UsersFoundMessage,
                        Payload = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAppUsersAsync(It.IsAny<int>())
                            .Result
                            .Objects
                            .ConvertAll(u => (object)u)
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.GetLicenseAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new LicenseResult()
                    {
                        IsSuccess = true,
                        Message = AppsMessages.AppFoundMessage,
                        License = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetLicenseAsync(It.IsAny<int>())
                            .Result
                    } as ILicenseResult));

            PromoteUserFailsRequest.Setup(service =>
                service.AddAppUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .AddAppUserAsync(It.IsAny<int>(), It.IsAny<string>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.UserAddedToAppMessage,
                        Payload =
                            [
                                MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<string>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.UserRemovedFromAppMessage,
                        Payload =
                            [
                                MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.ActivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .ActivateAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppActivatedMessage,
                        Payload =
                            [
                                MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            ]
                } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.DeactivateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .DeactivateAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppDeactivatedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(service =>
                service.DeleteOrResetAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<App>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.AppDeletedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(appsService =>
                appsService.ActivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppAdminsRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserHasNotBeenPromotedToAdminMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(appsService =>
                appsService.ActivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppAdminsRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = UsersMessages.UserHasNotBeenPromotedToAdminMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(appsService =>
                appsService.DeactivateAdminPrivilegesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedAppAdminsRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = AppsMessages.DeactivationOfAdminPrivilegesFailedMessage
                    } as Core.Interfaces.Models.DomainObjects.Params.IResult));

            PromoteUserFailsRequest.Setup(appsService =>
                appsService.GetGalleryAppsAsync(It.IsAny<IPaginator>()))
                .ReturnsAsync(new Result()
                {
                    IsSuccess = true,
                    Message = AppsMessages.AppsFoundMessage
                });
            #endregion
        }
    }
}
