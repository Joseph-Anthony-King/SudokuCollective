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

namespace SudokuCollective.Test.Repositories
{
    public class MockedDifficultiesRepository
    {
        private readonly DatabaseContext context;
        internal Mock<IDifficultiesRepository<Difficulty>> SuccessfulRequest { get; set; }
        internal Mock<IDifficultiesRepository<Difficulty>> FailedRequest { get; set; }
        internal Mock<IDifficultiesRepository<Difficulty>> CreateDifficultyRequest { get; set; }

        public MockedDifficultiesRepository(DatabaseContext ctxt)
        {
            context = ctxt;

            SuccessfulRequest = new Mock<IDifficultiesRepository<Difficulty>>();
            FailedRequest = new Mock<IDifficultiesRepository<Difficulty>>();
            CreateDifficultyRequest = new Mock<IDifficultiesRepository<Difficulty>>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<Difficulty>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Object = new Difficulty() { DifficultyLevel = DifficultyLevel.NULL }
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Difficulties.FirstOrDefault(d => d.Id == 4)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Objects = context.Difficulties
                            .Where(d => 
                                d.DifficultyLevel != DifficultyLevel.NULL && d.DifficultyLevel != DifficultyLevel.TEST)
                            .ToList()
                            .ConvertAll(d => (IDomainEntity)d)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<Difficulty>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Difficulties.FirstOrDefault(d => d.Id == 4)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<Difficulty>>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    { 
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<Difficulty>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<Difficulty>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.HasDifficultyLevelAsync(It.IsAny<DifficultyLevel>()))
                    .Returns(Task.FromResult(true));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<Difficulty>()))
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
                repo.UpdateAsync(It.IsAny<Difficulty>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<Difficulty>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<Difficulty>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<Difficulty>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.HasDifficultyLevelAsync(It.IsAny<DifficultyLevel>()))
                    .Returns(Task.FromResult(false));
            #endregion

            #region CreateDifficultyRequest
            CreateDifficultyRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<Difficulty>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new Difficulty() { DifficultyLevel = DifficultyLevel.NULL }
                    } as IRepositoryResponse));

            CreateDifficultyRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Difficulties.FirstOrDefault(d => d.Id == 4)
                    } as IRepositoryResponse));

            CreateDifficultyRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Difficulties
                            .Where(d =>
                                d.DifficultyLevel != DifficultyLevel.NULL && d.DifficultyLevel != DifficultyLevel.TEST)
                            .ToList()
                            .ConvertAll(d => (IDomainEntity)d)
                    } as IRepositoryResponse));

            CreateDifficultyRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<Difficulty>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Difficulties.FirstOrDefault(d => d.Id == 4)
                    } as IRepositoryResponse));

            CreateDifficultyRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<Difficulty>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            CreateDifficultyRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<Difficulty>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            CreateDifficultyRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<Difficulty>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            CreateDifficultyRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            CreateDifficultyRequest.Setup(repo =>
                repo.HasDifficultyLevelAsync(It.IsAny<DifficultyLevel>()))
                    .Returns(Task.FromResult(false));
            #endregion
        }
    }
}
