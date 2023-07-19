using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.Repositories
{
    public class MockedAppsRepository
    {
        private readonly DatabaseContext context;
        internal Mock<IAppsRepository<App>> SuccessfulRequest { get; set; }
        internal Mock<IAppsRepository<App>> FailedRequest { get; set; }
        internal Mock<IAppsRepository<App>> InitiatePasswordSuccessfulRequest { get; set; }
        internal Mock<IAppsRepository<App>> PermitSuperUserRequest { get; set; }

        public MockedAppsRepository(DatabaseContext ctxt)
        {
            context = ctxt;
            var todaysDate = DateTime.UtcNow;

            SuccessfulRequest = new Mock<IAppsRepository<App>>();
            FailedRequest = new Mock<IAppsRepository<App>>();
            InitiatePasswordSuccessfulRequest = new Mock<IAppsRepository<App>>();
            PermitSuperUserRequest = new Mock<IAppsRepository<App>>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new App(
                            3,
                            "Test App 4",
                            "2b789e72-1df3-4313-8091-68cfa8a1db60",
                            1,
                            "TestSuperUser",
                            "https://localhost:8080",
                            "https://testapp3-dev.com",
                            "https://testapp3-qa.com",
                            "https://testapp3.com",
                            string.Empty,
                            true,
                            false,
                            true,
                            ReleaseEnvironment.LOCAL,
                            true,
                            string.Empty,
                            string.Empty,
                            false,
                            TimeFrame.DAYS,
                            1,
                            false,
                            todaysDate,
                            DateTime.MinValue)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetByLicenseAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Objects = context.Apps.ToList().ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 1)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetMyRegisteredAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 1)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAppUsersAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Objects = context.Users
                            .Where(user => user.Apps.Any(ua => ua.AppId == 1))
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetNonAppUsersAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users
                            .Where(user => user.Apps.Any(ua => ua.AppId != 1))
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo => 
                repo.UpdateRangeAsync(It.IsAny<List<App>>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.AddAppUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(a => a.Id == 2)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<App>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.ResetAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.IsAppLicenseValidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.GetLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(
                        TestObjects.GetLicense()));

            SuccessfulRequest.Setup(repo =>
                repo.IsUserRegisteredToAppAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.IsUserOwnerOThisfAppAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.GetGalleryAppsAsync())
                    .ReturnsAsync(new RepositoryResponse()
                        {
                            IsSuccess = true,
                            Objects = context
                                .Apps
                                .Where(a => a.IsActive && a.DisplayInGallery)
                                .ToList()
                                .ConvertAll(a => (IDomainEntity)a)
                        });
            #endregion

            #region FailedRequest
            FailedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Object = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Object = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetByLicenseAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Object = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Objects = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Objects = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetMyRegisteredAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Objects = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAppUsersAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Objects = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetNonAppUsersAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Objects = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Object = null
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<App>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.AddAppUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<App>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.ResetAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.IsAppLicenseValidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.GetLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(string.Empty));

            FailedRequest.Setup(repo =>
                repo.IsUserRegisteredToAppAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.IsUserOwnerOThisfAppAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.GetGalleryAppsAsync())
                    .ReturnsAsync(new RepositoryResponse()
                        {
                            IsSuccess = false,
                        });
            #endregion

            #region InitiatePasswordSuccessfulRequest
            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new App(
                            3,
                            "Test App 4",
                            "2b789e72-1df3-4313-8091-68cfa8a1db60",
                            1,
                            "TestSuperUser",
                            "https://localhost:8080",
                            "https://testapp3-dev.com",
                            "https://testapp3-qa.com",
                            "https://testapp3.com",
                            string.Empty,
                            true,
                            false,
                            true,
                            ReleaseEnvironment.LOCAL,
                            true,
                            string.Empty,
                            string.Empty,
                            false,
                            TimeFrame.DAYS,
                            1,
                            false,
                            todaysDate,
                            DateTime.MinValue)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 2)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetByLicenseAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Apps.ToList().ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 1)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetMyRegisteredAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false,
                        Objects = null
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetAppUsersAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users
                            .Where(user => user.Apps.Any(ua => ua.AppId == 1))
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetNonAppUsersAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users
                            .Where(user => user.Apps.Any(ua => ua.AppId != 1))
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<App>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.AddAppUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<App>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.ResetAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.IsAppLicenseValidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(
                        TestObjects.GetLicense()));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.IsUserRegisteredToAppAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.IsUserOwnerOThisfAppAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetGalleryAppsAsync())
                    .ReturnsAsync(new RepositoryResponse()
                        {
                            IsSuccess = true,
                            Objects = context
                                .Apps
                                .Where(a => a.IsActive && a.DisplayInGallery)
                                .ToList()
                                .ConvertAll(a => (IDomainEntity)a)
                        });
            #endregion

            #region PermitSuperUserRequest
            PermitSuperUserRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new App(
                            3,
                            "Test App 4",
                            "2b789e72-1df3-4313-8091-68cfa8a1db60",
                            1,
                            "TestSuperUser",
                            "https://localhost:8080",
                            "https://testapp3-dev.com",
                            "https://testapp3-qa.com",
                            "https://testapp3.com",
                            string.Empty,
                            true,
                            false,
                            true,
                            ReleaseEnvironment.LOCAL,
                            true,
                            string.Empty,
                            string.Empty,
                            false,
                            TimeFrame.DAYS,
                            1,
                            false,
                            todaysDate,
                            DateTime.MinValue)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 3)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetByLicenseAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 3)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Apps.ToList().ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 1)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetMyRegisteredAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 1)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetAppUsersAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users
                            .Where(user => user.Apps.Any(ua => ua.AppId == 1))
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetNonAppUsersAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users
                            .Where(user => user.Apps.Any(ua => ua.AppId != 1))
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<App>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.AddAppUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.RemoveAppUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(a => a.Id == 2)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<App>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.ResetAsync(It.IsAny<App>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Apps.FirstOrDefault(a => a.Id == 1)
                    } as IRepositoryResponse));

            PermitSuperUserRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserRequest.Setup(repo =>
                repo.IsAppLicenseValidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(
                        TestObjects.GetThirdLicense()));

            PermitSuperUserRequest.Setup(repo =>
                repo.IsUserRegisteredToAppAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserRequest.Setup(repo =>
                repo.IsUserOwnerOThisfAppAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserRequest.Setup(repo =>
                repo.GetGalleryAppsAsync())
                    .ReturnsAsync(new RepositoryResponse()
                        {
                            IsSuccess = true,
                            Objects = context
                                .Apps
                                .Where(a => a.IsActive && a.DisplayInGallery)
                                .ToList()
                                .ConvertAll(a => (IDomainEntity)a)
                        });
            #endregion
        }
    }
}
