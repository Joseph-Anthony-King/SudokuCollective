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

namespace SudokuCollective.Test.Repositories
{
    public class MockedGamesRepository
    {
        private readonly DatabaseContext context;
        internal Mock<IGamesRepository<Game>> SuccessfulRequest { get; set; }
        internal Mock<IGamesRepository<Game>> FailedRequest { get; set; }
        internal Mock<IGamesRepository<Game>> SolvedRequest { get; set; }
        internal Mock<IGamesRepository<Game>> UpdateFailedRequest { get; set; }

        public MockedGamesRepository(DatabaseContext ctxt)
        {
            context = ctxt;
            var todaysDate = DateTime.UtcNow;

            SuccessfulRequest = new Mock<IGamesRepository<Game>>();
            FailedRequest = new Mock<IGamesRepository<Game>>();
            SolvedRequest = new Mock<IGamesRepository<Game>>();
            UpdateFailedRequest = new Mock<IGamesRepository<Game>>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Object = new Game(
                            context.Users.FirstOrDefault(u => u.Id == 1),
                            new SudokuMatrix(),
                            context.Difficulties.FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.TEST),
                            context.Apps.Where(a => a.Id == 1).Select(a => a.Id).FirstOrDefault())
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<Game>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    { 
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse() 
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<Game>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(repo =>
                repo.GetAppGameAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAppGamesAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetMyGamesAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.DeleteMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<Game>()))
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
                repo.UpdateAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<Game>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<Game>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(repo =>
                repo.GetAppGameAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAppGamesAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetMyGamesAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.DeleteMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));
            #endregion

            #region SolvedRequest
            SolvedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new Game(
                            context.Users.FirstOrDefault(u => u.Id == 1),
                            new SudokuMatrix(),
                            context.Difficulties.FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.TEST),
                            context.Apps.Where(a => a.Id == 1).Select(a => a.Id).FirstOrDefault())
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 2)
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<Game>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<Game>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SolvedRequest.Setup(repo =>
                repo.GetAppGameAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.GetAppGamesAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.GetMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.GetMyGamesAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            SolvedRequest.Setup(repo =>
                repo.DeleteMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));
            #endregion

            #region UpdateFailedRequest
            UpdateFailedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new Game(
                            context.Users.FirstOrDefault(u => u.Id == 1),
                            new SudokuMatrix(),
                            context.Difficulties.FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.TEST),
                            context.Apps.Where(a => a.Id == 1).Select(a => a.Id).FirstOrDefault())
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.UpdateAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.UpdateRangeAsync(It.IsAny<List<Game>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.DeleteAsync(It.IsAny<Game>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.DeleteRangeAsync(It.IsAny<List<Game>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            UpdateFailedRequest.Setup(repo =>
                repo.GetAppGameAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.GetAppGamesAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.GetMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Games.FirstOrDefault(g => g.Id == 1)
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.GetMyGamesAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Games.ToList().ConvertAll(g => (IDomainEntity)g)
                    } as IRepositoryResponse));

            UpdateFailedRequest.Setup(repo =>
                repo.DeleteMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            #endregion
        }
    }
}
