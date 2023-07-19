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
    public class MockedPasswordResetsRepository
    {
        private readonly DatabaseContext context;
        internal Mock<IPasswordResetsRepository<PasswordReset>> SuccessfulRequest { get; set; }
        internal Mock<IPasswordResetsRepository<PasswordReset>> FailedRequest { get; set; }
        internal Mock<IPasswordResetsRepository<PasswordReset>> SuccessfullyCreatedRequest { get; set; }

        public MockedPasswordResetsRepository(DatabaseContext ctxt)
        {
            context = ctxt;

            SuccessfulRequest = new Mock<IPasswordResetsRepository<PasswordReset>>();
            FailedRequest = new Mock<IPasswordResetsRepository<PasswordReset>>();
            SuccessfullyCreatedRequest = new Mock<IPasswordResetsRepository<PasswordReset>>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(repo =>
                repo.CreateAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new PasswordReset()
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.PasswordResets.FirstOrDefault(pr => pr.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.PasswordResets
                            .ToList()
                            .ConvertAll(d => (IDomainEntity)d)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.PasswordResets.FirstOrDefault()
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.HasOutstandingPasswordResetAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.RetrievePasswordResetAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.PasswordResets.FirstOrDefault(pr => pr.Id == 1)
                    } as IRepositoryResponse));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(repo =>
                repo.CreateAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<string>()))
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
                repo.UpdateAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.HasOutstandingPasswordResetAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.RetrievePasswordResetAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));
            #endregion

            #region SuccessfullyCreatedRequest
            SuccessfullyCreatedRequest.Setup(repo =>
                repo.CreateAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new PasswordReset()
                    } as IRepositoryResponse));

            SuccessfullyCreatedRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.PasswordResets.FirstOrDefault(pr => pr.Id == 1)
                    } as IRepositoryResponse));

            SuccessfullyCreatedRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.PasswordResets
                            .ToList()
                            .ConvertAll(d => (IDomainEntity)d)
                    } as IRepositoryResponse));

            SuccessfullyCreatedRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.PasswordResets.FirstOrDefault()
                    } as IRepositoryResponse));

            SuccessfullyCreatedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<PasswordReset>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfullyCreatedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfullyCreatedRequest.Setup(repo =>
                repo.HasOutstandingPasswordResetAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            SuccessfullyCreatedRequest.Setup(repo =>
                repo.RetrievePasswordResetAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));
            #endregion
        }
    }
}
