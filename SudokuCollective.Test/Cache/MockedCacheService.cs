using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Test.TestData;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;
using SudokuCollective.Core.Interfaces.Models;

namespace SudokuCollective.Test.Cache
{
    public class MockedCacheService
    {
        internal MockedAppAdminsRepository MockedAppAdminsRepository { get; set; }
        internal MockedAppsRepository MockedAppsRepository { get; set; }
        internal MockedDifficultiesRepository MockedDifficultiesRepository { get; set; }
        internal MockedEmailConfirmationsRepository MockedEmailConfirmationsRepository { get; set; }
        internal MockedGamesRepository MockedGamesRepository { get; set; }
        internal MockedPasswordResetsRepository MockedPasswordResetsRepository { get; set; }
        internal MockedRolesRepository MockedRolesRepository { get; set; }
        internal MockedSolutionsRepository MockedSolutionsRepository { get; set; }
        internal MockedUsersRepository MockedUsersRepository { get; set; }

        internal Mock<ICacheService> SuccessfulRequest { get; set; }
        internal Mock<ICacheService> FailedRequest { get; set; }
        internal Mock<ICacheService> CreateDifficultyRoleSuccessfulRequest { get; set; }
        internal Mock<ICacheService> PermitSuperUserSuccessfulRequest { get; set; }

        public MockedCacheService(DatabaseContext context)
        {
            MockedAppAdminsRepository = new MockedAppAdminsRepository(context);
            MockedAppsRepository = new MockedAppsRepository(context);
            MockedDifficultiesRepository = new MockedDifficultiesRepository(context);
            MockedEmailConfirmationsRepository = new MockedEmailConfirmationsRepository(context);
            MockedGamesRepository = new MockedGamesRepository(context);
            MockedPasswordResetsRepository = new MockedPasswordResetsRepository(context);
            MockedRolesRepository = new MockedRolesRepository(context);
            MockedSolutionsRepository = new MockedSolutionsRepository(context);
            MockedUsersRepository = new MockedUsersRepository(context);

            SuccessfulRequest = new Mock<ICacheService>();
            FailedRequest = new Mock<ICacheService>();
            CreateDifficultyRoleSuccessfulRequest = new Mock<ICacheService>();
            PermitSuperUserSuccessfulRequest = new Mock<ICacheService>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedDifficultiesRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedRolesRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                    {
                        if (result != null)
                        {
                            result.IsSuccess = false;
                        }
                        return new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            }, result);
                    });

            SuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IDifficultiesRepository<Difficulty> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                {
                    if (result != null)
                    {
                        result.IsSuccess = false;
                    }
                    return new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse
                        {
                            IsSuccess = true,
                            Object = MockedDifficultiesRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }, result);
                });

            SuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IRolesRepository<Role> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                {
                    if (result != null)
                    {
                        result.IsSuccess = false;
                    }
                    return new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse
                        {
                            IsSuccess = true,
                            Object = MockedRolesRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }, result);
                });

            SuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                    {
                        if (result != null)
                        {
                            result.IsSuccess = false;
                        }
                        return new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            }, result);
                    });

            SuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IDifficultiesRepository<Difficulty> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedDifficultiesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IRolesRepository<Role> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedRolesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<SudokuSolution>(
                    It.IsAny<ISolutionsRepository<SudokuSolution>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    ISolutionsRepository<SudokuSolution> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedSolutionsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.HasEntityWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(cache =>
                cache.HasEntityWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(cache =>
                cache.RemoveKeysAsync(
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<List<string>>()));

            SuccessfulRequest.Setup(cache =>
                cache.GetAppByLicenseWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    string license,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetByLicenseAsync(It.IsAny<string>())
                                    .Result
                                    .Object
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetAppUsersWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAppUsersAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetNonAppUsersWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetNonAppUsersAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetMyAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetMyRegisteredAppsAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetMyRegisteredAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetMyRegisteredAppsAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            SuccessfulRequest.Setup(cache =>
                cache.GetLicenseWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    ICacheKeys cacheKeys,
                    int id,
                    IResult result) =>
                        new Tuple<string, IResult>(TestObjects.GetLicense(), result));

            SuccessfulRequest.Setup(cache =>
                cache.ResetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.ActivatetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.DeactivatetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.IsAppLicenseValidWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(cache =>
                cache.GetByUserNameWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    ICacheKeys cacheKeys,
                    string userName,
                    string license,
                    IResult result) =>
                    {
                        if (result != null)
                        {
                            result.IsSuccess = true;
                        }
                        return new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetByUserNameAsync(It.IsAny<string>())
                                    .Result
                                    .Object
                            }, result);
                    });

            SuccessfulRequest.Setup(cache =>
                cache.GetByEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    string email,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetByEmailAsync(It.IsAny<string>())
                                    .Result
                                    .Object
                            }, result));


            SuccessfulRequest.Setup(cache =>
                cache.ConfirmEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<EmailConfirmation>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.UpdateEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<EmailConfirmation>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            SuccessfulRequest.Setup(cache =>
                cache.IsUserRegisteredWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(cache =>
                cache.HasDifficultyLevelWithCacheAsync(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DifficultyLevel>()))
                .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(cache =>
                cache.HasRoleLevelWithCacheAsync(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<RoleLevel>()))
                .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(cache =>
                cache.GetValuesAsync(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<IEnumListItem>>(),
                    It.IsAny<List<IEnumListItem>>(),
                    It.IsAny<List<IEnumListItem>>(),
                    It.IsAny<IResult>()
                )).Returns(Task.FromResult(new Tuple<IValues, IResult>(
                    (IValues)(TestObjects.GetValues()), 
                    TestObjects.GetResult())));

            SuccessfulRequest.Setup(cache =>
                cache.GetGalleryAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()
                )).ReturnsAsync(new Tuple<IRepositoryResponse, IResult>(
                    new RepositoryResponse
                    {
                        IsSuccess = true,
                        Objects = (new List<GalleryApp>()).ConvertAll(a => (IDomainEntity)a)
                    },
                    TestObjects.GetResult()));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(cache =>
                cache.AddWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.AddWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.AddWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.AddWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.GetWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IDifficultiesRepository<Difficulty> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IRolesRepository<Role> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IDifficultiesRepository<Difficulty> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IRolesRepository<Role> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<SudokuSolution>(
                    It.IsAny<ISolutionsRepository<SudokuSolution>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    ISolutionsRepository<SudokuSolution> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.HasEntityWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(false));

            FailedRequest.Setup(cache =>
                cache.HasEntityWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(false));

            FailedRequest.Setup(cache =>
                cache.RemoveKeysAsync(
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<List<string>>()));

            FailedRequest.Setup(cache =>
                cache.GetAppByLicenseWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    string license,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetAppUsersWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetNonAppUsersWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetMyAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetMyRegisteredAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));

            FailedRequest.Setup(cache =>
                cache.GetLicenseWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    ICacheKeys cacheKeys,
                    int id,
                    IResult result) =>
                        new Tuple<string, IResult>(TestObjects.GetLicense(), result));

            FailedRequest.Setup(cache =>
                cache.ResetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.ActivatetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.DeactivatetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.IsAppLicenseValidWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            FailedRequest.Setup(cache =>
                cache.GetByUserNameWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    ICacheKeys cacheKeys,
                    string userName,
                    string license,
                    IResult result) =>
                {
                    if (result != null)
                    {
                        result.IsSuccess = false;
                    }
                    return new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result);
                    });

            FailedRequest.Setup(cache =>
                cache.GetByEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    string email,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = false
                            }, result));


            FailedRequest.Setup(cache =>
                cache.ConfirmEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<EmailConfirmation>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.UpdateEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<EmailConfirmation>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = false
                } as IRepositoryResponse));

            FailedRequest.Setup(cache =>
                cache.IsUserRegisteredWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(false));

            FailedRequest.Setup(cache =>
                cache.HasDifficultyLevelWithCacheAsync(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DifficultyLevel>()))
                .Returns(Task.FromResult(false));

            FailedRequest.Setup(cache =>
                cache.HasRoleLevelWithCacheAsync(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<RoleLevel>()))
                .Returns(Task.FromResult(false));

            FailedRequest.Setup(cache =>
                cache.GetGalleryAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()
                )).ReturnsAsync(new Tuple<IRepositoryResponse, IResult>(
                    new RepositoryResponse
                    {
                        IsSuccess = false
                    },
                    TestObjects.GetResult()));
            #endregion

            #region CreateDifficultyRoleSuccessfulRequest
            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedDifficultiesRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedRolesRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IDifficultiesRepository<Difficulty> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedDifficultiesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IRolesRepository<Role> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedRolesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IDifficultiesRepository<Difficulty> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedDifficultiesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IRolesRepository<Role> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedRolesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<SudokuSolution>(
                    It.IsAny<ISolutionsRepository<SudokuSolution>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    ISolutionsRepository<SudokuSolution> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedSolutionsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.HasEntityWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.HasEntityWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.RemoveKeysAsync(
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<List<string>>()));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetAppByLicenseWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    string license,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetByLicenseAsync(It.IsAny<string>())
                                    .Result
                                    .Object
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetAppUsersWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAppUsersAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetNonAppUsersWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetNonAppUsersAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetMyAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetMyRegisteredAppsAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetMyRegisteredAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetMyRegisteredAppsAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetLicenseWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    ICacheKeys cacheKeys,
                    int id,
                    IResult result) =>
                        new Tuple<string, IResult>(TestObjects.GetLicense(), result));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.ResetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.ActivatetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.DeactivatetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.IsAppLicenseValidWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetByUserNameWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    ICacheKeys cacheKeys,
                    string userName,
                    string license,
                    IResult result) =>
                {
                    result.IsSuccess = true;
                    return new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse
                        {
                            IsSuccess = true,
                            Object = MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .GetByUserNameAsync(It.IsAny<string>())
                                .Result
                                .Object
                        }, result);
                });

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetByEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    string email,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetByEmailAsync(It.IsAny<string>())
                                    .Result
                                    .Object
                            }, result));


            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.ConfirmEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<EmailConfirmation>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.UpdateEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<EmailConfirmation>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.IsUserRegisteredWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.HasDifficultyLevelWithCacheAsync(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DifficultyLevel>()))
                .Returns(Task.FromResult(false));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.HasRoleLevelWithCacheAsync(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<RoleLevel>()))
                .Returns(Task.FromResult(false));

            CreateDifficultyRoleSuccessfulRequest.Setup(cache =>
                cache.GetGalleryAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()
                )).ReturnsAsync(new Tuple<IRepositoryResponse, IResult>(
                    new RepositoryResponse
                    {
                        IsSuccess = true,
                        Objects = (new List<GalleryApp>()).ConvertAll(a => (IDomainEntity)a)
                    },
                    TestObjects.GetResult()));
            #endregion

            #region PermitSuperUserSuccessfulRequest
            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedDifficultiesRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedRolesRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.AddWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                {
                    if (result != null)
                    {
                        result.IsSuccess = false;
                    }
                    return new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse
                        {
                            IsSuccess = true,
                            Object = MockedAppsRepository
                                .PermitSuperUserRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }, result);
                });

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IDifficultiesRepository<Difficulty> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                {
                    if (result != null)
                    {
                        result.IsSuccess = false;
                    }
                    return new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse
                        {
                            IsSuccess = true,
                            Object = MockedDifficultiesRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }, result);
                });

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IRolesRepository<Role> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                {
                    if (result != null)
                    {
                        result.IsSuccess = false;
                    }
                    return new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse
                        {
                            IsSuccess = true,
                            Object = MockedRolesRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }, result);
                });

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                {
                    if (result != null)
                    {
                        result.IsSuccess = false;
                    }
                    return new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse
                        {
                            IsSuccess = true,
                            Object = MockedUsersRepository
                                .PermitSuperUserSuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }, result);
                });

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IDifficultiesRepository<Difficulty> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedDifficultiesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IRolesRepository<Role> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedRolesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<SudokuSolution>(
                    It.IsAny<ISolutionsRepository<SudokuSolution>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    ISolutionsRepository<SudokuSolution> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedSolutionsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetAllWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAllAsync()
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.UpdateWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<Difficulty>(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<Role>(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<Role>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.DeleteWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.HasEntityWithCacheAsync<App>(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.HasEntityWithCacheAsync<User>(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.RemoveKeysAsync(
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<List<string>>()));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetAppByLicenseWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    string license,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetByLicenseAsync(It.IsAny<string>())
                                    .Result
                                    .Object
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetAppUsersWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAppUsersAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetNonAppUsersWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetNonAppUsersAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetMyAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetMyRegisteredAppsAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetMyRegisteredAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    int id,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Objects = MockedAppsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetMyRegisteredAppsAsync(It.IsAny<int>())
                                    .Result
                                    .Objects
                            }, result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetLicenseWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IAppsRepository<App> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    ICacheKeys cacheKeys,
                    int id,
                    IResult result) =>
                        new Tuple<string, IResult>(TestObjects.GetThirdLicense(), result));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.ResetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<App>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.ActivatetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.DeactivatetWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedAppsRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.IsAppLicenseValidWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetByUserNameWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    ICacheKeys cacheKeys,
                    string userName,
                    string license,
                    IResult result) =>
                {
                    if (result != null)
                    {
                        result.IsSuccess = true;
                    }
                    return new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse
                        {
                            IsSuccess = true,
                            Object = MockedUsersRepository
                                .SuccessfulRequest
                                .Object
                                .GetByUserNameAsync(It.IsAny<string>())
                                .Result
                                .Object
                        }, result);
                });

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetByEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<IResult>()))
                .ReturnsAsync((
                    IUsersRepository<User> repo,
                    IDistributedCache cache,
                    string cacheKey,
                    DateTime expiration,
                    string email,
                    IResult result) =>
                        new Tuple<IRepositoryResponse, IResult>(
                            new RepositoryResponse
                            {
                                IsSuccess = true,
                                Object = MockedUsersRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetByEmailAsync(It.IsAny<string>())
                                    .Result
                                    .Object
                            }, result));


            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.ConfirmEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<EmailConfirmation>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.UpdateEmailWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<ICacheKeys>(),
                    It.IsAny<EmailConfirmation>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(new RepositoryResponse()
                {
                    IsSuccess = true,
                    Object = MockedUsersRepository
                        .SuccessfulRequest
                        .Object
                        .GetAsync(It.IsAny<int>())
                        .Result
                        .Object
                } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.IsUserRegisteredWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.HasDifficultyLevelWithCacheAsync(
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DifficultyLevel>()))
                .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.HasRoleLevelWithCacheAsync(
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<RoleLevel>()))
                .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(cache =>
                cache.GetGalleryAppsWithCacheAsync(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>()
                )).ReturnsAsync(new Tuple<IRepositoryResponse, IResult>(
                    new RepositoryResponse
                    {
                        IsSuccess = true,
                        Objects = (new List<GalleryApp>()).ConvertAll(a => (IDomainEntity)a)
                    },
                    TestObjects.GetResult()));
            #endregion
        }
    }
}
