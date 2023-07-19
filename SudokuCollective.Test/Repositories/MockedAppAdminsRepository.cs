using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;

namespace SudokuCollective.Test.Repositories
{
    public class MockedAppAdminsRepository
    {
        private readonly DatabaseContext context;
        internal Mock<IAppAdminsRepository<AppAdmin>> SuccessfulRequest { get; set; }
        internal Mock<IAppAdminsRepository<AppAdmin>> FailedRequest { get; set; }
        internal Mock<IAppAdminsRepository<AppAdmin>> PromoteUserRequest { get; set; }

        public MockedAppAdminsRepository(DatabaseContext ctxt)
        {
            context = ctxt;

            SuccessfulRequest = new Mock<IAppAdminsRepository<AppAdmin>>();
            FailedRequest = new Mock<IAppAdminsRepository<AppAdmin>>();
            PromoteUserRequest = new Mock<IAppAdminsRepository<AppAdmin>>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<AppAdmin>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Object = new AppAdmin() { 
                            Id = 2,
                            AppId = 1,
                            UserId = 2,
                            IsActive = true }
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.AppAdmins.FirstOrDefault(aa => aa.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Objects = context.AppAdmins
                            .ToList()
                            .ConvertAll(aa => (IDomainEntity)aa)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<AppAdmin>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Difficulties.FirstOrDefault(d => d.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<AppAdmin>>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    { 
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<AppAdmin>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<AppAdmin>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.HasAdminRecordAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.GetAdminRecordAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.AppAdmins.FirstOrDefault(aa => aa.Id == 1)
                    } as IRepositoryResponse));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<AppAdmin>()))
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
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<AppAdmin>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<AppAdmin>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<AppAdmin>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<AppAdmin>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.HasAdminRecordAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.GetAdminRecordAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));
            #endregion

            #region PromoteUserRequest
            PromoteUserRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<AppAdmin>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new AppAdmin()
                        {
                            Id = 2,
                            AppId = 1,
                            UserId = 2,
                            IsActive = true
                        }
                    } as IRepositoryResponse));

            PromoteUserRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.AppAdmins.FirstOrDefault(aa => aa.Id == 1)
                    } as IRepositoryResponse));

            PromoteUserRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.AppAdmins
                            .ToList()
                            .ConvertAll(aa => (IDomainEntity)aa)
                    } as IRepositoryResponse));

            PromoteUserRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<AppAdmin>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Difficulties.FirstOrDefault(d => d.Id == 1)
                    } as IRepositoryResponse));

            PromoteUserRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<AppAdmin>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PromoteUserRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<AppAdmin>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PromoteUserRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<AppAdmin>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            PromoteUserRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            PromoteUserRequest.Setup(repo =>
                repo.HasAdminRecordAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            PromoteUserRequest.Setup(repo =>
                repo.GetAdminRecordAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.AppAdmins.FirstOrDefault(aa => aa.Id == 1)
                    } as IRepositoryResponse));
            #endregion
        }
    }
}
