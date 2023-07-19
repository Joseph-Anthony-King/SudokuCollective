using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;

namespace SudokuCollective.Test.Repositories
{
    public class MockedUsersRepository
    {
        private readonly DatabaseContext context;
        internal Mock<IUsersRepository<User>> SuccessfulRequest { get; set; }
        internal Mock<IUsersRepository<User>> FailedRequest { get; set; }
        internal Mock<IUsersRepository<User>> EmailFailedRequest { get; set; }
        internal Mock<IUsersRepository<User>> InitiatePasswordSuccessfulRequest { get; set; }
        internal Mock<IUsersRepository<User>> ResendEmailConfirmationSuccessfulRequest { get; set; }
        internal Mock<IUsersRepository<User>> PermitSuperUserSuccessfulRequest { get; set; }

        public MockedUsersRepository(DatabaseContext ctxt)
        {
            context = ctxt;
            var todaysDate = DateTime.UtcNow;

            SuccessfulRequest = new Mock<IUsersRepository<User>>();
            FailedRequest = new Mock<IUsersRepository<User>>();
            EmailFailedRequest = new Mock<IUsersRepository<User>>();
            InitiatePasswordSuccessfulRequest = new Mock<IUsersRepository<User>>();
            ResendEmailConfirmationSuccessfulRequest = new Mock<IUsersRepository<User>>();
            PermitSuperUserSuccessfulRequest = new Mock<IUsersRepository<User>>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new User(
                            4,
                            "TestUser3",
                            "Test",
                            "User 3",
                            "Test User 3",
                            "TestUser3@example.com",
                            true,
                            false,
                            "password",
                            false,
                            true,
                            todaysDate,
                            DateTime.MinValue)
                        {
                            Apps = new List<UserApp>()
                            {
                                new UserApp()
                                {
                                    App = new App()
                                }
                            }
                        }
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 2)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetByUserNameAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.UserName.Equals("TestSuperUser"))
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetByEmailAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.Email.Equals("TestSuperUser@example.com"))
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Users
                            .ToList()
                            .ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 2)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users.ToList().ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.ConfirmEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 2)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)

                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAppLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(TestData.TestObjects.GetLicense()));

            SuccessfulRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.PromoteToAdminAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.IsUserRegisteredAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.IsUserNameUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.IsEmailUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.IsUpdatedUserNameUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.IsUpdatedEmailUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetByUserNameAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetByEmailAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.ConfirmEmailAsync(It.IsAny<IEmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false

                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAppLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(string.Empty));

            FailedRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.PromoteToAdminAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.IsUserRegisteredAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.IsUserNameUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.IsEmailUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.IsUpdatedUserNameUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.IsUpdatedEmailUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(false));
            #endregion

            #region EmailFailedRequest
            EmailFailedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new User(
                            4,
                            "TestUser3",
                            "Test",
                            "User 3",
                            "Test User 3",
                            "TestUser3@example.com",
                            false,
                            false,
                            "password",
                            false,
                            true,
                            todaysDate,
                            DateTime.MinValue)
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.GetByUserNameAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.UserName.Equals("TestSuperUser"))
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.GetByEmailAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.Email.Equals("TestSuperUser@example.com"))
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Users
                            .ToList()
                            .ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users.ToList().ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            EmailFailedRequest.Setup(repo =>
                repo.AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.ConfirmEmailAsync(It.IsAny<IEmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.UpdateEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false

                    } as IRepositoryResponse));

            EmailFailedRequest.Setup(repo =>
                repo.GetAppLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(string.Empty));

            EmailFailedRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            EmailFailedRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            EmailFailedRequest.Setup(repo =>
                repo.PromoteToAdminAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            EmailFailedRequest.Setup(repo =>
                repo.IsUserRegisteredAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            EmailFailedRequest.Setup(repo =>
                repo.IsUserNameUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            EmailFailedRequest.Setup(repo =>
                repo.IsEmailUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(false));

            EmailFailedRequest.Setup(repo =>
                repo.IsUpdatedUserNameUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            EmailFailedRequest.Setup(repo =>
                repo.IsUpdatedEmailUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(false));
            #endregion

            #region InitiatePasswordSuccessfulRequest
            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new User(
                            4,
                            "TestUser3",
                            "Test",
                            "User 3",
                            "Test User 3",
                            "TestUser3@example.com",
                            true,
                            false,
                            "password",
                            false,
                            true,
                            todaysDate,
                            DateTime.MinValue)
                        {
                            Apps = new List<UserApp>()
                            {
                                new UserApp()
                                {
                                    App = new App()
                                }
                            }
                        }
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 3)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetByUserNameAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.UserName.Equals("TestSuperUser"))
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetByEmailAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.Email.Equals("TestSuperUser@example.com"))
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Users
                            .ToList()
                            .ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users.ToList().ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.ConfirmEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.UpdateEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 2)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)

                    } as IRepositoryResponse));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.GetAppLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(TestData.TestObjects.GetLicense()));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.PromoteToAdminAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.IsUserRegisteredAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.IsUserNameUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.IsEmailUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.IsUpdatedUserNameUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            InitiatePasswordSuccessfulRequest.Setup(repo =>
                repo.IsUpdatedEmailUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));
            #endregion

            #region ResendEmailConfirmationSuccessfulRequest
            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new User(
                            4,
                            "TestUser3",
                            "Test",
                            "User 3",
                            "Test User 3",
                            "TestUser3@example.com",
                            true,
                            false,
                            "password",
                            false,
                            true,
                            todaysDate,
                            DateTime.MinValue)
                        {
                            Apps = new List<UserApp>()
                            {
                                new UserApp()
                                {
                                    App = new App()
                                }
                            }
                        }
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 3)
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.GetByUserNameAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.UserName.Equals("TestSuperUser"))
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.GetByEmailAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.Email.Equals("TestSuperUser@example.com"))
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Users
                            .ToList()
                            .ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users.ToList().ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.ConfirmEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.UpdateEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 2)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)

                    } as IRepositoryResponse));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.GetAppLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(TestData.TestObjects.GetLicense()));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.PromoteToAdminAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.IsUserRegisteredAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.IsUserNameUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.IsEmailUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.IsUpdatedUserNameUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            ResendEmailConfirmationSuccessfulRequest.Setup(repo =>
                repo.IsUpdatedEmailUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));
            #endregion

            #region PermitSuperUserSuccessfulRequest
            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new User(
                            4,
                            "TestUser3",
                            "Test",
                            "User 3",
                            "Test User 3",
                            "TestUser3@example.com",
                            true,
                            false,
                            "password",
                            false,
                            true,
                            todaysDate,
                            DateTime.MinValue)
                        {
                            Apps = new List<UserApp>()
                            {
                                new UserApp()
                                {
                                    App = new App()
                                }
                            }
                        }
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.GetByUserNameAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.UserName.Equals("TestSuperUser"))
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.GetByEmailAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context
                            .Users
                            .FirstOrDefault(u => u.Email.Equals("TestSuperUser@example.com"))
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Users
                            .ToList()
                            .ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 2)
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Users.ToList().ConvertAll(u => (IDomainEntity)u)
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<User>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<User>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.AddRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.RemoveRolesAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.ConfirmEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.UpdateEmailAsync(It.IsAny<EmailConfirmation>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Users.FirstOrDefault(u => u.Id == 1)
                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.GetMyAppsAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context
                            .Apps
                            .Where(a => a.OwnerId == 2)
                            .ToList()
                            .ConvertAll(a => (IDomainEntity)a)

                    } as IRepositoryResponse));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.GetAppLicenseAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(TestData.TestObjects.GetLicense()));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.ActivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.DeactivateAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.PromoteToAdminAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.IsUserRegisteredAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.IsUserNameUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.IsEmailUniqueAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.IsUpdatedUserNameUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            PermitSuperUserSuccessfulRequest.Setup(repo =>
                repo.IsUpdatedEmailUniqueAsync(It.IsAny<int>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));
            #endregion
        }
    }
}
